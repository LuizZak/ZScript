using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the DefinitionsAnalyzer class and related components
    /// </summary>
    [TestClass]
    public class DefinitionsAnalyzerTests
    {
        /// <summary>
        /// Tests reporting duplicated local definitions in the same scope
        /// </summary>
        [TestMethod]
        public void TestDuplicatedLocalDefinition()
        {
            const string input = "func f1() { var a; var a; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition));
        }

        /// <summary>
        /// Tests reporting duplicated global definitions in the same scope
        /// </summary>
        [TestMethod]
        public void TestDuplicateGlobalDefinition()
        {
            const string input = "var a; var a;";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(2, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition));
        }

        /// <summary>
        /// Tests global variable shadowing
        /// </summary>
        [TestMethod]
        public void TestGlobalVariableShadowing()
        {
            const string input = "var a; func f1() { var a; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition));
        }

        /// <summary>
        /// Tests local field shadowing
        /// </summary>
        [TestMethod]
        public void TestClassFieldShadowing()
        {
            const string input = "class c1 { var i:int; func f1(i:int) { } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition));
        }
    }
}