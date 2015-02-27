namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Represents a method defined in a type alias definition
    /// </summary>
    public class TypeAliasMethodDefinition : FunctionDefinition
    {
        /// <summary>
        /// Initialzes a new instance of the TypeMethodDefinition class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="parameters">The parameters for the method</param>
        public TypeAliasMethodDefinition(string name, FunctionArgumentDefinition[] parameters)
            : base(name, null, parameters)
        {

        }
    }
}