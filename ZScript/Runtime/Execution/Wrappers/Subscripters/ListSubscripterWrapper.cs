﻿#region License information
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
using System.Collections;

namespace ZScript.Runtime.Execution.Wrappers.Subscripters
{
    /// <summary>
    /// Object capable of enabling subscription to IList objects
    /// </summary>
    public class ListSubscripterWrapper : ISubscripterWrapper
    {
        /// <summary>
        /// The IList being subscripted
        /// </summary>
        private readonly IList _list;

        /// <summary>
        /// Gets the IList being subscripted by this ListSubscripter
        /// </summary>
        public IList List
        {
            get { return _list; }
        }

        /// <summary>
        /// Gets or sets an index on the IList being subscripted
        /// </summary>
        /// <param name="indexer">The index to subscript into the list. This index must be an Int32 or Int64 value</param>
        /// <returns>The object that was in the given index on the underlying list</returns>
        public object this[object indexer]
        {
            get
            {
                if (indexer is int)
                {
                    return _list[(int)indexer];
                }
                if (indexer is long)
                {
                    return _list[(int)(long)indexer];
                }
                
                throw new ArgumentException("The index for subscripting an array must be an Int32 or Int64 value");
            }
            set
            {
                if (indexer is int)
                {
                    _list[(int)indexer] = value;
                    return;
                }
                if (indexer is long)
                {
                    _list[(int)(long)indexer] = value;
                    return;
                }

                throw new ArgumentException("The index for subscripting an array must be an Int32 or Int64 value");
            }
        }

        /// <summary>
        /// Initializes a new instance of the ListSubscripter class
        /// </summary>
        /// <param name="list">The list to subscript</param>
        public ListSubscripterWrapper(IList list)
        {
            _list = list;
        }

        /// <summary>
        /// Returns boolean value specifying whether this ISubscripter value can subscript with the specified value type
        /// </summary>
        /// <param name="type">The type of the value to use as an indexer on subscript operations</param>
        /// <returns>A boolean value specifying whether this ISubscripter value can subscript with the specified value type</returns>
        public bool CanSubscriptWithIndexType(Type type)
        {
            return type == typeof(int) || type == typeof(long);
        }
    }
}