using System;
using System.Collections;

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
        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [TestMethod]
        public void TestBasicIndexing()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripter(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, 10);

            Assert.AreEqual(subscripter, indexed.Subscripter, "The subscripter on the indexer must be the subscripter passed at its constructor");
            Assert.AreEqual(10, indexed.IndexValue, "The index value on the indexer must be the index value passed at its constructor");
        }

        /// <summary>
        /// Tests usage of the Get/SetValue methods
        /// </summary>
        [TestMethod]
        public void TestGetSetValues()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripter(array);

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
            var subscripter = new ListSubscripter(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, "abc");
        }
    }
}