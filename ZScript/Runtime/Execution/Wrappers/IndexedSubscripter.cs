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
using System.Collections;
using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Represents an object that contains an ISubscripter object and an index object bundled into a single object.
    /// This is used by the FunctionVM during GetSubscript instructions to fetch subscript of objects on the stack
    /// </summary>
    public sealed class IndexedSubscripter : IValueHolder
    {
        /// <summary>
        /// The object being subscripted
        /// </summary>
        private readonly ISubscripterWrapper _subscripterWrapper;

        /// <summary>
        /// The object to index the subscripter as
        /// </summary>
        private readonly object _indexValue;

        /// <summary>
        /// Gets the object being subscripted
        /// </summary>
        public ISubscripterWrapper SubscripterWrapper => _subscripterWrapper;

        /// <summary>
        /// Gets the object to index the subscripter as
        /// </summary>
        public object IndexValue => _indexValue;

        /// <summary>
        /// Initializes a new instance of the IndexedSubscripter class.
        /// The constructor raises an exception if the subscripter object cannot be indexed by
        /// the type of the index object by calling CanSubscriptWithIndexType on the subscripter
        /// </summary>
        /// <param name="subscripterWrapper">The subscripter to subscript into</param>
        /// <param name="indexValue">The index on the subscripter to subscript to</param>
        /// <exception cref="ArgumentException">The proided subscripter cannot be subscripted with an object of the index's type</exception>
        public IndexedSubscripter(ISubscripterWrapper subscripterWrapper, object indexValue)
        {
            _subscripterWrapper = subscripterWrapper;
            _indexValue = indexValue;

            if (!_subscripterWrapper.CanSubscriptWithIndexType(indexValue.GetType()))
            {
                throw new ArgumentException("The provided subscripter object '" + subscripterWrapper + "' cannot subscript to objects of type '" + indexValue.GetType() + "'");
            }
        }

        /// <summary>
        /// Initializes a new instance of the IndexedSubscripter class.
        /// The constructor raises an exception if the subscripter object cannot be indexed by
        /// the type of the index object by calling CanSubscriptWithIndexType on the subscripter
        /// </summary>
        /// <param name="subscripterWrapper">The subscripter to subscript into</param>
        /// <param name="indexValue">The index on the subscripter to subscript to</param>
        /// <exception cref="ArgumentException">The proided subscripter cannot be subscripted with an object of the index's type</exception>
        private IndexedSubscripter(object indexValue, ISubscripterWrapper subscripterWrapper)
        {
            _subscripterWrapper = subscripterWrapper;
            _indexValue = indexValue;
        }

        /// <summary>
        /// Gets the value at the index of the array pointed by this index subscripter
        /// </summary>
        /// <returns>The value pointed by this index subscripter</returns>
        public object GetValue()
        {
            return _subscripterWrapper[_indexValue];
        }

        /// <summary>
        /// Sets the value at the index of the array pointed by this index subscripter
        /// </summary>
        /// <param name="value">The value to set on the index pointed by this index subscripter</param>
        public void SetValue(object value)
        {
            _subscripterWrapper[_indexValue] = value;
        }

        /// <summary>
        /// Creates and returns a subscripter fit for the given object and type.
        /// If no subscripter fits the object, an Exception is raised
        /// </summary>
        /// <param name="target">The target object to generate the subscripter with</param>
        /// <param name="indexValue">The index to generate the subscripter with</param>
        /// <returns>A new IndexedSubscripter with the target object and index value binded on</returns>
        public static IndexedSubscripter CreateSubscripter(object target, object indexValue)
        {
            return new IndexedSubscripter(indexValue, GetSubscripterForObject(target, indexValue.GetType()));
        }

        /// <summary>
        /// Gets a subscripter fit for the given object and type.
        /// If no subscripter fits the object, an Exception is raised
        /// </summary>
        /// <param name="target">The target object to generate the subscripter with</param>
        /// <param name="subscriptType">The type of object used to subscript on the target object</param>
        /// <returns>A new ISubscripterWrapper that can subscript the target object with the type provided</returns>
        public static ISubscripterWrapper GetSubscripterForObject(object target, Type subscriptType)
        {
            // target itself is a subscript wrapper
            var o = target as ISubscripterWrapper;
            if (o != null)
                return o;

            var list = target as IList;
            if (list != null)
            {
                var sub = new ListSubscripterWrapper(list);

                if (!sub.CanSubscriptWithIndexType(subscriptType))
                {
                    throw new Exception("List objects must have a subscript type of Int32 or Int64 type.");
                }

                return sub;
            }

            // Search a public property that can be subscripted
            var properties = target.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetIndexParameters().Length == 1 && property.GetIndexParameters()[0].ParameterType == subscriptType)
                {
                    return new PropertySubscripterWrapper(target, property);
                }
            }

            throw new Exception("No subscripter found that can subscript objects of type '" + target.GetType() + "' with indexes of type '" + subscriptType + "'.");
        }
    }
}