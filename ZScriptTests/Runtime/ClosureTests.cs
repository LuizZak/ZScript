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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Elements;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests closure parsing and execution functionality
    /// </summary>
    [TestClass]
    public class ClosureTests
    {
        #region Parsing tests

        [TestMethod]
        public void TestParseClosure()
        {
            const string input = "func f() { var c = (i) : int => { return 0; }; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZClosureFunctionDefinitions.Any(f => f.Name == ClosureDefinition.ClosureNamePrefix + 0));
        }

        [TestMethod]
        public void TestParseSingleParameteredClosure()
        {
            const string input = "func f() { var c = (i) : int => { return 0; }; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZClosureFunctionDefinitions.Any(f => f.Name == ClosureDefinition.ClosureNamePrefix + 0));
        }

        [TestMethod]
        public void TestParseClosureCall()
        {
            const string input = "func f() { var c = (i) : int => { return 0; }(10); }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZClosureFunctionDefinitions.Any(f => f.Name == ClosureDefinition.ClosureNamePrefix + 0));
        }

        [TestMethod]
        public void TestParseGlobalVariableClosure()
        {
            const string input = "var a = (i) : int => { return 0; };";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZClosureFunctionDefinitions.Any(f => f.Name == ClosureDefinition.ClosureNamePrefix + 0));
        }

        [TestMethod]
        public void TestParseClosureCallSubscript()
        {
            const string input = "func f() { var c = (i) : int => { return 0; }(10)[0]; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZClosureFunctionDefinitions.Any(f => f.Name == ClosureDefinition.ClosureNamePrefix + 0));
        }

        #endregion
        
        #region Execution tests

        /// <summary>
        /// Tests parsing a closure and calling it in the script
        /// </summary>
        [TestMethod]
        public void TestClosureCall()
        {
            const string input = "@__trace(a...) func funca(){ var a = () => { __trace(0); }; a(); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)0, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests parsing a closure and calling it in the script
        /// </summary>
        [TestMethod]
        public void TestSingleParameteredClosureCall()
        {
            const string input = "@__trace(a...) func funca(){ var a = i => { __trace(i); }; a(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests parsing closures via returns and calling them
        /// </summary>
        [TestMethod]
        public void TestReturnClosureCall()
        {
            const string input = "@__trace(a...) func funca(){ var a = (i:int):(->) => { if(i == 0) return () => { __trace(0); }; else return () => { __trace(1); }; }; a(0)(); a(1)(); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)0, owner.TraceObjects[0]);
            Assert.AreEqual((long)1, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests parsing a closure that contains parameters and calling it in the script
        /// </summary>
        [TestMethod]
        public void TestParameterClosureCall()
        {
            const string input = "@__trace(a...) func funca(){ var a = (i:int) => { __trace(i); }; a(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests calling closures that are returned by function calls
        /// </summary>
        [TestMethod]
        public void TestReturnCall()
        {
            const string input = "@__trace(a...) func funca(){ funcb()(); } func funcb():(->){ return () => { __trace(0); }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)0, owner.TraceObjects[0]);
        }
        
        /// <summary>
        /// Tests closure variable capturing
        /// </summary>
        [TestMethod]
        public void TestClosureVariableCapturing()
        {
            const string input = "@__trace(a...) func funca(){ var b = 10; var a = () => { __trace(b); b = 11; }; a(); __trace(b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)10, owner.TraceObjects[0]);
            Assert.AreEqual((long)11, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests closure variable capturing in another terminated function scope
        /// </summary>
        [TestMethod]
        public void TestReturnedClosureVariableCapturing()
        {
            const string input = "@__trace(a...) func funca() { var a = funcb(); a(); } func funcb() : (->) { var b = 10; return () => { __trace(b); }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)10, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests closure variable capturing in another terminated function scope, in which the closure captures also a parameter
        /// </summary>
        [TestMethod]
        public void TestParameterReturnedClosureVariableCapturing()
        {
            const string input = "@__trace(a...) func funca() { var a = funcb(1); var b = funcb(2); __trace(a()); __trace(b()); } func funcb(i:int) : (->) { return () : int => { return i + 5; }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)6, owner.TraceObjects[0]);
            Assert.AreEqual((long)7, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests nesting closures with side-effects
        /// </summary>
        [TestMethod]
        public void TestSimpleClosureInClosure()
        {
            const string input = "@__trace(a...) func funca() { var a = (i):(int->int) => { var b = i; return (_i:int):int => { return b + _i; }; }; __trace(a(0)(1)); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests nesting closures with side-effects
        /// </summary>
        [TestMethod]
        public void TestClosureInClosure()
        {
            const string input = "@__trace(a...) func funca() { var a = (i:int):(int->int) => { var b = i; return (_i:int):int => { return b + _i; }; }; __trace(a(0)(1)); __trace(a(1)(1)); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, owner.TraceObjects[0]);
            Assert.AreEqual((long)2, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests nesting closures with side-effects
        /// </summary>
        [TestMethod]
        public void TestLocalClosureInStoredClosure()
        {
            const string input = "@__trace(a...) func funca() { var a = (i:int):(int->int) => { var b = i; return (_i:int):int => { return b + _i; }; }; var c = a(1); __trace(c(2)); __trace(c(3)); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)3, owner.TraceObjects[0]);
            Assert.AreEqual((long)4, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests nesting closures with side-effects
        /// </summary>
        [TestMethod]
        public void TestLocalClosureInStoredClosureParameterReference()
        {
            const string input = "@__trace(a...) func funca() { var a = (i:int):(int->int) => { return (_i:int):int => { return i + _i; }; }; var c = a(1); __trace(c(2)); __trace(c(3)); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)3, owner.TraceObjects[0]);
            Assert.AreEqual((long)4, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests an enumeration effect using closures and enclosed scopes
        /// </summary>
        [TestMethod]
        public void TestClosureEnumeration()
        {
            const string input = "@__trace(a...) func funca() { var g1 = generator(1); var g2 = generator(3); __trace(g1());  __trace(g2()); __trace(g1()); __trace(g2()); __trace(g1()); __trace(g2()); } func generator(start:int) : (->int) { return ():int => { return start++; }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)1, owner.TraceObjects[0]);
            Assert.AreEqual((long)3, owner.TraceObjects[1]);
            Assert.AreEqual((long)2, owner.TraceObjects[2]);
            Assert.AreEqual((long)4, owner.TraceObjects[3]);
            Assert.AreEqual((long)3, owner.TraceObjects[4]);
            Assert.AreEqual((long)5, owner.TraceObjects[5]);
        }

        /// <summary>
        /// Tests utilizing a subscript after a closure call
        /// </summary>
        [TestMethod]
        public void TestClosureCallSubscriptExecution()
        {
            const string input = "@__trace(a...) func funca() { __trace((i:int) : [int] => { return [i]; }(1)[0]); }";
            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests an enumeration effect using closures and enclosed scopes
        /// </summary>
        [TestMethod]
        public void TestClosureMemoryOverhead()
        {
            const string input = "@__trace(a...) func funca() { for(var i = 0; i < 10000; i++) { var g1 = generator(1); g1(); } } func generator(start:int) : (->int) { return ():int => { return start++; }; }";
            const long expectedOverhead = 300 * 1024; // This expected overhead is completely arbitrary, based on previous runs, and is not calculated in any deterministic way

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            var sw = Stopwatch.StartNew();

            long totalBefore = GC.GetTotalMemory(true);

            runtime.CallFunction("funca");

            long totalAfter = GC.GetTotalMemory(true);
            long difference = totalAfter - totalBefore;

            Console.WriteLine("Time: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine("Detected overhead: " + FormatByteSize(difference));

            Assert.IsTrue(difference < expectedOverhead, "The memory overhead after using the script is above the threshold value (difference: " + FormatByteSize(difference) + ")");
        }

        /// <summary>
        /// Tests behavior of closures created inside class instance environments
        /// </summary>
        [TestMethod]
        public void TestClassInstanceClosure()
        {
            const string input = "@__trace(a...) func funca(){ var a = o(); var b = a.closure(); b(10); __trace(a.c); } object o { var c = 0; func closure() : (int->) { return (i:int) => { c = i; }; } }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)10, owner.TraceObjects[0]);
        }

        #endregion
        
        #region Helper Methods

        /// <summary>
        /// Returns a formated sting that contains the most significant magnitude
        /// representation of the given number of bytes
        /// </summary>
        /// <param name="bytes">The number of bytes</param>
        /// <returns>A formated string with the byte count converted to the most significant magnitude</returns>
        public static string FormatByteSize(long bytes)
        {
            int magnitude = 0;
            string[] sulfixes = { "b", "kb", "mb", "gb", "tb", "pt", "eb", "zb", "yb" };

            float b = bytes;

            while (b > 1024)
            {
                b /= 1024;
                magnitude++;
            }

            if (magnitude >= sulfixes.Length)
            {
                magnitude = sulfixes.Length - 1;
            }

            return Math.Round(b * 1024) / 1024 + sulfixes[magnitude];
        }

        #endregion
    }
}