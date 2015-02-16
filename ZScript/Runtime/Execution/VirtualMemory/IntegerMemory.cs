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
using System.Collections.Generic;

namespace ZScript.Runtime.Execution.VirtualMemory
{
    public class IntegerMemory : IMemory<int>
    {
        /// <summary>
        /// The memory dictionary
        /// </summary>
        private readonly Dictionary<int, object> _memory;

        /// <summary>
        /// Initializes a new instance of the Memory class
        /// </summary>
        public IntegerMemory()
        {
            _memory = new Dictionary<int, object>();
        }

        /// <summary>
        /// Returns whether the given varID exists in the memory
        /// </summary>
        /// <param name="address">The variable to seek in the memory</param>
        /// <returns>Whether the variable exists or not</returns>
        public bool HasVariable(int address)
        {
            return _memory.ContainsKey(address);
        }

        /// <summary>
        /// Gets the desired variable from the memory
        /// </summary>
        /// <param name="address">The variable ID to get the value form</param>
        /// <returns>The current value stored on the variable</returns>
        public object GetVariable(int address)
        {
            return _memory[address];
        }

        /// <summary>
        /// Tries to get a variable on this Memory object, returing a boolean value
        /// that specifies whether the fetch was successful or not
        /// </summary>
        /// <param name="identifier">The identifier of the variable to try to get</param>
        /// <param name="value">The value that was fetched. Will be null, if the fetch fails</param>
        /// <returns>Whether the fetch was successful</returns>
        public bool TryGetVariable(int identifier, out object value)
        {
            return _memory.TryGetValue(identifier, out value);
        }

        /// <summary>
        /// Sets the desired variable to the given value on the memory
        /// </summary>
        /// <param name="address">The variable ID to change</param>
        /// <param name="value">The new value to set the variable to</param>
        public void SetVariable(int address, object value)
        {
            _memory[address] = value;
        }

        /// <summary>
        /// Clears the given variable from the memory now
        /// </summary>
        /// <param name="address">The address to clear</param>
        public void ClearVariable(int address)
        {
            _memory.Remove(address);
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
        public Dictionary<int, object> GetObjectMemory()
        {
            return _memory;
        }

        /// <summary>
        /// Merges the given list of memory objects into a single memory block
        /// </summary>
        /// <param name="memories">The memory blocks to merge</param>
        /// <returns>A merged memory block</returns>
        public static IntegerMemory MergeMemory(params IntegerMemory[] memories)
        {
            IntegerMemory retMemory = new IntegerMemory();

            // Merge the memories one by one
            foreach (IntegerMemory mem in memories)
            {
                // Merge using the dictionary keys
                foreach (int key in mem._memory.Keys)
                {
                    retMemory._memory[key] = mem._memory[key];
                }
            }

            return retMemory;
        }
    }
}