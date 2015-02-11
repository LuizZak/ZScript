using System;
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

        /// <summary>
        /// Called by the runtime when a 'new' instruction has been hit
        /// </summary>
        /// <param name="typeName">The name of the type trying to be instantiated</param>
        /// <param name="parameters">The parameters collected from the function call</param>
        /// <returns>The newly created object</returns>
        object CreateType(string typeName, params object[] parameters);
    }
}