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
using System.Linq;
using System.Reflection;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a string primitive type definition
    /// </summary>
    public class StringTypeDef : NativeTypeDef, IListTypeDef
    {
        // TODO: Deal with this cyclic reference thing 
        /// <summary>
        /// Singleton instance of the StringTypeDef used for cyclic references
        /// </summary>
        private static StringTypeDef _singleton;

        /// <summary>
        /// Returns the type enclosed within the string type definition
        /// </summary>
        public TypeDef EnclosingType => AnyType;

        /// <summary>
        /// Gets the type of objects accepted by the subscript of the string type
        /// </summary>
        public TypeDef SubscriptType => IntegerType;

        /// <summary>
        /// Initilaizes a new instance of the StringTypeDef class
        /// </summary>
        public StringTypeDef() : base(typeof(string))
        {
            _singleton = this;

            // Add the base functions from the String library
            var type = typeof(string);

            // Add 'length' parameter
            AddField(new TypeFieldDef("Length", new NativeTypeDef(typeof(int)), true));

            var nativeMethods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in nativeMethods)
            {
                AddMethodFrom(method);
            }
        }

        /// <summary>
        /// Inserts a method on this TypeDef from a given method information
        /// </summary>
        /// <param name="method">The method to add to this TypeDef</param>
        private void AddMethodFrom(MethodInfo method)
        {
            // Create the parameters
            var parameters = method.GetParameters();

            var paramInfos = new ParameterInfo[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                bool variadic = parameters[i].GetCustomAttributes(typeof(ParamArrayAttribute)).Any();

                paramInfos[i] = new ParameterInfo(parameters[i].Name,
                    method.ReturnType == typeof(string) ? _singleton : MostFittingType(parameters[i].ParameterType),
                    variadic, parameters[i].HasDefaultValue);
            }

            var returnType = method.ReturnType == typeof(string) ? _singleton : MostFittingType(method.ReturnType);
            var methodDef = new TypeMethodDef(method.Name, paramInfos, returnType);

            AddMethod(methodDef);
        }
    }
}