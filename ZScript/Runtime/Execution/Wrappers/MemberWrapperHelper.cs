#region License information
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
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;

using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers.Callables;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Helper class used during member wrapping
    /// </summary>
    public static class MemberWrapperHelper
    {
        /// <summary>
        /// Returns an IMemberWrapper for a given target object's member.
        /// If the target object does not contains a matching field and it is not a ZObject instance, an ArgumentException is raised
        /// </summary>
        /// <param name="target">The target to wrap</param>
        /// <param name="memberName">The name of the member to wrap</param>
        /// <returns>An IMemberWrapper created from the given target</returns>
        /// <exception cref="ArgumentException">No member with the given name found on target object</exception>
        public static IMemberWrapper CreateMemberWrapper(object target, string memberName)
        {
            // ZObjects always wrap any member name incoming
            var zObj = target as ZObject;
            if (zObj != null)
            {
                return new ZObjectMember(zObj, memberName);
            }

            // Try to wrap around the class instance, if any of the fields equals to the passed member name
            var zCls = target as ZClassInstance;
            if (zCls != null)
            {
                var fields = zCls.Class.Fields;
                foreach (ZClassField field in fields)
                {
                    if (field.Name == memberName)
                    {
                        return new ZClassMember(zCls, memberName);
                    }
                }
            }

            // Fall-back to normal class member search
            return ClassMember.CreateMemberWrapper(target, memberName);
        }

        /// <summary>
        /// Returns an ICallableWrapper for a given target object's method.
        /// If the target object does not contains a matching method, an ArgumentException is raised
        /// </summary>
        /// <param name="target">The target to wrap</param>
        /// <param name="callableName">The name of the method to wrap</param>
        /// <returns>An ICallableWrapper created from the given target</returns>
        /// <exception cref="ArgumentException">No method with the given name found on target object</exception>
        public static ICallableWrapper CreateCallableWrapper(object target, string callableName)
        {
            // ZClassInstance method
            var zClassInstance = target as ZClassInstance;
            if (zClassInstance != null)
            {
                var zMethods = zClassInstance.Class.Methods;
                for (int i = 0; i < zMethods.Length; i++)
                {
                    if (zMethods[i].Name == callableName)
                    {
                        return new ZClassMethod(zClassInstance, zMethods[i]);
                    }
                }
            }
            
            // Native method
            var methods = MethodsNamedFor(target.GetType(), callableName);

            if (methods.Length == 0)
                throw new ArgumentException("No public method of name '" + callableName + "' found on object of type '" + target.GetType() + "'", "callableName");

            return new ClassMethod(target, methods);
        }

        /// <summary>
        /// Gets all methods available for a given type
        /// </summary>
        /// <param name="type">The type to get the methods of</param>
        /// <returns>An array containing all of the methods for the given type</returns>
        private static MethodInfo[] MethodsForType(Type type)
        {
            MethodInfo[] o;
            if (TypeMethods.TryGetValue(type, out o))
            {
                return o;
            }

            return TypeMethods[type] = type.GetMethods();
        }

        /// <summary>
        /// Gets all methods on a given type that match the provided name
        /// </summary>
        /// <param name="type">The type to get the methods of</param>
        /// <param name="methodName">The name of the methods to match</param>
        /// <returns>An array containing all of the methods for the given type</returns>
        private static MethodInfo[] MethodsNamedFor(Type type, string methodName)
        {
            List<TypeMethodsNamed> cachedMethods;
            if (!CachedTypeMethodsNamed.TryGetValue(type, out cachedMethods))
            {
                cachedMethods = CachedTypeMethodsNamed[type] = new List<TypeMethodsNamed>();
            }

            for (int i = 0; i < cachedMethods.Count; i++)
            {
                if (cachedMethods[i].MethodName == methodName)
                    return cachedMethods[i].Methods;
            }

            // Add a new entry to the method cache list
            var methods = MethodsForType(type);
            var methodsNamed = new List<MethodInfo>();

            foreach (var method in methods)
            {
                if (method.Name == methodName)
                    methodsNamed.Add(method);
            }

            var cached = new TypeMethodsNamed(methodsNamed.ToArray(), methodName);

            cachedMethods.Add(cached);

            return cached.Methods;
        }

        /// <summary>
        /// Internal dictionary used to improve performance of the CreateCallableWrapper method
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo[]> TypeMethods = new Dictionary<Type, MethodInfo[]>();

        /// <summary>
        /// Internal dictionary used to improve performance of the CreateCallableWrapper method
        /// </summary>
        private static readonly Dictionary<Type, List<TypeMethodsNamed>> CachedTypeMethodsNamed = new Dictionary<Type, List<TypeMethodsNamed>>();

        /// <summary>
        /// Struct that is used to cache lookups of type methods by name
        /// </summary>
        private struct TypeMethodsNamed
        {
            /// <summary>
            /// Gets the methods associated with this TypeMethodsNamed struct
            /// </summary>
            public readonly MethodInfo[] Methods;

            /// <summary>
            /// Gets the method names associated with this TypeMethodsNamed struct
            /// </summary>
            public readonly string MethodName;

            /// <summary>
            /// Initializes a new TypeMethodsNamed struct
            /// </summary>
            /// <param name="methods">The array of matching methods to associate with this struct</param>
            /// <param name="methodName">The name of the method group</param>
            public TypeMethodsNamed(MethodInfo[] methods, string methodName)
            {
                Methods = methods;
                MethodName = methodName;
            }
        }
    }
}