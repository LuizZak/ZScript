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

using System;
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
        /// <param name="attributes">The attributes to use when searching the members to fetch</param>
        /// <returns>A list of all the fields inherited and defined by this given class definition</returns>
        public abstract List<TypeFieldDefinition> GetAllFields(TypeMemberAttribute attributes = TypeMemberAttribute.CompleteInheritance);
    }

    /// <summary>
    /// Specifies attributes to use when searching members on a type container
    /// </summary>
    [Flags]
    public enum TypeMemberAttribute
    {
        /// <summary>Gets members defined in the current type</summary>
        Defined = 0x1 << 0,
        /// <summary>Gets members inherited in the current type</summary>
        Inherited = 0x1 << 1,
        /// <summary>Gets all members from the complete inheritance chain</summary>
        CompleteInheritance = 0x1 | (0x1 << 1)
    }
}