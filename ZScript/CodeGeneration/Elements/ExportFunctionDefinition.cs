namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a function definition
    /// </summary>
    public class ExportFunctionDefinition : FunctionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the ExportFunctionDefinition class
        /// </summary>
        /// <param name="name">The name for the definition</param>
        /// <param name="arguments">The list of arguments for the definition</param>
        public ExportFunctionDefinition(string name, FunctionArgumentDefinition[] arguments)
            : base(name, null, arguments)
        {

        }
    }
}