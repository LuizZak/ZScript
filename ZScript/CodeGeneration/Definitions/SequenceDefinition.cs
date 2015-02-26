#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion
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