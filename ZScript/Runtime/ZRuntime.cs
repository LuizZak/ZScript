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
using System.Collections.Generic;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Typing;

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
        /// Stack of functions being executed currently
        /// </summary>
        private readonly Stack<ZFunction> _functionStack; 

        /// <summary>
        /// The type provider for runtime type conversions
        /// </summary>
        private readonly TypeProvider _typeProvider;

        /// <summary>
        /// Whether the runtime has expanded the global variables yet
        /// </summary>
        private bool _expandedGlobals;

        /// <summary>
        /// The index at the functions array closures start
        /// </summary>
        private readonly int _closuresStart;

        /// <summary>
        /// The list of all functions defined in this ZRuntime instance
        /// </summary>
        private readonly ZFunction[] _zFunctions;

        /// <summary>
        /// The list of all classes defined in this ZRuntime instance
        /// </summary>
        private readonly ZClass[] _zClasses;

        /// <summary>
        /// The global memory for the runtime
        /// </summary>
        private readonly Memory _globalMemory;

        /// <summary>
        /// The global memory for the runtime
        /// </summary>
        private readonly LocalMemory _globalAddressedMemory;

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
            _functionStack = new Stack<ZFunction>();

            _owner = owner;
            _globalMemory = new Memory();
            _globalAddressedMemory = new LocalMemory(0);
            _typeProvider = new TypeProvider();

            _closuresStart = definition.ZFunctionDefinitions.Length + definition.ZExportFunctionDefinitions.Length;
            _zClasses = definition.ClassDefinitions;
        }

        /// <summary>
        /// Calls a function with a specified name, using an array of objects as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="functionName">The name of the function to execute</param>
        /// <param name="callArgs">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunction(string functionName, params object[] callArgs)
        {
            return CallFunction(functionName, new CallArguments(callArgs));
        }

        /// <summary>
        /// Calls a function with a specified name, using an array of objects as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="functionName">The name of the function to execute</param>
        /// <param name="callArgs">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunction(string functionName, CallArguments callArgs)
        {
            var funcDef = FunctionWithName(functionName);

            if(funcDef == null)
                throw new Exception("Trying to call undefined function '" + functionName + "'");

            return CallFunction(funcDef, callArgs);
        }

        /// <summary>
        /// Calls a wrapped callable, using an array of objects as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="callable">The callable to execute</param>
        /// <param name="callArgs">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallWrapper(ICallableWrapper callable, CallArguments callArgs)
        {
            bool pushed = false;

            if(callable.LocalMemory != null)
            {
                _localMemoriesStack.Push(callable.LocalMemory);
                pushed = true;
            }

            var ret = callable.Call(new VmContext(_globalMemory, _globalAddressedMemory, this, _owner, _typeProvider), callArgs);

            if (pushed)
                _localMemoriesStack.Pop();

            return ret;
        }

        /// <summary>
        /// Calls a specified function, using an array of objects as arguments.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="funcDef">The function to execute</param>
        /// <param name="callArgs">The arguments for the function to execute</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunction(ZFunction funcDef, CallArguments callArgs)
        {
            // TODO: Clean the clutter on this method
            if (funcDef == null) throw new ArgumentNullException("funcDef");
            
            if (!_expandedGlobals)
            {
                ExpandGlobalVariables();
            }

            // Export functions are handled separatedly
            var exportFunction = funcDef as ZExportFunction;
            if (exportFunction != null)
            {
                if(_owner.RespondsToFunction(exportFunction))
                    return _owner.CallFunction(exportFunction, callArgs);

                throw new UnrecognizedExportFunctionException("The runtime owner responded false to RespondsToFunction: It does not recognizes the export function " + funcDef);
            }

            IMemory<string> localMemory = Memory.CreateMemoryFromArgs(funcDef, callArgs.Arguments);

            // Closures must trap the local memory into their own scope
            if (funcDef.IsClosure)
            {
                ((ZClosureFunction)funcDef).CapturedMemory.AddMemory(localMemory);

                localMemory = ((ZClosureFunction)funcDef).CapturedMemory;
            }

            // Class constructors
            var constructor = funcDef as ZConstructor;
            if (constructor != null)
            {
                // Call the base constructor sequentially
                if(constructor.BaseMethod != null && constructor.RequiresBaseCall)
                {
                    CallFunction(constructor.BaseMethod, callArgs);
                }

                // Wrap the method's memory and the class' memory into a memory mapper
                var classMemory = new MemoryMapper();

                classMemory.AddMemory(constructor.ClassInstance.LocalMemory);
                classMemory.AddMemory(localMemory);

                localMemory = classMemory;

                // Init the fields beforehand
                MemoryMapper constructorMapper = new MemoryMapper();
                constructorMapper.AddMemory(_globalMemory);
                constructorMapper.AddMemory(constructor.ClassInstance.LocalMemory);
                constructor.InitFields(new VmContext(constructorMapper, _globalAddressedMemory, this, _owner, _typeProvider, new TypeList(callArgs.GenericTypes)));
            }

            return CallFunctionWithMemory(funcDef, localMemory, callArgs);
        }

        /// <summary>
        /// Calls a specified function, using a given memory to map local variables.
        /// The method raises an exception when the function call fails for any reason
        /// </summary>
        /// <param name="funcDef">The function to execute</param>
        /// <param name="localMemory">The local memory for the method to call</param>
        /// <param name="callArgs">The arguments for the function call</param>
        /// <returns>The return of the function that was called</returns>
        /// <exception cref="ArgumentException">A function with the specified name does not exists</exception>
        /// <exception cref="Exception">The function call failed</exception>
        public object CallFunctionWithMemory(ZFunction funcDef, IMemory<string> localMemory, CallArguments callArgs)
        {
            // TODO: Clean the clutter on this method
            if (funcDef == null) throw new ArgumentNullException("funcDef");

            if (!_expandedGlobals)
            {
                ExpandGlobalVariables();
            }

            var constructor = funcDef as ZConstructor;

            MemoryMapper mapper = new MemoryMapper();
            mapper.AddMemory(_globalMemory);
            mapper.AddMemory(localMemory);

            _localMemoriesStack.Push(localMemory);
            _functionStack.Push(funcDef);

            FunctionVM vm = new FunctionVM(funcDef.Tokens, new VmContext(mapper, _globalAddressedMemory, this, _owner, _typeProvider, new TypeList(callArgs.GenericTypes)));
            vm.Execute();

            _functionStack.Pop();
            _localMemoriesStack.Pop();

            mapper.Clear();

            // In case we just ran a constructor, return the class instance created by the constructor
            if (constructor != null)
            {
                return constructor.ClassInstance;
            }

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
            // TODO: Deal with this special 'base' case here
            if (functionName == "base" && _functionStack.Count > 0)
            {
                var topMethod = _functionStack.Peek() as ZMethod;
                if (topMethod != null)
                {
                    var cloneBaseMethod = topMethod.BaseMethod.Clone();
                    cloneBaseMethod.LocalMemory = topMethod.LocalMemory;

                    return cloneBaseMethod;
                }
            }

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

            // Search for constructors now
            foreach (var zClass in _zClasses)
            {
                if (zClass.ClassName == functionName)
                {
                    return CreateConstructor(zClass);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new constructor and binds it to an instance
        /// </summary>
        /// <param name="zClass">The class to create the constructor out of</param>
        /// <returns>A ZConstructor that when executed returns an instance of a class</returns>
        private ZConstructor CreateConstructor(ZClass zClass)
        {
            var instType = (ZClassInstance)Activator.CreateInstance(zClass.NativeType, zClass);

            return new ZConstructor(instType, instType.Class.ConstructorRequiresBaseCall);
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

            var vmContext = new VmContext(_globalMemory, _globalAddressedMemory, this, _owner, _typeProvider);

            foreach (var vDef in _definition.GlobalVariableDefinitions)
            {
                if (vDef.HasValue)
                {
                    var vm = new FunctionVM(vDef.ExpressionTokens, vmContext);
                    vm.Execute();

                    _globalMemory.SetVariable(vDef.Name, vm.PopValueImplicit());
                }
                else
                {
                    _globalMemory.SetVariable(vDef.Name, null);
                }
            }

            _expandedGlobals = true;
        }
    }

    /// <summary>
    /// Class that encapsulates information about a function call
    /// </summary>
    public class CallArguments
    {
        /// <summary>
        /// The parameters for the function call
        /// </summary>
        public readonly object[] Arguments;

        /// <summary>
        /// Whether the function call contains generic type information
        /// </summary>
        public readonly bool IsGeneric;

        /// <summary>
        /// The generic types being passed to the function call
        /// </summary>
        public readonly Type[] GenericTypes;

        /// <summary>
        /// Initializes a new instance of the CallInformation class with a set of parameters as arguments
        /// </summary>
        /// <param name="arguments">The parameters for the function call</param>
        public CallArguments(params object[] arguments)
            : this(arguments, new Type[0])
        {

        }

        /// <summary>
        /// Initializes a new instance of the CallInformation class with a set of parameters as arguments and a set of generic types
        /// </summary>
        /// <param name="arguments">The parameters for the function call</param>
        /// <param name="genericTypes">The generic types being passed to the function call</param>
        public CallArguments(object[] arguments, Type[] genericTypes)
        {
            Arguments = arguments;
            GenericTypes = genericTypes;
            IsGeneric = genericTypes.Length > 0;
        }
    }

    /// <summary>
    /// Represents an exception raisd when trying to execute an export function not recognized by a runtime owner
    /// </summary>
    public class UnrecognizedExportFunctionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the UnrecognizedExportFunctionException class
        /// </summary>
        /// <param name="message">The message for the exception</param>
        public UnrecognizedExportFunctionException(string message)
            : base(message)
        {

        }
    }
}