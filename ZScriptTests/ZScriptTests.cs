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
using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScriptTests.Utils;

namespace ZScriptTests
{
    /// <summary>
    /// Tests the functionality of the ZScript class and related components
    /// </summary>
    [TestClass]
    public class ZScriptTests
    {
        // These are some of the first dummy tests created, used mostly as integration tests.
        // These are kept here mostly because of th- actually no reason whatsoever, but I keep them in anyway

        [TestMethod]
        public void TestVirtualMachine()
        {
            const string input = "var a = 0; func f() { a = 10; }";
            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.IsTrue(runtime.GlobalMemory.HasVariable("a"), "The variable 'a' should be globally visible after being set by a function that was called through the Runtime");
            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
        }

        // Perhaps the only really useful test here, tests parsing of a large script containing all the language constructs available to test.
        [TestMethod]
        public void TestLargeCodeParsingSpeed()
        {
            var reader = new StreamReader(@"ZScript sample.zs");
            var input = reader.ReadToEnd();
            reader.Close();

            var generator = TestUtils.CreateGenerator(input);

            var sw = Stopwatch.StartNew();

            generator.ParseSources();

            Console.WriteLine("Parsing time:    " + sw.ElapsedMilliseconds);

            long parseT = sw.ElapsedMilliseconds;

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);

            sw.Start();

            Console.WriteLine("Generation time: " + (sw.ElapsedMilliseconds - parseT));
            Console.WriteLine("Total time:      " + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Tests basic syntax error detection
        /// </summary>
        [TestMethod]
        public void TestSyntaxError()
        {
            const string input = "func fib(n:int) : int { ";

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.AreEqual(1, generator.MessageContainer.SyntaxErrors.Length);
        }

        /// <summary>
        /// Tests a recursive Fibonacci implementation in ZScript
        /// </summary>
        [TestMethod]
        public void TestRecursiveFibonacci()
        {
            const string input = "func fib(n:int) : int { if(n <= 0) return 0; if(n == 1) return 1; return fib(n - 1) + fib(n - 2); }";
            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);

            var sw = Stopwatch.StartNew();

            // Get the return value for the 12th fibonacci number
            var ret = runtime.CallFunction("fib", 12);

            // Print the time it took, because why not
            sw.Stop();
            Console.WriteLine("Runtime: " + sw.ElapsedMilliseconds + "ms");

            Assert.AreEqual(144L, ret, "The runtime failed to derive the expected 12th fibonacci number");
        }

        /// <summary>
        /// Tests an iterative Fibonacci implementation in ZScript
        /// </summary>
        [TestMethod]
        public void TestIterativeFibonacci()
        {
            const string input = "func fib(n:int) : int {" +
                                 "  if (n == 0) return 0;" +
                                 "  if (n == 1) return 1;" +
                                 "  var prevPrev = 0;" +
                                 "  var prev = 1;" +
                                 "  var result = 0;" +
                                 "  for (var i = 2; i <= n; i++)" +
                                 "  {" +
                                 "    result = prev + prevPrev;" +
                                 "    prevPrev = prev;" +
                                 "    prev = result;" +
                                 "  }" +
                                 "  return result;" +
                                 "}";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);

            var sw = Stopwatch.StartNew();

            // Get the return value for the 12th fibonacci number
            var ret = runtime.CallFunction("fib", 12);

            // Print the time it took, because why not
            sw.Stop();
            Console.WriteLine("Runtime: " + sw.ElapsedMilliseconds + "ms");

            Assert.AreEqual(144L, ret, "The runtime failed to derive the expected 12th fibonacci number");
        }

        /// <summary>
        /// Tests optimization by removal of constant-valued if expressions
        /// </summary>
        [TestMethod]
        public void TestConstantIfExpanding()
        {
            const string input = "func a() { if(true) { } else if(false) { } else { } }";

            var generator = TestUtils.CreateGenerator(input);
            var definition = generator.GenerateRuntimeDefinition();

            Assert.AreEqual(0, definition.ZFunctionDefinitions[0].Tokens.Tokens.Length, "The constant, empty if statements inside the function 'a' where expected to be optimized out completely");
        }

        /// <summary>
        /// Tests implict casting of an array literal's contents
        /// </summary>
        [TestMethod]
        public void TestArrayLiteralCastong()
        {
            const string input = "func f1() { var a:[float] = [0]; }";

            var generator = TestUtils.CreateGenerator(input);
            generator.GenerateRuntimeDefinition();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }
    }
}