using System;
using System.Diagnostics;

using ZScript.CodeGeneration;
using ZScriptTests.Performance;
using ZScriptTests.Runtime.Execution;

namespace StandaloneTests
{
    class Program
    {
        unsafe public static void swap(int* a, int* b)
        {
            int c = *a;
            *a = *b;
            *b = c;
        }

        unsafe static void TestMove()
        {
            Stopwatch sw = new Stopwatch();

            // 
            const int iter = 100000000;
            Console.WriteLine(iter + " iterations");
            Console.WriteLine("===");
            Console.WriteLine("swap()");

            sw.Restart();
            for (int i = 0; i < iter; i++)
            {
                int a = 3;
                int b = 4;

                swap(&a, &b);
            }
            sw.Stop();

            Console.WriteLine("Time: " + ((float)sw.ElapsedTicks / Stopwatch.Frequency * 1000) + "ms\n");

            Console.Read();
        }

        static void TestThing()
        {
            Stopwatch sw = new Stopwatch();

            // 
            const int iter = 1000000;
            Console.WriteLine(iter + " iterations");
            Console.WriteLine("===");
            Console.WriteLine("object[] implementation:");

            sw.Restart();
            for (int i = 0; i < iter; i++)
            {
                TestObjectArray();
            }
            sw.Stop();

            Console.WriteLine("Time: " + ((float)sw.ElapsedTicks / Stopwatch.Frequency * 1000) + "ms\n");
            Console.WriteLine("===");
            Console.WriteLine("Pointer implementation:");

            sw.Restart();
            for (int i = 0; i < iter; i++)
            {
                TestPointer();
            }
            sw.Stop();

            Console.WriteLine("Time: " + ((float)sw.ElapsedTicks / Stopwatch.Frequency * 1000) + "ms\n\n");

            Console.WriteLine("Press any key to exit.");

            Console.Read();
        }

        static void TestObjectArray()
        {
            object[] objects = new object[6];

            // Fill in the values
            objects[0] = 10;
            objects[1] = 1.0f;
            objects[2] = (byte)5;

            // Do a simple juggling of values
            objects[3] = (int)objects[0];
            objects[4] = (float)objects[1];
            objects[5] = (byte)objects[2];

            //Assert.AreEqual(objects[0], objects[3]);
            //Assert.AreEqual(objects[1], objects[4]);
            //Assert.AreEqual(objects[2], objects[5]);
        }

        unsafe static void TestPointer()
        {
            const int bufferSize = sizeof(int) * 2 + sizeof(float) * 2 + sizeof(byte) * 2;

            const int i1Offset = 0;
            const int i2Offset = sizeof(int) + sizeof(float) + sizeof(byte);

            const int f1Offset = sizeof(int);
            const int f2Offset = sizeof(int) + sizeof(float) + sizeof(byte) + sizeof(int);

            const int b1Offset = sizeof(int) + sizeof(float);
            const int b2Offset = sizeof(int) + sizeof(float) + sizeof(byte) + sizeof(int) + sizeof(float);

            // Create the buffer
            byte* buffer = stackalloc byte[bufferSize];
            
            // Fill in the buffer
            *(int*)(buffer + i1Offset) = 10;
            *(float*)(buffer + f1Offset) = 1.0f;
            *(buffer + b1Offset) = 5;

            // Move the items around
            *(int*)(buffer + i2Offset) = *(int*)(buffer + i1Offset);
            *(float*)(buffer + f2Offset) = *(float*)(buffer + f1Offset);
            *(buffer + b2Offset) = *(buffer + b1Offset);

            //Assert.AreEqual(*(int*)(buffer + i2Offset), *(int*)(buffer + i1Offset));
            //Assert.AreEqual(*(float*)(buffer + f2Offset), *(float*)(buffer + f1Offset));
            //Assert.AreEqual(*(buffer + b2Offset), *(buffer + b1Offset));
        }

        static void Main(string[] args)
        {
            //TestThing();
            
            //TestMove();

            PerformanceTests tests = new PerformanceTests();

            tests.TestObjectFunctionCallPerformance();

            Console.Read();

            return;

            FunctionVmTests ftests = new FunctionVmTests();

            ftests.TestManualAddressing();

            Console.Read();

            ftests.TestNamedVariableFetching();

            Console.Read();

            

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