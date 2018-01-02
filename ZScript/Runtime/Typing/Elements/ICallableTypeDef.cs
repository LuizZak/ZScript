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

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents an interface to be implemented by types whose values can be a function-like manner
    /// </summary>
    public interface ICallableTypeDef
    {
        /// <summary>
        /// Gets the types for the parameters of this callable type definition
        /// </summary>
        TypeDef[] ParameterTypes { get; }

        /// <summary>
        /// Gets a tuple that represents the function parameters
        /// </summary>
        TupleTypeDef ParameterTuple { get; }

        /// <summary>
        /// Gets the information for the parameters of this callable type definition
        /// </summary>
        CallableTypeDef.CallableParameterInfo[] ParameterInfos { get; }
        
        /// <summary>
        /// Gets the count of arguments required by this callable type definition
        /// </summary>
        int RequiredArgumentsCount { get; }

        /// <summary>
        /// Gets the total count of arguments accepted by this callable type definition.
        /// If there is at least one variadic argument, the value returned is int.MaxValue
        /// </summary>
        int MaximumArgumentsCount { get; }

        /// <summary>
        /// Gets a value specifying whether a return type has been provided
        /// </summary>
        bool HasReturnType { get; }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        TypeDef ReturnType { get; }
    }
}