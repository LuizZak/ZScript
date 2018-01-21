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
using ZScript.Elements.ValueHolding;

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a processed class read from a script source
    /// </summary>
    public class ZClass
    {
        /// <summary>
        /// Gets the name for the class
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Gets the methods for this class
        /// </summary>
        public ZMethod[] Methods { get; }

        /// <summary>
        /// Gets the constructor for this class
        /// </summary>
        public ZMethod Constructor { get; }

        /// <summary>
        /// Gets a value specifying whether the constructor for this class requires a base call, or the user already performed the call
        /// </summary>
        public bool ConstructorRequiresBaseCall { get; }

        /// <summary>
        /// Gets the fields for this class
        /// </summary>
        public ZClassField[] Fields { get; }

        /// <summary>
        /// Gets the native type associated with this ZClass
        /// </summary>
        public Type NativeType { get; }

        /// <summary>
        /// Initializes a new instance of the ZClass class
        /// </summary>
        /// <param name="className">The name for the class</param>
        /// <param name="methods">The array of methods for the class</param>
        /// <param name="fields">The array of fields for the class</param>
        /// <param name="constructor">The constructor for this class</param>
        /// <param name="nativeType">The native type associated with this ZClass</param>
        /// <param name="constructorRequiresBaseCall">Whether the constructor for this class requires a base call, or the user already performed the call</param>
        public ZClass(string className, ZMethod[] methods, ZClassField[] fields, ZMethod constructor, Type nativeType, bool constructorRequiresBaseCall)
        {
            ClassName = className;
            Methods = methods;
            Fields = fields;
            Constructor = constructor;
            NativeType = nativeType;
            ConstructorRequiresBaseCall = constructorRequiresBaseCall;
        }
    }

    /// <summary>
    /// Represents a variable for a ZClass
    /// </summary>
    public class ZClassField : Variable
    {
        /// <summary>
        /// The tokens to execute to initialize the variable's value when the class is created
        /// </summary>
        public TokenList Tokens { get; set; }

        /// <summary>
        /// Initializes a new instance of the ZClassField class
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="tokens">The tokens to execute to initialize the variable's value when the class is created</param>
        public ZClassField(string fieldName, TokenList tokens)
        {
            Name = fieldName;
            Tokens = tokens;
        }
    }
}