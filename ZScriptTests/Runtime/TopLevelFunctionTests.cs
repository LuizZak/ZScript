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
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests parsing and execution of top level functions
    /// </summary>
    [TestClass]
    public class TopLevelFunctionTests
    {
        /// <summary>
        /// Tests basic function calling
        /// </summary>
        [TestMethod]
        public void TestBasicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); } func funcb() { __trace(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with multiple parameters
        /// </summary>
        [TestMethod]
        public void TestMultiParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(1, 2); } func funcb(a:int, b:int) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[1], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with default parameters
        /// </summary>
        [TestMethod]
        public void TestDefaultParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(a:int = 0, b:int = 0) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(0L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(0L, owner.TraceObjects[1], "The function call did not occur as expected");

            Assert.AreEqual(1L, owner.TraceObjects[2], "The function call did not occur as expected");
            Assert.AreEqual(0L, owner.TraceObjects[3], "The function call did not occur as expected");

            Assert.AreEqual(1L, owner.TraceObjects[4], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[5], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with variadic parameters
        /// </summary>
        [TestMethod]
        public void TestVariadicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(args...) { __trace(args); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(1L, owner.TraceObjects[1], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[2], "The function call did not occur as expected");
        }
    }
}