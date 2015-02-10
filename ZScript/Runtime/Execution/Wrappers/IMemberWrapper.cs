using System;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Represents a wrapper for members of objects
    /// </summary>
    public interface IMemberWrapper : IValueHolder
    {
        /// <summary>
        /// Gets the name of the member wrapped by this IMemberWrapper
        /// </summary>
        string MemberName { get; }

        /// <summary>
        /// Gets the type for the member wrapped by this IMemberWrapper
        /// </summary>
        Type MemberType { get; }
    }
}