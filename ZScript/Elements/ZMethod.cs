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
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a class method
    /// </summary>
    public class ZMethod : ZFunction
    {
        /// <summary>
        /// Gets or sets the local memory for the class running this method
        /// </summary>
        public Memory LocalMemory { get; set; }

        /// <summary>
        /// Gets or sets the base method for this ZMethod
        /// </summary>
        public ZMethod BaseMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the ZMethod class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="tokens">The tokens for the method</param>
        /// <param name="arguments">The arguments for the method call</param>
        /// <param name="returnType">The return type for the method</param>
        public ZMethod(string name, TokenList tokens, FunctionArgument[] arguments, Type returnType)
            : base(name, tokens, arguments, returnType)
        {

        }

        /// <summary>
        /// Performs a shallow clone of this ZMethod class, copying field values
        /// </summary>
        /// <returns>A shallow clone of this ZMethod class</returns>
        public ZMethod Clone()
        {
            var newMethod = (ZMethod)MemberwiseClone();

            return newMethod;
        }
    }
}