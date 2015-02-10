using System;
using System.Reflection;

namespace ZScript.Runtime.Execution.Wrappers.Subscripters
{
    /// <summary>
    /// Wraps a subscripter that binds to a public indexed property of a target object
    /// </summary>
    public class ProperySubscripterWrapper : ISubscripterWrapper
    {
        /// <summary>
        /// The object being subscripted
        /// </summary>
        private readonly object _target;

        /// <summary>
        /// The property being subscripted in
        /// </summary>
        private readonly PropertyInfo _property;

        /// <summary>
        /// Gets the object being subscripted by this ListSubscripter
        /// </summary>
        public object Target
        {
            get { return _target; }
        }

        /// <summary>
        /// Gets or sets an index on the object being subscripted
        /// </summary>
        /// <param name="indexer">The index to subscript into the object</param>
        /// <returns>The object that was in the given index on the underlying list</returns>
        public object this[object indexer]
        {
            get
            {
                return _property.GetValue(_target, new[] { indexer });
            }
            set
            {
                _property.SetValue(_target, value, new[] { indexer });
            }
        }

        /// <summary>
        /// Initializes a new instance of the ProperySubscripterWrapper class
        /// </summary>
        /// <param name="target">The object to subscript</param>
        /// <param name="property">The indexed property to index with</param>
        public ProperySubscripterWrapper(object target, PropertyInfo property)
        {
            _target = target;
            _property = property;
        }

        /// <summary>
        /// Returns boolean value specifying whether this ISubscripter value can subscript with the specified value type
        /// </summary>
        /// <param name="type">The type of the value to use as an indexer on subscript operations</param>
        /// <returns>A boolean value specifying whether this ISubscripter value can subscript with the specified value type</returns>
        public bool CanSubscriptWithIndexType(Type type)
        {
            return _property.GetIndexParameters()[0].ParameterType == type;
        }
    }
}