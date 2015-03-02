namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents an interface to be implemented by types whose values can be a function-like manner
    /// </summary>
    public interface ICallableTypeDef
    {
        /// <summary>
        /// Gets the types for the parameters of this callable type definition
        /// </summary>
        TypeDef[] ParameterTypes { get; }

        /// <summary>
        /// Gets the information for the parameters of this callable type definition
        /// </summary>
        CallableTypeDef.CallableParameterInfo[] ParameterInfos { get; }

        /// <summary>
        /// Gets the count of arguments required by this callable type definition
        /// </summary>
        int RequiredArgumentsCount { get; }

        /// <summary>
        /// Gets the total count of arguments accepted by this callable type definition.
        /// If there is at least one variadic argument, the value returned is int.MaxValue
        /// </summary>
        int MaximumArgumentsCount { get; }

        /// <summary>
        /// Gets a value specifying whether a return type has been provided
        /// </summary>
        bool HasReturnType { get; }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        TypeDef ReturnType { get; }
    }
}