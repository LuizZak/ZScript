using System;
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