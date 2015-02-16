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

using ZScript.CodeGeneration.Elements;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Static class responsible for generation of primitive and basic types that are exposed to the runtime and compiler
    /// </summary>
    public class TypeFactory
    {
        /// <summary>
        /// The original object type definition all types inherit from
        /// </summary>
        private static readonly TypeDef _objectTypeDef;

        /// <summary>
        /// A string type definition
        /// </summary>
        private static readonly TypeDef _stringTypeDef;

        /// <summary>
        /// A boolean type definition
        /// </summary>
        private static readonly TypeDef _boolTypeDef;

        /// <summary>
        /// Static constructor for the TypeFactory class
        /// </summary>
        static TypeFactory()
        {
            TypeDef.GenerateBaseTypes(out _objectTypeDef, out _stringTypeDef, out _boolTypeDef);
        }

        /// <summary>
        /// Creates a type definition that represents an integer (Int64) definition
        /// </summary>
        /// <returns>The type definition for an integer</returns>
        public static TypeDef CreateIntegerType()
        {
            var alias = new TypeAliasDefinition { Name = "int", IsValueType = true };

            PopulateWithDefaultMembers(alias);

            return alias.ToTypeDef();
        }

        /// <summary>
        /// Populates a given type alias with all default members that should be present in all types
        /// The alias includes a ToString(), GetType(), and Equals() implementation
        /// </summary>
        /// <param name="alias">The alias to populate</param>
        private static void PopulateWithDefaultMembers(TypeAliasDefinition alias)
        {
            var toString = new FunctionDefinition("ToString", null, new FunctionArgumentDefinition[0]);
            var getType  = new FunctionDefinition("GetType", null, new FunctionArgumentDefinition[0]);
            var equals = new FunctionDefinition("Equals", null, new[] { new FunctionArgumentDefinition { Name = "object", Type = _objectTypeDef } }) { ReturnType = _boolTypeDef };

            alias.AddFunctionDefinition(toString);
            alias.AddFunctionDefinition(getType);
            alias.AddFunctionDefinition(equals);
        }
    }
}