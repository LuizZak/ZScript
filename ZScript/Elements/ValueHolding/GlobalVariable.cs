namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a variable that is globally scoped
    /// </summary>
    public class GlobalVariable : Variable
    {
        /// <summary>
        /// Gets or sets the token list containing the expression for the global variable
        /// </summary>
        public TokenList ExpressionTokens { get; set; }
    }
}