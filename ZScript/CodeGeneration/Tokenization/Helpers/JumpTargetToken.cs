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
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Special class that can be used as a target to jump tokens.
    /// This special token is analyzed by the JumpTokenExpander and is removed, moving the jump
    /// target of all jump tokens pointing to it to the immediate next instruction in the token list
    /// </summary>
    public class JumpTargetToken : Token, IEquatable<JumpTargetToken>
    {
        /// <summary>
        /// Initializes a new instance of the JumpTokenTarget class
        /// </summary>
        public JumpTargetToken()
            : base(TokenType.Value, null)
        {

        }

        /// <summary>
        /// Returns a string representation of this JumpTargetToken
        /// </summary>
        /// <returns>A string representation of this JumpTargetToken</returns>
        public override string ToString()
        {
            return "{ JumpTargetToken }";
        }

        #region Equality members

        public bool Equals(JumpTargetToken other)
        {
            return ReferenceEquals(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((JumpTargetToken)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator==(JumpTargetToken left, JumpTargetToken right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(JumpTargetToken left, JumpTargetToken right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}