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
using JetBrains.Annotations;
using ZScript.Elements;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Execution.Wrappers.Callables
{
    /// <summary>
    /// Represents a wrapped class method
    /// </summary>
    public class ZClassMethod : ICallableWrapper
    {
        /// <summary>
        /// The method infos wrapped in this class method
        /// </summary>
        private readonly ZMethod _method;

        /// <summary>
        /// The target object to perform method calls on
        /// </summary>
        private readonly ZClassInstance _target;

        /// <summary>
        /// Gets the name of the callable wrapped by this ClassMethod
        /// </summary>
        public string CallableName => _method.Name;

        /// <summary>
        /// Gets the local memory o the class instance binded to this ZClassMethod
        /// </summary>
        public IMemory<string> LocalMemory => _target.LocalMemory;

        /// <summary>
        /// Initializes a new instance of the ClassMethod class
        /// </summary>
        /// <param name="target">The target for this wrapper</param>
        /// <param name="method">The method to wrap</param>
        public ZClassMethod(ZClassInstance target, ZMethod method)
        {
            _target = target;
            _method = method;
        }

        /// <summary>
        /// Calls the method wrapped in this ClassMethod instance.
        /// Raises an exception, if no method matches the given set of arguments
        /// </summary>
        /// <param name="arguments">The arguments for the method call</param>
        /// <param name="context">A VM context to use when executing the method</param>
        /// <returns>The return of the method call</returns>
        /// <exception cref="Exception"></exception>
        public object Call([NotNull] VmContext context, [NotNull] CallArguments arguments)
        {
            // Wrap the context's local memory
            var mapper = new MemoryMapper();

            // Add the local memory of the class object
            mapper.AddMemory(_target.LocalMemory);
            // Create the arguments for the function and wrap them too
            mapper.AddMemory(Memory.CreateMemoryFromArgs(_method, arguments.Arguments));

            return context.Runtime.CallFunctionWithMemory(_method, mapper, arguments);
        }

        /// <summary>
        /// Returns the type for the callable wrapped by this ICallableWrapper when presented with a given list of arguments.
        /// May return null, if no matching method is found that matches all of the given arguments
        /// </summary>
        /// <param name="arguments">The list of arguments to get the callable type info of</param>
        /// <returns>A CallableTypeDef for a given argument list</returns>
        public CallableTypeDef CallableTypeWithArguments(CallArguments arguments)
        {
            return _method.Signature;
        }
    }
}