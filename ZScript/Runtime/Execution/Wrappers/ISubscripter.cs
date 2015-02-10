using System;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Interface for objects that wrap a subscript-capable object into a common access point
    /// </summary>
    public interface ISubscripter
    {
        /// <summary>
        /// Gets or sets a subscript value on this ISubscripter object
        /// </summary>
        /// <param name="indexer">The value to use when subscripting</param>
        /// <returns>An object that is subscripted into the given index</returns>
        object this[object indexer] { get; set; }

        /// <summary>
        /// Returns boolean value specifying whether this ISubscripter value can subscript with the specified value type
        /// </summary>
        /// <param name="type">The type of the value to use as an indexer on subscript operations</param>
        /// <returns>A boolean value specifying whether this ISubscripter value can subscript with the specified value type</returns>
        bool CanSubscriptWithIndexType(Type type);
    }
}