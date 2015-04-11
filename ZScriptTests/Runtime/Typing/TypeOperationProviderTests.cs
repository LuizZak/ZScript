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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;
using ZScript.Runtime.Typing;

namespace ZScriptTests.Runtime.Typing
{
    /// <summary>
    /// Tests the functionality of the TypeOperationProvider class
    /// </summary>
    [TestClass]
    public class TypeOperationProviderTests
    {
        /// <summary>
        /// Tests optional value comparision
        /// </summary>
        [TestMethod]
        public void TestOptionalValue()
        {
            var nullOpt = new Optional<long>();
            var valuedOpt = new Optional<long>(1);

            var provider = new TypeOperationProvider();

            Assert.IsTrue(provider.Equals(nullOpt, null));
            Assert.IsFalse(provider.Equals(valuedOpt, null));

            Assert.IsTrue(provider.Equals(valuedOpt, 1L));
        }

        /// <summary>
        /// Tests optional value comparision
        /// </summary>
        [TestMethod]
        public void TestChainedOptionalValueComparision()
        {
            var nullOpt = new Optional<Optional<long>>(new Optional<long>());
            var valuedOpt = new Optional<Optional<long>>(1);

            var provider = new TypeOperationProvider();

            Assert.IsTrue(provider.Equals(nullOpt, null));
            Assert.IsFalse(provider.Equals(valuedOpt, null));

            Assert.IsTrue(provider.Equals(valuedOpt, 1L));
        }
    }
}