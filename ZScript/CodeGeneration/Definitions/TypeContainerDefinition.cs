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