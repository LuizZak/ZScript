using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of dictionary code
    /// </summary>
    [TestClass]
    public class DictionaryTests
    {
        /// <summary>
        /// Tests parsing of dictionary literals
        /// </summary>
        [TestMethod]
        public void TestParseDictionaryLiteral()
        {
            const string input = "var dict:[int:string] = [0: 'apples', 1: 'oranges'];";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests parsing of dictionary literals
        /// </summary>
        [TestMethod]
        public void TestParseImplicitDictionaryLiteral()
        {
            const string input = "var dict = [0: 'apples', 1: 'oranges'];";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests execution of dictionary literals
        /// </summary>
        [TestMethod]
        public void TestExecuteDictionaryLiteral()
        {
            const string input = "var dict:[int:string]; func f() { dict = [0: 'apples', 1: 'oranges']; }";

            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            var dict = (Dictionary<long, string>)memory.GetVariable("dict");

            Assert.AreEqual("apples", dict[0], "The runtime failed to create the expected dictionary");
            Assert.AreEqual("oranges", dict[1], "The runtime failed to create the expected dictionary");
        }
    }
}