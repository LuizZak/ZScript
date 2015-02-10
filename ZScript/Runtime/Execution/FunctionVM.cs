using System;
using System.Collections;
using System.Collections.Generic;

using ZScript.Elements;
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
        public object ReturnValue
        {
            get
            {
                if (!_hasReturnValue)
                    throw new Exception("No return value was specified by the VM code to return");
                
                return _returnValue;
            }
        }

        /// <summary>
        /// Gets the stack of items being evaluated in this function VM
        /// </summary>
        public Stack<Object> Stack
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
        /// Executes the instructions of this VM
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
                        PerformOperation(token.Instruction);
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
                                _codePosition = (int)_stack.Pop();
                                continue;
                            case VmInstruction.JumpIfTrue:
                                int target = (int)_stack.Pop();
                                if ((bool)PopValueImplicit())
                                {
                                    _codePosition = target;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfFalse:
                                target = (int)_stack.Pop();
                                if (!(bool)PopValueImplicit())
                                {
                                    _codePosition = target;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfTruePeek:
                                target = (int)_stack.Pop();
                                if ((bool)PeekValueImplicit())
                                {
                                    _codePosition = target;
                                    continue;
                                }
                                break;
                            case VmInstruction.JumpIfFalsePeek:
                                target = (int)_stack.Pop();
                                if (!(bool)PeekValueImplicit())
                                {
                                    _codePosition = target;
                                    continue;
                                }
                                break;
                            default:
                                PerformInstruction(token.Instruction);
                                break;
                        }

                        break;
                    case TokenType.MemberName:
                        _stack.Push(token);
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
        /// <param name="instruction">An instruction that specified the operation to perform</param>
        void PerformOperation(VmInstruction instruction)
        {
            // Special treatement for the increment/decrement/unary negate/logical negate operators
            switch (instruction)
            {
                case VmInstruction.IncrementPrefix:
                case VmInstruction.IncrementPostfix:
                case VmInstruction.DecrementPrefix:
                case VmInstruction.DecrementPostfix:
                    PerformIncrementDecrement(instruction);
                    return;
                case VmInstruction.UnaryNegate:
                    _stack.Push(-(dynamic)PopValueImplicit());
                    return;
                case VmInstruction.LogicalNegate:
                    _stack.Push(!(dynamic)PopValueImplicit());
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
                    //ret = (value2 + value1);
                    ret = _typeOperationProvider.Sum(value1, value2);
                    break;
                case VmInstruction.Subtract:
                    //ret = (value2 - value1);
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
                case NumberType.Short:
                    short valueShort = (short)value;
                    _stack.Push((short)(valueShort + changeOnStack));
                    SetValue(variable, (short)(valueShort + changeOnMemory));
                    return;
                case NumberType.UShort:
                    ushort valueUShort = (ushort)value;
                    _stack.Push((ushort)(valueUShort + changeOnStack));
                    SetValue(variable, (ushort)(valueUShort + changeOnMemory));
                    return;
                case NumberType.Integer:
                    _stack.Push((int)value + changeOnStack);
                    SetValue(variable, (int)value + changeOnMemory);
                    return;
                case NumberType.UInteger:
                    uint valueUInt = (uint)value;
                    _stack.Push((uint)(valueUInt + changeOnStack));
                    SetValue(variable, (uint)(valueUInt + changeOnMemory));
                    return;
                case NumberType.Long:
                    long valueLong = (long)value;
                    _stack.Push(valueLong + changeOnStack);
                    SetValue(variable, valueLong + changeOnMemory);
                    return;
                case NumberType.ULong:
                    ulong valueULong = (ulong)value;
                    _stack.Push((valueULong + (ulong)changeOnStack));
                    SetValue(variable, valueULong + (ulong)changeOnMemory);
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
        /// <exception cref="ArgumentException">The provided instruction is not a valid command instruction</exception>
        void PerformInstruction(VmInstruction instruction)
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

            //if (variable is int)
            {
                //_context.AddressedMemory.SetVariable((int)variable, value);
            }
            //else
            {
                SetValue(variable, value);
            }

            _stack.Push(value);
        }

        /// <summary>
        /// Performs a Get instruction with the value on the top of the stack
        /// </summary>
        void PerformGetAtAddressInstruction()
        {
            // Pop the variable and value to get
            var variable = _stack.Pop();
            //_stack.Push(_context.AddressedMemory.GetVariable((int)variable));
            _stack.Push(GetValue(variable));
        }

        /// <summary>
        /// Performs a Set instruction with the values on the top of the stack
        /// </summary>
        void PerformSetAtAddressInstruction()
        {
            // Pop the variable and value to set
            var variable = _stack.Pop();
            object value = PopValueImplicit();

            SetValue(variable, value);
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

            // Pop the function to call
            var callable = PopCallable();

            var zFunction = callable as ZFunction;
            if (zFunction != null)
            {
                _stack.Push(_context.Runtime.CallFunction(zFunction, arguments.ToArray()));
            }
            else
            {
                _stack.Push(_context.Runtime.CallFunction((string)callable, arguments));
            }
        }

        /// <summary>
        /// Pops a value from the stack, and if it is a token, implicitly fetch the value from the memory
        /// </summary>
        /// <returns>A value popped from the stack, and fetched from memory, if needed</returns>
        object PopValueImplicit()
        {
            var obj = _stack.Pop();

            var t = obj as Token;
            if (t != null)
            {
                // Pop the variable and value to get
                if (!_context.Memory.HasVariable((string)t.TokenObject))
                {
                    var f = _context.Runtime.FunctionWithName((string)t.TokenObject);

                    if (f != null)
                        return f;

                    throw new Exception("Trying to access undefined variable '" + (string)t.TokenObject + "'.");
                }

                return _context.Memory.GetVariable((string)t.TokenObject);
            }

            return obj;
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
            var obj = _stack.Peek();

            var t = obj as Token;
            if (t != null)
            {
                // Pop the variable and value to get
                return _context.Memory.GetVariable((string)t.TokenObject);
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
            }
            else if (valueContainer is int)
            {
                _context.AddressedMemory.SetVariable((int)valueContainer, value);
            }
            else
            {
                throw new Exception("Unexpected variable '" + valueContainer + "' that cannot have its value set");   
            }
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
        /// Returns the type that equates to the specified number type
        /// </summary>
        /// <param name="type">The type of number to get the type of</param>
        /// <returns>The type that equates to the specified number type</returns>
        public static Type GetTypeForNumberType(NumberType type)
        {
            switch (type)
            {
                case NumberType.Short:
                    return typeof(short);
                case NumberType.UShort:
                    return typeof(ushort);
                case NumberType.Integer:
                    return typeof(int);
                case NumberType.UInteger:
                    return typeof(uint);
                case NumberType.Long:
                    return typeof(long);
                case NumberType.ULong:
                    return typeof(ulong);
                case NumberType.Float:
                    return typeof(float);
                case NumberType.Double:
                    return typeof(double);

                default:
                    return null;
            }
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
        /// <summary>Represents an instruction that fetches the subscript of the object on top of the stack</summary>
        GetSubscript,

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

        /// <summary>Unary negation ('-') instruction</summary>
        UnaryNegate,
        /// <summary>Logical negation ('!') instruction</summary>
        LogicalNegate
    }
}