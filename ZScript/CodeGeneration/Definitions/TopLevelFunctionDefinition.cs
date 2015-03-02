namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a top-level function definition
    /// </summary>
    public class TopLevelFunctionDefinition : FunctionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the TopLevelFunctionDefinition class
        /// </summary>
        /// <param name="name">The name for the definition</param>
        /// <param name="bodyContext">The context containing the function body's statements</param>
        /// <param name="parameters">The arguments for this function definition</param>
        public TopLevelFunctionDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] parameters) : base(name, bodyContext, parameters)
        {

        }
    }
}