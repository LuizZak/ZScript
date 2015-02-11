using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration;

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
            const string input = "func funca { var a = 0; for(var i = 0; i < 1000000; i++) { a += i; } }";

            var generator = CreateGenerator(input);

            generator.ParseInputString();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.Write(sw.ElapsedMilliseconds);
        }

        [TestMethod]
        public void TestSubscriptPerformance()
        {
            const string input = "func funca { var a = [0]; for(var i = 0; i < 1000000; i++) { a[0] += i; } }";

            var generator = CreateGenerator(input);

            generator.ParseInputString();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            // Test the script time
            var sw = Stopwatch.StartNew();

            runtime.CallFunction("funca");

            sw.Stop();

            Console.Write(sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Creates the default generator to use in tests
        /// </summary>
        /// <param name="input">The input string to use in the generator</param>
        /// <returns>A default runtime generator to use in tests</returns>
        public ZRuntimeGenerator CreateGenerator(string input)
        {
            var gen = new ZRuntimeGenerator(input) { Debug = true };
            return gen;
        }
    }
}