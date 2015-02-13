using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScriptTests.Runtime;

namespace ZScriptTests.CodeGeneration.Tokenizers
{
    [TestClass]
    public class StatementTokenizerTests
    {
        #region IF statement generation and execution

        [TestMethod]
        public void TestIfCodeGeneration()
        {
            const string input = "func f() { var a = 0; if(a == 10) { a = 20; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseCodeGeneration()
        {
            const string input = "func f() { var a = 0; var b = 0; if(a == 10) { a = 20; } else { b = 5; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseIfCodeGeneration()
        {
            const string input = "[ a = 0; b = 0; c = 0; ] func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(0, memory.GetVariable("a"));
            Assert.AreEqual(0, memory.GetVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestNestedIfCodeGeneration()
        {
            const string input = "[ a = 0; b = 0; c = 0; ] func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual(0, memory.GetVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestIfExpressionShortCircuit()
        {
            const string input = "[ a = 0; b = 0; c = 0; d = null; ] func f() { if(5 == 10 && d > 0) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual(0, memory.GetVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        #endregion

        #region FOR statement generation and execution

        [TestMethod]
        public void TestForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNoInitForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(; i < 10; i++) { a = i + 2; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNoConditionForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; ; i++) { a = i + 2;  if(i == 9) { i++; break; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForBreakStatement()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; break; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(0, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(2, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForContinueStatement()
        {
            const string input = "[ i = 0; a = 0; b = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; continue; b = a; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedForStatement()
        {
            const string input = "[ i = 0; a = 0; b = 0; ] func f() { for(i = 0; i < 10; i++) { for(var j = 0; j < 10; j++) { a = i + j + 2; continue; b = a; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(20, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region WHILE statement generation and execution

        [TestMethod]
        public void TestWhileCodeGeneration()
        {
            const string input = "[ a = 0; ] func f() { while(a < 10) { a++; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestWhileContinueStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; continue; b = 10; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestWhileBreakStatement()
        {
            const string input = "[ a = 0; ] func f() { while(true) { a++; if(a >= 10) { break; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNestedWhileStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; while(b / a < 10) { b++; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(100, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedWhileBreakStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; while(b / a < 10) { b++; if(b > 6) { break; } } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(16, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region RETURN statement generation and execution

        [TestMethod]
        public void TestReturnStatement()
        {
            const string input = "func f() { return 10+10; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.AreEqual(20, ret, "The valued return statement did not execute as expected");
        }

        [TestMethod]
        public void TestValuelessReturnStatement()
        {
            const string input = "func f() { return; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.IsNull(ret, "The valueless return statement did not execute as expected");
        }

        #endregion

        #region SWITCH statement generation and execution

        [TestMethod]
        public void TestSwitchStatement()
        {
            const string input = "[ a = 5; b = 20; c = null; ] func f() { switch(a + b) { case 25: c = 10; break; case 30: c = 5; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestDefaultSwitchStatement()
        {
            const string input = "[ c = null; d = null; ] func f() { var a = 5; var b = 20; switch(a + b) { case 10: c = 5; break; default: c = 10; break; } switch(a + b) { case 25: d = 10; break; default: d = 5; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("c"), "The statement did not execute as expected");
            Assert.AreEqual(10, memory.GetVariable("d"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestMultipleCasesSwitchStatement()
        {
            const string input = "[ a = 5; b = 20; c; ] func f() { switch(a + b) { case 10: case 25: case 30: c = 10; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        #endregion
    }
}