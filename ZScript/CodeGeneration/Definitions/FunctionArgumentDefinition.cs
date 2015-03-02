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

using System.Text;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a variable definition
    /// </summary>
    public class FunctionArgumentDefinition : ValueHolderDefinition
    {
        /// <summary>
        /// Gets or sets a value specifying whether the function argument represented by this definition is variadic in nature.
        /// Variadic arguments allow for the caller to specify as many values as desired, separated by commas like normal arguments.
        /// Functions can only have one variadic argument, appearing at the end of the argument list
        /// </summary>
        public bool IsVariadic { get; set; }

        /// <summary>
        /// The compile-time constant defining the value for the function argument
        /// </summary>
        public ZScriptParser.CompileConstantContext DefaultValue { get; set; }

        /// <summary>
        /// Creates an argument information based on the information of this function argument
        /// </summary>
        /// <returns>A callble argument information generated from this function argument</returns>
        public CallableTypeDef.CallableParameterInfo ToArgumentInfo()
        {
            return new CallableTypeDef.CallableParameterInfo(Type, HasType, HasValue, IsVariadic);
        }

        /// <summary>
        /// Returns a string representation of this function argument
        /// </summary>
        /// <returns>A string representation of this function argument</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Name);

            // Type
            builder.Append(": ");
            if (IsVariadic && Type is IListTypeDef)
            {
                builder.Append(((IListTypeDef)Type).EnclosingType);
                builder.Append("...");
            }
            else
            {
                builder.Append(Type);
            }

            // Return type
            if (HasValue)
            {
                builder.Append(" = ");
                builder.Append(DefaultValue);
            }

            return builder.ToString();
        }
    }
}