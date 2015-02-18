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
using ZScript.Runtime.Execution;

namespace ZScript.Elements
{
    /// <summary>
    /// Defines a single token for a virtual machine
    /// </summary>
    public class Token : IEquatable<Token>
    {
        /// <summary>
        /// The type of this token
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The token itself. When the Type corresponds to a VM instruction, this value is not used
        /// </summary>
        public readonly Object TokenObject;

        /// <summary>
        /// An instruction that can be executed by a VM, in case this token has a token type of
        /// TokenType.Instruction or TokenType.Operator, otherwise this value should be ignored
        /// </summary>
        public readonly VmInstruction Instruction;

        /// <summary>
        /// Gets or sets a value specifying the reachability of this token.
        /// This is a meta-data type property used by IntermediaryTokenLists to mark the reachability of instruction tokens
        /// </summary>
        public bool Reachable { get; set; }

        /// <summary>
        /// Initializes a new instance of the Token class
        /// </summary>
        /// <param name="type">A type of token to create</param>
        /// <param name="tokenObject">The underlying token object to create</param>
        public Token(TokenType type, Object tokenObject)
            : this(type, tokenObject, VmInstruction.Noop)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Token class
        /// </summary>
        /// <param name="type">A type of token to create</param>
        /// <param name="tokenObject">The underlying token object to create</param>
        /// <param name="instruction">An instruction this token represents</param>
        public Token(TokenType type, Object tokenObject, VmInstruction instruction)
        {
            Type = type;
            TokenObject = tokenObject;
            Instruction = instruction;
            Reachable = true; // By deafult, tokens always start marked as reachable
        }

        /// <summary>
        /// Returns a string representation of this token object
        /// </summary>
        /// <returns>A string representation of this token object</returns>
        public override string ToString()
        {
            return "{ Token type:" + Type + ", value:'" + TokenObject + "', instruction:" + Instruction + " }";
        }

        #region Equality members

        public bool Equals(Token other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return Type == other.Type && Equals(TokenObject, other.TokenObject) && Instruction == other.Instruction;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Token)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (TokenObject != null ? TokenObject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Instruction;
                return hashCode;
            }
        }

        public static bool operator==(Token left, Token right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(Token left, Token right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    /// <summary>
    /// Specifies a type of token
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Specifies a token that contains a name of a variable/member name
        /// </summary>
        MemberName,
        /// <summary>
        /// Specifies a static (numeric or boolean) token
        /// </summary>
        Value,
        /// <summary>
        /// Specifies a static (string) token
        /// </summary>
        String,
        /// <summary>
        /// Specifies an operator token
        /// </summary>
        Operator,
        /// <summary>
        /// Specifies a statement token
        /// </summary>
        Statement,
        /// <summary>
        /// Specifies a token that contains an instruction that can be executed by a VM
        /// </summary>
        Instruction,
        /// <summary>
        /// Specifies a token that is pointing to a global function, by address
        /// </summary>
        GlobalFunction
    }
}