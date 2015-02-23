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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScriptTests.Utils;

namespace ZScriptTests.Performance
{
    /// <summary>
    /// Performs performance tests on the scripting engine
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void TestForLoopPerformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 200;

            const string input = "func funca { for(var i = 0; i < 100000; i++) { } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds < threshold, "The performance test failed to meet the threshold of " + threshold + "ms.");
        }

        [TestMethod]
        public void TestAssignPerformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 350;

            const string input = "func funca { var a = 0; for(var i = 0; i < 100000; i++) { a += i; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds < threshold, "The performance test failed to meet the threshold of " + threshold  + "ms.");
        }

        [TestMethod]
        public void TestSubscriptPerformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 400;

            const string input = "func funca { var a = [0]; for(var i = 0; i < 100000; i++) { a[0] += i; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds < threshold, "The performance test failed to meet the threshold of " + threshold + "ms.");
        }

        [TestMethod]
        public void TestFunctionCallPerformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 400;

            const string input = "func funca { for(var i = 0; i < 100000; i++) { funcb(); } } func funcb { }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds < threshold, "The performance test failed to meet the threshold of " + threshold + "ms.");
        }

        [TestMethod]
        public void TestObjectFunctionCallPerformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 400;

            const string input = "func funca { for(var i = 0; i < 100000; i++) { 'a'.ToString(); } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds < threshold, "The performance test failed to meet the threshold of " + threshold + "ms.");
        }
    }
}