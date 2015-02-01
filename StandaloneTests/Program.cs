﻿using System;
using System.Diagnostics;
using ZScript.CodeGeneration;

namespace StandaloneTests
{
    class Program
    {
        static void Main(string[] args)
        {
            const string input = "func funca { var a = 0; for(var i = 0; i < 100000; i++) { a += i; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

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