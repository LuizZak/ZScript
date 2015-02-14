using System;
using System.Diagnostics;

using ZScript.CodeGeneration;
using ZScriptTests.Performance;
using ZScriptTests.Runtime.Execution;

namespace StandaloneTests
{
    class Program
    {
        static void Main(string[] args)
        {
            FunctionVmTests ftests = new FunctionVmTests();

            ftests.TestManualAddressing();

            Console.Read();

            ftests.TestNamedVariableFetching();

            Console.Read();

            PerformanceTests tests = new PerformanceTests();

            tests.TestFunctionCallPerformance();

            Console.Read();

            tests.TestSubscriptPerformance();

            Console.Read();

            const string input = "func funca { var a = 0; for(var i = 0; i < 100000; i++) { a += i; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var runtime = generator.GenerateRuntime(null);

            // Test the script time
            var sw = Stopwatch.StartNew();
            
            runtime.CallFunction("funca");
            
            sw.Stop();

            Console.Write(sw.ElapsedMilliseconds);
            Console.Read();
        }
    }
}