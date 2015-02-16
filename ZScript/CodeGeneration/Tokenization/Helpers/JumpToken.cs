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

using System;
using System.Text;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Represents a jump token
    /// </summary>
    public class JumpToken : Token, IEquatable<JumpToken>
    {
        /// <summary>
        /// The target token to jump to
        /// </summary>
        public Token TargetToken;

        /// <summary>
        /// Whether this jump is conditional (e.g. it requires a boolean bellow on the stack)
        /// </summary>
        public readonly bool Conditional;

        /// <summary>
        /// Whether the jump (in case it is conditional) should be made if the value at the top is true.
        /// Setting to false realizes a 'Jump if false' type jump
        /// </summary>
        public readonly bool ConditionToJump;

        /// <summary>
        /// Whether the jump instruction should consume the stack by popping or only peek it, leaving the boolean value on
        /// </summary>
        public readonly bool ConsumesStack;

        /// <summary>
        /// Initializes a new instance of the JumpToken class
        /// </summary>
        /// <param name="targetToken">The target token to jump to</param>
        /// <param name="conditional">
        /// Whether this jump is a conditional jump or not. Conditional jumps consume the top-most value of the stack as a boolean and jump when that value is equal to true
        /// </param>
        /// <param name="conditionToJump">
        /// Whether the jump (in case it is conditional) should be made if the value at the top is true.
        /// Setting to false realizes a 'Jump if false' type jump
        /// </param>
        /// <param name="consumesStack">Whether the jump instruction should consume the stack by popping or only peek it, leaving the boolean value on</param>
        public JumpToken(Token targetToken, bool conditional = false, bool conditionToJump = true, bool consumesStack = true)
            : base(TokenType.Value, null)
        {
            TargetToken = targetToken;
            Conditional = conditional;
            ConditionToJump = conditionToJump;
            ConsumesStack = consumesStack;
        }

        /// <summary>
        /// Returns a string representation of this token object
        /// </summary>
        /// <returns>A string representation of this token object</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{ JumpToken type:" + Type);
            builder.Append(", value:'" + TokenObject + "'");
            builder.Append(", instruction:" + Instruction);
            builder.Append(", target:" + TargetToken);
            builder.Append(", conditional:" + Conditional + "");
            builder.Append(", conditionToJump:" + ConditionToJump + "");
            builder.Append(", consumesStack:" + ConsumesStack + "");
            builder.Append(" }");

            return builder.ToString();
        }

        #region Equality members

        public bool Equals(JumpToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return base.Equals(other) && Conditional.Equals(other.Conditional) && ConditionToJump.Equals(other.ConditionToJump) && ConsumesStack.Equals(other.ConsumesStack);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((JumpToken)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Conditional.GetHashCode();
                hashCode = (hashCode * 397) ^ ConditionToJump.GetHashCode();
                hashCode = (hashCode * 397) ^ ConsumesStack.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator==(JumpToken left, JumpToken right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(JumpToken left, JumpToken right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}