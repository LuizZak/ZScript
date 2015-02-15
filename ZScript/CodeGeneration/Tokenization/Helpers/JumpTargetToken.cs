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