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

using Xunit;

using ZScript.Elements;

namespace ZScriptTests.Elements
{
    /// <summary>
    /// Tests the functionality of the ZObject class
    /// </summary>
    public class ZObjectTests
    {
        /// <summary>
        /// Tests the CanSubscriptWithType implementation
        /// </summary>
        [Fact]
        public void TestCanSubscriptWithType()
        {
            var obj = new ZObject();

            Assert.True(obj.CanSubscriptWithIndexType(typeof(string)));
            Assert.False(obj.CanSubscriptWithIndexType(typeof(void)));
            Assert.False(obj.CanSubscriptWithIndexType(typeof(int)));
        }

        /// <summary>
        /// Tests subscription with ZObjects
        /// </summary>
        [Fact]
        public void TestSubcript()
        {
            var obj = new ZObject();

            obj["a"] = 10;
            obj["b"] = "bcd";

            Assert.Equal(10, obj["a"]);
            Assert.Equal("bcd", obj["b"]);
            Assert.Equal(null, obj["c"]);
        }

        /// <summary>
        /// Tests subscription with non string objects on ZObjects
        /// </summary>
        [Fact]
        public void TestSubcriptWithNonString()
        {
            var obj = new ZObject();

            Assert.Throws<ArgumentException>(() => obj[10] = 10);
        }

        /// <summary>
        /// Tests subscription with null objects on ZObjects
        /// </summary>
        [Fact]
        public void TestSubcriptWithNullException()
        {
            var obj = new ZObject();

            Assert.Throws<ArgumentNullException>(() => obj[null] = 10);
        }
    }
}