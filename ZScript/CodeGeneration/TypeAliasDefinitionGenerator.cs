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

using ZScript.CodeGeneration.Definitions;
using ZScript.Parsing;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Provides functionality for generating type aliases from AST nodes
    /// </summary>
    public static class TypeAliasDefinitionGenerator
    {
        /// <summary>
        /// Generates a type alias from a given type alias context
        /// </summary>
        /// <param name="context">The context containing the type alias to generate</param>
        /// <returns>A type alias definition generated from the given type alias context</returns>
        public static TypeAliasDefinition GenerateTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            var runtimeTypeName = ConstantAtomParser.ParseStringAtom(context.stringLiteral());
            var definition = new TypeAliasDefinition
            {
                Context = context,
                Name = context.typeAliasName().GetText(),
                TypeName = runtimeTypeName,
                BaseTypeName = context.typeAliasInherit() == null ? "" : context.typeAliasInherit().typeAliasName().GetText(),
                IdentifierContext = context.typeAliasName().complexTypeName()
            };

            return definition;
        }

        /// <summary>
        /// Generates a type alias variable from a given type alias context
        /// </summary>
        /// <param name="context">The context containing the type alias variable to get</param>
        /// <returns>A new TypeFieldDefinition that corresponds to the given type alias variable context</returns>
        public static TypeFieldDefinition GenerateTypeField(ZScriptParser.TypeAliasVariableContext context)
        {
            var declare = context.valueDeclareStatement();
            var name = declare.valueHolderDecl().valueHolderName().memberName().GetText();

            var definition = new TypeFieldDefinition(name);

            DefinitionGenerator.FillValueHolderDef(definition, declare.valueHolderDecl());

            return definition;
        }

        /// <summary>
        /// Generates a type alias method from a given type alias context
        /// </summary>
        /// <param name="context">The context containing the type alias method to get</param>
        /// <returns>A new TypeAliasMethodDefinition that corresponds to the given type alias method context</returns>
        public static TypeAliasMethodDefinition GenerateTypeMethod(ZScriptParser.TypeAliasFunctionContext context)
        {
            var args = DefinitionGenerator.CollectFunctionArguments(context.functionArguments());

            return new TypeAliasMethodDefinition(context.functionName().IDENT().GetText(), args)
            {
                HasReturnType = true,
                ReturnTypeContext = context.returnType(),
                IdentifierContext = context.functionName()
            };
        }
    }
}