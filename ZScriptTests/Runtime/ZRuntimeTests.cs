using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the functionality of the ZRuntime class and related components
    /// </summary>
    [TestClass]
    public class ZRuntimeTests
    {
        #region Logical operator short circuiting

        [TestMethod]
        public void TestLogicalAndShortCircuiting()
        {
            // We define 'a' and 'c', but ommit 'b' and 'd', and expect the short circuiting to avoid reaching the parts that access these values
            const string input = "[ a = true; c = false; ] func f() { a = a && (c && b > 0) && d; }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(false, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestLogicalOrShortCircuiting()
        {
            // We define 'a' and 'c', but ommit 'b' and 'd', and expect the short circuiting to avoid reaching the parts that access these values
            const string input = "[ a = true; c = true; ] func f() { a = c || (b || d); }";

            var generator = new ZRuntimeGenerator(input);

            generator.ParseInputString();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(true, runtime.GlobalMemory.GetVariable("a"));
        }

        #endregion

        /// <summary>
        /// Tests calling a function that expects parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredFunctionCall()
        {
            const string input = "func f1(a:int) { return a; }";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 20);

            Assert.AreEqual((long)10, ret1, "The function did not return the expected value");
            Assert.AreEqual((long)20, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function that expects parameters and has default values for one or more of the parameters
        /// </summary>
        [TestMethod]
        public void TestParameteredDefaultValueFunctionCall()
        {
            const string input = "func f1(a:int, b:int = 0) { return a + b; }";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret1 = runtime.CallFunction("f1", 10);
            var ret2 = runtime.CallFunction("f1", 10, 1);

            Assert.AreEqual((long)10, ret1, "The function did not return the expected value");
            Assert.AreEqual((long)11, ret2, "The function did not return the expected value");
        }

        /// <summary>
        /// Tests calling a function from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestInnerFunctionCall()
        {
            const string input = "func f1() { return f2(); } func f2() { return 10; }";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual(10, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests calling a function with a parameter from within a running function in the runtime
        /// </summary>
        [TestMethod]
        public void TestParameteredInnerFunctionCall()
        {
            const string input = "func f1() { return f2(5); } func f2(a:int) { return a; }";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1");

            Assert.AreEqual((long)5, ret, "The statement did not execute as expected");
        }
        /// <summary>
        /// Tests calling a function recursively
        /// </summary>
        [TestMethod]
        public void TestRecursiveFunctionCall()
        {
            const string input = "func f1(a:int) { if(a >= 5) { return 0; } return f1(a + 1) + 1; }";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f1", 0);

            Assert.AreEqual(5, ret, "The statement did not execute as expected");
        }

        /// <summary>
        /// Tests the creation of global variables
        /// </summary>
        [TestMethod]
        public void TestGlobalVariables()
        {
            const string input = "[ a = 0; b = null; ]";
            var generator = CreateGenerator(input);
            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            Assert.AreEqual(0, memory.GetVariable("a"), "The global variables where not parsed as expected");
            Assert.AreEqual(null, memory.GetVariable("b"), "The global variables where not parsed as expected");
        }

        /// <summary>
        /// Creates the default generator to use in tests
        /// </summary>
        /// <param name="input">The input string to use in the generator</param>
        /// <returns>A default runtime generator to use in tests</returns>
        public static ZRuntimeGenerator CreateGenerator(string input)
        {
            var gen = new ZRuntimeGenerator(input) { Debug = true };
            return gen;
        }
    }
}