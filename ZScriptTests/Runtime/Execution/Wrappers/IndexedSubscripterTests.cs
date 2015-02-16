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
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScriptTests.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Tests the functionality of the IndexedSubscripter class
    /// </summary>
    [TestClass]
    public class IndexedSubscripterTests
    {
        #region List subscripting

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [TestMethod]
        public void TestBasicIndexing()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, 10);

            Assert.AreEqual(subscripter, indexed.SubscripterWrapper, "The subscripter on the indexer must be the subscripter passed at its constructor");
            Assert.AreEqual(10, indexed.IndexValue, "The index value on the indexer must be the index value passed at its constructor");
        }

        /// <summary>
        /// Tests usage of the Get/SetValue methods
        /// </summary>
        [TestMethod]
        public void TestGetSetValues()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, 0);

            Assert.AreEqual(array[0], indexed.GetValue(), "Calling GetValue() should return the value on the index of the array pointed by the IndexedSubscripter");

            indexed.SetValue(1);

            Assert.AreEqual(array[0], indexed.GetValue(), "Calling SetValue() should set the value on the index of the array pointed by the IndexedSubscripter");
        }

        /// <summary>
        /// Tests exception raising of indexed subscripters
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to created an indexed subscripter with an invalid subscript value must raise an ArgumentException")]
        public void TestFailedCreation()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, "abc");
        }

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [TestMethod]
        public void TestAutoSubscripterCreation()
        {
            var array = new ArrayList { 0, 1 };

            // ReSharper disable once UnusedVariable
            var indexed = IndexedSubscripter.CreateSubscripter(array, 10);

            Assert.AreEqual(10, indexed.IndexValue, "The index value on the indexer must be the index value passed at its constructor");
        }

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception), "Trying to created an indexed subscripter with an invalid subscript value must raise an ArgumentException")]
        public void TestFailedAutoSubscripterCreation()
        {
            var array = new ArrayList { 0, 1 };

            // ReSharper disable once UnusedVariable
            var indexed = IndexedSubscripter.CreateSubscripter(array, "abc");
        }

        #endregion

        #region General subscripting

        /// <summary>
        /// Tests subscripting a string
        /// </summary>
        [TestMethod]
        public void TestStringSubscripting()
        {
            const string str = "abc";

            var indexed = IndexedSubscripter.CreateSubscripter(str, 0);

            Assert.AreEqual('a', indexed.GetValue(), "The string subscripter failed to fetch the expected value");
        }

        /// <summary>
        /// Tests subscripting a dictionary
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscripting()
        {
            var dict = new Dictionary<string, object> { {"abc", 10} };

            var indexed = IndexedSubscripter.CreateSubscripter(dict, "abc");

            Assert.AreEqual(10, indexed.GetValue(), "The dictionary subscripter failed to fetch the expected value");

            indexed.SetValue(5);

            Assert.AreEqual(5, indexed.GetValue(), "The dictionary subscripter failed to set the value correctly");
        }

        #endregion

        /// <summary>
        /// Tests exception raising when trying to subscript objects that do not have subscripting support
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception), "Trying to create a subscripter for a type that does not supports subscripting should raise an exception")]
        public void TestNonSubscriptableObject()
        {
            var obj = new object();

            // ReSharper disable once UnusedVariable
            var indexed = IndexedSubscripter.CreateSubscripter(obj, 10);
        }
    }
}