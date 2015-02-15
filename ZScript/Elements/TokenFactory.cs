using System;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Runtime.Execution;

namespace ZScript.Elements
{
    /// <summary>
    /// Class used to create Token instances
    /// </summary>
    public class TokenFactory
    {
        /// <summary>
        /// Token that represents a GET_SUBSCRIPT call
        /// </summary>
        public readonly Token GetSubscriptToken = new Token(TokenType.Instruction, null, VmInstruction.GetSubscript);

        /// <summary>
        /// Creates a new token that represents a member name
        /// </summary>
        /// <param name="memberName">A string that represents a member name</param>
        /// <returns>A Token with the member name provided binded in</returns>
        public static Token CreateMemberNameToken(string memberName)
        {
            return new Token(TokenType.MemberName, memberName, VmInstruction.Noop);
        }

        /// <summary>
        /// Creates a new variable token that represents a variable name
        /// </summary>
        /// <param name="variableName">A string that represents a variable name</param>
        /// <param name="isGet">Whether to create the token with a get access</param>
        /// <returns>A VariableToken with the variable name provided binded in</returns>
        public static VariableToken CreateVariableToken(string variableName, bool isGet)
        {
            return new VariableToken(variableName, isGet);
        }

        /// <summary>
        /// Creates a new token with a given string as the content.
        /// The resulting token will have a TokenType.String associated with it
        /// </summary>
        /// <param name="content">A string to populate the token with</param>
        /// <returns>A Token with the string provided binded in</returns>
        public static Token CreateStringToken(string content)
        {
            return new Token(TokenType.String, content, VmInstruction.Noop);
        }

        /// <summary>
        /// Creates a new token with a given boxed value as the content.
        /// The boxed value may be of any value; the returned token will have the TokenType.Value type associated with it.
        /// </summary>
        /// <param name="boxedValue">A boxed value to populate the token with</param>
        /// <returns>A Token with the boxed value provided binded in</returns>
        public static Token CreateBoxedValueToken(object boxedValue)
        {
            return new Token(TokenType.Value, boxedValue, VmInstruction.Noop);
        }

        /// <summary>
        /// Creates a new operator token based on the given operator string.
        /// The string must contain a string that represents the token of the arithmetic operator desired.
        /// The method raises an exception when the operator is unkown
        /// </summary>
        /// <param name="operatorString">An operator string, containing the token that corresponds to an arithmetic operator</param>
        /// <returns>A new token with the operator desired</returns>
        /// <exception cref="ArgumentException">The operator string provided was not recognized</exception>
        public static Token CreateOperatorToken(string operatorString)
        {
            var inst = InstructionForOperator(operatorString);

            return new Token(TokenType.Operator, null, inst);
        }

        /// <summary>
        /// Returns the VmInstruction associated with a given operator string.
        /// Throws an exception if not valid and if the throwOnError parameter is set to true, otherwise, returns VmInstruction.Noop
        /// </summary>
        /// <param name="operatorString">The string containing the operator</param>
        /// <param name="throwOnError">Whether to raise an exception, if the operator is not valid</param>
        /// <returns>A VmInstruction associated with the operator, or VmInstruction.Noop, if the operator is not valid and throwOnError is not found</returns>
        /// <exception cref="ArgumentException">The operator string provided was not recognized</exception>
        public static VmInstruction InstructionForOperator(string operatorString, bool throwOnError = true)
        {
            VmInstruction inst;

            switch (operatorString)
            {
                case "*":
                    inst = VmInstruction.Multiply;
                    break;
                case "/":
                    inst = VmInstruction.Divide;
                    break;
                case "%":
                    inst = VmInstruction.Modulo;
                    break;

                case "+":
                    inst = VmInstruction.Add;
                    break;
                case "-":
                    inst = VmInstruction.Subtract;
                    break;

                case "&":
                    inst = VmInstruction.BitwiseAnd;
                    break;
                case "^":
                    inst = VmInstruction.BitwiseXOr;
                    break;
                case "|":
                    inst = VmInstruction.BitwiseOr;
                    break;

                case "==":
                    inst = VmInstruction.Equals;
                    break;
                case "!=":
                    inst = VmInstruction.Unequals;
                    break;

                case ">=":
                    inst = VmInstruction.GreaterOrEquals;
                    break;
                case "<=":
                    inst = VmInstruction.LessOrEquals;
                    break;

                case ">":
                    inst = VmInstruction.Greater;
                    break;
                case "<":
                    inst = VmInstruction.Less;
                    break;

                case "&&":
                    inst = VmInstruction.LogicalAnd;
                    break;
                case "||":
                    inst = VmInstruction.LogicalOr;
                    break;

                default:
                    if (throwOnError)
                        throw new ArgumentException("Unkown operator '" + operatorString + "' does not map to any known VM instruction.");
                    inst = VmInstruction.Noop;
                    break;
            }
            return inst;
        }

        /// <summary>
        /// Returns the VmInstruction associated with a given unary operator string.
        /// Throws an exception if not valid and if the throwOnError parameter is set to true, otherwise, returns VmInstruction.Noop
        /// </summary>
        /// <param name="operatorString">The string containing the unary operator</param>
        /// <param name="throwOnError">Whether to raise an exception, if the operator is not valid</param>
        /// <returns>A VmInstruction associated with the unary operator, or VmInstruction.Noop, if the operator is not valid and throwOnError is not found</returns>
        /// <exception cref="ArgumentException">The operator string provided was not recognized</exception>
        public static VmInstruction InstructionForUnaryOperator(string operatorString, bool throwOnError = true)
        {
            VmInstruction inst;

            switch (operatorString)
            {
                case "-":
                    inst = VmInstruction.ArithmeticNegate;
                    break;
                case "!":
                    inst = VmInstruction.LogicalNegate;
                    break;

                default:
                    if (throwOnError)
                        throw new ArgumentException("Unkown operator '" + operatorString + "' does not map to any known VM instruction.");
                    inst = VmInstruction.Noop;
                    break;
            }
            return inst;
        }

        /// <summary>
        /// Creates a new instruction token based on a given VM instruction
        /// </summary>
        /// <param name="instruction">A VM instruction to create the token with</param>
        /// <returns>A Token with the VM instruction provided binded in</returns>
        public static Token CreateInstructionToken(VmInstruction instruction)
        {
            return new Token(TokenType.Instruction, null, instruction);
        }

        /// <summary>
        /// Creates a new´operator token based on a given VM instruction
        /// </summary>
        /// <param name="operatorInstruction">A VM instruction to create the token with</param>
        /// <returns>A Token with the VM instruction provided binded in</returns>
        public static Token CreateOperatorToken(VmInstruction operatorInstruction)
        {
            return new Token(TokenType.Operator, null, operatorInstruction);
        }

        /// <summary>
        /// Creates a token with a constant FALSE value
        /// </summary>
        /// <returns>A Token with a type of TokenType.Value and a boolean false value associated</returns>
        public static Token CreateFalseToken()
        {
            return new Token(TokenType.Value, false, VmInstruction.Noop);
        }

        /// <summary>
        /// Creates a token with a constant true value
        /// </summary>
        /// <returns>A Token with a type of TokenType.Value and a boolean true value associated</returns>
        public static Token CreateTrueToken()
        {
            return new Token(TokenType.Value, true, VmInstruction.Noop);
        }

        /// <summary>
        /// Creates a token with a constant null value
        /// </summary>
        /// <returns>A Token with a type of TokenType.Value and a null value associated</returns>
        public static Token CreateNullToken()
        {
            return new Token(TokenType.Value, null, VmInstruction.Noop);
        }
    }
}