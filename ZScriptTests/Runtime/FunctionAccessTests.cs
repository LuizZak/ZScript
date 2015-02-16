using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of function accesses
    /// </summary>
    [TestClass]
    public class FunctionAccessTests
    {
        /// <summary>
        /// Tests basic function calling
        /// </summary>
        [TestMethod]
        public void TestBasicFunctionCall()
        {
            const string input = "var a = 0; var b = 10; func funca(){ a = b.ToString(); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual("10", memory.GetVariable("a"), "The function call did not occur as expected");
        }

        /// <summary>
        /// Tests chained function calling
        /// </summary>
        [TestMethod]
        public void TestChainedFunctionCall()
        {
            const string input = "var a; func funca(){ a = 1234.ToString().IndexOf('4'); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(3, memory.GetVariable("a"), "The function call did not occur as expected");
        }
    }
}