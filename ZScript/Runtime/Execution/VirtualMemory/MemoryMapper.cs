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
    /// <summary>
    /// Class that maps variables from multiple memory blocks into a common access point.
    /// This is mostly used by closures to capture local memory from the scope they are called on
    /// </summary>
    public class MemoryMapper : IMemory<string>
    {
        /// <summary>
        /// The list of memory locations this memory mapper is mapping to
        /// </summary>
        private readonly List<IMemory<string>> _memoryList;

        /// <summary>
        /// Initializes a new instance of the MemoryMapper class
        /// </summary>
        public MemoryMapper()
        {
            _memoryList = new List<IMemory<string>>();
        }

        /// <summary>
        /// Initializes a new instance of the MemoryMapper class
        /// </summary>
        /// <param name="memoryList">The list of memories for this memory mapper</param>
        protected MemoryMapper(List<IMemory<string>> memoryList)
        {
            _memoryList = memoryList;
        }

        /// <summary>
        /// Performs a shallow clone of this MemoryMapper instance
        /// </summary>
        /// <returns>A shallow clone of this MemoryMapper instance</returns>
        public MemoryMapper Clone()
        {
            return new MemoryMapper(new List<IMemory<string>>(_memoryList));
        }

        /// <summary>
        /// Returns whether the given varID exists in the memory
        /// </summary>
        /// <param name="variableName">The variable to seek in the memory</param>
        /// <returns>Whether the variable exists or not</returns>
        public bool HasVariable(string variableName)
        {
            for (int i = _memoryList.Count - 1; i >= 0; i--)
            {
                if (_memoryList[i].HasVariable(variableName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the desired variable from the memory
        /// </summary>
        /// <param name="variableName">The variable ID to get the value form</param>
        /// <returns>The current value stored on the variable</returns>
        public object GetVariable(string variableName)
        {
            for (int i = _memoryList.Count - 1; i >= 0; i--)
            {
                object value;
                if (_memoryList[i].TryGetVariable(variableName, out value))
                {
                    return value;
                }
            }

            return null;
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
            for (int i = _memoryList.Count - 1; i >= 0; i--)
            {
                if (_memoryList[i].TryGetVariable(identifier, out value))
                    return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Sets the desired variable to the given value on the memory.
        /// The scopes are sweeped from inner-most to outer-most, and the first memory to contain the variable is used to set.
        /// If no scope contains the variable, the inner-most scope has the variable set instead.
        /// </summary>
        /// <param name="variableName">The variable ID to change</param>
        /// <param name="value">The new value to set the variable to</param>
        public void SetVariable(string variableName, object value)
        {
            if (_memoryList.Count == 0)
                return;

            for (int i = _memoryList.Count - 1; i >= 0; i--)
            {
                if (_memoryList[i].HasVariable(variableName))
                {
                    _memoryList[i].SetVariable(variableName, value);
                    return;
                }
            }

            // Fallback - no memory has that variable, set it at the inner-most scope
            _memoryList[_memoryList.Count - 1].SetVariable(variableName, value);
        }

        /// <summary>
        /// Clears this memory mapper instance
        /// </summary>
        public void Clear()
        {
            // Just clear the memory list
            _memoryList.Clear();
        }

        /// <summary>
        /// Returns the count of items inside this Memory object
        /// </summary>
        /// <returns>The count of items inside this Memory object</returns>
        public int GetCount()
        {
            int c = 0;
            for (int i = 0; i < _memoryList.Count; i++)
            {
                c += _memoryList[i].GetCount();
            }

            return c;
        }

        /// <summary>
        /// Adds a memory to this memory mapper instance
        /// </summary>
        /// <param name="memory">The memory to add to this memory mapper</param>
        public void AddMemory(IMemory<string> memory)
        {
            _memoryList.Add(memory);
        }
    }
}