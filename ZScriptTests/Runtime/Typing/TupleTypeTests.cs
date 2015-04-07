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
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.Runtime.Typing
{
    /// <summary>
    /// Tests tuple types in the type system
    /// </summary>
    [TestClass]
    public class TupleTypeTests
    {
        /// <summary>
        /// Tests optional type creating
        /// </summary>
        [TestMethod]
        public void TestOptionalCreation()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.IntegerType());
            var tuple2 = provider.TupleForTypes(new [] { "x", "y" }, new [] { provider.IntegerType(), provider.IntegerType() });

            Assert.AreEqual(tuple1, tuple2);
        }
    }
}