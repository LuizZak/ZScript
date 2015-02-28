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
    /// Specifies a common definition
    /// </summary>
    public class Definition
    {
        /// <summary>
        /// Gets or sets the name for this definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the context for the definition
        /// </summary>
        public ParserRuleContext Context { get; set; }

        /// <summary>
        /// Gets or sets the parse rule context that specifies the identifier name for the definition
        /// </summary>
        public ParserRuleContext IdentifierContext { get; set; }
    }
}