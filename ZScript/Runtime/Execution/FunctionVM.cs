#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;

using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Typing;

namespace ZScript.Runtime.Execution
{
    /// <summary>
    /// Represents a virtual machine used to run a single function call
    /// </summary>
    public class FunctionVM
    {
        /// <summary>
        /// Whether the virtual machine has a return value associated
        /// </summary>
        private bool _hasReturnValue;

        /// <summary>
        /// The return value for the virtual machine execution
        /// </summary>
        private object _returnValue;

        /// <summary>
        /// The context for this function call virtual machine
        /// </summary>
        private readonly VmContext _context;

        /// <summary>
        /// The list of tokens to execute
        /// </summary>
        private readonly TokenList _tokens;

        /// <summary>
        /// The current stack of values
        /// </summary>
        private readonly Stack<object> _stack;

        /// <summary>
        /// The offset of the current token being executed by the virtual machine
        /// </summary>
        private int _codePosition;

        /// <summary>
        /// The type operations provider for the virtual machine
        /// </summary>
        private readonly TypeOperationProvider _typeOperationProvider;

        /// <summary>
        /// Gets a value specifying whether the virtual machine has a return value associated
        /// </summary>
        public bool HasReturnValue
        {
            get { return _hasReturnValue; }
        }

        /// <summary>
        /// Gets the return value for the virtual machine execution.
        /// Raises an exception if the VM has no return value associated. Use the HasReturnValue property to verify if a return value is present, first
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The virtual machine had no return value specified. Use <see cref="HasReturnValue"/> to verify the existence of a return value before accessing this property.
        /// </exception>
        public object ReturnValue
        {
            get
            {
                if (!_hasReturnValue)
                    throw new InvalidOperationException("No return value was specified by the VM code to return");
                
                return _returnValue;
            }
        }

        /// <summary>
        /// Gets the stack of items being evaluated in this function VM
        /// </summary>
        public Stack<object> Stack
        {
            get { return _stack; }
        }

        /// <summary>
        /// Initializes a new instance of the FunctionVM class with a list of tokens to execute
        /// </summary>
        /// <param name="tokens">The tokens to use when executing the VM</param>
        /// <param name="context">The context of the function VM</param>
        public FunctionVM(TokenList tokens, VmContext context)
        {
            _stack = new Stack<object>();
            _tokens = tokens;
            _context = context;
            _typeOperationProvider = new TypeOperationProvider();
        }

        /// <summary>
        /// Executes the instructions of the token list on this VM
        /// </summary>
        public void Execute()
        {
            // The current instruction being executed
            _codePosition = 0;

            while (_codePosition < _tokens.Tokens.Length)
            {
                var token = _tokens.Tokens[_codePosition];

                switch (token.Type)
                {
                    case TokenType.Operator:
                        PerformOperation(token);
                        break;
                    case TokenType.Instruction:
                        // Virtual machine interruption
                        switch (token.Instruction)
                        {
                            case VmInstruction.Interrupt:
                                return;
                            case VmInstruction.Ret:
                                if (_stack.Count > 0)
                                {
                                    _returnValue = PopValueImplicit();
                                    _hasReturnValue = true;
                                }

                                return;
                            case VmInstruction.Jump:
                                _codePosition = (int)token.TokenObject;
                                continue;
                            case VmInstruction.JumpIfTrue:
                                if ((bool)PopValueImplicit())
                                {
                                    //_codePosition = target;
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfFalse:
                                if (!(bool)PopValueImplicit())
                                {
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfTruePeek:
                                if ((bool)PeekValueImplicit())
                                {
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfFalsePeek:
                                if (!(bool)PeekValueImplicit())
                                {
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;
                            default:
                                PerformInstruction(token.Instruction, token.TokenObject);
                                break;
                        }

                        break;
                    case TokenType.MemberName:
                        _stack.Push(token);
                        break;
                    case TokenType.GlobalFunction:
                        _stack.Push(_context.Runtime.FunctionAtIndex((int)token.TokenObject));
                        break;
                    default:
                        _stack.Push(token.TokenObject);
                        break;
                }

                _codePosition++;
            }
        }

        /// <summary>
        /// Performs an operation specified by a given instruction on items on the top of in the stack, an pushes the result in back again
        /// </summary>
        /// <param name="token">A token containing the instruction that specifies the operation to perform</param>
        void PerformOperation(Token token)
        {
            VmInstruction instruction = token.Instruction;

            switch (instruction)
            {
                // Special treatment for 'is' and 'cast' operators
                case VmInstruction.Is:
                    PerformIsOperator(token);
                    return;
                case VmInstruction.Cast:
                    PerformCastOperation(token);
                    return;

                // Special treatement for the increment/decrement/unary negate/logical negate operators
                case VmInstruction.IncrementPrefix:
                case VmInstruction.IncrementPostfix:
                case VmInstruction.DecrementPrefix:
                case VmInstruction.DecrementPostfix:
                    PerformIncrementDecrement(instruction);
                    return;
                case VmInstruction.ArithmeticNegate:
                    _stack.Push(_typeOperationProvider.ArithmeticNegate(PopValueImplicit()));
                    return;
                case VmInstruction.LogicalNegate:
                    _stack.Push(_typeOperationProvider.LogicalNegate(PopValueImplicit()));
                    return;
            }

            object value2 = PopValueImplicit();
            object value1 = PopValueImplicit();

            object ret;
            
            // Evaluate the operation
            switch (instruction)
            {
                // Sum and subtraction
                case VmInstruction.Add:
                    ret = _typeOperationProvider.Sum(value1, value2);
                    break;
                case VmInstruction.Subtract:
                    ret = _typeOperationProvider.Subtract(value1, value2);
                    break;
                // Multiplication and division
                case VmInstruction.Multiply:
                    ret = _typeOperationProvider.Multiply(value1, value2);
                    break;
                case VmInstruction.Divide:
                    ret = _typeOperationProvider.Divide(value1, value2);
                    break;
                // Modulo operator
                case VmInstruction.Modulo:
                    ret = _typeOperationProvider.Modulo(value1, value2);
                    break;
                // Bitwise operators
                case VmInstruction.BitwiseAnd:
                    ret = _typeOperationProvider.BitwiseAnd(value1, value2);
                    break;
                case VmInstruction.BitwiseOr:
                    ret = _typeOperationProvider.BitwiseOr(value1, value2);
                    break;
                case VmInstruction.BitwiseXOr:
                    ret = _typeOperationProvider.BitwiseXOr(value1, value2);
                    break;
                case VmInstruction.ShiftLeft:
                    ret = _typeOperationProvider.ShiftLeft(value1, value2);
                    break;
                case VmInstruction.ShiftRight:
                    ret = _typeOperationProvider.ShiftRight(value1, value2);
                    break;
                // Equality/Inequality checks
                case VmInstruction.Equals:
                    ret = _typeOperationProvider.Equals(value1, value2);
                    break;
                case VmInstruction.Unequals:
                    ret = !_typeOperationProvider.Equals(value1, value2);
                    break;
                case VmInstruction.Less:
                    ret = _typeOperationProvider.Less(value1, value2);
                    break;
                case VmInstruction.LessOrEquals:
                    ret = _typeOperationProvider.LessOrEquals(value1, value2);
                    break;
                case VmInstruction.Greater:
                    ret = _typeOperationProvider.Greater(value1, value2);
                    break;
                case VmInstruction.GreaterOrEquals:
                    ret = _typeOperationProvider.GreaterOrEquals(value1, value2);
                    break;
                case VmInstruction.LogicalAnd:
                    ret = ((bool)value1 && (bool)value2);
                    break;
                case VmInstruction.LogicalOr:
                    ret = ((bool)value1 || (bool)value2);
                    break;
                default:
                    throw new ArgumentException("The VM instruction " + instruction + " cannot be used as a valid binary operator");
            }
            
            _stack.Push(ret);
        }

        /// <summary>
        /// Performs an increment/decrement on the value on the top of the stack, based on the given instruction type.
        /// If the instruction provided is not IncrementPrefix, IncrementPostfix, DecrementPrefix, or DecrementPostfix, an exception is raised
        /// </summary>
        /// <param name="instruction">The instruction containing the operation to perform</param>
        /// <exception cref="ArgumentException">The provided instruction is not a valid increment/decrement instruction</exception>
        void PerformIncrementDecrement(VmInstruction instruction)
        {
            // Pop the variable and value to get
            var variable = _stack.Pop();
            var value = GetValue(variable);
            var type = NumberTypeForBoxedNumber(value);

            if (instruction != VmInstruction.IncrementPostfix && instruction != VmInstruction.IncrementPrefix &&
                instruction != VmInstruction.DecrementPostfix && instruction != VmInstruction.DecrementPrefix)
                throw new ArgumentException("The provided instruction is not a valid increment/decrement instruction");

            int changeOnStack = instruction == VmInstruction.IncrementPrefix  ?  1 :
                                instruction == VmInstruction.DecrementPrefix  ? -1 : 0;

            int changeOnMemory = (instruction == VmInstruction.IncrementPrefix || instruction == VmInstruction.IncrementPostfix ? 1 : -1);

            switch (type)
            {
                case NumberType.Integer:
                    _stack.Push((int)value + changeOnStack);
                    SetValue(variable, (int)value + changeOnMemory);
                    return;
                case NumberType.Long:
                    long valueLong = (long)value;
                    _stack.Push(valueLong + changeOnStack);
                    SetValue(variable, valueLong + changeOnMemory);
                    return;
                case NumberType.Double:
                    double valueDouble = (double)value;
                    _stack.Push(valueDouble + changeOnStack);
                    SetValue(variable, valueDouble + changeOnMemory);
                    return;
            }

            dynamic valueDynamic = value;

            switch (instruction)
            {
                case VmInstruction.IncrementPrefix:
                    _stack.Push(valueDynamic + 1);
                    SetValue(variable, valueDynamic + 1);
                    break;
                case VmInstruction.IncrementPostfix:
                    _stack.Push(valueDynamic);
                    SetValue(variable, valueDynamic + 1);
                    break;
                case VmInstruction.DecrementPrefix:
                    _stack.Push(valueDynamic - 1);
                    SetValue(variable, valueDynamic - 1);
                    break;
                case VmInstruction.DecrementPostfix:
                    _stack.Push(valueDynamic);
                    SetValue(variable, valueDynamic - 1);
                    break;
            }
        }

        /// <summary>
        /// Performs an instruction specified by a given instruction.
        /// If the instruction provided is not a valid command/control instruction (e.g. it's an operator)
        /// </summary>
        /// <param name="instruction">The instruction to perform</param>
        /// <param name="parameter">The parameter for the instruction</param>
        /// <exception cref="ArgumentException">The provided instruction is not a valid command instruction</exception>
        void PerformInstruction(VmInstruction instruction, object parameter)
        {
            switch (instruction)
            {
                // No operation
                case VmInstruction.Noop:
                    return;

                // Memory get
                case VmInstruction.Get:
                    PerformGetInstruction();
                    break;

                // Memory set
                case VmInstruction.Set:
                    PerformSetInstruction();
                    break;

                // Memory get
                case VmInstruction.GetAtAddress:
                    PerformGetAtAddressInstruction();
                    break;

                // Memory set
                case VmInstruction.SetAtAddress:
                    PerformSetAtAddressInstruction();
                    break;

                // Clear Stack
                case VmInstruction.ClearStack:
                    _stack.Clear();
                    break;

                // Swap Stack
                case VmInstruction.Swap:
                    var value1 = _stack.Pop();
                    var value2 = _stack.Pop();

                    _stack.Push(value1);
                    _stack.Push(value2);
                    break;

                // Duplicate Stack
                case VmInstruction.Duplicate:
                    _stack.Push(_stack.Peek());
                    break;

                // Function call
                case VmInstruction.Call:
                    PerformFunctionCall();
                    break;

                // Array literal creation
                case VmInstruction.CreateArray:
                    PerformArrayCreation(parameter);
                    break;
                    
                // Dictionary literal creation
                case VmInstruction.CreateDictionary:
                    PerformDictionaryCreation(parameter);
                    break;

                // Subscripter fetching
                case VmInstruction.GetSubscript:
                    PerformGetSubscripter();
                    break;

                // Field fetching
                case VmInstruction.GetMember:
                    PerformGetMember();
                    break;

                // Method fetching
                case VmInstruction.GetCallable:
                    PerformGetCallable();
                    break;

                // Object Literal creation
                case VmInstruction.CreateObject:
                    PerformObjectCreation();
                    break;

                // New instruction
                case VmInstruction.New:
                    PerformNewInstruction();
                    break;

                default:
                    throw new ArgumentException("Unspecified virtual machine instruction '" + instruction + "'. Maybe it's an operation?");
            }
        }

        /// <summary>
        /// Performs a Get instruction with the value on the top of the stack
        /// </summary>
        void PerformGetInstruction()
        {
            // Pop the variable and value to get
            var variable = _stack.Pop();
            _stack.Push(GetValue(variable));
        }

        /// <summary>
        /// Performs a Set instruction with the values on the top of the stack
        /// </summary>
        void PerformSetInstruction()
        {
            // Pop the variable and value to set
            var variable = _stack.Pop();
            object value = PopValueImplicit();
            SetValue(variable, value);

            _stack.Push(value);
        }

        /// <summary>
        /// Performs a Get instruction with the value on the top of the stack
        /// </summary>
        void PerformGetAtAddressInstruction()
        {
            // Pop the variable and value to get
            var variable = _stack.Pop();
            _stack.Push(_context.AddressedMemory.GetVariable((int)variable));
        }

        /// <summary>
        /// Performs a Set instruction with the values on the top of the stack
        /// </summary>
        void PerformSetAtAddressInstruction()
        {
            // Pop the variable and value to set
            var variable = _stack.Pop();
            object value = PopValueImplicit();

            _context.AddressedMemory.SetVariable((int)variable, value);
            _stack.Push(value);
        }

        /// <summary>
        /// Performs a function call, utilizing the objects on the stack to locate and call the function
        /// on the current runtime
        /// </summary>
        void PerformFunctionCall()
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Pop the arguments from the stack
            ArrayList arguments = new ArrayList();

            for (int i = 0; i < argCount; i++)
            {
                arguments.Add(PopValueImplicit());
            }

            // Reverse the array, so early arguments come first
            arguments.Reverse();

            // Pop the function to call
            var callable = PopCallable();

            var zFunction = callable as ZFunction;
            if (zFunction != null)
            {
                _stack.Push(_context.Runtime.CallFunction(zFunction, arguments.ToArray()));
                return;
            }
            var wrapper = callable as ICallableWrapper;
            if (wrapper != null)
            {
                _stack.Push(_context.Runtime.CallFunction(wrapper, arguments.ToArray()));
                return;
            }

            var s = callable as string;
            if(s != null)
            {
                _stack.Push(_context.Runtime.CallFunction(s, arguments));
            }
        }

        /// <summary>
        /// Performs an array creation using the values on the stack, pushing the created array back into the top of the stack
        /// </summary>
        /// <param name="parameter">The parameter for the array creation, taken directly from a valid array creation instruction token</param>
        void PerformArrayCreation(object parameter)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Create the list
            var array = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType((Type)parameter));

            for (int i = 0; i < argCount; i++)
            {
                array.Insert(0, PopValueImplicit());
            }

            // Push the array back into the stack
            _stack.Push(array);
        }

        /// <summary>
        /// Performs a dictionary creation using the values on the stack, pushing the created dictionary back into the top of the stack
        /// </summary>
        /// <param name="parameter">The parameter for the dictionary creation, taken directly from a valid dictionary creation instruction token</param>
        void PerformDictionaryCreation(object parameter)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Get the types for the dictionary key and values
            var types = (Type[])parameter;
            var keyType = types[0];
            var valueType = types[1];

            // Create the dictionary
            var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

            // Pop the entries from the stack
            for (int i = 0; i < argCount; i++)
            {
                var value = PopValueImplicit();
                var key = PopValueImplicit();

                dict.Add(key, value);
            }

            // Push the array back into the stack
            _stack.Push(dict);
        }

        /// <summary>
        /// Performs a ZObject creation using the values on the stack, pushing the created object back into the top of the stack
        /// </summary>
        void PerformObjectCreation()
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Pop the arguments from the stack
            var obj = new ZObject();

            for (int i = 0; i < argCount; i++)
            {
                obj[(string)_stack.Pop()] = PopValueImplicit();
            }

            // Push the array back into the stack
            _stack.Push(obj);
        }

        /// <summary>
        /// Performs a New instruction using the values on the stack, pushing the created object back into the top of the stack
        /// </summary>
        void PerformNewInstruction()
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Pop the arguments from the stack
            ArrayList arguments = new ArrayList();

            for (int i = 0; i < argCount; i++)
            {
                arguments.Add(PopValueImplicit());
            }

            // Pop the type to create
            var typeName = (string)_stack.Pop();

            _stack.Push(_context.Owner.CreateType(typeName, arguments.ToArray()));
        }
        
        /// <summary>
        /// Performs a subscripter-fetch on the object on top of the stack, pushing a resulting ISubcripter back on the stack.
        /// All types that implement IList can be subscripted by the operation.
        /// If the value cannot be subscripted, an exception is raised.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value on top of the stack cannot be subscripted</exception>
        void PerformGetSubscripter()
        {
            object index = PopValueImplicit();
            object target = PopValueImplicit();

            _stack.Push(IndexedSubscripter.CreateSubscripter(target, index));
        }

        /// <summary>
        /// Performs a member-fetch on the object on top of the stack, pushing a resulting IMemberWrapper back on the stack
        /// </summary>
        void PerformGetMember()
        {
            object memberName = _stack.Pop();
            object target = PopValueImplicit();

            if (memberName is Token)
            {
                memberName = (string)((Token)memberName).TokenObject;
            }

            _stack.Push(MemberWrapperHelper.CreateMemberWrapper(target, (string)memberName));
        }

        /// <summary>
        /// Performs a method-fetch on the object on top of the stack, pushing a resulting ICallableWrapper back on the stack
        /// </summary>
        void PerformGetCallable()
        {
            object memberName = _stack.Pop();
            object target = PopValueImplicit();

            if (memberName is Token)
            {
                memberName = (string)((Token)memberName).TokenObject;
            }

            _stack.Push(MemberWrapperHelper.CreateCallableWrapper(target, (string)memberName));
        }

        /// <summary>
        /// Performs an 'is' operation with the values on top of the stack
        /// </summary>
        /// <param name="token">The 'is' operation token that contains the type to check against as argument</param>
        void PerformIsOperator(Token token)
        {
            object value = PopValueImplicit();

            _stack.Push(((Type)token.TokenObject).IsInstanceOfType(value));
        }

        /// <summary>
        /// Performs a cast operation with the values on top of the stack
        /// </summary>
        /// <param name="token">The cast operation token that contains the type to cast to as argument</param>
        void PerformCastOperation(Token token)
        {
            object value = PopValueImplicit();
            object castedObject = _context.TypeProvider.CastObject(value, (Type)token.TokenObject);

            _stack.Push(castedObject);
        }

        /// <summary>
        /// Pops a value from the stack, and if it is a token, implicitly fetch the value from the memory
        /// </summary>
        /// <returns>A value popped from the stack, and fetched from memory, if needed</returns>
        object PopValueImplicit()
        {
            return ExpandValue(_stack.Pop());
        }

        /// <summary>
        /// Pops a callable value from the stack, and if it is a token, implicitly fetch the function from the runtime
        /// </summary>
        /// <returns>A callable popped from the stack, and fetched from memory, if needed</returns>
        object PopCallable()
        {
            var obj = _stack.Pop();

            var t = obj as Token;
            if (t != null)
            {
                if (t.TokenObject is ZFunction)
                {
                    return t.TokenObject;
                }
                var s = t.TokenObject as string;
                if (s != null)
                {
                    var def = _context.Runtime.FunctionWithName(s);

                    if (def == null)
                    {
                        var callable = _context.Memory.GetVariable(s);
                        if (callable == null)
                        {
                            throw new Exception("Trying to call undefined callable '" + s + "'");
                        }
                        if (!IsCallable(callable))
                        {
                            throw new Exception("Trying to call non-callable object type " + callable);
                        }

                        return callable;
                    }

                    return def;
                }
            }
            if (obj is ZFunction)
            {
                return obj as ZFunction;
            }
            if (obj is ICallableWrapper)
            {
                return obj;
            }

            throw new Exception("Value on top of the stack cannot be called: '" + obj + "'");
        }

        /// <summary>
        /// Returns a value specifying whether a given object is a callable object type
        /// </summary>
        /// <param name="obj">An object to verify</param>
        /// <returns>Whether the given object is a callable object type</returns>
        bool IsCallable(object obj)
        {
            // TODO: deal with different callable types (MethodInfo, etc.)
            if (obj is ZFunction)
                return true;

            return false;
        }

        /// <summary>
        /// Peeks a value from the stack, and if it is a token, implicitly fetch the value from the memory
        /// </summary>
        /// <returns>A value peeked from the stack, and fetched from memory, if needed</returns>
        object PeekValueImplicit()
        {
            return ExpandValue(_stack.Peek());
        }

        /// <summary>
        /// Expands a given value, returning an object that the value represents
        /// </summary>
        /// <param name="obj">The object to expand</param>
        /// <returns>The value the object represents</returns>
        private object ExpandValue(object obj)
        {
            var t = obj as Token;
            if (t != null)
            {
                // Pop the variable and value to get
                object value;

                if (!_context.Memory.TryGetVariable((string)t.TokenObject, out value))
                {
                    var f = _context.Runtime.FunctionWithName((string)t.TokenObject);

                    if (f != null)
                        return f;

                    throw new Exception("Trying to access undefined variable '" + (string)t.TokenObject + "'.");
                }

                return value;
            }
            var valueHolder = obj as IValueHolder;
            if (valueHolder != null)
            {
                return valueHolder.GetValue();
            }

            return obj;
        }

        /// <summary>
        /// Gets the value of the specified object.
        /// The object must either be a boxed token containing a variable name from memory, FieldInfo or PropertyInfo
        /// </summary>
        /// <param name="valueContainer">The value containing the object to get</param>
        object GetValue(object valueContainer)
        {
            var token = valueContainer as Token;
            if (token != null)
            {
                return _context.Memory.GetVariable((string)token.TokenObject);
            }
            if (valueContainer is int)
            {
                return _context.AddressedMemory.GetVariable((int)valueContainer);
            }
            var valueHolder = valueContainer as IValueHolder;
            if (valueHolder != null)
            {
                return valueHolder.GetValue();
            }

            throw new Exception("Unexpected variable '" + valueContainer + "' that cannot have its value get");
        }

        /// <summary>
        /// Gets the value of the specified object.
        /// The object must either be a boxed token containing a variable name from memory, FieldInfo or PropertyInfo
        /// </summary>
        /// <param name="valueContainer">The value containing the object to get</param>
        /// <param name="value">The value to set</param>
        void SetValue(object valueContainer, object value)
        {
            var token = valueContainer as Token;
            if (token != null)
            {
                _context.Memory.SetVariable((string)token.TokenObject, value);
                return;
            }
            if (valueContainer is int)
            {
                _context.AddressedMemory.SetVariable((int)valueContainer, value);
                return;
            }
            var valueHolder = valueContainer as IValueHolder;
            if (valueHolder != null)
            {
                valueHolder.SetValue(value);
                return;
            }
            
            throw new Exception("Unexpected variable '" + valueContainer + "' that cannot have its value set");
        }

        /// <summary>
        /// Returns the number type for a given boxed number type
        /// </summary>
        /// <param name="boxedNumber">The boxed number</param>
        /// <returns>The number type for the given boxed number</returns>
        public static NumberType NumberTypeForBoxedNumber(object boxedNumber)
        {
            if(boxedNumber is short)
                return NumberType.Short;
            if(boxedNumber is ushort)
                return NumberType.UShort;
            if (boxedNumber is int)
                return NumberType.Integer;
            if (boxedNumber is uint)
                return NumberType.UInteger;
            if (boxedNumber is long)
                return NumberType.Long;
            if (boxedNumber is ulong)
                return NumberType.ULong;
            if (boxedNumber is float)
                return NumberType.Float;
            if (boxedNumber is double)
                return NumberType.Double;

            return NumberType.Unspecified;
        }

        /// <summary>
        /// Specified the type of a number
        /// </summary>
        public enum NumberType
        {
            /// <summary>An Int16 type</summary>
            Short,
            /// <summary>An UInt16 type</summary>
            UShort,
            /// <summary>An Int32 type</summary>
            Integer,
            /// <summary>An UInt32 type</summary>
            UInteger,
            /// <summary>An Int64 type</summary>
            Long,
            /// <summary>An Int64 type</summary>
            ULong,
            /// <summary>A single precision floating point type</summary>
            Float,
            /// <summary>A double precision floating point type</summary>
            Double,
            /// <summary>An unspecified numeric type</summary>
            Unspecified
        }
    }

    /// <summary>
    /// Represents a VM instruction
    /// </summary>
    public enum VmInstruction
    {
        /// <summary>Represents no operation</summary>
        Noop,

        /// <summary>Interrupts the virtual machine</summary>
        Interrupt,

        /// <summary>Postfix increment</summary>
        IncrementPostfix,
        /// <summary>Postfix decrement</summary>
        DecrementPostfix,

        /// <summary>Prefix increment</summary>
        IncrementPrefix,
        /// <summary>Prefix decrement</summary>
        DecrementPrefix,

        /// <summary>Multiplication arithmetic operation</summary>
        Multiply,
        /// <summary>Division arithmetic operation</summary>
        Divide,
        /// <summary>Modulo arithmetic operation</summary>
        Modulo,
        
        /// <summary>Addition arithmetic operation</summary>
        Add,
        /// <summary>Subtraction arithmetic operation</summary>
        Subtract,
        
        /// <summary>Bitwise AND operation</summary>
        BitwiseAnd,
        /// <summary>Bitwise XOR operation</summary>
        BitwiseXOr,
        /// <summary>Bitwise OR operation</summary>
        BitwiseOr,

        /// <summary>Shift bits left operation</summary>
        ShiftLeft,
        /// <summary>Shift bits right operation</summary>
        ShiftRight,
        
        /// <summary>Equality operator operation</summary>
        Equals,
        /// <summary>Unequality operator operation</summary>
        Unequals,
        /// <summary>Larger than or equals operation</summary>
        GreaterOrEquals,
        /// <summary>Larger than or equals operation</summary>
        LessOrEquals,
        /// <summary>Larger operation</summary>
        Greater,
        /// <summary>Larger operation</summary>
        Less,

        /// <summary>Performs a type check with two operands on the top of the stack, pushing a boolean result on the top of the stack</summary>
        Is,

        /// <summary>Performs a type cast with the value on the top of the stack, pushing a converted value result on the top of the stack</summary>
        Cast,
        
        /// <summary>Logical AND (&&) operation</summary>
        LogicalAnd,
        /// <summary>Logical OR (&&) operation</summary>
        LogicalOr,

        /// <summary>Ends the function, and if the stack is not empty, pops the top-most value of the stack and using it as a return value</summary>
        Ret,
        /// <summary>Performs a function call with the information provided on the stack</summary>
        Call,
        /// <summary>Fetches the member of a value on the stack</summary>
        GetMember,
        /// <summary>Fetches the subscript of the object on top of the stack</summary>
        GetSubscript,
        /// <summary>Fetches a callable function from the object on the top of the stack</summary>
        GetCallable,
        /// <summary>Creates a new instance of a type, using the objects at the stack as parameters for the creation</summary>
        New,

        /// <summary>Jumps to the position specified at the top of the stack</summary>
        Jump,
        /// <summary>Jumps to the position specified at the top of the stack, but only if the value directly bellow evaluates to true</summary>
        JumpIfTrue,
        /// <summary>Jumps to the position specified at the top of the stack, but only if the value directly bellow evaluates to false</summary>
        JumpIfFalse,
        /// <summary>
        /// Jumps to the position specified at the top of the stack, but only if the value directly bellow evaluates to true.
        /// Does not consumes the value on the top of the stack
        /// </summary>
        JumpIfTruePeek,
        /// <summary>
        /// Jumps to the position specified at the top of the stack, but only if the value directly bellow evaluates to false.
        /// Does not consumes the value on the top of the stack
        /// </summary>
        JumpIfFalsePeek,

        /// <summary>Pop A from the stack, and searches in memory a variable with its value, as an integer address</summary>
        GetAtAddress,
        /// <summary>Pops A, B from the stack, and set the value of variable at address B to be A</summary>
        SetAtAddress,

        /// <summary>Pop A from the stack, and searches in memory a variable with its value, as a string</summary>
        Get,
        /// <summary>Pops A, B from the stack, and set the value of variable B to be A</summary>
        Set,
        /// <summary>Swaps the two top-most values on the stack</summary>
        Swap,
        /// <summary>Duplicates the top-most value of the stack, effectively making two copies of the value rest at the top of the stack</summary>
        Duplicate,
        /// <summary>Clears the current stack of values of the virtual machine</summary>
        ClearStack,

        /// <summary>Creates an array using the values on the stack and pushes the result back into the stack</summary>
        CreateArray,
        /// <summary>Creates a dictionary using the values on the stack and pushes the result back into the stack</summary>
        CreateDictionary,

        /// <summary>Creates an object using the values on the stack and pushes the result back into the stack</summary>
        CreateObject,

        /// <summary>Unary negation ('-') instruction</summary>
        ArithmeticNegate,
        /// <summary>Logical negation ('!') instruction</summary>
        LogicalNegate
    }
}