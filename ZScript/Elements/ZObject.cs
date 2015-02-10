using System;
using System.Collections;
using System.Collections.Generic;

using ZScript.Runtime.Execution.Wrappers;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies an dynamic object that can store values in subscripting and direct field access fashions
    /// </summary>
    public class ZObject : ISubscripterWrapper, IDictionary
    {
        /// <summary>
        /// Dictionary containing the objects stored in this ZObject class
        /// </summary>
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

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

                return _dictionary[(string)indexer];
            }
            set
            {
                CheckType(indexer);

                _dictionary[(string)indexer] = value;
            }
        }

        /// <summary>
        /// Checks the type of the given object, raising an exception in case it is not a valid subscripter value for a ZObject
        /// </summary>
        /// <param name="obj">The object to check</param>
        private static void CheckType(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!(obj is string))
            {
                throw new Exception("ZObjects can only be indexed with string types");
            }
        }

        // 
        // CanSubscriptWithIndexType override
        // 
        public bool CanSubscriptWithIndexType(Type type)
        {
            return type == typeof(string);
        }

        #region IDictionary implementation

        // 
        // IDictionary.Keys implementation
        // 
        public ICollection Keys
        {
            get { return _dictionary.Keys; }
        }

        // 
        // IDictionary.Values implementation
        // 
        public ICollection Values
        {
            get { return _dictionary.Values; }
        }

        // 
        // IDictionary.IsReadOnly implementation
        // 
        public bool IsReadOnly
        {
            get { return false; }
        }

        // 
        // IDictionary.IsFixedSize implementation
        // 
        public bool IsFixedSize
        {
            get { return false; }
        }

        // 
        // IDictionary.Contains implementation
        // 
        public bool Contains(object key)
        {
            CheckType(key);

            return _dictionary.ContainsKey((string)key);
        }

        // 
        // IDictionary.Add implementation
        // 
        public void Add(object key, object value)
        {
            this[key] = value;
        }

        // 
        // IDictionary.Remove implementation
        // 
        public void Remove(object key)
        {
            CheckType(key);

            _dictionary.Remove((string)key);
        }

        // 
        // IDictionary.Clear implementation
        // 
        public void Clear()
        {
            _dictionary.Clear();
        }

        // 
        // IDictionary.IDictionaryEnumerator implementation
        // 
        public IDictionaryEnumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        // 
        // IEnumerable.GetEnumerator implementation
        // 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        // 
        // ICollection.CopyTo implementation
        // 
        public void CopyTo(Array array, int index)
        {
            ((ICollection)_dictionary).CopyTo(array, index);
        }

        // 
        // ICollection.Count implementation
        // 
        public int Count
        {
            get { return _dictionary.Count; }
        }

        // 
        // ICollection.SyncRoot implementation
        // 
        public object SyncRoot
        {
            get { return ((ICollection)_dictionary).SyncRoot; }
        }

        // 
        // ICollection.IsSynchronized implementation
        // 
        public bool IsSynchronized
        {
            get { return ((ICollection)_dictionary).IsSynchronized; }
        }

        #endregion
    }
}