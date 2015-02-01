using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration;

namespace ZScriptTests.CodeGeneration.Tokenizers
{
    [TestClass]
    public class StatementTokenizerTests
    {
        #region IF statement generation and execution

        [TestMethod]
        public void TestIfCodeGeneration()
        {
            const string input = "func f() { if(a == 10) { a = 20; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseCodeGeneration()
        {
            const string input = "func f() { if(a == 10) { a = 20; } else { b = 5; } }";

            var generator = new ZRuntimeGenerator(input);
            generator.ParseInputString();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseIfCodeGeneration()
        {
            const string input = "func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; } else { c = 10; } }";
            var generator = new ZRuntimeGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.IsFalse(memory.HasVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestNestedIfCodeGeneration()
        {
            const string input = "func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = new ZRuntimeGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.IsFalse(memory.HasVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestIfExpressionShortCircuit()
        {
            const string input = "func f() { if(5 == 10 && d > 0) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = new ZRuntimeGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.IsFalse(memory.HasVariable("c"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        #endregion

        #region FOR statement generation and execution

        [TestMethod]
        public void TestForGeneration()
        {
            const string input = "func f() { for(var i = 0; i < 10; i++) { a = i + 2; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
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
            const string input = "func f() { i = 0; for(; i < 10; i++) { a = i + 2; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
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
            const string input = "func f() { for(var i = 0; ; i++) { a = i + 2;  if(i == 9) { i++; break; } } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
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
            const string input = "func f() { for(var i = 0; i < 10; i++) { a = i + 2; break; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
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
            const string input = "func f() { for(var i = 0; i < 10; i++) { a = i + 2; continue; b = a; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11, runtime.GlobalMemory.GetVariable("a"));
            Assert.IsFalse(runtime.GlobalMemory.HasVariable("b"));
        }

        [TestMethod]
        public void TestNestedForStatement()
        {
            const string input = "func f() { for(var i = 0; i < 10; i++) { for(var j = 0; j < 10; j++) { a = i + j + 2; continue; b = a; } } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(20, runtime.GlobalMemory.GetVariable("a"));
            Assert.IsFalse(runtime.GlobalMemory.HasVariable("b"));
        }

        #endregion

        #region WHILE statement generation and execution

        [TestMethod]
        public void TestWhileCodeGeneration()
        {
            const string input = "func f() { var a = 0; while(a < 10) { a++; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

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
            const string input = "func f() { var a = 0; while(a < 10) { a++; continue; b = 0; } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
            Assert.IsFalse(runtime.GlobalMemory.HasVariable("b"));
        }

        [TestMethod]
        public void TestWhileBreakStatement()
        {
            const string input = "func f() { var a = 0; while(true) { a++; if(a >= 10) { break; } } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

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
            const string input = "func f() { var a = 0; var b = 0; while(a < 10) { a++; while(b / a < 10) { b++; } } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

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
            const string input = "func f() { var a = 0; var b = 0; while(a < 10) { a++; while(b / a < 10) { b++; if(b > 6) { break; } } } }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(16, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion
    }
}