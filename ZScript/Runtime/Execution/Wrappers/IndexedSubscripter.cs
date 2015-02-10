using System;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Represents an object that contains an ISubscripter object and an index object bundled into a single object.
    /// This is used by the FunctionVM during GetSubscript instructions to fetch subscript of objects on the stack
    /// </summary>
    public sealed class IndexedSubscripter
    {
        /// <summary>
        /// The object being subscripted
        /// </summary>
        private readonly ISubscripter _subscripter;

        /// <summary>
        /// The object to index the subscripter as
        /// </summary>
        private readonly object _indexValue;

        /// <summary>
        /// Gets the object being subscripted
        /// </summary>
        public ISubscripter Subscripter
        {
            get { return _subscripter; }
        }

        /// <summary>
        /// Gets the object to index the subscripter as
        /// </summary>
        public object IndexValue
        {
            get { return _indexValue; }
        }

        /// <summary>
        /// Initializes a new instance of the IndexedSubscripter class.
        /// The constructor raises an exception if the subscripter object cannot be indexed by
        /// the type of the index object by calling CanSubscriptWithIndexType on the subscripter
        /// </summary>
        /// <param name="subscripter">The subscripter to subscript into</param>
        /// <param name="indexValue">The index on the subscripter to subscript to</param>
        /// <exception cref="ArgumentException">The proided subscripter cannot be subscripted with an object of the index's type</exception>
        public IndexedSubscripter(ISubscripter subscripter, object indexValue)
        {
            _subscripter = subscripter;
            _indexValue = indexValue;

            if (!_subscripter.CanSubscriptWithIndexType(indexValue.GetType()))
            {
                throw new ArgumentException("The provided subscripter object cannot subscript to objects of type '" + indexValue.GetType() + "'");
            }
        }

        /// <summary>
        /// Gets the value at the index of the array pointed by this index subscripter
        /// </summary>
        /// <returns>The value pointed by this index subscripter</returns>
        public object GetValue()
        {
            return _subscripter[_indexValue];
        }

        /// <summary>
        /// Sets the value at the index of the array pointed by this index subscripter
        /// </summary>
        /// <param name="value">The value to set on the index pointed by this index subscripter</param>
        public void SetValue(object value)
        {
            _subscripter[_indexValue] = value;
        }
    }
}