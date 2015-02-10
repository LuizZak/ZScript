using System;
using System.Collections.Generic;
using ZScript.Elements;
using ZScript.Runtime;

namespace ZScriptTests
{
    /// <summary>
    /// Runtime owner used in tests
    /// </summary>
    public class TestRuntimeOwner : IRuntimeOwner
    {
        /// <summary>
        /// List of objects traced during the lifetime of this test runtime owner instance
        /// </summary>
        public List<object> TraceObjects = new List<object>();

        /// <summary>
        /// Called by the runtime when an export function was invoked by the script
        /// </summary>
        /// <param name="func">The function that was invoked</param>
        /// <param name="parameters">The list of parameters the function was called with</param>
        /// <returns>The return value for the function call</returns>
        public object CallFunction(ZExportFunction func, params object[] parameters)
        {
            if (func.Name == "__trace")
            {
                TraceObjects.AddRange(parameters);
                return parameters;
            }

            throw new ArgumentException("Unkown or invalid export function '" + func.Name + "' that is not recognized by this runtime owner");
        }
    }
}