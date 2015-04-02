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
using System.Collections.Generic;
using System.Reflection;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Elements
{
    /// <summary>
    /// Class used to create Token instances
    /// </summary>
    public static class TokenFactory
    {
        /// <summary>
        /// Token that represents a GET_SUBSCRIPT call
        /// </summary>
        public readonly static Token GetSubscriptToken = new Token(TokenType.Instruction, null, VmInstruction.GetSubscript);

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
        /// Creates a new global function token that represents an indexed global function
        /// </summary>
        /// <param name="functionIndex">The index of the function to associate with the token</param>
        /// <returns>A Token with the global function index provided binded in</returns>
        public static Token CreateGlobalFunctionToken(int functionIndex)
        {
            return new Token(TokenType.GlobalFunction, functionIndex, VmInstruction.Noop);
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
        /// Creates a new token with a given type context as the content.
        /// </summary>
        /// <param name="tokenType">The type to associate with the token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="context">A type context to associate with the token</param>
        /// <returns>A Token with the type context provided binded in</returns>
        public static TypedToken CreateTypeToken(TokenType tokenType, VmInstruction instruction, ZScriptParser.TypeContext context)
        {
            return new TypedToken(tokenType, instruction, context);
        }

        /// <summary>
        /// Creates a new token with a given type definition as the content.
        /// </summary>
        /// <param name="tokenType">The type to associate with the token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="type">A type definition to associate with the token</param>
        /// <returns>A Token with the type definition provided binded in</returns>
        public static TypedToken CreateTypeToken(TokenType tokenType, VmInstruction instruction, TypeDef type)
        {
            return new TypedToken(tokenType, instruction, type);
        }

        /// <summary>
        /// Creates a new token with a given type definition as the content.
        /// </summary>
        /// <param name="tokenType">The type to associate with the token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="type">A type definition to associate with the token</param>
        /// <returns>A Token with the type definition provided binded in</returns>
        public static TypedToken CreateTypeToken(TokenType tokenType, VmInstruction instruction, Type type)
        {
            return new TypedToken(tokenType, instruction, type);
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

                case "<<":
                    inst = VmInstruction.ShiftLeft;
                    break;
                case ">>":
                    inst = VmInstruction.ShiftRight;
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

                case "is":
                    inst = VmInstruction.Is;
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
        /// <param name="argument">An object to use as an instruction argument</param>
        /// <returns>A Token with the VM instruction provided binded in</returns>
        public static Token CreateInstructionToken(VmInstruction instruction, object argument = null)
        {
            return new Token(TokenType.Instruction, argument, instruction);
        }

        /// <summary>
        /// Creates a new´operator token based on a given VM instruction
        /// </summary>
        /// <param name="operatorInstruction">A VM instruction to create the token with</param>
        /// <param name="argument">An object to use as an operator argument</param>
        /// <returns>A Token with the VM instruction provided binded in</returns>
        public static Token CreateOperatorToken(VmInstruction operatorInstruction, object argument = null)
        {
            return new Token(TokenType.Operator, argument, operatorInstruction);
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

        #region Complex operation token generation

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a method call on a specified variable with the specified argument list
        /// </summary>
        /// <param name="variableName">The name of the variable to call the function on</param>
        /// <param name="methodName">The function to call on the variable</param>
        /// <param name="argumentList">A list of IEnumerable objects containing the arguments for the call</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMethodCall(string variableName, string methodName, params IEnumerable<Token>[] argumentList)
        {
            var tokens = new List<Token>();

            tokens.AddRange(CreateMemberAccess(variableName, methodName, MemberAccessType.MethodAccess, true));
            tokens.AddRange(CreateFunctionCall(argumentList));
            
            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a method call on a specified variable with the specified argument list
        /// </summary>
        /// <param name="variableName">The name of the variable to call the function on</param>
        /// <param name="method">The reflected method to call on the variable</param>
        /// <param name="argumentList">A list of IEnumerable objects containing the arguments for the call</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMethodCall(string variableName, MethodInfo method, params IEnumerable<Token>[] argumentList)
        {
            var tokens = new List<Token> { CreateVariableToken(variableName, true) };

            tokens.AddRange(CreateFunctionCall(method, argumentList));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a function call on a specified variable with the specified argument list
        /// </summary>
        /// <param name="methodInfo">A reflected method information to call on</param>
        /// <param name="argumentList">A list of IEnumerable objects containing the arguments for the call</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateFunctionCall(MethodInfo methodInfo, params IEnumerable<Token>[] argumentList)
        {
            var tokens = new List<Token>();

            // Add the arguments
            foreach (var args in argumentList)
            {
                tokens.AddRange(args);
            }

            tokens.Add(CreateBoxedValueToken(argumentList.Length));
            tokens.Add(CreateInstructionToken(VmInstruction.Call, methodInfo));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a function call on a specified variable with the specified argument list
        /// </summary>
        /// <param name="argumentList">A list of IEnumerable objects containing the arguments for the call</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateFunctionCall(params IEnumerable<Token>[] argumentList)
        {
            var tokens = new List<Token>();

            // Add the arguments
            foreach (var args in argumentList)
            {
                tokens.AddRange(args);
            }

            tokens.Add(CreateBoxedValueToken(argumentList.Length));
            tokens.Add(CreateInstructionToken(VmInstruction.Call));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a member get on a specified variable with the specified argument list
        /// </summary>
        /// <param name="variableName">The name of the variable to call the function on</param>
        /// <param name="memberName">The member to get from the variable</param>
        /// <param name="accessType">The type of access to perform</param>
        /// <param name="isGetAccess">Whether to add a Get instruction after the member fetch</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMemberAccess(string variableName, string memberName, MemberAccessType accessType, bool isGetAccess)
        {
            var tokens = new List<Token>
            {
                CreateVariableToken(variableName, true),
                CreateMemberNameToken(memberName)
            };

            switch (accessType)
            {
                case MemberAccessType.FieldAccess:
                    tokens.Add(CreateInstructionToken(VmInstruction.GetMember));
                    break;
                case MemberAccessType.MethodAccess:
                    tokens.Add(CreateInstructionToken(VmInstruction.GetCallable));
                    break;
            }

            if (accessType != MemberAccessType.MethodAccess && isGetAccess)
                tokens.Add(CreateInstructionToken(VmInstruction.Get));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a member get on a specified variable with the specified argument list
        /// </summary>
        /// <param name="variableName">The name of the variable to call the function on</param>
        /// <param name="property">The member to get from the variable</param>
        /// <param name="isGetAccess">Whether to add a Get instruction after the member fetch</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMemberAccess(string variableName, PropertyInfo property, bool isGetAccess)
        {
            var tokens = new List<Token>
            {
                CreateVariableToken(variableName, true),
                CreateInstructionToken(VmInstruction.GetMember, property)
            };

            if (isGetAccess)
                tokens.Add(CreateInstructionToken(VmInstruction.Get));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a member get on a specified variable with the specified argument list
        /// </summary>
        /// <param name="variableName">The name of the variable to call the function on</param>
        /// <param name="field">The member to get from the variable</param>
        /// <param name="isGetAccess">Whether to add a Get instruction after the member fetch</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMemberAccess(string variableName, FieldInfo field, bool isGetAccess)
        {
            var tokens = new List<Token>
            {
                CreateVariableToken(variableName, true),
                CreateInstructionToken(VmInstruction.GetMember, field)
            };

            if (isGetAccess)
                tokens.Add(CreateInstructionToken(VmInstruction.Get));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a member get on a value on top of the stack with the specified argument list
        /// </summary>
        /// <param name="memberName">The member to get from the variable</param>
        /// <param name="accessType">The type of access to perform</param>
        /// <param name="isGetAccess">Whether to add a Get instruction after the member fetch</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateMemberAccess(string memberName, MemberAccessType accessType, bool isGetAccess)
        {
            var tokens = new List<Token>
            {
                CreateMemberNameToken(memberName)
            };

            switch (accessType)
            {
                case MemberAccessType.FieldAccess:
                    tokens.Add(CreateInstructionToken(VmInstruction.GetMember));
                    break;
                case MemberAccessType.MethodAccess:
                    tokens.Add(CreateInstructionToken(VmInstruction.GetCallable));
                    break;
            }

            if (accessType != MemberAccessType.MethodAccess && isGetAccess)
                tokens.Add(CreateInstructionToken(VmInstruction.Get));

            return tokens;
        }

        /// <summary>
        /// Creates an enumerable of tokens containing a syntax for a variable assignment operation
        /// </summary>
        /// <param name="variableName">The name of the variable to assign</param>
        /// <param name="value">An enumerable of tokens containing the instructions that will generate the value to set</param>
        /// <returns>An IEnumerable containing the instruction tokens for the operation</returns>
        public static IEnumerable<Token> CreateVariableAssignment(string variableName, IEnumerable<Token> value)
        {
            var tokens = new List<Token>(value)
            {
                CreateVariableToken(variableName, true),
                CreateInstructionToken(VmInstruction.Set)
            };

            return tokens;
        }

        #endregion
    }

    /// <summary>
    /// Specifies the type of a member access on TokenFactory.CreateMemberAccess calls
    /// </summary>
    public enum MemberAccessType
    {
        /// <summary>
        /// Specifies a method access
        /// </summary>
        MethodAccess,
        /// <summary>
        /// Specifies a field or property access
        /// </summary>
        FieldAccess,
    }
}