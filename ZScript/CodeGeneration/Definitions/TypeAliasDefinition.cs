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
    public class TypeAliasDefinition : Definition
    {
        /// <summary>
        /// List of fields accessible on this TypeAliasDefinition
        /// </summary>
        private readonly List<ValueHolderDefinition> _fields = new List<ValueHolderDefinition>();

        /// <summary>
        /// List of function definitions accessible on this TypeAliasDefinition
        /// </summary>
        private readonly List<FunctionDefinition> _functions = new List<FunctionDefinition>();

        /// <summary>
        /// Gets or sets a value specifying whether this TypeAliasDefinition represents a value type
        /// </summary>
        public bool IsValueType { get; set; }

        /// <summary>
        /// Gets an array of all fields accessible on this TypeAliasDefinition
        /// </summary>
        public ValueHolderDefinition[] Fields
        {
            get { return _fields.ToArray(); }
        }

        /// <summary>
        /// Gets an array of all function definitions accessible on this TypeAliasDefinition
        /// </summary>
        public FunctionDefinition[] Functions
        {
            get { return _functions.ToArray(); }
        }

        /// <summary>
        /// Adds a function definition on this TypeAliasDefinition
        /// </summary>
        /// <param name="definition">The function definition to add to this alias</param>
        public void AddFunctionDefinition(FunctionDefinition definition)
        {
            _functions.Add(definition);
        }

        /// <summary>
        /// Removes a function definition from this TypeAliasDefinition instance
        /// </summary>
        /// <param name="definition">The function definition to remove from this TypeAliasDefinition</param>
        public void RemoveFunctionDefinition(FunctionDefinition definition)
        {
            _functions.Remove(definition);
        }

        /// <summary>
        /// Adds a field definition on this TypeAliasDefinition
        /// </summary>
        /// <param name="definition">The field definition to add to this alias</param>
        public void AddFieldDefinition(ValueHolderDefinition definition)
        {
            _fields.Add(definition);
        }

        /// <summary>
        /// Removes a field definition from this TypeAliasDefinition instance
        /// </summary>
        /// <param name="definition">The field definition to remove from this TypeAliasDefinition</param>
        public void RemoveFieldDefinition(ValueHolderDefinition definition)
        {
            _fields.Remove(definition);
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