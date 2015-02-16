using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Represents a wrapper for callable members of objects
    /// </summary>
    public interface ICallableWrapper
    {
        /// <summary>
        /// Gets the name of the callable wrapped by this ICallableWrapper
        /// </summary>
        string CallableName { get; }

        /// <summary>
        /// REturns the type for the callable wrapped by this ICallableWrapper when presented with a given list of arguments
        /// </summary>
        /// <param name="arguments">The list of arguments to get the callable type info of</param>
        /// <returns>A CallableTypeDef for a given argument list</returns>
        CallableTypeDef CallableTypeWithArguments(params object[] arguments);

        /// <summary>
        /// Performs a call of the callable, utilizing a given array as arguments
        /// </summary>
        /// <param name="arguments">The arguments for the call</param>
        /// <returns>The return of the call. Will be null, if the wrapped callable's return type is void</returns>
        object Call(params object[] arguments);
    }
}