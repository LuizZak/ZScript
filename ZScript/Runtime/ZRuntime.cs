using System;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents a runtime that is encapsulated
    /// </summary>
    public class ZRuntime
    {
        /// <summary>
        /// The owner of this ZRuntime
        /// </summary>
        private readonly IRuntimeOwner _owner;

        /// <summary>
        /// The list of all functions defined in this ZRuntime instance
        /// </summary>
        private readonly FunctionDef[] _functionDefs;

        /// <summary>
        /// The global memory for the runtime
        /// </summary>
        private readonly Memory _globalMemory;

        /// <summary>
        /// Gets the current global memory for the runtime
        /// </summary>
        public IMemory<string> GlobalMemory
        {
            get
            {
                return _globalMemory;
            }
        }

        /// <summary>
        /// Gets the owner of this ZRuntime object
        /// </summary>
        public IRuntimeOwner Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Initializes a new instance of the ZRuntime class from the following definition, and with an owner to specify
        /// </summary>
        /// <param name="definition">The runtime definition object to create this runtime from</param>
        /// <param name="owner">The owner of this ZRuntime</param>
        public ZRuntime(ZRuntimeDefinition definition, IRuntimeOwner owner)
        {
            _functionDefs = definition.FunctionDefinitions;
            _owner = owner;
            _globalMemory = new Memory();
        }

        /// <summary>
        /// Calls a function with a specified name, using the specified functions as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="functionName">The name of the function to execute</param>
        /// <param name="arguments">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunction(string functionName, params object[] arguments)
        {
            var funcDef = FunctionWithName(functionName);

            if (funcDef == null)
                return null;

            FunctionVM vm = new FunctionVM(funcDef.Tokens, new VmContext(_globalMemory, this));

            vm.Execute();

            if (vm.HasReturnValue)
            {
                return vm.ReturnValue;
            }

            // Disabling this exception for now because it may interfere with function calls that do not expect a return value anyways
            /*throw new Exception("The function called did not return any value because it did not execute a 'return' statement with a value specified.");*/

            return null;
        }

        /// <summary>
        /// Fetches a function definition by name on this ZRuntime object, or null, if none was found
        /// </summary>
        /// <param name="functionName">The name of the function to fetch</param>
        /// <returns>The function definition with the given name, or null, if none was found</returns>
        FunctionDef FunctionWithName(string functionName)
        {
            for (int i = 0; i < _functionDefs.Length; i++)
            {
                if (_functionDefs[i].Name == functionName)
                    return _functionDefs[i];
            }

            return null;
        }
    }
}