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

using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Represents a wrapper for callable members of objects
    /// </summary>
    public interface ICallableWrapper
    {
        /// <summary>
        /// Gets the name of the callable wrapped by this ICallableWrapper
        /// </summary>
        string CallableName { get; }

        /// <summary>
        /// Gets the local memory for this ICallableWrapper.
        /// May be null, if no local memory is available
        /// </summary>
        IMemory<string> LocalMemory { get; }

        /// <summary>
        /// Returns the type for the callable wrapped by this ICallableWrapper when presented with a given list of arguments
        /// </summary>
        /// <param name="arguments">The arguments to get the callable type info of</param>
        /// <returns>A CallableTypeDef for a given argument list</returns>
        CallableTypeDef CallableTypeWithArguments(CallArguments arguments);

        /// <summary>
        /// Performs a call of the callable, utilizing a given array as arguments
        /// </summary>
        /// <param name="arguments">The arguments for the call</param>
        /// <param name="context">A VM context to use when executing the method</param>
        /// <returns>The return of the call. Will be null, if the wrapped callable's return type is void</returns>
        object Call(VmContext context, CallArguments arguments);
    }
}