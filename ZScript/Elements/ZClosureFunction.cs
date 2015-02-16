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

using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies a closure function
    /// </summary>
    public class ZClosureFunction : ZFunction
    {
        /// <summary>
        /// Gets or sets the currently captured memory for this closure function
        /// </summary>
        public MemoryMapper CapturedMemory { get; set; }

        /// <summary>
        /// Initializes a new instance of the ZClosureFunction class
        /// </summary>
        /// <param name="name">The name for the closure</param>
        /// <param name="tokens">The list of tokens representing the closure's body</param>
        /// <param name="arguments">The list of arguments for the closure</param>
        public ZClosureFunction(string name, TokenList tokens, FunctionArgument[] arguments)
            : base(name, tokens, arguments)
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

            clone.CapturedMemory = CapturedMemory == null ? null : CapturedMemory.Clone();

            return clone;
        }
    }
}