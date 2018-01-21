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
using System.Text;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Typing.Elements;

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
        /// <param name="returnType">The return type for the ZFunction</param>
        public ZFunction(string name, TokenList tokens, FunctionArgument[] arguments, Type returnType)
        {
            Name = name;
            Tokens = tokens;
            Arguments = arguments;
            ReturnType = returnType;
        }

        /// <summary>
        /// Returns a string representation of this ZFunction
        /// </summary>
        /// <returns>A string representation of this ZFunction</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            // Name
            builder.Append(Name);
            
            // TODO: Add generic signature handling

            // Parameters
            builder.Append("(");
            foreach (var argument in Arguments)
            {
                builder.Append(argument.Name);
                builder.Append(": ");
                builder.Append(argument.Type);
                if (argument.HasValue)
                {
                    builder.Append(" = ");
                    builder.Append(argument.DefaultValue);
                }
            }
            builder.Append(")");

            // Return type
            builder.Append(" : ");
            builder.Append(ReturnType);

            return builder.ToString();
        }

        /// <summary>
        /// The name for this function definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the tokens associated with this function definition
        /// </summary>
        public TokenList Tokens { get; }

        /// <summary>
        /// Gets or sets a value specifying whether this function is a closure function type
        /// </summary>
        public bool IsClosure { get; set; }

        /// <summary>
        /// Gets or sets the signature for this function
        /// </summary>
        public CallableTypeDef Signature { get; set; }

        /// <summary>
        /// Gets an array of the arguments for this function
        /// </summary>
        public FunctionArgument[] Arguments { get; }

        /// <summary>
        /// Gets the return type for this ZFunction
        /// </summary>
        public Type ReturnType { get; }
    }
}