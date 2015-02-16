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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Messages;
using ZScriptTests.Runtime;
using TestUtils = ZScriptTests.Utils.TestUtils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the ReturnStatementAnalyzer class
    /// </summary>
    [TestClass]
    public class ReturnStatementAnalyzerTests
    {
        /// <summary>
        /// Tests reporting partial return paths on IF statements
        /// </summary>
        [TestMethod]
        public void TestIfMissingReturnPaths()
        {
            const string input = "func f() { if(true) { return 10; } else { } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
        }

        /// <summary>
        /// Tests reporting partial return paths on chained IF statements
        /// </summary>
        [TestMethod]
        public void TestComplexIfMissingReturnPaths()
        {
            const string input = "func f() { if(true) { return 10; } else if(true) { } else { return 10; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
        }

        /// <summary>
        /// Tests complete return paths on chained IF statements
        /// </summary>
        [TestMethod]
        public void TestWorkingIfReturnPaths()
        {
            const string input = "func f() : int { if(true) { return 10; } else if(true) { return 5; } else { return 10; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting partial return paths on SWITCH statements
        /// </summary>
        [TestMethod]
        public void TestSwitchMissingReturnPaths()
        {
            const string input = "func f() { switch(10) { case 10: return 10; case 11: return 11; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
        }

        /// <summary>
        /// Tests reporting partial return paths on SWITCH statements with case fallthrough
        /// </summary>
        [TestMethod]
        public void TestSwitchFallthroughMissingReturnPaths()
        {
            const string input = "func f() { switch(10) { case 9: break; case 10: case 11: return 11; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
        }

        /// <summary>
        /// Tests complete return paths on SWITCH statements
        /// </summary>
        [TestMethod]
        public void TestWorkingSwitchReturnPaths()
        {
            const string input = "func f() : int { switch(10) { case 10: return 10; case 11: return 11; default: return 11; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests complete return paths on SWITCH statements with case fallthrough
        /// </summary>
        [TestMethod]
        public void TestWorkingSwitchFallthroughReturnPaths()
        {
            const string input = "func f() : int { switch(10) { case 9: break; case 10: case 11: return 11; default: return 11; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests early returns
        /// </summary>
        [TestMethod]
        public void TestWorkingEarlyReturns()
        {
            const string input = "func f() { if(true) { return; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests failed early returns by providing a return value to the early return
        /// </summary>
        [TestMethod]
        public void TestFailedEarlyReturns()
        {
            const string input = "func f() { if(true) { return 10; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPathsWithValuedReturn));
        }

        /// <summary>
        /// Tests reporting inconsistent return values
        /// </summary>
        [TestMethod]
        public void TestInconsistentReturns()
        {
            const string input = "func f() { if(true) { return 10; } else { return; } }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InconsistentReturns));
        }

        /// <summary>
        /// Tests reporting missing return values on non-void contexts
        /// </summary>
        [TestMethod]
        public void TestMissingReturnsOnNonVoid()
        {
            const string input = "func f() : int { return; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.MissingReturnValueOnNonvoid));
        }

        /// <summary>
        /// Tests valid cases of valued returns on non-void contexts
        /// </summary>
        [TestMethod]
        public void TestValidValuedReturn()
        {
            const string input = "func f() : int { return 10; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting valued returns on void contexts
        /// </summary>
        [TestMethod]
        public void TestReturnValueOnVoid()
        {
            const string input = "func f() : void { return 10; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ReturningValueOnVoidFunction));
        }

        /// <summary>
        /// Tests valid cases of valueless returns on void contexts
        /// </summary>
        [TestMethod]
        public void TestValidVoidReturn()
        {
            const string input = "func f() : void { return; }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests valid cases of no returns on on void contexts
        /// </summary>
        [TestMethod]
        public void TestEmptyVoidFunction()
        {
            const string input = "func f() : void { }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }
    }
}