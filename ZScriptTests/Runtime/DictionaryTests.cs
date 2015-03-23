#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion
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
        /// Tests parsing of dictionary literal intialization
        /// </summary>
        [TestMethod]
        public void TestParseDictionaryLiteralInit()
        {
            const string input = "var dict = [int:string]();";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests parsing of dictionary literal intialization with access
        /// </summary>
        [TestMethod]
        public void TestParseDictionaryLiteralInitAccess()
        {
            const string input = "var dict = [int:string]()[0];";

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

        /// <summary>
        /// Tests execution of dictionary literals
        /// </summary>
        [TestMethod]
        public void TestExecuteImplicitDictionaryLiteral()
        {
            const string input = "var dict:[int: float]; func f() { dict = [0: 0]; }";

            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            var dict = (Dictionary<long, double>)memory.GetVariable("dict");

            Assert.AreEqual(0.0, dict[0], "The runtime failed to create the expected dictionary");
        }

        /// <summary>
        /// Tests execution of dictionary literal intialization
        /// </summary>
        [TestMethod]
        public void TestExecuteDictionaryLiteralInit()
        {
            const string input = "var dict:any; func f() { dict = [int: float](); }";

            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.IsInstanceOfType(memory.GetVariable("dict"), typeof(Dictionary<long, double>), "The list created by the script is not of the expected type");
        }
    }
}