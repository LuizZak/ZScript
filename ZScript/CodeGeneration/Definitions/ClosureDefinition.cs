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

using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a closure definition
    /// </summary>
    public class ClosureDefinition : FunctionDefinition
    {
        /// <summary>
        /// The prefix used in closure names, used during operations that replace closure expressions with closure accesses
        /// </summary>
        public static readonly string ClosureNamePrefix = "$__closure";

        /// <summary>
        /// Initializes a new instance of the ClosureDefinition class
        /// </summary>
        /// <param name="name">The name for this closure</param>
        /// <param name="bodyContext">The body context containing the closure's statements</param>
        /// <param name="parameters">The list of arguments for the closure</param>
        public ClosureDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] parameters)
            : base(name, bodyContext, parameters)
        {

        }
    }
}