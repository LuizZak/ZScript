using System.Collections.Generic;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a sequence definition
    /// </summary>
    public class SequenceDefinition : TypeContainerDefinition
    {
        /// <summary>
        /// Gets or sets the context containing the sequence definition
        /// </summary>
        public ZScriptParser.SequenceBlockContext SequenceContext { get; set; }

        /// <summary>
        /// Gets all the fields defined in this sequence definition
        /// </summary>
        public override TypeFieldDefinition[] Fields
        {
            get { return fields.ToArray(); }
        }

        /// <summary>
        /// Initializes a new instance of the SequenceDefinition class
        /// </summary>
        /// <param name="sequenceName">The name to define the sequence with</param>
        public SequenceDefinition(string sequenceName)
        {
            Name = sequenceName;
            fields = new List<TypeFieldDefinition>();
        }

        /// <summary>
        /// Adds a field to this sequence definition
        /// </summary>
        /// <param name="field">The field to add to this definition</param>
        public override void AddField(TypeFieldDefinition field)
        {
            fields.Add(field);
        }

        /// <summary>
        /// Gets all the fields visible in this sequence definition, optionally fetching only the inherited fields
        /// </summary>
        /// <param name="inheritedOnly">Whether to fetch inherited fields only</param>
        /// <returns>A list of all fields visible in this sequence definition</returns>
        public override List<TypeFieldDefinition> GetAllFields(bool inheritedOnly = false)
        {
            return fields;
        }
    }
}