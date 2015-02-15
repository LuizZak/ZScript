namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies an expression composed from an expression context read from a script parser
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// The expression context for this expression
        /// </summary>
        private readonly ZScriptParser.ExpressionContext _expressionContext;

        /// <summary>
        /// Gets the expression context for this expression
        /// </summary>
        public ZScriptParser.ExpressionContext ExpressionContext
        {
            get { return _expressionContext; }
        }

        /// <summary>
        /// Initializes a new instance of the Expression class
        /// </summary>
        /// <param name="expressionContext">An expression context read from a script parser</param>
        public Expression(ZScriptParser.ExpressionContext expressionContext)
        {
            _expressionContext = expressionContext;
        }
    }
}