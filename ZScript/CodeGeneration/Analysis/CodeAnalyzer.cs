using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Performs analysis in scopes to guarantee that variables are scoped correctly
    /// </summary>
    public class CodeAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// The container for error and warning messages
        /// </summary>
        private MessageContainer _messageContainer;

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private DefinitionsCollector _definitionsCollector;

        /// <summary>
        /// Gets the base scope for the definitions that were collected by the scope analyzer
        /// </summary>
        public DefinitionsCollector DefinitionsCollector
        {
            get { return _definitionsCollector; }
        }

        /// <summary>
        /// Gets or sets the message container for this scope analyzer
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
            set { _messageContainer = value; }
        }

        /// <summary>
        /// Initializes a new instance of the ScopeAnalyzer class
        /// </summary>
        /// <param name="messageContainer">The container that error and warning messages will be reported to</param>
        public CodeAnalyzer(MessageContainer messageContainer)
        {
            _messageContainer = new MessageContainer();
            _messageContainer = messageContainer;
        }

        /// <summary>
        /// Analyzes a given program context for undeclared variable errors
        /// </summary>
        /// <param name="context">The context of the program to analyze</param>
        public void AnalyzeProgram(ZScriptParser.ProgramContext context)
        {
            _definitionsCollector = new DefinitionsCollector(_messageContainer);
            

            // Walk twice - the first pass collects definitions, the second pass analyzes bodies
            DefinitionsCollector collector = new DefinitionsCollector(_messageContainer);
            collector.Collect(context);

            _definitionsCollector = collector;
        }
    }
}