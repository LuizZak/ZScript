namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a function definition
    /// </summary>
    public class FunctionDefinition : Definition
    {
        /// <summary>
        /// The context containing the function body's statements
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// An array of all the function arguments for this function
        /// </summary>
        private readonly FunctionArgumentDefinition[] _arguments;

        /// <summary>
        /// The return type for the function
        /// </summary>
        private readonly TypeDef _returnType;

        /// <summary>
        /// Gets the context containing the function body's statements
        /// </summary>
        public ZScriptParser.FunctionBodyContext BodyContext
        {
            get { return _bodyContext; }
        }

        /// <summary>
        /// Gets an array of all the function arguments for this function
        /// </summary>
        public FunctionArgumentDefinition[] Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        /// Initializes a new instance of the FunctionDefinition class
        /// </summary>
        /// <param name="name">The name for the definition</param>
        /// <param name="bodyContext">The context containing the function body's statements</param>
        /// <param name="arguments"></param>
        public FunctionDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] arguments)
        {
            Name = name;
            _bodyContext = bodyContext;
            _arguments = arguments;
        }
    }
}