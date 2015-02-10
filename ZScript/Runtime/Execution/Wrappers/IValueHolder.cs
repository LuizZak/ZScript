namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Interface to be implemented by objects that have values that can be get/set
    /// </summary>
    public interface IValueHolder
    {
        /// <summary>
        /// Sets the value of the member wrapped by this IValueHolder
        /// </summary>
        /// <param name="value">The value to set the member as</param>
        void SetValue(object value);

        /// <summary>
        /// Gets the value of the member wrapped by this IValueHolder
        /// </summary>
        /// <returns>The value of the member wrapped by this IValueHolder</returns>
        object GetValue();
    }
}