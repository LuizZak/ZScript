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
using Xunit;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the ControlFlowAnalyzer class and related components
    /// </summary>
    public class ControlFlowAnalyzerTests
    {
        #region Linear flow

        /// <summary>
        /// Tests analyzing a control flow with an empty code block
        /// </summary>
        [Fact]
        public void TestEmptyReachableEnd()
        {
            const string input = "{ }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that does not branches
        /// </summary>
        [Fact]
        public void TestAnalyzeLinarFlow()
        {
            const string input = "{ var a; var b; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that is interrupted midway through with a return statement
        /// </summary>
        [Fact]
        public void TestAnalyzeLinarFlowInterrupted()
        {
            const string input = "{ var a; var b; return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a lineary control flow that is interrupted midway through with a return statement
        /// </summary>
        [Fact]
        public void TestAnalyzeLinarEndFlowInterrupted()
        {
            const string input = "{ var a; var b; return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #region If conditional

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement
        /// </summary>
        [Fact]
        public void TestSimpleIfFlow()
        {
            const string input = "{ var a; var b; if(a) return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).ifStatement().statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement with a block statement within
        /// </summary>
        [Fact]
        public void TestBlockedIfFlow()
        {
            const string input = "{ var a; var b; if(a) { var d; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement with a block statement within
        /// </summary>
        [Fact]
        public void TestBlockedIfFlowInterrupted()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.False(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a branched flow by analyzing an if statement with an else statement coupled
        /// </summary>
        [Fact]
        public void TestIfElseBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else { var f; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");


            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.False(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.True(elseStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.True(elseStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a branched flow by analyzing an if statement with an else statement coupled
        /// </summary>
        [Fact]
        public void TestIfElseIfBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else if(a) { var f; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");


            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.False(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            var elseIfStatement = elseStatement.statement().ifStatement().statement();

            Assert.True(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.True(elseIfStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.True(elseIfStatement.blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing flow of an if-else statement that interrupts on both the if and the else inner statements
        /// </summary>
        [Fact]
        public void TestInterruptedIfElseBranching()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } else { var f; return; var g; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(2).ifStatement();
            var elseStatement = ifStatement.elseStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.False(ifStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner if statement reachability correctly");

            var innerElseStatement = elseStatement.statement();

            Assert.True(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.True(innerElseStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.True(innerElseStatement.blockStatement().statement(1).Reachable, "Failed mark the inner else statement reachability correctly");
            Assert.False(innerElseStatement.blockStatement().statement(2).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.False(blockBody.statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analysis of a control flow that passes through an if/else if/else construct, with returns only on the if and else blocks
        /// </summary>
        [Fact]
        public void TestIncompleteInterruptedElseIfBranching()
        {
            const string input = "{ var a = true; if(a) { return 10; } else if(a) { var a; } else { return 10; } var b; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");

            var ifStatement = blockBody.statement(1).ifStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            var ifElseStatement = ifStatement.elseStatement().statement().ifStatement();

            Assert.True(ifStatement.statement().Reachable, "Failed mark the if statement reachability correctly");
            Assert.True(ifStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner if statement reachability correctly");

            var elseStatement = ifElseStatement.elseStatement();
            var innerElseStatement = elseStatement.statement();

            Assert.True(elseStatement.statement().Reachable, "Failed mark the else statement reachability correctly");
            Assert.True(innerElseStatement.blockStatement().statement(0).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true if statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).ifStatement().statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false if statement
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.False(ifStatementContext.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false if statement with an else statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.False(ifStatementContext.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            var elseStatement = ifStatementContext.elseStatement().statement().blockStatement();
            Assert.True(elseStatement.statement(0).Reachable, "Failed mark the inner else statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region Switch statement

        /// <summary>
        /// Tests flow analysis in switch statements
        /// </summary>
        [Fact]
        public void TestSwitchStatementReachability()
        {
            const string input = "{ switch(a) { case b: var d; break; var e; }; var f; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.True(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that preceed a return statement
        /// </summary>
        [Fact]
        public void TestSwitchStatementBeforeInterrupt()
        {
            const string input = "{ switch(a) { case b: var d; break; var e; }; return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.True(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that contain fallthrough
        /// </summary>
        [Fact]
        public void TestSwitchStatementFallthroughReachability()
        {
            const string input = "{ switch(a) { case b: var c; case d: var e; break; var f; }; var g; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var case2 = switchStatement.caseBlock(1);

            Assert.True(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.True(case2.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case2.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that contain a default case
        /// </summary>
        [Fact]
        public void TestSwitchStatementDefaultReachability()
        {
            const string input = "{ switch(a) { case b: var c; case d: var e; break; default: var f; }; var g; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var case2 = switchStatement.caseBlock(1);

            Assert.True(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.True(case2.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.True(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements with an interrupted return
        /// </summary>
        [Fact]
        public void TestSwitchStatementReturn()
        {
            const string input = "{ switch(a) { case b: var d; return; var e; }; var f; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.True(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case1.statement(2).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that are completely surrounded with return statements
        /// </summary>
        [Fact]
        public void TestSwitchStatementInterruption()
        {
            const string input = "{ switch(a) { case b: return 0; default: return 0; }; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.True(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.False(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in switch statements that are completely surrounded with return statements
        /// </summary>
        [Fact]
        public void TestSwitchStatementFallthrough()
        {
            const string input = "{ switch(a) { case b: case c: return 0; default: return 0; }; var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case2 = switchStatement.caseBlock(1);

            Assert.True(case2.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");

            var defaultCase = switchStatement.defaultBlock();

            Assert.True(defaultCase.statement(0).Reachable, "Failed mark the inner-default statement reachability correctly");

            Assert.False(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant resolving

        /// <summary>
        /// Tests flow analysis in constant switch statements
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(0);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.False(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that execute a fallthrough case statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var case1 = switchStatement.caseBlock(1);

            Assert.True(case1.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(case1.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.False(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that always flow to the default case
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var switchStatement = blockBody.statement(0).switchStatement().switchBlock();
            var defaultBlock = switchStatement.defaultBlock();

            Assert.True(defaultBlock.statement(0).Reachable, "Failed mark the inner-case statement reachability correctly");
            Assert.False(defaultBlock.statement(1).Reachable, "Failed mark the inner-case statement reachability correctly");

            Assert.False(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in constant switch statements that never executes any of the inner statements
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region While loop

        /// <summary>
        /// Tests flow analysis in while loops
        /// </summary>
        [Fact]
        public void TestWhileLoopInterruptStatement()
        {
            const string input = "{ while(true) { var a; return; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.True(whileLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(whileLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in while loops that preceed return statements
        /// </summary>
        [Fact]
        public void TestWhileLoopBeforeInterruptStatement()
        {
            const string input = "{ while(true) { var a; var b; var c; } return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.True(whileLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the break statement
        /// </summary>
        [Fact]
        public void TestSequentialWhileLoopBreakStatement()
        {
            const string input = "{ while(true) { var a; var b; } while(true) { var a; break; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop1 = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.True(whileLoop1.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop1.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");

            var whileLoop2 = blockBody.statement(1).whileStatement().statement().blockStatement();

            Assert.True(whileLoop2.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop2.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(whileLoop2.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow disrupting with a misplaced break statement
        /// </summary>
        [Fact]
        public void TestHangingBreakStatement()
        {
            const string input = "{ while(true) { var a; var b; } break; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop1 = blockBody.statement(0).whileStatement().statement().blockStatement();
            
            Assert.True(whileLoop1.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(whileLoop1.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.False(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true while statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false false statement
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.False(body.blockStatement().statement(2).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite while loop that does not contains a break statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(0).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.False(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite while loop that contains a branched break statement
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(0).whileStatement().statement().Reachable, "Failed mark the inner while statement reachability correctly");

            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region For loop

        /// <summary>
        /// Tests flow analysis in for loops which contain return statements
        /// </summary>
        [Fact]
        public void TestForLoopInterruptStatement()
        {
            const string input = "{ for(;a;) { var a; return; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in for loops that preceed return statements
        /// </summary>
        [Fact]
        public void TestForLoopBeforeInterruptStatement()
        {
            const string input = "{ for(;a;) { var a; var b; var c; } return; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the break statement
        /// </summary>
        [Fact]
        public void TestForLoopBreakStatement()
        {
            const string input = "{ for(;a;) { var a; break; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a break statement contained within a conditional
        /// </summary>
        [Fact]
        public void TestForLoopNestedBreakStatement()
        {
            const string input = "{ for(;a;) { var a; if(a) { break; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the continue statement
        /// </summary>
        [Fact]
        public void TestForLoopContinueStatement()
        {
            const string input = "{ for(;a;) { var a; continue; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a continue statement contained within a conditional
        /// </summary>
        [Fact]
        public void TestForLoopNestedContinueStatement()
        {
            const string input = "{ for(;a;) { var a; if(a) { continue; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a nested loop break statement
        /// </summary>
        [Fact]
        public void TestNestedForLoopBreakStatement()
        {
            const string input = "{ for(;a;) { for(;a;) { var a; break; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the outer-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the outer-loop statement reachability correctly");

            var innerForLoop = forLoop.statement(0).forStatement().statement().blockStatement();

            Assert.True(innerForLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(innerForLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(innerForLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with a nested loop continue statement
        /// </summary>
        [Fact]
        public void TestNestedForLoopContinueStatement()
        {
            const string input = "{ for(;a;) { for(;a;) { var a; continue; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.True(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.True(forLoop.statement(0).Reachable, "Failed mark the outer-loop statement reachability correctly");
            Assert.True(forLoop.statement(1).Reachable, "Failed mark the outer-loop statement reachability correctly");

            var innerForLoop = forLoop.statement(0).forStatement().statement().blockStatement();

            Assert.True(innerForLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.True(innerForLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.False(innerForLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.True(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #region Constant evaluation

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant true for statement
        /// </summary>
        [Fact]
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

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a for statement with no condition
        /// </summary>
        [Fact]
        public void TestNoConditionFor()
        {
            const string input = "{ var a; var b; for(;;) { return; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.False(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing a constant false for statement
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");

            Assert.False(body.blockStatement().statement(2).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite for loop that does not contains a break statement
        /// </summary>
        [Fact]
        public void TestInfiniteForLoopUnreachability()
        {
            const string input = "{ for(;;) { } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.False(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(0).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.False(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing an infinite for loop that contains a branched break statement
        /// </summary>
        [Fact]
        public void TestInfiniteForLoopBranchedBreakUnreachability()
        {
            const string input = "{ for(;;) { if(a) break; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.True(analyzer.IsEndReachable);

            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");

            Assert.True(body.blockStatement().statement(0).forStatement().statement().Reachable, "Failed mark the inner for statement reachability correctly");

            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        #endregion

        #endregion

        #region Bug tests

        /// <summary>
        /// Tests a bug in flow detection related to the fibonacci sample
        /// </summary>
        [Fact]
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

            Assert.True(ifStatement1.statement().Reachable, "Failed mark the inner if statement reachability correctly");
            Assert.True(ifStatement2.statement().Reachable, "Failed mark the inner if statement reachability correctly");

            Assert.True(forStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.True(forStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.True(forStatement.statement().blockStatement().statement(2).Reachable, "Failed mark the inner loop statement reachability correctly");

            // Direct flow reachability
            Assert.True(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(4).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(5).Reachable, "Failed mark the statement reachability correctly");
            Assert.True(body.blockStatement().statement(6).Reachable, "Failed mark the statement reachability correctly");

            Assert.False(analyzer.IsEndReachable);
        }

        /// <summary>
        /// Tests a break preceeding a return statement inside a constant while
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(whileStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.False(whileStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
        }

        /// <summary>
        /// Tests a break preceeding a return statement inside a constant for
        /// </summary>
        [Fact]
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

            Assert.True(analyzer.IsEndReachable);

            Assert.True(forStatement.statement().blockStatement().statement(0).Reachable, "Failed mark the inner loop statement reachability correctly");
            Assert.False(forStatement.statement().blockStatement().statement(1).Reachable, "Failed mark the inner loop statement reachability correctly");
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests a break statement that does not contains a valid target
        /// </summary>
        [Fact]
        public void TestNoBreakTarget()
        {
            const string input = "{ break; while(true) { break; } for(;;) { break; } break; switch(a) { case 0: break; break; } }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var container = new MessageContainer();
            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(messageContainer: container), body);
            
            analyzer.Analyze();

            // "Failed to detect mismatched breaks correctly"
            Assert.Equal(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForBreakStatement));
        }

        /// <summary>
        /// Tests a continue statement that does not contains a valid target
        /// </summary>
        [Fact]
        public void TestNoContinueTarget()
        {
            const string input = "{ continue; while(true) { continue; } for(;;) { continue; } continue; switch(a) { case 0: continue; } }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var container = new MessageContainer();
            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(messageContainer: container), body);

            analyzer.Analyze();

            // "Failed to detect mismatched continues correctly"
            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForContinueStatement));
        }

        #endregion
    }
}