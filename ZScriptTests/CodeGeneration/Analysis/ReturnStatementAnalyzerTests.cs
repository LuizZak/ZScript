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
    /// Tests the functionality of the ReturnStatementAnalyzer class
    /// </summary>
    [TestClass]
    public class ReturnStatementAnalyzerTests
    {
        /// <summary>
        /// Tests reporting complete return paths on constant IF statements
        /// </summary>
        [TestMethod]
        public void TestConstantIf()
        {
            const string input = "func f() : int { if(true) { return 10; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant ELSE IF statements
        /// </summary>
        [TestMethod]
        public void TestConstantElseIf()
        {
            const string input = "func f() : int { if(false) { } else if(true) { return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant ELSE statements
        /// </summary>
        [TestMethod]
        public void TestConstantElse()
        {
            const string input = "func f() : int { if(false) { } else { return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant while statements
        /// </summary>
        [TestMethod]
        public void TestConstantWhile()
        {
            const string input = "func f() : int { while(true) { return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant for statements
        /// </summary>
        [TestMethod]
        public void TestConstantFor()
        {
            const string input = "func f() : int { for(;;) { return 0; } } func f1() : int { for(;true;) { return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting incomplete return paths on constant for statements which have a flow break statement within
        /// </summary>
        [TestMethod]
        public void TestFailedConstantForWithBreak()
        {
            const string input = "func f() : int { for(;;) { return 0; } } func f1() : int { for(;true;) { break; return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant switch case labels
        /// </summary>
        [TestMethod]
        public void TestConstantSwitchCase()
        {
            const string input = "func f() : int { switch(10) { case 10: return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant switch case label that fallsthrough another case label with a return statement within
        /// </summary>
        [TestMethod]
        public void TestConstantSwitchCaseFallthrough()
        {
            const string input = "func f() : int { switch(10) { case 10: case 11: return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting complete return paths on constant switch default labels
        /// </summary>
        [TestMethod]
        public void TestConstantSwitchDefault()
        {
            const string input = "func f() : int { switch(10) { default: return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests reporting partial return paths on IF statements
        /// </summary>
        [TestMethod]
        public void TestIfMissingReturnPaths()
        {
            const string input = "func f() : int { var a = true; if(a) { return 10; } else { } }";
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
            const string input = "func f() : int { var a = true; if(a) { return 10; } else if(a) { } else { return 10; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() : int { var a = true; if(a) { return 10; } else if(a) { return 5; } else { return 10; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() : int { var a = 10; switch(a) { case 10: return 10; case 11: return 11; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() : int { var a = 10; switch(a) { case 9: break; case 10: case 11: return 11; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() : int { var a = 10; switch(a) { case 10: return 10; case 11: return 11; default: return 11; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() : int { var a = 10; switch(a) { case 9: return 10; case 10: case 11: return 11; default: return 11; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests complete return paths on SWITCH statements with case fallthrough that falls to the default case
        /// </summary>
        [TestMethod]
        public void TestWorkingSwitchFallthroughToDefaultReturnPaths()
        {
            const string input = "func f() : int { var a = 10; switch(a) { case 9: return 10; case 10: case 11: default: return 11; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() { var a = true; if(a) { return; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests failed early returns by providing a return value to the early return
        /// </summary>
        [TestMethod]
        public void TestFailedEarlyReturns()
        {
            const string input = "func f() { var a = true; if(a) { return 10; } }";
            var generator = TestUtils.CreateGenerator(input);
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
            const string input = "func f() { var a = true; if(a) { return 10; } else { return; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InconsistentReturns));
        }

        /// <summary>
        /// Tests reporting missing return values on non-void contexts
        /// </summary>
        [TestMethod]
        public void TestMissingReturnValueOnNonVoid()
        {
            const string input = "func f() : int { return; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.MissingReturnValueOnNonvoid));
        }

        /// <summary>
        /// Tests reporting missing return values on non-void contexts
        /// </summary>
        [TestMethod]
        public void TestIncompleteReturnsOnNonVoid()
        {
            const string input = "func f() : int { var a = false; if(a) { return 0; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths));
        }

        /// <summary>
        /// Tests valid cases of valued returns on non-void contexts
        /// </summary>
        [TestMethod]
        public void TestValidValuedReturn()
        {
            const string input = "func f() : int { return 10; }";
            var generator = TestUtils.CreateGenerator(input);
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
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ReturningValueOnVoidFunction));
        }

        /// <summary>
        /// Tests valid cases of valueless returns on void contexts
        /// </summary>
        [TestMethod]
        public void TestValidVoidReturn()
        {
            const string input = "func f() : void { return; }";
            var generator = TestUtils.CreateGenerator(input);
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
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.CodeErrors.Length);
        }
    }
}