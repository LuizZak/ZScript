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
using System.Collections;
using System.Collections.Generic;
using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Typing;

namespace ZScript.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Memory used to store variables for the runner
    /// </summary>
    public class Memory : IMemory<string>
    {
        /// <summary>
        /// The memory dictionary
        /// </summary>
        private readonly Dictionary<string, object> _memory;

        /// <summary>
        /// Initializes a new instance of the Memory class
        /// </summary>
        public Memory()
        {
            _memory = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns whether the given varID exists in the memory
        /// </summary>
        /// <param name="variableName">The variable to seek in the memory</param>
        /// <returns>Whether the variable exists or not</returns>
        public bool HasVariable(string variableName)
        {
            return _memory.ContainsKey(variableName);
        }

        /// <summary>
        /// Gets the desired variable from the memory
        /// </summary>
        /// <param name="variableName">The variable ID to get the value form</param>
        /// <returns>The current value stored on the variable</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The variable name was not found on this memory object</exception>
        public object GetVariable(string variableName)
        {
            return _memory[variableName];
        }

        /// <summary>
        /// Tries to get a variable on this Memory object, returing a boolean value
        /// that specifies whether the fetch was successful or not
        /// </summary>
        /// <param name="identifier">The identifier of the variable to try to get</param>
        /// <param name="value">The value that was fetched. Will be null, if the fetch fails</param>
        /// <returns>Whether the fetch was successful</returns>
        public bool TryGetVariable(string identifier, out object value)
        {
            return _memory.TryGetValue(identifier, out value);
        }

        /// <summary>
        /// Sets the desired variable to the given value on the memory
        /// </summary>
        /// <param name="variableName">The variable ID to change</param>
        /// <param name="value">The new value to set the variable to</param>
        public void SetVariable(string variableName, object value)
        {
            _memory[variableName] = value;
        }

        /// <summary>
        /// Clears the given variable from the memory now
        /// </summary>
        /// <param name="variableName">The variable to clear</param>
        public void ClearVariable(string variableName)
        {
            _memory.Remove(variableName);
        }

        /// <summary>
        /// Clears the memory
        /// </summary>
        public void Clear()
        {
            _memory.Clear();
        }

        /// <summary>
        /// Returns the count of items inside this Memory object
        /// </summary>
        /// <returns>The count of items inside this Memory object</returns>
        public int GetCount()
        {
            return _memory.Count;
        }

        /// <summary>
        /// Returns the dictionary currently being used as memory
        /// </summary>
        /// <returns>The dictionary currently being used as memory</returns>
        public Dictionary<string, object> GetObjectMemory()
        {
            return _memory;
        }

        /// <summary>
        /// Creates a Memory block with the given arguments set as memory spaces
        /// </summary>
        /// <param name="def">Definition to be used as base when replacing variables with their names</param>
        /// <param name="arguments">The arguments to use as memory spaces</param>
        /// <returns>A memory block, with the given arguments used as memory spaces</returns>
        public static Memory CreateMemoryFromArgs(ZFunction def, params object[] arguments)
        {
            Memory mem = new Memory();
            int i;
            bool arrayRest = false;
            FunctionArgument varArg = null;

            // Set the default values now
            foreach (var arg in def.Arguments)
            {
                if (arg.HasValue)
                {
                    mem.SetVariable(arg.Name, arg.DefaultValue);
                }
                // Variadic
                if (arg.IsVariadic)
                {
                    mem.SetVariable(arg.Name, VarArgsFromArgument(arg));
                }
            }

            // Fetch each variable name from the function definition
            for (i = 0; i < arguments.Length; i++)
            {
                if (def.Arguments[i].IsVariadic)
                {
                    arrayRest = true;
                    varArg = def.Arguments[i];
                    break;
                }

                mem.SetVariable(def.Arguments[i].Name, TypeOperationProvider.TryCastNumber(arguments[i]));
            }

            if (arrayRest && varArg != null)
            {
                var rest = VarArgsFromArgument(varArg);
                for (int j = i; j < arguments.Length; j++)
                {
                    // Concat ArrayList arguments
                    var varArgs = arguments[j] as IVarArgs;
                    if (varArgs != null)
                    {
                        foreach (var obj in varArgs)
                        {
                            rest.Add(obj);
                        }
                    }
                    else
                    {
                        rest.Add(arguments[j]);
                    }
                }
                mem.SetVariable(def.Arguments[i].Name, rest);
            }

            return mem;
        }

        /// <summary>
        /// Creates a new VarArgsArrayList from the given function argument definition
        /// </summary>
        /// <param name="argument">The function argument that contains the type of variable arguments list to create</param>
        /// <returns>A new generic VarArgsArrayList created from the given function argument</returns>
        public static IVarArgs VarArgsFromArgument(FunctionArgument argument)
        {
            return (IVarArgs)Activator.CreateInstance(typeof(VarArgsArrayList<>).MakeGenericType(argument.Type));
        }

        /// <summary>
        /// Specifies a list of variable arguments
        /// </summary>
        public class VarArgsArrayList<T> : List<T>, IVarArgs
        {
            
        }

        /// <summary>
        /// Interface that exposes a non-generic version of the VarArgsArrayList class
        /// </summary>
        public interface IVarArgs : IList
        {
            
        }
    }
}