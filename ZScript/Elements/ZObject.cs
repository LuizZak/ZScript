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

using ZScript.Runtime.Execution.Wrappers;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies an dynamic object that can store values in subscripting and direct field access fashions
    /// </summary>
    public class ZObject : Dictionary<string, object>, ISubscripterWrapper
    {
        /// <summary>
        /// Gets or sets a value on this ZObject
        /// </summary>
        /// <param name="indexer">The object to index this ZObject with</param>
        /// <returns>The value corresponding to the indexer</returns>
        public object this[object indexer]
        {
            get
            {
                CheckType(indexer);

                object ret;
                if (TryGetValue((string)indexer, out ret))
                {
                    return new Optional<object>(ret);
                }

                return Optional<object>.Empty;
            }
            set
            {
                CheckType(indexer);

                base[(string)indexer] = value;
            }
        }

        /// <summary>
        /// Checks the type of the given object, raising an exception in case it is not a valid subscripter value for a ZObject
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <exception cref="ArgumentException">The given object is not a string type</exception>
        /// <exception cref="ArgumentNullException">The provided obj argument is null</exception>
        private static void CheckType(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!(obj is string))
            {
                throw new ArgumentException("ZObjects can only be indexed with string types");
            }
        }

        // 
        // CanSubscriptWithIndexType override
        // 
        public bool CanSubscriptWithIndexType(Type type)
        {
            return type == typeof(string);
        }
    }
}