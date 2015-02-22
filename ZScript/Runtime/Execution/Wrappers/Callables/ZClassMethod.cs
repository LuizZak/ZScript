﻿using System;
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
        public string CallableName
        {
            get { return _method.Name; }
        }

        /// <summary>
        /// Gets the local memory o the class instance binded to this ZClassMethod
        /// </summary>
        public IMemory<string> LocalMemory
        {
            get { return _target.LocalMemory; }
        }

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
        public object Call(VmContext context, params object[] arguments)
        {
            // Wrap the context's local memory
            MemoryMapper mapper = new MemoryMapper();

            mapper.AddMemory(context.Memory);
            mapper.AddMemory(_target.LocalMemory);

            // Create the arguments for the function and wrap them too
            mapper.AddMemory(Memory.CreateMemoryFromArgs(_method, arguments));

            var newContext = new VmContext(mapper, context.AddressedMemory, context.Runtime, context.Owner, context.TypeProvider);

            FunctionVM vm = new FunctionVM(_method.Tokens, newContext);

            vm.Execute();

            if (vm.HasReturnValue)
                return vm.ReturnValue;

            return null;
        }

        /// <summary>
        /// Returns the type for the callable wrapped by this ICallableWrapper when presented with a given list of arguments.
        /// May return null, if no matching method is found that matches all of the given arguments
        /// </summary>
        /// <param name="arguments">The list of arguments to get the callable type info of</param>
        /// <returns>A CallableTypeDef for a given argument list</returns>
        public CallableTypeDef CallableTypeWithArguments(params object[] arguments)
        {
            return _method.Signature;
        }
    }
}