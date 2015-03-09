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
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Represents a type alias, which is a collection of fields and functions that expose native classes and structs
    /// </summary>
    public class TypeAliasDefinition : TypeContainerDefinition
    {
        /// <summary>
        /// List of fields accessible on this TypeAliasDefinition
        /// </summary>
        private readonly List<TypeFieldDefinition> _fields = new List<TypeFieldDefinition>();

        /// <summary>
        /// List of method definitions accessible on this TypeAliasDefinition
        /// </summary>
        private readonly List<TypeAliasMethodDefinition> _methods = new List<TypeAliasMethodDefinition>();

        /// <summary>
        /// Gets or sets a value specifying whether this TypeAliasDefinition represents a value type
        /// </summary>
        public bool IsValueType { get; set; }

        /// <summary>
        /// Gets or sets the underlying runtime type this type alias definition refers to
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the base type name for the type alias definition
        /// </summary>
        public string BaseTypeName { get; set; }

        /// <summary>
        /// Gets an array of all fields accessible on this TypeAliasDefinition
        /// </summary>
        public override TypeFieldDefinition[] Fields
        {
            get { return _fields.ToArray(); }
        }

        /// <summary>
        /// Gets an array of all method definitions accessible on this TypeAliasDefinition
        /// </summary>
        public TypeAliasMethodDefinition[] Functions
        {
            get { return _methods.ToArray(); }
        }

        /// <summary>
        /// Adds a method definition on this TypeAliasDefinition
        /// </summary>
        /// <param name="definition">The method definition to add to this alias</param>
        public void AddMethod(TypeAliasMethodDefinition definition)
        {
            _methods.Add(definition);
        }

        /// <summary>
        /// Removes a method definition from this TypeAliasDefinition instance
        /// </summary>
        /// <param name="definition">The method definition to remove from this TypeAliasDefinition</param>
        public void RemoveMethod(TypeAliasMethodDefinition definition)
        {
            _methods.Remove(definition);
        }

        /// <summary>
        /// Adds a field definition on this TypeAliasDefinition
        /// </summary>
        /// <param name="definition">The field definition to add to this alias</param>
        public override void AddField(TypeFieldDefinition definition)
        {
            _fields.Add(definition);
        }

        /// <summary>
        /// Removes a field definition from this TypeAliasDefinition instance
        /// </summary>
        /// <param name="definition">The field definition to remove from this TypeAliasDefinition</param>
        public void RemoveFieldDefinition(TypeFieldDefinition definition)
        {
            _fields.Remove(definition);
        }

        /// <summary>
        /// Gets all the fields defined in this type alias definition
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>A list of fields from this type definition</returns>
        public override List<TypeFieldDefinition> GetAllFields(TypeMemberAttribute attributes = TypeMemberAttribute.CompleteInheritance)
        {
            return _fields;
        }

        /// <summary>
        /// Converts this TypeAliasDefinition into an immutable TypeDef object
        /// </summary>
        /// <returns>A TypeDef generate from this type alias definition</returns>
        public TypeDef ToTypeDef()
        {
            return null;
        }
    }
}