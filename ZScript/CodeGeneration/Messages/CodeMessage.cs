using Antlr4.Runtime;

namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a message that is raised during code analysis
    /// </summary>
    public abstract class CodeMessage
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the line the message is relevant at
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the offset in the line the message is relevant at
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the context the message is contained at
        /// </summary>
        public ParserRuleContext Context { get; set; }

        /// <summary>
        /// Gets a friendly version of the context for this code message
        /// </summary>
        public string ContextName
        {
            get
            {
                string s = "";

                // If the context is a block statement, navigate until the top-most function definition
                RuleContext context = Context;
                if (context is ZScriptParser.BlockStatementContext)
                {
                    while (context.Parent != null && !(context is ZScriptParser.FunctionDefinitionContext))
                    {
                        context = context.Parent;
                    }
                }

                var fd = context as ZScriptParser.FunctionDefinitionContext;
                if (fd != null)
                {
                    s = "function " + fd.functionName().IDENT().GetText();
                }

                return s;
            }
        }
    }
}