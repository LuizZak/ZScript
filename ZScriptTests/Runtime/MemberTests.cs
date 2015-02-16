﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;

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
            const string input = "var a = 0; var b = 10; func funca(){ a = [0].Count; b = \"abc\".Length; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");
            
            // Assert the correct call was made
            Assert.AreEqual(1, memory.GetVariable("a"), "The member fetch did not occur as expected");
            Assert.AreEqual(3, memory.GetVariable("b"), "The member fetch did not occur as expected");
        }

        /// <summary>
        /// Tests compound assignments performed with member accesses
        /// </summary>
        [TestMethod]
        public void TestMemberCompoundAssignment()
        {
            const string input = "var a; func funca(){ a = { x:10 }; a.x += 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual((long)20, ((ZObject)memory.GetVariable("a"))["x"], "The member fetch did not occur as expected");
        }
    }
}