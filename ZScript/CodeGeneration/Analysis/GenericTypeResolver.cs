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
using ZScript.CodeGeneration.Messages;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class capable of resolving the generic types of an expression
    /// </summary>
    public class GenericTypeResolver
    {
        /// <summary>
        /// Function calls enqueued on this generic type resolver
        /// </summary>
        private readonly Stack<ZScriptParser.FunctionCallContext> _functionCalls;

        /// <summary>
        /// Gets the context associated with this generic type resolver
        /// </summary>
        public RuntimeGenerationContext Context { private set; get; }

        /// <summary>
        /// Fetches a copy of the stored function calls on this generic type resolver
        /// </summary>
        public ZScriptParser.FunctionCallContext[] EnqueuedFunctionCalls => _functionCalls.ToArray();

        /// <summary>
        /// Initializes a new instance of the GenericTypeResolver with an associated runtime generation context
        /// </summary>
        /// <param name="context">The generation context for this generic type resolver</param>
        public GenericTypeResolver(RuntimeGenerationContext context)
        {
            _functionCalls = new Stack<ZScriptParser.FunctionCallContext>();
            Context = context;
        }

        /// <summary>
        /// Pushes a generic function call context in this generic type resolver to be resolved later
        /// </summary>
        /// <param name="functionCall">Function call to store on this generic type resolver</param>
        public void PushGenericCallContext(ZScriptParser.FunctionCallContext functionCall)
        {
            _functionCalls.Push(functionCall);
        }

        /// <summary>
        /// Resolves all generic calls stored on this generic type resolver
        /// </summary>
        public void ResolveGenericCalls()
        {
            
        }

        /// <summary>
        /// Resolves the generic types on a given function call context, returning the collected messages
        /// </summary>
        /// <param name="functionCall">The context for the function call to resolve</param>
        /// <returns>Any associated messages generated during the type resolving</returns>
        public CodeMessage[] ResolveFunctionCall(ZScriptParser.FunctionCallContext functionCall)
        {
            return new CodeMessage[0];
        }
    }
}
