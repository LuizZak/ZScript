﻿/*
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
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual((long)0, list[0], "The list did not have the expected values");
            Assert.AreEqual((long)1, list[1], "The list did not have the expected values");
            Assert.AreEqual((long)2, list[2], "The list did not have the expected values");
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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.IsInstanceOfType(list[0], typeof(IList), "The list created by the script is not of the expected type");
            Assert.AreEqual((long)0, ((IList)list[0])[0], "The list did not have the expected values");
            Assert.AreEqual((long)1, list[1], "The list did not have the expected values");
            Assert.AreEqual((long)2, list[2], "The list did not have the expected values");
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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)0, memory.GetVariable("a"), "The list did not have the expected values");
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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, memory.GetVariable("a"), "The list did not have the expected values");
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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual((long)1, list[0], "The list created by the script is not of the expected type");
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

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual((long)2, ((IList)list[0])[1], "The list created by the script is not of the expected type");
        }
    }
}