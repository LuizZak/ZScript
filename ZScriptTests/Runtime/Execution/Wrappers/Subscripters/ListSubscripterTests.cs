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

using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScriptTests.Runtime.Execution.Wrappers.Subscripters
{
    /// <summary>
    /// Tests the functionality of the ListSubscripter class
    /// </summary>
    public class ListSubscripterTests
    {
        /// <summary>
        /// Tests the creation and valid usage of a ListSubscripter object
        /// </summary>
        [Fact]
        public void TestSubscription()
        {
            var array = new ArrayList { 0, 0, 2, 0 };
            var subscripter = new ListSubscripterWrapper(array);

            Assert.Equal(subscripter.List, array);

            subscripter[1] = 1;
            subscripter[3L] = 3;

            Assert.Equal(0, subscripter[0]);
            Assert.Equal(1, subscripter[1L]);
            Assert.Equal(2, subscripter[2]);
            Assert.Equal(3, subscripter[3L]);
        }

        /// <summary>
        /// Tests the functionality for the type checking of the subscripted array
        /// </summary>
        [Fact]
        public void TestSubscriptionTypeChecking()
        {
            var array = new ArrayList();
            var subscripter = new ListSubscripterWrapper(array);

            Assert.True(subscripter.CanSubscriptWithIndexType(typeof(int)),
                "The ListSubscripter must return true when calling CanSubscriptWithIndexType with Int32 types");
            Assert.True(subscripter.CanSubscriptWithIndexType(typeof(long)),
                "The ListSubscripter must return true when calling CanSubscriptWithIndexType with Int64 types");
            Assert.False(subscripter.CanSubscriptWithIndexType(typeof(string)),
                "The ListSubscripter must return false when calling CanSubscriptWithIndexType with any type other than Int32 and Int64");
        }

        /// <summary>
        /// Tests the usage of the ListSubscripter with a generic list of items
        /// </summary>
        [Fact]
        public void TestGenericSubscription()
        {
            var array = new List<int> { 0, 0, 2, 0 };
            var subscripter = new ListSubscripterWrapper(array);

            subscripter[1] = 1;
            subscripter[3L] = 3;

            Assert.Equal(0, subscripter[0]);
            Assert.Equal(1, subscripter[1L]);
            Assert.Equal(2, subscripter[2]);
            Assert.Equal(3, subscripter[3L]);
        }

        /// <summary>
        /// Tests the incorrect usage of a ListSubscripter object by providing an invalid index type when getting a value
        /// </summary>
        [Fact]
        public void TestInvalidSubscritionGet()
        {
            var array = new ArrayList { 0, 1, 2, 3 };
            var subscripter = new ListSubscripterWrapper(array);

            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentException>(() => subscripter["invalidIndex"]);
        }

        /// <summary>
        /// Tests the incorrect usage of a ListSubscripter object by providing an invalid index type when setting a value
        /// </summary>
        [Fact]
        public void TestInvalidSubscrition()
        {
            var array = new ArrayList { 0, 1, 2, 3 };
            var subscripter = new ListSubscripterWrapper(array);

            Assert.Throws<ArgumentException>(() => subscripter["invalidIndex"] = 10);
        }
    }
}