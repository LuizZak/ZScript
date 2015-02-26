using System.Collections.Generic;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a definition that can hold variables and methods inside
    /// </summary>
    public abstract class TypeContainerDefinition : Definition
    {
        /// <summary>
        /// Internal list of fields for this class definition
        /// </summary>
        protected List<TypeFieldDefinition> fields;

        /// <summary>
        /// Gets the list of fields colelcted in this class definition
        /// </summary>
        public abstract TypeFieldDefinition[] Fields { get; }

        /// <summary>
        /// Adds a new field definition to this class
        /// </summary>
        /// <param name="field">The field to add to this class</param>
        public abstract void AddField(TypeFieldDefinition field);

        /// <summary>
        /// Returns a list of all the fields inherited and defined by this class definition
        /// </summary>
        /// <param name="inheritedOnly">Whether to only get fields that where inherited</param>
        /// <returns>A list of all the fields inherited and defined by this given class definition</returns>
        public abstract List<TypeFieldDefinition> GetAllFields(bool inheritedOnly = false);
    }
}