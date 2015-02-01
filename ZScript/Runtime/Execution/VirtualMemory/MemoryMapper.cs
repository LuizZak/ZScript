﻿using System.Collections.Generic;

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
                if (_memoryList[i].HasVariable(variableName))
                    return _memoryList[i].GetVariable(variableName);
            }

            return null;
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