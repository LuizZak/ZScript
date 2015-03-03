using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests parsing and execution of top level functions
    /// </summary>
    [TestClass]
    public class TopLevelFunctionTests
    {
        /// <summary>
        /// Tests basic function calling
        /// </summary>
        [TestMethod]
        public void TestBasicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); } func funcb() { __trace(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with multiple parameters
        /// </summary>
        [TestMethod]
        public void TestMultiParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(1, 2); } func funcb(a:int, b:int) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[1], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with default parameters
        /// </summary>
        [TestMethod]
        public void TestDefaultParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(a:int = 0, b:int = 0) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(0L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(0L, owner.TraceObjects[1], "The function call did not occur as expected");

            Assert.AreEqual(1L, owner.TraceObjects[2], "The function call did not occur as expected");
            Assert.AreEqual(0L, owner.TraceObjects[3], "The function call did not occur as expected");

            Assert.AreEqual(1L, owner.TraceObjects[4], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[5], "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests function calling with variadic parameters
        /// </summary>
        [TestMethod]
        public void TestVariadicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(args...) { __trace(args); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1L, owner.TraceObjects[0], "The function call did not occur as expected");
            Assert.AreEqual(1L, owner.TraceObjects[1], "The function call did not occur as expected");
            Assert.AreEqual(2L, owner.TraceObjects[2], "The function call did not occur as expected");
        }
    }
}