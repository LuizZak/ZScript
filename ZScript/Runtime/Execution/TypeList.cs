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
using JetBrains.Annotations;

namespace ZScript.Runtime.Execution
{
    /// <summary>
    /// Represents an object that encapsulates a list of types that is used
    /// in generic contexts to dynamically fetch the type of objects
    /// </summary>
    public class TypeList
    {
        /// <summary>
        /// The list of types visible in this type list
        /// </summary>
        private readonly List<Type> _types;

        /// <summary>
        /// Gets the count of types stored in this type list
        /// </summary>
        public int Count => _types.Count;

        /// <summary>
        /// Initializes a new instance of the TypeList class with an empty type list
        /// </summary>
        public TypeList()
            : this(new Type [0])
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the TypeList class with a specified enumerable object of types to start from
        /// </summary>
        /// <param name="types">An enumerable to create the type list from</param>
        public TypeList([NotNull] IEnumerable<Type> types)
        {
            _types = new List<Type>(types);
        }

        /// <summary>
        /// Gets a type at the specified index on this type list.
        /// If the index is out of bounds, an <see cref="ArgumentOutOfRangeException"/> is raised
        /// </summary>
        /// <param name="index">The index of the type to get</param>
        /// <returns>The type at the specified index</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified index is out of bounds</exception>
        public Type TypeAtIndex(int index)
        {
            return _types[index];
        }
    }
}