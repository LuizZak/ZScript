namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a method definition from a class
    /// </summary>
    public class MethodDefinition : FunctionDefinition
    {
        /// <summary>
        /// Whether this is an override of a method defined in a base class
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Gets or sets the clas this method was defined in
        /// </summary>
        public ClassDefinition Class { get; set; }

        /// <summary>
        /// Initializes a new instance of the MethodDefinition class
        /// </summary>
        /// <param name="name">The name of the method to create</param>
        /// <param name="bodyContext">The context containing the body of the method</param>
        /// <param name="parameters">The parameters for the method</param>
        public MethodDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] parameters)
            : base(name, bodyContext, parameters)
        {

        }
    }
}