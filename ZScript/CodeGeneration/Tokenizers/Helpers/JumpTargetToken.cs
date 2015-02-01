using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenizers.Helpers
{
    /// <summary>
    /// Special class that can be used as a target to jump tokens, being removed after being processed
    /// </summary>
    public class JumpTargetToken : Token
    {
        /// <summary>
        /// Initializes a new instance of the JumpTokenTarget class
        /// </summary>
        public JumpTargetToken()
            : base(TokenType.Value, null)
        {

        }
    }
}