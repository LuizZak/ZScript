/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Reflection;

using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Execution.Wrappers.Callables
{
    /// <summary>
    /// Represents a wrapped class method
    /// </summary>
    public class ClassMethod : ICallableWrapper
    {
        /// <summary>
        /// The method infos wrapped in this class method
        /// </summary>
        private readonly MethodInfo[] _methodInfos;

        /// <summary>
        /// The target object to perform method calls on
        /// </summary>
        private readonly object _target;

        /// <summary>
        /// Gets the name of the callable wrapped by this ClassMethod
        /// </summary>
        public string CallableName
        {
            get { return _methodInfos[0].Name; }
        }

        /// <summary>
        /// Initializes a new instance of the ClassMethod class
        /// </summary>
        /// <param name="target">The target for this wrapper</param>
        /// <param name="methodInfos">The methods to wrap on this wrapper</param>
        public ClassMethod(object target, MethodInfo[] methodInfos)
        {
            _target = target;
            _methodInfos = methodInfos;
        }

        /// <summary>
        /// Calls the method wrapped in this ClassMethod instance.
        /// Raises an exception, if no method matches the given set of arguments
        /// </summary>
        /// <param name="arguments">The arguments for the method call</param>
        /// <returns>The return of the method call</returns>
        /// <exception cref="Exception"></exception>
        public object Call(params object[] arguments)
        {
            // TODO: Se how we are going to deal with long -> int conversions during native calls
            var method = MatchMethod(arguments);

            return method.Invoke(_target, arguments);
        }

        /// <summary>
        /// Returns the type for the callable wrapped by this ICallableWrapper when presented with a given list of arguments.
        /// May return null, if no matching method is found that matches all of the given arguments
        /// </summary>
        /// <param name="arguments">The list of arguments to get the callable type info of</param>
        /// <returns>A CallableTypeDef for a given argument list</returns>
        public CallableTypeDef CallableTypeWithArguments(params object[] arguments)
        {
            var info = MatchMethod(arguments);

            if (info != null)
                return CallableFromMethodInfo(info);

            return null;
        }

        /// <summary>
        /// Matches one of the available methods on this ClassMethod with a given set of arguments
        /// </summary>
        /// <param name="arguments">The list of arguments to get the matching method information from</param>
        /// <returns>A MethodBase that matches the given argument list, or null, if none was found</returns>
        private MethodInfo MatchMethod(object[] arguments)
        {
            var ps = new ParameterModifier[1];

            if(arguments.Length > 0)
                ps[0] = new ParameterModifier(arguments.Length);

            object state;
            var info = Type.DefaultBinder.BindToMethod(BindingFlags.Public, _methodInfos, ref arguments, ps, null, null, out state);

            if(state != null)
                Type.DefaultBinder.ReorderArgumentArray(ref arguments, state);

            return info as MethodInfo;
        }

        /// <summary>
        /// Returns a callable type definition created from the signature of a given MethodInfo object
        /// </summary>
        /// <param name="info">The method information to convert</param>
        /// <returns>A callable type definition for the given method information</returns>
        private static CallableTypeDef CallableFromMethodInfo(MethodInfo info)
        {
            // TODO: Dump this into a utility class so the functionality can be accessed by type aliasers and etc.
            var methodParams = info.GetParameters();
            
            var parameterInfos = new CallableTypeDef.CallableParameterInfo[methodParams.Length];
            var returnType = TypeDefFromNativeType(info.ReturnType);
            
            for (int i = 0; i < methodParams.Length; i++)
            {
                var paramType = TypeDefFromNativeType(methodParams[i].ParameterType);
                bool variadic = Attribute.IsDefined(methodParams[i], typeof(ParamArrayAttribute));

                parameterInfos[i] = new CallableTypeDef.CallableParameterInfo(paramType, true, methodParams[i].HasDefaultValue, variadic);
            }

            return new CallableTypeDef(parameterInfos, returnType, true);
        }

        /// <summary>
        /// Creates a type def that wraps a given Type object
        /// </summary>
        /// <param name="type">The type to create the TypeDef from</param>
        /// <returns>A TypeDef that wraps the given Type class</returns>
        private static TypeDef TypeDefFromNativeType(Type type)
        {
            return new TypeDef(type.AssemblyQualifiedName, true);
        }
    }
}