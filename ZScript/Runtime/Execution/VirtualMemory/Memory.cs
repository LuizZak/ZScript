using System;
using System.Collections;
using System.Collections.Generic;
using ZScript.Elements;
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
        /// Merges the given list of memory objects into a single memory block
        /// </summary>
        /// <param name="memories">The memory blocks to merge</param>
        /// <returns>A merged memory block</returns>
        public static Memory MergeMemory(params Memory[] memories)
        {
            Memory retMemory = new Memory();

            // Merge the memories one by one
            foreach (Memory mem in memories)
            {
                // Merge using the dictionary keys
                foreach (string key in mem._memory.Keys)
                {
                    retMemory._memory[key] = mem._memory[key];
                }
            }

            return retMemory;
        }

        /// <summary>
        /// Creates a Memory block with the given arguments set as memory spaces
        /// </summary>
        /// <param name="def">Definition to be used as base when replacing variables with their names</param>
        /// <param name="byName">Whether the parameters array should be used as a 'by name' argument selector</param>
        /// <param name="arguments">The arguments to use as memory spaces</param>
        /// <returns>A memory block, with the given arguments used as memory spaces</returns>
        public static Memory CreateMemoryFromArgs(ZFunction def, bool byName, params object[] arguments)
        {
            Memory mem = new Memory();

            if (byName)
            {
                int i;
                bool arrayRest = false;

                // Set the default values now
                foreach (var arg in def.Arguments)
                {
                    if (arg.HasValue)
                    {
                        mem.SetVariable(arg.Name, arg.DefaultValue);
                    }
                }

                // Fetch each variable name from the function definition
                for (i = 0; i < arguments.Length; i++)
                {
                    if (def.Arguments[i].IsVariadic)
                    {
                        arrayRest = true;
                        break;
                    }

                    mem.SetVariable(def.Arguments[i].Name, TypeOperationProvider.TryCastNumber(arguments[i]));
                }

                if (arrayRest)
                {
                    VarArgsArrayList rest = new VarArgsArrayList();
                    for (int j = i; j < arguments.Length; j++)
                    {
                        // Concat ArrayList arguments
                        var varArgs = arguments[j] as VarArgsArrayList;
                        if (varArgs != null)
                        {
                            rest.InsertRange(rest.Count, varArgs);
                        }
                        else
                        {
                            rest.Add(arguments[j]);
                        }
                    }
                    mem.SetVariable(def.Arguments[i].Name, rest);
                }
            }
            else if (arguments.Length > 1)
            {
                for (int i = 0; i < arguments.Length; i += 2)
                {
                    mem.SetVariable(arguments[i] as String, arguments[i + 1]);
                }
            }

            return mem;
        }

        /// <summary>
        /// Specifies a list of variable arguments
        /// </summary>
        public class VarArgsArrayList : ArrayList
        {
            /// <summary>
            /// Transforms this VarAgsArrayList into a plain ArrayList object
            /// </summary>
            /// <returns>A plain ArrayList object</returns>
            public ArrayList ToArrayList()
            {
                return new ArrayList(this);
            }
        }
    }
}