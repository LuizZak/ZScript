using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScriptTests.Runtime;

namespace ZScriptTests.Performance
{
    /// <summary>
    /// Performs performance tests on the scripting engine
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void TestForLoopPeformance()
        {
            // The threshold for the test in milliseconds, based on previous runs.
            // This value is obviously dependent on the system, and I use it mostly to test in my local machine
            const long threshold = 300;

            const string input = "func funca { var a = 0; for(var i = 0; i < 100000; i++) { a += i; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

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
            const long threshold = 370;

            const string input = "func funca { var a = [0]; for(var i = 0; i < 100000; i++) { a[0] += i; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

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