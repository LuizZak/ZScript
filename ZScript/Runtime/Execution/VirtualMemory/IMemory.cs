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

namespace ZScript.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Interface to be implemented by objects that simmulate memory access
    /// </summary>
    /// <typeparam name="T">The type used to map between the variable and the value</typeparam>
    public interface IMemory<in T>
    {
        /// <summary>
        /// Returns whether the given varID exists in the memory
        /// </summary>
        /// <param name="identifier">The variable to seek in the memory</param>
        /// <returns>Whether the variable exists or not</returns>
        bool HasVariable(T identifier);

        /// <summary>
        /// Tries to get a variable on this IMemory object, returing a boolean value
        /// that specifies whether the fetch was successful or not
        /// </summary>
        /// <param name="identifier">The identifier of the variable to try to get</param>
        /// <param name="value">The value that was fetched. Will be null, if the fetch fails</param>
        /// <returns>Whether the fetch was successful</returns>
        bool TryGetVariable(T identifier, out object value);

        /// <summary>
        /// Gets the desired variable from the memory
        /// </summary>
        /// <param name="identifier">The variable ID to get the value form</param>
        /// <returns>The current value stored on the variable</returns>
        object GetVariable(T identifier);

        /// <summary>
        /// Sets the desired variable to the given value on the memory
        /// </summary>
        /// <param name="identifier">The variable ID to change</param>
        /// <param name="value">The new value to set the variable to</param>
        void SetVariable(T identifier, object value);

        /// <summary>
        /// Clears this memory of all its contents
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the count of items inside this Memory object
        /// </summary>
        /// <returns>The count of items inside this Memory object</returns>
        int GetCount();
    }
}