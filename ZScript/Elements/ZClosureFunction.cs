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

using System;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies a closure function
    /// </summary>
    public class ZClosureFunction : ZFunction, ICallableWrapper
    {
        /// <summary>
        /// Gets or sets the currently captured memory for this closure function
        /// </summary>
        public MemoryMapper CapturedMemory { get; set; }

        /// <summary>
        /// The name for this closure
        /// </summary>
        public string CallableName => Name;

        /// <summary>
        /// The local memory for this closure
        /// </summary>
        public IMemory<string> LocalMemory => CapturedMemory;

        /// <summary>
        /// Initializes a new instance of the ZClosureFunction class
        /// </summary>
        /// <param name="name">The name for the closure</param>
        /// <param name="tokens">The list of tokens representing the closure's body</param>
        /// <param name="arguments">The list of arguments for the closure</param>
        /// <param name="returnType">The return type for the closure</param>
        public ZClosureFunction(string name, TokenList tokens, FunctionArgument[] arguments, Type returnType)
            : base(name, tokens, arguments, returnType)
        {
            IsClosure = true;
        }

        /// <summary>
        /// Performs a shallow clone of this ZClosureFunction object
        /// </summary>
        /// <returns>A shallow copy of this ZClosureFunction object</returns>
        public ZClosureFunction Clone()
        {
            var clone = (ZClosureFunction)MemberwiseClone();

            clone.CapturedMemory = CapturedMemory?.Clone();

            return clone;
        }

        // 
        // ICallableWrapper.CallableTypeWithArguments implementation
        // 
        public CallableTypeDef CallableTypeWithArguments(CallArguments arguments)
        {
            return Signature;
        }

        // 
        // ICallableWrapper.Call implementation
        // 
        public object Call(VmContext context, CallArguments arguments)
        {
            return context.Runtime.CallFunction(this, arguments);
        }
    }
}