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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;

namespace ZScriptTests.Elements
{
    /// <summary>
    /// Tests the functionality of the Optional&lt;T&gt; class
    /// </summary>
    [TestClass]
    public class OptionalTests
    {
        /// <summary>
        /// Tests the creation of a few optional objects
        /// </summary>
        [TestMethod]
        public void TestOptionalCreation()
        {
            Optional<long> valuedOpt = 0;
            Optional<long> emptyOpt = Optional<long>.Empty;

            Assert.IsTrue(valuedOpt.HasValue);
            Assert.IsTrue(valuedOpt.HasInnerValue);

            Assert.IsFalse(emptyOpt.HasValue);
            Assert.IsFalse(emptyOpt.HasInnerValue);
        }

        /// <summary>
        /// Tests fetching the value an optional object
        /// </summary>
        [TestMethod]
        public void TestOptionalValue()
        {
            Optional<long> valuedOpt = 1L;

            Assert.AreEqual(1L, valuedOpt.Value);
            Assert.AreEqual(1L, valuedOpt.InnerValue);
        }

        /// <summary>
        /// Tests explicitly fetching the value an optional object
        /// </summary>
        [TestMethod]
        public void TestExplicitOptionalValue()
        {
            Optional<long> valuedOpt = 1L;

            long? value = (long)valuedOpt;
            
            Assert.AreEqual(1L, value);
        }

        /// <summary>
        /// Tests exception raising when fetching the value of an empty optional
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestEmptyOptionalValue()
        {
            Optional<long> emptyOpt = Optional<long>.Empty;

            Assert.AreEqual(0, emptyOpt.Value);
        }

        /// <summary>
        /// Tests exception raising when explicitly fetching the value of an empty optional
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestEmptyExplicitOptionalValue()
        {
            Optional<long> valuedOpt = Optional<long>.Empty;

            long value = (long)valuedOpt;
            Assert.AreEqual(1L, value);
        }
    }
}