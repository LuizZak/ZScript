namespace ZScript.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Interface to be implemented by objects that simmulate memory access
    /// </summary>
    /// <typeparam name="T">The type used to map between the variable and the value</typeparam>
    public interface IMemory<T>
    {
        /// <summary>
        /// Returns whether the given varID exists in the memory
        /// </summary>
        /// <param name="identifier">The variable to seek in the memory</param>
        /// <returns>Whether the variable exists or not</returns>
        bool HasVariable(T identifier);

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