namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a closure definition
    /// </summary>
    public class ClosureDefinition : FunctionDefinition
    {
        /// <summary>
        /// The prefix used in closure names, used during operations that replace closure expressions with closure accesses
        /// </summary>
        public static readonly string ClosureNamePrefix = "$__closure";

        /// <summary>
        /// Initializes a new instance of the ClosureDefinition class
        /// </summary>
        /// <param name="name">The name for this closure</param>
        /// <param name="bodyContext">The body context containing the closure's statements</param>
        /// <param name="arguments">The list of arguments for the closure</param>
        public ClosureDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] arguments)
            : base(name, bodyContext, arguments)
        {

        }
    }
}