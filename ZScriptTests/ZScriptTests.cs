using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;

namespace ZScriptTests
{
    /// <summary>
    /// Tests the functionality of the ZScript class and related components
    /// </summary>
    [TestClass]
    public class ZScriptTests
    {
        [TestMethod]
        public void TestExpressionCodeGeneration()
        {
            const string input = "[ a; ] func f() { a = ((a = c) && b); (a == 10 || c > 10) && (b()); a && false; (a == 10) && (b()); (a == 10 && c > 10) || (b()); a = a && (c && b > 0) && a; (a+(5+5).a)[0]; a.b(1 + 5); a[i].get[1] = 0; a[i].get[0]; a = 5 * (2 + a()) - 5; gameFPS = toFloat(1000 / level.MainEngine.Game.TargetElapsedTime.TotalMilliseconds); a++; a.a++; a[0].a++; a = b++ - 5; a = 0; a = b = c; b += 1; b += (b = 1); a = b + (c = 0) * 1; a = b + (c += 2) * 1; a[i] = 0; a[i][1 + 1] = 0; a[i][1 + 1] += 0; a[i][1 + 1] *= 0; a.get(); a[i].get(); a[i].get()[0] = 0; 5 + a(5,-6); -6; 5 + ((1 + 2) * 4) - 3; a5*(a(10*5 + 7 * ((7+5)*7), 5 + a(5, -6))[0]); elevatorDisplayLights[i].X = 273 + toInt((_floor / 21.0) * 17); enArray[i].AIEnabled = false; }";

            var owner = new TestRuntimeOwner();

            var sw = Stopwatch.StartNew();

            var generator = new ZRuntimeGenerator(input) { Debug = true };
            generator.ParseInputString();

            // Generate the runtime now
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);

            Assert.Fail(sw.ElapsedMilliseconds + "");
        }

        [TestMethod]
        public void TestVirtualMachine()
        {
            const string input = "[ a = 0; ] func f() { a = 10; }";
            var owner = new TestRuntimeOwner();
            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.IsTrue(runtime.GlobalMemory.HasVariable("a"), "The variable 'a' should be globally visible after being set by a function that was called through the Runtime");
            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestLargeCodeParsingSpeed()
        {
            var reader = new StreamReader(@"C:\Users\Luiz Fernando\Desktop\ZHScript test 3.txt");
            var input = reader.ReadToEnd();
            reader.Close();

            var generator = new ZRuntimeGenerator(input);

            var sw = Stopwatch.StartNew();

            generator.ParseInputString();

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
    }
}