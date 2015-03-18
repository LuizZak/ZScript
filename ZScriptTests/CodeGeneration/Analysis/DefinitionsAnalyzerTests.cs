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
        /// Tests reporting undefined definition usages
        /// </summary>
        [TestMethod]
        public void TestUndeclaredDefinitionUsage()
        {
            const string input = "func f() { var a = a; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UndeclaredDefinition));
        }

        /// <summary>
        /// Tests handling using definitions before shadowing
        /// </summary>
        [TestMethod]
        public void TestUsageBeforeShadowing()
        {
            const string input = "func f() { a(); var a = 10; } func a() { }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
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