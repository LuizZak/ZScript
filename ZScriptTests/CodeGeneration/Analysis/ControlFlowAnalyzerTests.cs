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

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the ControlFlowAnalyzer class and related components
    /// </summary>
    [TestClass]
    public class ControlFlowAnalyzerTests
    {
        #region Linear flow

        /// <summary>
        /// Tests analyzing a control flow with an empty code block
        /// </summary>
        [TestMethod]
        public void TestEmptyReachableEnd()
        {
            const string input = "{ }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that does not branches
        /// </summary>
        [TestMethod]
        public void TestAnalyzeLinarFlow()
        {
            const string input = "{ var a; var b; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that is interrupted midway through with a return statement
        /// </summary>
        [TestMethod]
        public void TestAnalyzeLinarFlowInterrupted()
        {
            const string input = "{ var a; var b; return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that is interrupted midway through with a return statement
        /// </summary>
        [TestMethod]
        public void TestAnalyzeLinarEndFlowInterrupted()
        {
            const string input = "{ var a; var b; return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #region If conditional

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement
        /// </summary>
        [TestMethod]
        public void TestSimpleIfFlow()
        {
            const string input = "{ var a; var b; if(a) return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).ifStatement().statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement with a block statement within
        /// </summary>
        [TestMethod]
        public void TestBlockedIfFlow()
        {
            const string input = "{ var a; var b; if(a) { var d; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement with a block statement within
        /// </summary>
        [TestMethod]
        public void TestBlockedIfFlowInterrupted()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsFalse(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a branched flow by analyzing an if statement with an else statement coupled
        /// </summary>
        [TestMethod]
        public void TestIfElseBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else { var f; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");


            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsFalse(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.IsTrue(elseStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.IsTrue(elseStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a branched flow by analyzing an if statement with an else statement coupled
        /// </summary>
        [TestMethod]
        public void TestIfElseIfBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else if(a) { var f; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");


            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsFalse(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            var elseIfStatement = elseStatement.statement().ifStatement().statement();

            Assert.IsTrue(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.IsTrue(elseIfStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.IsTrue(elseIfStatement.blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing flow of an if-else statement that interrupts on both the if and the else inner statements
        /// </summary>
        [TestMethod]
        public void TestInterruptedIfElseBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else { var f; return; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsFalse(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            var innerElseStatement = elseStatement.statement();

            Assert.IsTrue(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.IsTrue(innerElseStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.IsTrue(innerElseStatement.blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.IsFalse(innerElseStatement.blockStatement().statement(2).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.IsFalse(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analysis of a control flow that passes through an if/else if/else construct, with returns only on the if and else blocks
        /// </summary>
        [TestMethod]
        public void TestIncompleteInterruptedElseIfBranching()
        {
            const string input = "{ var a = true; if(a) { return 10; } else if(a) { var a; } else { return 10; } var b; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(1).ifStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            var ifElseStatement = ifStatement.elseStatement().statement().ifStatement();

            Assert.IsTrue(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.IsTrue(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            var elseStatement = ifElseStatement.elseStatement();
            var innerElseStatement = elseStatement.statement();

            Assert.IsTrue(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.IsTrue(innerElseStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true if statement
        /// </summary>
        [TestMethod]
        public void TestConstantTrueIf()
        {
            const string input = "{ var a; var b; if(a) return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the if as constant
            body.blockStatement().statement(2).ifStatement().IsConstant = true;
            body.blockStatement().statement(2).ifStatement().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).ifStatement().statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false if statement
        /// </summary>
        [TestMethod]
        public void TestConstantFalseIf()
        {
            const string input = "{ var a; var b; if(a) { } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the if as constant
            var ifStatementContext = body.blockStatement().statement(2).ifStatement();

            ifStatementContext.IsConstant = true;
            ifStatementContext.ConstantValue = false;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsFalse(ifStatementContext.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false if statement with an else statement
        /// </summary>
        [TestMethod]
        public void TestConstantFalseIfElse()
        {
            const string input = "{ var a; var b; if(a) { } else { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the if as constant
            var ifStatementContext = body.blockStatement().statement(2).ifStatement();

            ifStatementContext.IsConstant = true;
            ifStatementContext.ConstantValue = false;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsFalse(ifStatementContext.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            var elseStatement = ifStatementContext.elseStatement().statement().blockStatement();
            Assert.IsTrue(elseStatement.statement(0).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region Trailing If

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a trailing if statement
        /// </summary>
        [TestMethod]
        public void TestSimpleTrailingIfFlow()
        {
            const string input = "{ var a; var b; return if(a); var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true trailing if statement
        /// </summary>
        [TestMethod]
        public void TestConstantTrueTrailingIf()
        {
            const string input = "{ var a; var b; return if(a); var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the if as constant
            body.blockStatement().statement(2).trailingIfStatement().IsConstant = true;
            body.blockStatement().statement(2).trailingIfStatement().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region Switch statement

        /// <summary>
        /// Tests flow analysis in switch statements
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementReachability()
        {
            const string input = "{ switch(a) { case b: var d; break; var e; }; var f; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsTrue(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that preceed a return statement
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementBeforeInterrupt()
        {
            const string input = "{ switch(a) { case b: var d; break; var e; }; return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsTrue(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that contain fallthrough
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementFallthroughReachability()
        {
            const string input = "{ switch(a) { case b: var c; case d: var e; break; var f; }; var g; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var case2 = switchStatement.caseBlock(1);

            Assert.IsTrue(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsTrue(case2.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case2.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that contain a default case
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementDefaultReachability()
        {
            const string input = "{ switch(a) { case b: var c; case d: var e; break; default: var f; }; var g; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var case2 = switchStatement.caseBlock(1);

            Assert.IsTrue(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsTrue(case2.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.IsTrue(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements with an interrupted return
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementReturn()
        {
            const string input = "{ switch(a) { case b: var d; return; var e; }; var f; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsTrue(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that are completely surrounded with return statements
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementInterruption()
        {
            const string input = "{ switch(a) { case b: return 0; default: return 0; }; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.IsTrue(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.IsFalse(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that are completely surrounded with return statements
        /// </summary>
        [TestMethod]
        public void TestSwitchStatementFallthrough()
        {
            const string input = "{ switch(a) { case b: case c: return 0; default: return 0; }; var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case2 = switchStatement.caseBlock(1);

            Assert.IsTrue(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.IsTrue(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.IsFalse(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant resolving

        /// <summary>
        /// Tests flow analysis in constant switch statements
        /// </summary>
        [TestMethod]
        public void TestConstantSwitchStatement()
        {
            const string input = "{ switch(a) { case b: return 0; var c; }; var e; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the switch as constant
            body.blockStatement().statement(0).switchStatement().IsConstant = true;
            body.blockStatement().statement(0).switchStatement().ConstantCaseIndex = 0;
            body.blockStatement().statement(0).switchStatement().ConstantCase =
                body.blockStatement().statement(0).switchStatement().switchBlock().caseBlock(0);

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsFalse(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that execute a fallthrough case statement
        /// </summary>
        [TestMethod]
        public void TestConstantFallthroughSwitchStatement()
        {
            const string input = "{ switch(a) { case b: case c: return 0; var d; }; var e; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the switch as constant
            body.blockStatement().statement(0).switchStatement().IsConstant = true;
            body.blockStatement().statement(0).switchStatement().ConstantCaseIndex = 0;
            body.blockStatement().statement(0).switchStatement().ConstantCase =
                body.blockStatement().statement(0).switchStatement().switchBlock().caseBlock(0);

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(1);

            Assert.IsTrue(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsFalse(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that always flow to the default case
        /// </summary>
        [TestMethod]
        public void TestConstantDefaultSwitchStatement()
        {
            const string input = "{ switch(a) { case b: var c; default: return; var d; }; var e; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the switch as constant
            body.blockStatement().statement(0).switchStatement().IsConstant = true;
            body.blockStatement().statement(0).switchStatement().ConstantCaseIndex = -1;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var defaultBlock = switchStatement.defaultBlock();

            Assert.IsTrue(defaultBlock.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.IsFalse(defaultBlock.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.IsFalse(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that never executes any of the inner statements
        /// </summary>
        [TestMethod]
        public void TestConstantFalseSwitchStatement()
        {
            const string input = "{ switch(a) { case b: var c; }; var e; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the switch as constant
            body.blockStatement().statement(0).switchStatement().IsConstant = true;
            body.blockStatement().statement(0).switchStatement().ConstantCaseIndex = -1;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region While loop

        /// <summary>
        /// Tests flow analysis in while loops
        /// </summary>
        [TestMethod]
        public void TestWhileLoopInterruptStatement()
        {
            const string input = "{ while(true) { var a; return; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(whileLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in while loops that preceed return statements
        /// </summary>
        [TestMethod]
        public void TestWhileLoopBeforeInterruptStatement()
        {
            const string input = "{ while(true) { var a; var b; var c; } return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the break statement
        /// </summary>
        [TestMethod]
        public void TestSequentialWhileLoopBreakStatement()
        {
            const string input = "{ while(true) { var a; var b; } while(true) { var a; break; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop1 = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop1.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop1.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");

            var whileLoop2 = blockBody.statement(1).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop2.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop2.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(whileLoop2.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow disrupting with a misplaced break statement
        /// </summary>
        [TestMethod]
        public void TestHangingBreakStatement()
        {
            const string input = "{ while(true) { var a; var b; } break; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop1 = blockBody.statement(0).whileStatement().statement().blockStatement();
            
            Assert.IsTrue(whileLoop1.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop1.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsFalse(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true while statement
        /// </summary>
        [TestMethod]
        public void TestConstantTrueWhile()
        {
            const string input = "{ var a; var b; while(a) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the while as constant
            body.blockStatement().statement(2).whileStatement().IsConstant = true;
            body.blockStatement().statement(2).whileStatement().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false false statement
        /// </summary>
        [TestMethod]
        public void TestConstantFalseWhile()
        {
            const string input = "{ var a; var b; while(a) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the while as constant
            body.blockStatement().statement(2).whileStatement().IsConstant = true;
            body.blockStatement().statement(2).whileStatement().ConstantValue = false;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsFalse(body.blockStatement().statement(2).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite while loop that does not contains a break statement
        /// </summary>
        [TestMethod]
        public void TestInfiniteWhileLoopUnreachability()
        {
            const string input = "{ while(a) { } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the while as constant
            body.blockStatement().statement(0).whileStatement().IsConstant = true;
            body.blockStatement().statement(0).whileStatement().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(0).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.IsFalse(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite while loop that contains a branched break statement
        /// </summary>
        [TestMethod]
        public void TestInfiniteWhileLoopBranchedBreakUnreachability()
        {
            const string input = "{ while(true) { if(a) break; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the while as constant
            body.blockStatement().statement(0).whileStatement().IsConstant = true;
            body.blockStatement().statement(0).whileStatement().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(0).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region For loop

        /// <summary>
        /// Tests flow analysis in for loops which contain return statements
        /// </summary>
        [TestMethod]
        public void TestForLoopInterruptStatement()
        {
            const string input = "{ for(;a;) { var a; return; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in for loops that preceed return statements
        /// </summary>
        [TestMethod]
        public void TestForLoopBeforeInterruptStatement()
        {
            const string input = "{ for(;a;) { var a; var b; var c; } return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the break statement
        /// </summary>
        [TestMethod]
        public void TestForLoopBreakStatement()
        {
            const string input = "{ for(;a;) { var a; break; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a break statement contained within a conditional
        /// </summary>
        [TestMethod]
        public void TestForLoopNestedBreakStatement()
        {
            const string input = "{ for(;a;) { var a; if(a) { break; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the continue statement
        /// </summary>
        [TestMethod]
        public void TestForLoopContinueStatement()
        {
            const string input = "{ for(;a;) { var a; continue; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a continue statement contained within a conditional
        /// </summary>
        [TestMethod]
        public void TestForLoopNestedContinueStatement()
        {
            const string input = "{ for(;a;) { var a; if(a) { continue; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a nested loop break statement
        /// </summary>
        [TestMethod]
        public void TestNestedForLoopBreakStatement()
        {
            const string input = "{ for(;a;) { for(;a;) { var a; break; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the outer-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the outer-loop statement reachability correctly");

            var innerForLoop = forLoop.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(innerForLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(innerForLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(innerForLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a nested loop continue statement
        /// </summary>
        [TestMethod]
        public void TestNestedForLoopContinueStatement()
        {
            const string input = "{ for(;a;) { for(;a;) { var a; continue; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the outer-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the outer-loop statement reachability correctly");

            var innerForLoop = forLoop.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(innerForLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(innerForLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(innerForLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true for statement
        /// </summary>
        [TestMethod]
        public void TestConstantTrueFor()
        {
            const string input = "{ var a; var b; for(;a;) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the for as constant
            body.blockStatement().statement(2).forStatement().forCondition().IsConstant = true;
            body.blockStatement().statement(2).forStatement().forCondition().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a for statement with no condition
        /// </summary>
        [TestMethod]
        public void TestNoConditionFor()
        {
            const string input = "{ var a; var b; for(;;) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false for statement
        /// </summary>
        [TestMethod]
        public void TestConstantFalseFor()
        {
            const string input = "{ var a; var b; for(;a;) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            // Set the for as constant
            body.blockStatement().statement(2).forStatement().forCondition().IsConstant = true;
            body.blockStatement().statement(2).forStatement().forCondition().ConstantValue = false;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsFalse(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite for loop that does not contains a break statement
        /// </summary>
        [TestMethod]
        public void TestInfiniteForLoopUnreachability()
        {
            const string input = "{ for(;;) { } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsFalse(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(0).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.IsFalse(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite for loop that contains a branched break statement
        /// </summary>
        [TestMethod]
        public void TestInfiniteForLoopBranchedBreakUnreachability()
        {
            const string input = "{ for(;;) { if(a) break; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(0).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region Bug tests

        /// <summary>
        /// Tests a bug in flow detection related to the fibonacci sample
        /// </summary>
        [TestMethod]
        public void TestFibonacciReturn()
        {
            const string input = " {" +
                                 "  if (n == 0) return 0;" +
                                 "  if (n == 1) return 1;" +
                                 "  var prevPrev = 0;" +
                                 "  var prev = 1;" +
                                 "  var result = 0;" +
                                 "  for (var i = 2; i <= n; i++)" +
                                 "  {" +
                                 "    result = prev + prevPrev;" +
                                 "    prevPrev = prev;" +
                                 "    prev = result;" +
                                 "  }" +
                                 "  return result;" +
                                 "}";

            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            var ifStatement1 = body.blockStatement().statement(0).ifStatement();
            var ifStatement2 = body.blockStatement().statement(1).ifStatement();
            var forStatement = body.blockStatement().statement(5).forStatement();

            Assert.IsTrue(ifStatement1.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.IsTrue(ifStatement2.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.IsTrue(forStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.IsTrue(forStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.IsTrue(forStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner loop statement reachability correctly");

            // Direct flow reachability
            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(4).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(5).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(6).Reachable, "Failed mark the statement reachability correctly");

            Assert.IsFalse(analyzer.IsEndReachable);
        }

        /// <summary>
        /// Tests a break preceeding a return statement inside a constant while
        /// </summary>
        [TestMethod]
        public void TestBreakOnConstantTrueWhile()
        {
            const string input = "{ while (true) { break; return 0; } }";

            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var whileStatement = body.blockStatement().statement(0).whileStatement();
            whileStatement.IsConstant = true;
            whileStatement.ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(whileStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.IsFalse(whileStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
        }

        /// <summary>
        /// Tests a break preceeding a return statement inside a constant for
        /// </summary>
        [TestMethod]
        public void TestBreakOnConstantTrueFor()
        {
            const string input = "{ for (;true;) { break; return 0; } }";

            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var forStatement = body.blockStatement().statement(0).forStatement();
            forStatement.forCondition().IsConstant = true;
            forStatement.forCondition().ConstantValue = true;

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.IsEndReachable);

            Assert.IsTrue(forStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.IsFalse(forStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests a break statement that does not contains a valid target
        /// </summary>
        [TestMethod]
        public void TestNoBreakTarget()
        {
            const string input = "{ break; while(true) { break; } for(;;) { break; } break; switch(a) { case 0: break; break; } }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var container = new MessageContainer();
            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(messageContainer: container), body);
            
            analyzer.Analyze();

            Assert.AreEqual(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForBreakStatement), "Failed to detect mismatched breaks correctly");
        }

        /// <summary>
        /// Tests a continue statement that does not contains a valid target
        /// </summary>
        [TestMethod]
        public void TestNoContinueTarget()
        {
            const string input = "{ continue; while(true) { continue; } for(;;) { continue; } continue; switch(a) { case 0: continue; } }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var container = new MessageContainer();
            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(messageContainer: container), body);

            analyzer.Analyze();

            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForContinueStatement), "Failed to detect mismatched continues correctly");
        }

        #endregion
    }
}