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

namespace ZScript.Elements
{
    /// <summary>
    /// Describes a function that can be executed by the runtime
    /// </summary>
    public class ZFunction
    {
        /// <summary>
        /// Creates a new function definition with a given name
        /// </summary>
        /// <param name="name">The name for this function definition</param>
        /// <param name="tokens">A list of tokens containing the instructions this function will perform</param>
        /// <param name="arguments">An array of the arguments for this function</param>
        public ZFunction(string name, TokenList tokens, FunctionArgument[] arguments)
        {
            Name = name;
            Tokens = tokens;
            _arguments = arguments;
        }

        /// <summary>
        /// The name for this function definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the tokens associated with this function definition
        /// </summary>
        public TokenList Tokens { get; private set; }

        /// <summary>
        /// Gets or sets a value specifying whether this function is a closure function type
        /// </summary>
        public bool IsClosure { get; set; }

        /// <summary>
        /// An array of the arguments for this function
        /// </summary>
        private readonly FunctionArgument[] _arguments;

        /// <summary>
        /// Gets an array of the arguments for this function
        /// </summary>
        public FunctionArgument[] Arguments { get { return _arguments; } }
    }
}