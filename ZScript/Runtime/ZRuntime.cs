using System;
using System.Collections.Generic;
using System.Linq;

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
        /// The ZRuntimeDefinition that was used to create this runtime
        /// </summary>
        private readonly ZRuntimeDefinition _definition;

        /// <summary>
        /// Stack of local memories for the currently executing functions.
        /// Used to enable closure capture
        /// </summary>
        private readonly Stack<IMemory<string>> _localMemoriesStack;

        /// <summary>
        /// Whether the runtime has expanded the global variables yet
        /// </summary>
        private bool _expandedGlobals;

        /// <summary>
        /// The index at the functions array closures start
        /// </summary>
        private int _closuresStart;

        /// <summary>
        /// The list of all functions defined in this ZRuntime instance
        /// </summary>
        private readonly ZFunction[] _zFunctions;

        /// <summary>
        /// The global memory for the runtime
        /// </summary>
        private readonly Memory _globalMemory;

        /// <summary>
        /// The global memory for the runtime
        /// </summary>
        private readonly IntegerMemory _globalAddressedMemory;

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
            _definition = definition;
            _zFunctions = definition.GetFunctions();
            _localMemoriesStack = new Stack<IMemory<string>>();
            _owner = owner;
            _globalMemory = new Memory();
            _globalAddressedMemory = new IntegerMemory();

            _closuresStart = definition.ZFunctionDefinitions.Length + definition.ZExportFunctionDefinitions.Length;
        }

        /// <summary>
        /// Calls a function with a specified name, using an array of objects as arguments.
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

            if(funcDef == null)
                throw new Exception("Trying to call undefined function '" + functionName + "'");

            return CallFunction(funcDef, arguments);
        }

        /// <summary>
        /// Calls a specified function, using an array of objects as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="funcDef">The function to execute</param>
        /// <param name="arguments">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunction(ZFunction funcDef, params object[] arguments)
        {
            if (!_expandedGlobals)
            {
                ExpandGlobalVariables();
            }

            if (funcDef == null)
                return null;

            // Export functions are handled separatedly
            var exportFunction = funcDef as ZExportFunction;
            if (exportFunction != null)
            {
                return _owner.CallFunction(exportFunction, arguments);
            }

            IMemory<string> localMemory = Memory.CreateMemoryFromArgs(funcDef, true, arguments);

            // Closures must trap the local memory into their own scope
            if (funcDef.IsClosure)
            {
                ((ZClosureFunction)funcDef).CapturedMemory.AddMemory(localMemory);

                localMemory = ((ZClosureFunction)funcDef).CapturedMemory;
            }

            MemoryMapper mapper = new MemoryMapper();
            mapper.AddMemory(_globalMemory);
            mapper.AddMemory(localMemory);

            _localMemoriesStack.Push(localMemory);

            FunctionVM vm = new FunctionVM(funcDef.Tokens, new VmContext(mapper, _globalAddressedMemory, this));
            vm.Execute();

            _localMemoriesStack.Pop();

            mapper.Clear();

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
        /// <param name="captureClosures">Whether to automatically capture closures when fetching by name</param>
        /// <returns>The function definition with the given name, or null, if none was found</returns>
        public ZFunction FunctionWithName(string functionName, bool captureClosures = true)
        {
            foreach (ZFunction func in _zFunctions)
            {
                if (func.Name == functionName)
                {
                    var closure = func as ZClosureFunction;
                    if (closure != null && captureClosures)
                    {
                        return CaptureClosure(closure);
                    }

                    return func;
                }
            }

            return null;
        }

        /// <summary>
        /// Captures a given closure on the currently executed context.
        /// The returned closure is a shallow clone of the passed closure, with the captured memory copied from the current top of the local memory stack
        /// </summary>
        /// <param name="closure">The closure to capture</param>
        /// <returns>A closure that represents the captured losure</returns>
        private ZClosureFunction CaptureClosure(ZClosureFunction closure)
        {
            // Capture the memory now
            var capturedMemory = new MemoryMapper();

            if (_localMemoriesStack.Count > 0 && _localMemoriesStack.Peek().GetCount() > 0)
            {
                capturedMemory.AddMemory(_localMemoriesStack.Peek());
            }

            var newClosure = closure.Clone();
            newClosure.CapturedMemory = capturedMemory;

            return newClosure;
        }

        /// <summary>
        /// Returns a ZFunction contained at a given index on this ZRuntime
        /// </summary>
        /// <param name="index">The index of the function to get</param>
        /// <returns>A ZFunction contained at the given index</returns>
        public ZFunction FunctionAtIndex(int index)
        {
            var func = _zFunctions[index];

            if (index >= _closuresStart)
            {
                return CaptureClosure(func as ZClosureFunction);
            }

            return func;
        }

        /// <summary>
        /// Expands the global variables contained within the ZRuntimeDefinition this runtime was created with
        /// </summary>
        public void ExpandGlobalVariables()
        {
            if (_expandedGlobals)
            {
                return;
            }

            var vmContext = new VmContext(_globalMemory, _globalAddressedMemory, this);

            foreach (var vDef in _definition.GlobalVariableDefinitions)
            {
                if (vDef.HasValue)
                {
                    var vm = new FunctionVM(vDef.ExpressionTokens, vmContext);
                    vm.Execute();

                    _globalMemory.SetVariable(vDef.Name, vm.Stack.Pop());
                }
                else
                {
                    _globalMemory.SetVariable(vDef.Name, null);
                }
            }

            _expandedGlobals = true;
        }
    }
}