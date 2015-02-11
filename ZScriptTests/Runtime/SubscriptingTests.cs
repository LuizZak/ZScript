using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests subscription of different types of values on the VM
    /// </summary>
    [TestClass]
    public class SubscriptingTests
    {
        /// <summary>
        /// Tests parsing and execution of dictionary subscription value fetching
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptGet()
        {
            const string input = "[ a = 0; b; ] func funca(){ a = b[\"abc\"]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            memory.SetVariable("b", new Dictionary<string, object> { { "abc", 10 } });

            runtime.CallFunction("funca");

            Assert.AreEqual(10, memory.GetVariable("a"), "The VM failed to fetch the correct subscription for the dictionary object stored");
        }

        /// <summary>
        /// Tests parsing and execution of dictionary subscription value setting
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptSet()
        {
            const string input = "[ b; ] func funca(){ b[\"abc\"] = 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            var dict = new Dictionary<string, object>();
            memory.SetVariable("b", dict);

            runtime.CallFunction("funca");

            Assert.AreEqual(10, dict["abc"], "The VM failed to set the correct subscription for the dictionary object stored");
        }

        /// <summary>
        /// Tests utilizing a subscript with a compound operator
        /// </summary>
        [TestMethod]
        public void TestSubscriptCompoundAssign()
        {
            const string input = "[ b; ] func funca(){ b['abc'] += 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            var dict = new Dictionary<string, object> { { "abc", 10 } };
            memory.SetVariable("b", dict);

            runtime.CallFunction("funca");

            Assert.AreEqual(20, dict["abc"], "The VM failed to set the correct subscription for the dictionary object stored");
        }
    }
}