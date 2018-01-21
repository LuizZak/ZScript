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
using JetBrains.Annotations;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents a type definition that has a direct native equivalent
    /// </summary>
    public class NativeTypeDef : TypeDef
    {
        /// <summary>
        /// The type this native type is equivalent of
        /// </summary>
        protected Type nativeType;

        /// <summary>
        /// Gets the type this native type is equivalent of
        /// </summary>
        public Type NativeType => nativeType;

        /// <summary>
        /// Initializes a new instance of the NativeTypeDef class
        /// </summary>
        /// <param name="type">The type to create this native type out of</param>
        public NativeTypeDef([NotNull] Type type) : base(type.Name, true)
        {
            nativeType = type;
        }

        /// <summary>
        /// Initializes a new instance of the NativeTypeDef class
        /// </summary>
        /// <param name="type">The type to create this native type out of</param>
        /// <param name="name">The usage name for the type</param>
        public NativeTypeDef(Type type, string name)
            : base(name, true)
        {
            nativeType = type;
        }
    }
}