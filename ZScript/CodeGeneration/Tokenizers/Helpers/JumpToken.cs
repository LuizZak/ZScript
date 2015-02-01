using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenizers.Helpers
{
    /// <summary>
    /// Represents a jump token
    /// </summary>
    public class JumpToken : Token
    {
        /// <summary>
        /// The target token to jump to
        /// </summary>
        public Token TargetToken;

        /// <summary>
        /// Whether this jump is conditional (e.g. it requires a boolean bellow on the stack)
        /// </summary>
        public bool Conditional;

        /// <summary>
        /// Whether the jump (in case it is conditional) should be made if the value at the top is true.
        /// Setting to false realizes a 'Jump if false' type jump
        /// </summary>
        public bool ConditionToJump;

        /// <summary>
        /// Whether the jump instruction should consume the stack by popping or only peek it, leaving the boolean value on
        /// </summary>
        public bool ConsumesStack = true;

        /// <summary>
        /// Initializes a new instance of the JumpToken class
        /// </summary>
        /// <param name="targetToken">The target token to jump to</param>
        /// <param name="conditional">Whether this jump is a conditional jump or not</param>
        public JumpToken(Token targetToken, bool conditional = false)
            : base(TokenType.Value, null)
        {
            TargetToken = targetToken;
            Conditional = conditional;
        }
    }
}