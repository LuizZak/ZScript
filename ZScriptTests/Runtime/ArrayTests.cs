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
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of array code
    /// </summary>
    [TestClass]
    public class ArrayTests
    {
        /// <summary>
        /// Tests parsing and execution of array literal creation
        /// </summary>
        [TestMethod]
        public void TestArrayLiteral()
        {
            const string input = "var a; func funca(){ a = [0, 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList<long>), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(0L, list[0], "The list did not have the expected values");
            Assert.AreEqual(1L, list[1], "The list did not have the expected values");
            Assert.AreEqual(2L, list[2], "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of array literal intialization
        /// </summary>
        [TestMethod]
        public void TestArrayLiteralInit()
        {
            const string input = "var a; func funca(){ a = [int](); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList<long>), "The list created by the script is not of the expected type");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation
        /// </summary>
        [TestMethod]
        public void TestNestedArrayLiteral()
        {
            const string input = "var a; func funca(){ a = [[0], 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.IsInstanceOfType(list[0], typeof(IList), "The list created by the script is not of the expected type");
            Assert.AreEqual(0L, ((IList)list[0])[0], "The list did not have the expected values");
            Assert.AreEqual(1L, list[1], "The list did not have the expected values");
            Assert.AreEqual(2L, list[2], "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of array literal creation and access
        /// </summary>
        [TestMethod]
        public void TestArrayLiteralAccess()
        {
            const string input = "var a = 0; func funca(){ a = [0, 1, 2][0]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(0L, memory.GetVariable("a"), "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation and access
        /// </summary>
        [TestMethod]
        public void TestNestedArrayLiteralAccess()
        {
            const string input = "var a = 0; func funca(){ a = [[0, 1], 1, 2][0][1]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, memory.GetVariable("a"), "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of array literal value set
        /// </summary>
        [TestMethod]
        public void TestSetArrayIndex()
        {
            const string input = "var a; func funca(){ a = [0, 1, 2]; a[0] = 1; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(1L, list[0], "The list created by the script is not of the expected type");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal value set
        /// </summary>
        [TestMethod]
        public void TestSetNestedArrayIndex()
        {
            const string input = "var a; func funca(){ a = [[0, 1], 1, 2]; a[0][1] = 2; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(2L, ((IList)list[0])[1], "The list created by the script is not of the expected type");
        }
    }
}