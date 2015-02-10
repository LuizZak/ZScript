using ZScript.Elements;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents an interface to be implemented by objects that will provide interop between a ZRuntime and the program
    /// </summary>
    public interface IRuntimeOwner
    {
        /// <summary>
        /// Called by the runtime when an export function was invoked by the script
        /// </summary>
        /// <param name="func">The function that was invoked</param>
        /// <param name="parameters">The list of parameters the function was called with</param>
        /// <returns>The return value for the function call</returns>
        object CallFunction(ZExportFunction func, params object[] parameters);
    }
}