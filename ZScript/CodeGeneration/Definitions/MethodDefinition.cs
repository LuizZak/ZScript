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

using JetBrains.Annotations;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a method definition from a class
    /// </summary>
    public class MethodDefinition : FunctionDefinition
    {
        /// <summary>
        /// Whether this is an override of a method defined in a base class
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Gets or sets the clas this method was defined in
        /// </summary>
        public ClassDefinition Class { get; set; }

        /// <summary>
        /// Gets or sets the method this method definition is overriding
        /// </summary>
        public MethodDefinition BaseMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the MethodDefinition class
        /// </summary>
        /// <param name="name">The name of the method to create</param>
        /// <param name="bodyContext">The context containing the body of the method</param>
        /// <param name="parameters">The parameters for the method</param>
        public MethodDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, [NotNull] FunctionArgumentDefinition[] parameters)
            : base(name, bodyContext, parameters)
        {

        }
    }
}