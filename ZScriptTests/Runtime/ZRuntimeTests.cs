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
using Rhino.Mocks;
using ZScript.Elements;
using ZScript.Runtime;
using ZScriptTests.Utils;

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
            const string input = "var a = true; var c = false; var b:int = 0; var d:int = 0; func f() { a = a && (c && bSet()) && dSet(); } func bSet() : bool { b = 10; return true; } func dSet() : bool { d = 10; return true; }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(false, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("b"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("d"));
        }

        [TestMethod]
        public void TestLogicalOrShortCircuiting()
        {
            // We define 'a' and 'c', but ommit 'b' and 'd', and expect the short circuiting to avoid reaching the parts that access these values
            const string input = "var a = true; var c = true; var b:int = 0; var d:int = 0; func f() { a = c || (bSet() || dSet()); } func bSet() : bool { b = 10; return true; } func dSet() : bool { d = 10; return true; }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(true, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("b"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("d"));
        }

        #endregion

        /// <summary>
        /// Tests calling an export function not recognized by a runtime owner
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UnrecognizedExportFunctionException),
            "Trying to execute an unrecognizeable export function must raise an UnrecognizedExportFunctionException exception"
            )]
        public void TestUnrecognizedExportfunction()
        {
            const string input = "@invalid func f1() { invalid(); }";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = MockRepository.Mock<IRuntimeOwner>();
            owner.Stub(x => x.RespondsToFunction(Arg<ZExportFunction>.Is.Anything)).Return(false);

            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f1");
        }

        /// <summary>
        /// Tests calling a function that expects parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredFunctionCall()
        {
            const string input = "func f1(a:int) : int { return a; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 20);

            Assert.AreEqual(10L, ret1, "The function did not return the expected value");
            Assert.AreEqual(20L, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function that expects parameters and has default values for one or more of the parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredDefaultValueFunctionCall()
        {
            const string input = "func f1(a:int, b:int = 0) : int { return a + b; }";
            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 10, 1);

            Assert.AreEqual(10L, ret1, "The function did not return the expected value");
            Assert.AreEqual(11L, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestInnerFunctionCall()
        {
            const string input = "func f1() : int { return f2(); } func f2() : int { return 10; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual(10L, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests calling a function with a parameter from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestParameteredInnerFunctionCall()
        {
            const string input = "func f1() : int { return f2(5); } func f2(a:int) : int { return a; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.Debug = true;

            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual(5L, ret, "The statement did not execute as expected");
        }
        /// <summary>
        /// Tests calling a function recursively
        /// </summary>
        [TestMethod]
        public void TestRecursiveFunctionCall()
        {
            const string input = "func f1(a:int) : int { if(a >= 5) { return 0; } return f1(a + 1) + 1; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1", 0);

            Assert.AreEqual(5L, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests the creation of global variables
        /// </summary>
        [TestMethod]
        public void TestGlobalVariables()
        {
            const string input = "var a = 0; var b:any? = null;";
            var generator = TestUtils.CreateGenerator(input);
            generator.Debug = true;
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Expands the global variables
            runtime.ExpandGlobalVariables();
            // Expand again to test resillience against multiple calls
            runtime.ExpandGlobalVariables();

            Assert.AreEqual(0L, memory.GetVariable("a"), "The global variables where not parsed as expected");
            Assert.AreEqual(null, memory.GetVariable("b"), "The global variables where not parsed as expected");
        }
    }
}