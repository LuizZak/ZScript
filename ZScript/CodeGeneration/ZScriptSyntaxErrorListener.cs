using Antlr4.Runtime;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Error listener for parsing of script snippets
    /// </summary>
    public class ZScriptSyntaxErrorListener : BaseErrorListener
    {
        /// <summary>
        /// The container errors will be reported to
        /// </summary>
        private MessageContainer _messageContainer;

        /// <summary>
        /// Gets or sets the container errors will be reported to
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
            set { _messageContainer = value; }
        }

        /// <summary>
        /// Initializes a new instance of the ZScriptErrorListener class
        /// </summary>
        /// <param name="messageContainer">The container errors will be reported to</param>
        public ZScriptSyntaxErrorListener(MessageContainer messageContainer)
        {
            _messageContainer = messageContainer;
        }

        // 
        // SintaxError listener
        // 
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _messageContainer.RegisterSyntaxError(new SyntaxError(line, charPositionInLine, msg));
        }
    }
}