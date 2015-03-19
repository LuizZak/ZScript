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
using System.Collections.Generic;

using Xunit;

using ZScript.Runtime.Execution.Wrappers;
using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScriptTests.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Tests the functionality of the IndexedSubscripter class
    /// </summary>
    public class IndexedSubscripterTests
    {
        #region List subscripting

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [Fact]
        public void TestBasicIndexing()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, 10);

            Assert.Equal(subscripter, indexed.SubscripterWrapper);
            Assert.Equal(10, indexed.IndexValue);
        }

        /// <summary>
        /// Tests usage of the Get/SetValue methods
        /// </summary>
        [Fact]
        public void TestGetSetValues()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            var indexed = new IndexedSubscripter(subscripter, 0);

            Assert.Equal(array[0], indexed.GetValue());

            indexed.SetValue(1);

            Assert.Equal(array[0], indexed.GetValue());
        }

        /// <summary>
        /// Tests exception raising of indexed subscripters
        /// </summary>
        [Fact]
        public void TestFailedCreation()
        {
            var array = new ArrayList { 0, 1 };
            var subscripter = new ListSubscripterWrapper(array);

            Assert.Throws<ArgumentException>(() => new IndexedSubscripter(subscripter, "abc"));
        }

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [Fact]
        public void TestAutoSubscripterCreation()
        {
            var array = new ArrayList { 0, 1 };

            // ReSharper disable once UnusedVariable
            var indexed = IndexedSubscripter.CreateSubscripter(array, 10);

            Assert.Equal(10, indexed.IndexValue);
        }

        /// <summary>
        /// Tests generation of indexed subscripters
        /// </summary>
        [Fact]
        public void TestFailedAutoSubscripterCreation()
        {
            var array = new ArrayList { 0, 1 };

            // ReSharper disable once UnusedVariable
            Assert.Throws<Exception>(() => IndexedSubscripter.CreateSubscripter(array, "abc"));
        }

        #endregion

        #region General subscripting

        /// <summary>
        /// Tests subscripting a string
        /// </summary>
        [Fact]
        public void TestStringSubscripting()
        {
            const string str = "abc";

            var indexed = IndexedSubscripter.CreateSubscripter(str, 0);

            Assert.Equal('a', indexed.GetValue());
        }

        /// <summary>
        /// Tests subscripting a dictionary
        /// </summary>
        [Fact]
        public void TestDictionarySubscripting()
        {
            var dict = new Dictionary<string, object> { {"abc", 10} };
            
            var indexed = IndexedSubscripter.CreateSubscripter(dict, "abc");

            Assert.Equal(10, indexed.GetValue());

            indexed.SetValue(5);

            Assert.Equal(5, indexed.GetValue());
        }

        #endregion

        /// <summary>
        /// Tests exception raising when trying to subscript objects that do not have subscripting support
        /// </summary>
        [Fact]
        public void TestNonSubscriptableObject()
        {
            var obj = new object();

            // ReSharper disable once UnusedVariable
            Assert.Throws<Exception>(() => IndexedSubscripter.CreateSubscripter(obj, 10));
        }
    }
}