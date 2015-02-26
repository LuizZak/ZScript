namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a type field definition
    /// </summary>
    public class TypeFieldDefinition : ValueHolderDefinition
    {
        /// <summary>
        /// Initializes a new instance of the ClassFieldDefinition class
        /// </summary>
        /// <param name="fieldName">The name of the field for this class field</param>
        public TypeFieldDefinition(string fieldName)
        {
            Name = fieldName;
        }
    }
}