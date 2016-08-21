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

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Defines a runtime generation context object that bundles a few objects with different responsibilities to be used by different code generation objects
    /// </summary>
    public class RuntimeGenerationContext
    {
        /// <summary>
        /// Gets the message container to report errors and warnings to
        /// </summary>
        public MessageContainer MessageContainer { get; }

        /// <summary>
        /// Gets the internal type provider used to generate typing for the definitions
        /// </summary>
        public TypeProvider TypeProvider { get; }

        /// <summary>
        /// Gets or sets the definition type provider for the code generation
        /// </summary>
        public IDefinitionTypeProvider DefinitionTypeProvider { get; set; }

        /// <summary>
        /// Gets or sets the contxt type provider for the conte generation
        /// </summary>
        public IContextTypeProvider ContextTypeProvider { get; set; }

        /// <summary>
        /// Gets the base scope for the runtime generation, containg all of the combined definitions collected from the script sources
        /// </summary>
        public CodeScope BaseScope { get; }

        /// <summary>
        /// Initializes a new instance of the RuntimeGenerationContext class
        /// </summary>
        /// <param name="baseScope">The base scope for the runtime generation, containg all of the combined definitions collected from the script sources</param>
        /// <param name="messageContainer">The message container to report errors and warnings to</param>
        /// <param name="typeProvider">The internal type provider used to generate typing for the definitions</param>
        /// <param name="definitionTypeProvider">The definition type provider for the code generation</param>
        public RuntimeGenerationContext(CodeScope baseScope = null, MessageContainer messageContainer = null,
            TypeProvider typeProvider = null, IDefinitionTypeProvider definitionTypeProvider = null)
        {
            BaseScope = baseScope;
            MessageContainer = messageContainer;
            TypeProvider = typeProvider;
            DefinitionTypeProvider = definitionTypeProvider;
        }
    }
}