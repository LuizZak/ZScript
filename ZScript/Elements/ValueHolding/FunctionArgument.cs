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
namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a function argument
    /// </summary>
    public class FunctionArgument : Variable
    {
        /// <summary>
        /// Gets a value specifying whether the function argument represented by this definition is variadic in nature.
        /// Variadic arguments allow for the caller to specify as many values as desired, separated by commas like normal arguments.
        /// Functions can only have one variadic argument, appearing at the end of the argument list
        /// </summary>
        public bool IsVariadic { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the FunctionArgument class
        /// </summary>
        /// <param name="name">The name for the function argument</param>
        /// <param name="isVariadic">Whether the function argument is variadic in nature</param>
        /// <param name="hasValue">Whether the argument has a default value</param>
        /// <param name="value">The default value for the function argument</param>
        public FunctionArgument(string name, bool isVariadic = false, bool hasValue = false, object value = null)
        {
            Name = name;
            IsVariadic = isVariadic;
            HasValue = hasValue;
            DefaultValue = value;
        }
    }
}