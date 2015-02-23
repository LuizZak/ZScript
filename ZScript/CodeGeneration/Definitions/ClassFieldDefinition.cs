namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a class field definition
    /// </summary>
    public class ClassFieldDefinition : ValueHolderDefinition
    {
        /// <summary>
        /// Initializes a new instance of the ClassFieldDefinition class
        /// </summary>
        /// <param name="fieldName">The name of the field for this class field</param>
        public ClassFieldDefinition(string fieldName)
        {
            Name = fieldName;
        }
    }
}