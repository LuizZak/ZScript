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

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the functionality of the ZRuntime class and related components
    /// </summary>
    [TestClass]
    public class ZRuntimeTests
    {
        #region Logical operator short circuiting

        [TestMethod]
        public void TestLogicalAndShortCircuiting()
        {
            // We define 'a' and 'c', but ommit 'b' and 'd', and expect the short circuiting to avoid reaching the parts that access these values
            const string input = "var a = true; var c = false; var b:int = null; var d:bool = null; func f() { a = a && (c && b > 0) && d; }";

            var generator = Utils.TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(false, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestLogicalOrShortCircuiting()
        {
            // We define 'a' and 'c', but ommit 'b' and 'd', and expect the short circuiting to avoid reaching the parts that access these values
            const string input = "var a = true; var c = true; var b:bool = null; var d:bool = null; func f() { a = c || (b || d); }";

            var generator = Utils.TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(true, runtime.GlobalMemory.GetVariable("a"));
        }

        #endregion

        /// <summary>
        /// Tests calling a function that expects parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredFunctionCall()
        {
            const string input = "func f1(a:int) : int { return a; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 20);

            Assert.AreEqual((long)10, ret1, "The function did not return the expected value");
            Assert.AreEqual((long)20, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function that expects parameters and has default values for one or more of the parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredDefaultValueFunctionCall()
        {
            const string input = "func f1(a:int, b:int = 0) : int { return a + b; }";
            var generator = Utils.TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 10, 1);

            Assert.AreEqual((long)10, ret1, "The function did not return the expected value");
            Assert.AreEqual((long)11, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestInnerFunctionCall()
        {
            const string input = "func f1() : int { return f2(); } func f2() : int { return 10; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.Debug = true;

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual((long)10, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests calling a function with a parameter from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestParameteredInnerFunctionCall()
        {
            const string input = "func f1() : int { return f2(5); } func f2(a:int) : int { return a; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.Debug = true;

            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual((long)5, ret, "The statement did not execute as expected");
        }
        /// <summary>
        /// Tests calling a function recursively
        /// </summary>
        [TestMethod]
        public void TestRecursiveFunctionCall()
        {
            const string input = "func f1(a:int) : int { if(a >= 5) { return 0; } return f1(a + 1) + 1; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1", 0);

            Assert.AreEqual((long)5, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests the creation of global variables
        /// </summary>
        [TestMethod]
        public void TestGlobalVariables()
        {
            const string input = "var a = 0; var b = null;";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Expands the global variables
            runtime.ExpandGlobalVariables();

            Assert.AreEqual((long)0, memory.GetVariable("a"), "The global variables where not parsed as expected");
            Assert.AreEqual(null, memory.GetVariable("b"), "The global variables where not parsed as expected");
        }
    }
}