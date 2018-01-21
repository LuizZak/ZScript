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
using System.Reflection;
using JetBrains.Annotations;
using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Execution.Wrappers.Callables;
using ZScript.Runtime.Execution.Wrappers.Members;
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
        public bool HasReturnValue => _hasReturnValue;

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
        public Stack<object> Stack => _stack;

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
                            case VmInstruction.Pop:
                                _stack.Pop();
                                break;
                            case VmInstruction.Jump:
                                _codePosition = (int)token.TokenObject;
                                continue;
                            case VmInstruction.JumpIfTrue:
                                if ((bool)PopValueImplicit())
                                {
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
                            case VmInstruction.JumpIfNotNull:
                                if (PopValueImplicit() != null)
                                {
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfNull:
                                if (PopValueImplicit() == null)
                                {
                                    _codePosition = (int)token.TokenObject;
                                    continue;
                                }
                                break;

                            // Type check
                            case VmInstruction.CheckType:
                                PerformTypeCheck(token);
                                break;

                            // Null check
                            case VmInstruction.CheckNull:
                                PerformCheckNull();
                                break;

                            default:
                                PerformInstruction(token);
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
        void PerformOperation([NotNull] Token token)
        {
            var instruction = token.Instruction;

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

            var value2 = PopValueImplicit();
            var value1 = PopValueImplicit();

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

            int changeOnStack = instruction == VmInstruction.IncrementPrefix  ?  1 :
                                instruction == VmInstruction.DecrementPrefix  ? -1 : 0;

            int changeOnMemory = (instruction == VmInstruction.IncrementPrefix || instruction == VmInstruction.IncrementPostfix ? 1 : -1);

            switch (type)
            {
                case NumberType.Integer:
                    int valueInt = (int)value;
                    _stack.Push(valueInt + changeOnStack);
                    SetValue(variable, valueInt + changeOnMemory);
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
        /// <param name="token">The token containing the instruction to perform</param>
        /// <exception cref="ArgumentException">The provided instruction is not a valid command instruction</exception>
        void PerformInstruction([NotNull] Token token)
        {
            var instruction = token.Instruction;

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

                // Try dispose
                case VmInstruction.TryDispose:
                    PerformTryDispose();
                    break;

                // Function call
                case VmInstruction.Call:
                    PerformFunctionCall(token);
                    break;

                // Generic function call
                case VmInstruction.CallGeneric:
                    PerformGenericFunctionCall(token);
                    break;

                // Array literal creation
                case VmInstruction.CreateArray:
                    PerformArrayCreation(token);
                    break;
                    
                // Tuple creation
                case VmInstruction.CreateTuple:
                    PerformTupleCreation(token);
                    break;

                // Dictionary literal creation
                case VmInstruction.CreateDictionary:
                    PerformDictionaryCreation(token);
                    break;

                // Subscripter fetching
                case VmInstruction.GetSubscript:
                    PerformGetSubscripter();
                    break;

                // Field fetching
                case VmInstruction.GetMember:
                    PerformGetMember(token);
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

                // Unwrap
                case VmInstruction.Unwrap:
                    PerformUnwrap();
                    break;

                // Wrap
                case VmInstruction.Wrap:
                    PerformWrap(token);
                    break;

                // Safe unwrap
                case VmInstruction.SafeUnwrap:
                    PerformSafeUnwrap(false);
                    break;

                // Safe unwrap nullified
                case VmInstruction.SafeUnwrapNullified:
                    PerformSafeUnwrap(true);
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
            var value = PopValueImplicit(true);
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
            var value = PopValueImplicit(true);

            _context.AddressedMemory.SetVariable((int)variable, value);
            _stack.Push(value);
        }

        /// <summary>
        /// Performs a generic function call, utilizing the objects on the stack to locate and call the function
        /// on the current runtime, and using the operands on the token as the generic parameters
        /// </summary>
        /// <param name="token">The token for the CallGeneric instruction being executed</param>
        void PerformGenericFunctionCall([NotNull] Token token)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Pop the arguments from the stackh
            var arguments = new object[argCount];

            for (int i = 0; i < argCount; i++)
            {
                arguments[argCount - i - 1] = PopValueImplicit(true);
            }

            var callable = PopCallable();

            // Fetch the generic types
            var types = ExtractTypes(token);

            CallFunction(callable, types, arguments);
        }

        /// <summary>
        /// Performs a function call, utilizing the objects on the stack to locate and call the function
        /// on the current runtime
        /// </summary>
        /// <param name="token">The token for the Call instruction being executed</param>
        void PerformFunctionCall([NotNull] Token token)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Pop the arguments from the stackh
            var arguments = new object[argCount];

            for (int i = argCount - 1; i >= 0; i--)
            {
                arguments[i] = PopValueImplicit(true);
            }

            object callable;

            // Verify whether the call instruction contains a wrapped method information
            if (token.TokenObject != null)
            {
                // Direct MethodInfo call
                var mInfo = token.TokenObject as MethodInfo;
                if (mInfo != null)
                {
                    var target = PopValueImplicit();
                    _stack.Push(mInfo.Invoke(target, arguments));
                    return;
                }

                if (token.TokenObject is ZMethod zMethod)
                {
                    callable = new ZClassMethod((ZClassInstance)PopValueImplicit(), zMethod);
                }
                else
                {
                    if (token.TokenObject is ZFunction zFunc)
                    {
                        callable = zFunc;
                    }
                    else
                    {
                        throw new VirtualMachineException("Operand on Call instruction is not a valid method or ZMethod reference");
                    }
                }
            }
            else
            {
                // Pop the function to call
                callable = PopCallable();
            }

            CallFunction(callable, new Type[0], arguments);
        }

        /// <summary>
        /// Calls a callable function object with the specified parameters on this FunctionVM
        /// </summary>
        private void CallFunction(object callable, [NotNull] Type[] genericTypes, object[] arguments)
        {
            if (callable is ZClosureFunction closure)
            {
                callable = closure.Clone();
            }

            var args = new CallArguments(arguments, genericTypes);

            switch (callable)
            {
                case ZFunction zFunction:
                    _stack.Push(_context.Runtime.CallFunction(zFunction, args));
                    return;
                case ICallableWrapper wrapper:
                    _stack.Push(_context.Runtime.CallWrapper(wrapper, args));
                    break;
            }
        }

        /// <summary>
        /// Performs an array creation using the values on the stack, pushing the created array back into the top of the stack
        /// </summary>
        /// <param name="token">The token for the array creation instruction</param>
        void PerformArrayCreation([NotNull] Token token)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Create the list
            var array = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ExtractType(token)));

            for (int i = 0; i < argCount; i++)
            {
                array.Insert(0, PopValueImplicit(true));
            }

            // Push the array back into the stack
            _stack.Push(array);
        }

        /// <summary>
        /// Performs a tuple creation using the values on the stack, pushing the created tuple back into the top of the stack
        /// </summary>
        /// <param name="token">The token for the tuple creation instruction</param>
        void PerformTupleCreation([NotNull] Token token)
        {
            // Pop the types 
            var tupleType = ExtractType(token);

            // Create the list
            var array = new object[tupleType.GetFields().Length];

            for (int i = array.Length - 1; i >= 0; i--)
            {
                array[i] = PopValueImplicit(true);
            }
            
            if(tupleType.GetGenericArguments().Length == 0)
            {
                tupleType = tupleType.MakeGenericType(Type.GetTypeArray(array));
            }

            // Push the array back into the stack
            _stack.Push(Activator.CreateInstance(tupleType, array));
        }

        /// <summary>
        /// Performs a dictionary creation using the values on the stack, pushing the created dictionary back into the top of the stack
        /// </summary>
        /// <param name="token">The token for the dictionary creation</param>
        void PerformDictionaryCreation([NotNull] Token token)
        {
            // Pop the argument count
            int argCount = (int)_stack.Pop();

            // Get the types for the dictionary key and values
            var types = ExtractTypes(token);
            var keyType = types[0];
            var valueType = types[1];

            // Create the dictionary
            var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

            // Pop the entries from the stack
            for (int i = 0; i < argCount; i++)
            {
                var value = PopValueImplicit(true);
                var key = PopValueImplicit(true);

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
                obj[_stack.Pop()] = PopValueImplicit(true);
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
            var arguments = new ArrayList();

            for (int i = 0; i < argCount; i++)
            {
                arguments.Add(PopValueImplicit(true));
            }

            // Pop the type to create
            var typeName = (string)_stack.Pop();

            _stack.Push(_context.Owner.CreateType(typeName, new CallArguments(arguments.ToArray())));
        }
        
        /// <summary>
        /// Performs a subscripter-fetch on the object on top of the stack, pushing a resulting ISubcripter back on the stack.
        /// All types that implement IList can be subscripted by the operation.
        /// If the value cannot be subscripted, an exception is raised.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value on top of the stack cannot be subscripted</exception>
        void PerformGetSubscripter()
        {
            var index = PopValueImplicit(true);
            var target = PopValueImplicit();

            _stack.Push(IndexedSubscripter.CreateSubscripter(target, index));
        }

        /// <summary>
        /// Performs a member-fetch on the object on top of the stack, pushing a resulting IMemberWrapper back on the stack
        /// </summary>
        void PerformGetMember([NotNull] Token token)
        {
            if (token.TokenObject != null)
            {
                var fieldInfo = token.TokenObject as FieldInfo;
                if (fieldInfo != null)
                {
                    _stack.Push(ClassMember.CreateFieldWrapper(PopValueImplicit(), fieldInfo));
                    return;
                }
                var propInfo = token.TokenObject as PropertyInfo;
                if (propInfo != null)
                {
                    _stack.Push(ClassMember.CreatePropertyWrapper(PopValueImplicit(), propInfo));
                    return;
                }
            }

            var memberName = _stack.Pop();
            var target = PopValueImplicit();

            memberName = (memberName as Token)?.TokenObject ?? memberName;

            _stack.Push(MemberWrapperHelper.CreateMemberWrapper(target, (string)memberName));
        }

        /// <summary>
        /// Performs a method-fetch on the object on top of the stack, pushing a resulting ICallableWrapper back on the stack
        /// </summary>
        void PerformGetCallable()
        {
            var memberName = _stack.Pop();
            var target = PopValueImplicit();

            memberName = (memberName as Token)?.TokenObject ?? memberName;

            _stack.Push(MemberWrapperHelper.CreateCallableWrapper(target, (string)memberName));
        }

        /// <summary>
        /// Performs an 'is' operation with the values on top of the stack
        /// </summary>
        /// <param name="token">The 'is' operation token that contains the type to check against as argument</param>
        void PerformIsOperator([NotNull] Token token)
        {
            var value = PopValueImplicit();

            _stack.Push(ExtractType(token).IsInstanceOfType(value));
        }

        /// <summary>
        /// Performs a cast operation with the values on top of the stack
        /// </summary>
        /// <param name="token">The cast operation token that contains the type to cast to as argument</param>
        void PerformCastOperation([NotNull] Token token)
        {
            var value = PopValueImplicit(true);
            var castedObject = _context.TypeProvider.CastObject(value, ExtractType(token));

            _stack.Push(castedObject);
        }

        /// <summary>
        /// Performs a type check on the value on top of the stack.
        /// The value on top of the stack is not removed
        /// </summary>
        /// <param name="token">The type check instruction token that contains the type to check against</param>
        void PerformTypeCheck([NotNull] Token token)
        {
            var value = PeekValueImplicit();
            var type = ExtractType(token);

            CheckType(value, type);
        }

        /// <summary>
        /// Performs a type check, raising a VirtualMachine exception when the value provided is not of the given type.
        /// If the value is null and the type is a value type, or the value is not null and it doesn't matches the type, an exception is raised
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="type">The type to verify against the value</param>
        /// <exception cref="VirtualMachineException">The value's type does not matches the provided type</exception>
        private static void CheckType(object value, [NotNull] Type type)
        {
            // Null value types
            if (value == null && type.IsValueType)
            {
                throw new VirtualMachineException("Cannot convert null to value type type " + type + ".");
            }
            // Null values
            if (value != null && !type.IsInstanceOfType(value))
            {
                throw new VirtualMachineException("Cannot convert type " + value.GetType() + " to type " + type + ".");
            }
        }

        /// <summary>
        /// Performs a null check on the value on top of the stack.
        /// The value on top of the stack is not removed
        /// </summary>
        void PerformCheckNull()
        {
            var obj = PeekValueImplicit();

            // Null and empty optional values
            var optional = obj as IOptional;
            if (obj == null || optional != null && !optional.HasInnerValue)
            {
                throw new VirtualMachineException("Value on top of the stack is null");
            }
        }

        /// <summary>
        /// Performs an unwrap on the optional value contained on top of the stack.
        /// The contained value is pushed on top of the stack.
        /// Raises an exception, if the value on top of the stack is not an optional value
        /// </summary>
        /// <exception cref="InvalidOperationException">The value on top of the stack is not an optional value</exception>
        void PerformUnwrap()
        {
            var opt = (IOptional)PopValueImplicit();

            _stack.Push(opt.InnerValue);
        }

        /// <summary>
        /// Performs a wrap on the optional value contained on top of the stack.
        /// The contained wrapped is pushed on top of the stack
        /// </summary>
        /// <param name="token">The token containing the instruction to wrap</param>
        void PerformWrap([NotNull] Token token)
        {
            var value = PopValueImplicit();
            if(value == null)
            {
                _stack.Push(Activator.CreateInstance((Type)token.TokenObject));
                return;
            }

            _stack.Push(Activator.CreateInstance((Type)token.TokenObject, value));
        }

        /// <summary>
        /// Performs a safe unwrap on the optional value contained on top of the stack.
        /// The contained value is pushed on the stack followed by a boolean value stating whether the unwrapping was successful.
        /// Raises an exception, if the value on top of the stack is not an optional value
        /// </summary>
        /// <param name="pushNull">Whether to push null if no value is contained within the optional</param>
        void PerformSafeUnwrap(bool pushNull)
        {
            var opt = (IOptional)PopValueImplicit();

            if(opt.HasInnerValue)
                _stack.Push(opt.InnerValue);
            else if (pushNull)
                _stack.Push(null);

            _stack.Push(opt.HasInnerValue);
        }

        /// <summary>
        /// Performs a dispose on the value on top of the stack, if it's an IDisposable object
        /// </summary>
        void PerformTryDispose()
        {
            (PopValueImplicit() as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Extracts the type contained within a given token.
        /// The type is extacted if the token contains a valid TokenObject derived from Type or an integer that points to a type
        /// from the type stack contained within the VM context.
        /// An exception is raised if the type cannot be extracted successfully
        /// </summary>
        /// <param name="token">The token containing the type to extract</param>
        /// <returns>A type extracted from the given token</returns>
        Type ExtractType([NotNull] Token token)
        {
            var type = token.TokenObject as Type;
            if(type != null)
                return type;

            if (token.TokenObject is int i)
            {
                return _context.TypeList.TypeAtIndex(i);
            }

            throw new VirtualMachineException("Could not extract type from tokens " + token);
        }

        /// <summary>
        /// Extracts the types contained within a given token.
        /// The types are extacted if the token contains a valid TokenObject derived from a Type array or an integer array that points to the types
        /// from the type stack contained within the VM context.
        /// An exception is raised if the types cannot be extracted successfully
        /// </summary>
        /// <param name="token">The token containing the types to extract</param>
        /// <returns>An array of types extracted from the given token</returns>
        Type[] ExtractTypes([NotNull] Token token)
        {
            if (token.TokenObject is Type[] type)
                return type;

            if (token.TokenObject is int[] ind)
            {
                var types = new Type[ind.Length];

                for (int i = 0; i < ind.Length; i++)
                {
                    types[i] = _context.TypeList.TypeAtIndex(ind[i]);
                }

                return types;
            }

            throw new VirtualMachineException("Could not extract type from tokens " + token);
        }

        #region Value manipulation

        /// <summary>
        /// Pops a value from the stack, and if it is a token, implicitly fetch the value from the memory
        /// </summary>
        /// <param name="copyTuples">Whether to perform the copy of tuples if the return value is a tuple type</param>
        /// <returns>A value popped from the stack, and fetched from memory, if needed</returns>
        public object PopValueImplicit(bool copyTuples = false)
        {
            return ExpandValue(_stack.Pop(), copyTuples);
        }

        /// <summary>
        /// Pops a callable value from the stack, and if it is a token, implicitly fetch the function from the runtime
        /// </summary>
        /// <returns>A callable popped from the stack, and fetched from memory, if needed</returns>
        public object PopCallable()
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
        public static bool IsCallable(object obj)
        {
            // TODO: deal with different callable types (MethodInfo, etc.)
            if (obj is ZFunction)
                return true;

            if (obj is ICallableWrapper)
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
        /// <param name="copyTuples">Whether to clone tuples when the provided value is an ITuple type</param>
        /// <returns>The value the object represents</returns>
        private object ExpandValue(object obj, bool copyTuples = false)
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

                if (copyTuples && value is ITuple)
                    value = Activator.CreateInstance(value.GetType(), value);

                return value;
            }
            var valueHolder = obj as IValueHolder;
            if (valueHolder != null)
            {
                var value = valueHolder.GetValue();

                if (copyTuples && value is ITuple)
                    value = Activator.CreateInstance(value.GetType(), value);

                return value;
            }

            return obj;
        }

        /// <summary>
        /// Gets the value of the specified object.
        /// The object must either be a boxed token containing a variable name from memory, FieldInfo or PropertyInfo
        /// </summary>
        /// <param name="valueContainer">The value containing the object to get</param>
        /// <exception cref="VirtualMachineException">The value container cannot have its value get</exception>
        object GetValue([NotNull] object valueContainer)
        {
            object value;

            var token = valueContainer as Token;
            if (token != null)
            {
                value = _context.Memory.GetVariable((string)token.TokenObject);
            }
            else if (valueContainer is int)
            {
                value = _context.AddressedMemory.GetVariable((int)valueContainer);
            }
            else
            {
                var valueHolder = valueContainer as IValueHolder;
                if (valueHolder != null)
                {
                    value = valueHolder.GetValue();
                }
                else
                {
                    throw new VirtualMachineException("Unexpected variable '" + valueContainer + "' that cannot have its value get");
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the value of the specified object.
        /// The object must either be a boxed token containing a variable name from memory, FieldInfo or PropertyInfo
        /// </summary>
        /// <param name="valueContainer">The value containing the object to get</param>
        /// <param name="value">The value to set</param>
        /// <exception cref="VirtualMachineException">The value container cannot have its value set</exception>
        void SetValue([NotNull] object valueContainer, object value)
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

            throw new VirtualMachineException("Unexpected variable '" + valueContainer + "' that cannot have its value set");
        }

        #endregion

        /// <summary>
        /// Returns the number type for a given boxed number type
        /// </summary>
        /// <param name="boxedNumber">The boxed number</param>
        /// <returns>The number type for the given boxed number</returns>
        public static NumberType NumberTypeForBoxedNumber(object boxedNumber)
        {
            // Leave the most common types first to decrease useless lookup times
            switch (boxedNumber)
            {
                case long _:
                    return NumberType.Long;
                case double _:
                    return NumberType.Double;
                case int _:
                    return NumberType.Integer;
                case byte _:
                    return NumberType.Byte;
                case sbyte _:
                    return NumberType.SByte;
                case decimal _:
                    return NumberType.Decimal;
                case short _:
                    return NumberType.Short;
                case ushort _:
                    return NumberType.UShort;
                case uint _:
                    return NumberType.UInteger;
                case ulong _:
                    return NumberType.ULong;
                case float _:
                    return NumberType.Float;
            }
            
            return NumberType.Unspecified;
        }

        /// <summary>
        /// Specified the type of a number
        /// </summary>
        public enum NumberType
        {
            /// <summary>A byte type</summary>
            Byte,
            /// <summary>A signed byte type</summary>
            SByte,
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
            /// <summary>A decimal type</summary>
            Decimal,
            /// <summary>A single precision floating point type</summary>
            Float,
            /// <summary>A double precision floating point type</summary>
            Double,
            /// <summary>An unspecified numeric type</summary>
            Unspecified
        }
    }

    /// <summary>
    /// Represents a general virtual machine exception
    /// </summary>
    public class VirtualMachineException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the VirtualMachineException class
        /// </summary>
        /// <param name="message">The message for the exception</param>
        public VirtualMachineException(string message)
            : base(message)
        {
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

        /// <summary>Pops and discards the top most value off of the stack</summary>
        Pop,

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
        /// <summary>Performs a type check with the value on top of the stack, raising a runtime exception if the type does not matches the expected type</summary>
        CheckType,
        /// <summary>Performs a null check with the value on top of the stack, raising a runtime exception if the value is null</summary>
        CheckNull,
        /// <summary>Performs an unwrap operation on the optional contained on the top of the stack, pushing the unwrapped value back on top of the stack</summary>
        Unwrap,
        /// <summary>Performs a wrap operation on the value contained on the top of the stack, pushing the wrapped value back on top of the stack</summary>
        Wrap,
        /// <summary>
        /// Performs a safe unwrap operation on the optional contained on the top of the stack, pushing the unwrapped value,
        /// if it existed, and a boolean value stating whether the optional contained a value.
        /// If the value does not exists, only the boolean flag is pushed
        /// </summary>
        SafeUnwrap,
        /// <summary>
        /// Performs a safe unwrap operation on the optional contained on the top of the stack, pushing the unwrapped value,
        /// if it existed, and a boolean value stating whether the optional contained a value.
        /// If the value does not exists, a null value is pushed instead
        /// </summary>
        SafeUnwrapNullified,

        /// <summary>Logical AND (&amp;&amp;) operation</summary>
        LogicalAnd,
        /// <summary>Logical OR (||) operation</summary>
        LogicalOr,

        /// <summary>Ends the function, and if the stack is not empty, pops the top-most value of the stack and using it as a return value</summary>
        Ret,
        /// <summary>Performs a function call with the information provided on the stack</summary>
        Call,
        /// <summary>Performs a generic function call with the information provided on the stack</summary>
        CallGeneric,
        /// <summary>Fetches the member of a value on the stack</summary>
        GetMember,
        /// <summary>Fetches the subscript of the object on top of the stack</summary>
        GetSubscript,
        /// <summary>Fetches a callable function from the object on the top of the stack</summary>
        GetCallable,
        /// <summary>Creates a new instance of a type, using the objects at the stack as parameters for the creation</summary>
        New,

        /// <summary>Jumps to the position specified at the argument</summary>
        Jump,
        /// <summary>Jumps to the position specified at the argument, but only if the value on top of the stack evaluates to true</summary>
        JumpIfTrue,
        /// <summary>Jumps to the position specified at the argument, but only if the value on top of the stack evaluates to false</summary>
        JumpIfFalse,
        /// <summary>
        /// Jumps to the position specified at the argument, but only if the value on top of the stack evaluates to true.
        /// Does not consumes the value on the top of the stack
        /// </summary>
        JumpIfTruePeek,
        /// <summary>
        /// Jumps to the position specified at the argument, but only if the value on top of the stack evaluates to false.
        /// Does not consumes the value on the top of the stack
        /// </summary>
        JumpIfFalsePeek,
        /// <summary>Jumps to the position specified at the argument, but only if the value on top of the stack does not evaluates to null</summary>
        JumpIfNotNull,
        /// <summary>Jumps to the position specified at the argument, but only if the value on top of the stack evaluates to null</summary>
        JumpIfNull,

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
        /// <summary>Pops the top-most value on the stack and tries disposing it if it implements the IDisposable interface</summary>
        TryDispose,

        /// <summary>Creates an array using the values on the stack and pushes the result back into the stack</summary>
        CreateArray,
        /// <summary>Creates a tuple using the values on the stack and pushes the result back into the stack</summary>
        CreateTuple,
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