namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a function argument
    /// </summary>
    public class FunctionArgument : Variable
    {
        /// <summary>
        /// Gets a value specifying whether the function argument represented by this definition is variadic in nature.
        /// Variadic arguments allow for the caller to specify as many values as desired, separated by commas like normal arguments.
        /// Functions can only have one variadic argument, appearing at the end of the argument list
        /// </summary>
        public bool IsVariadic { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the FunctionArgument class
        /// </summary>
        /// <param name="name">The name for the function argument</param>
        /// <param name="isVariadic">Whether the function argument is variadic in nature</param>
        /// <param name="hasValue">Whether the argument has a default value</param>
        /// <param name="value">The default value for the function argument</param>
        public FunctionArgument(string name, bool isVariadic = false, bool hasValue = false, object value = null)
        {
            Name = name;
            IsVariadic = isVariadic;
            HasValue = hasValue;
            DefaultValue = value;
        }
    }
}