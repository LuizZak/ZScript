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

using Antlr4.Runtime;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Declares the usage context of a definition
    /// </summary>
    public class DefinitionUsage
    {
        /// <summary>
        /// Gets the definition that was used
        /// </summary>
        public Definition Definition { get; }

        /// <summary>
        /// Gets the parser rule context that represents the definition's usage
        /// </summary>
        public ParserRuleContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the DefinitionUsage class
        /// </summary>
        /// <param name="definition">The definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        public DefinitionUsage(Definition definition, ParserRuleContext context)
        {
            Definition = definition;
            Context = context;
        }
    }
}