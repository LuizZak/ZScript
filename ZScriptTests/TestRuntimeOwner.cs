using System;
using System.Collections.Generic;
using System.Reflection;

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

        /// <summary>
        /// Called by the runtime when a 'new' instruction has been hit
        /// </summary>
        /// <param name="typeName">The name of the type trying to be instantiated</param>
        /// <param name="parameters">The parameters collected from the function call</param>
        /// <returns>The newly created object</returns>
        public object CreateType(string typeName, params object[] parameters)
        {
            var type = Type.GetType(typeName);

            if(type == null)
                throw new Exception("No type with type name '" + typeName + "' detected.");
            
            var types = Type.GetTypeArray(parameters);
            var ps = new ParameterModifier[1];
            ps[0] = new ParameterModifier(types.Length);

            ConstructorInfo[] constructors = type.GetConstructors();
            ConstructorInfo c = (ConstructorInfo)Type.DefaultBinder.SelectMethod(BindingFlags.Instance | BindingFlags.Public, constructors, types, ps);

            if (constructors.Length > 0 && c == null)
            {
                throw new Exception("No constructor overload for type '" + type + "' conforms to the passed types.");
            }

            return c.Invoke(parameters);
        }
    }
}