using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of member accesses
    /// </summary>
    [TestClass]
    public class MemberTests
    {
        /// <summary>
        /// Tests basic member accessing
        /// </summary>
        [TestMethod]
        public void TestBasicMemberAccess()
        {
            const string input = "[ a = 0; b = 10; ] func funca(){ a = [0].Count; b = \"abc\".Length; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");
            
            // Assert the correct call was made
            Assert.AreEqual(1, memory.GetVariable("a"), "The member fetch did not occur as expected");
            Assert.AreEqual(3, memory.GetVariable("b"), "The member fetch did not occur as expected");
        }
    }
}