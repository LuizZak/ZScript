using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the ControlFlowAnalyzer class and related components
    /// </summary>
    [TestClass]
    public class ControlFlowAnalyzerTests
    {
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

            Assert.IsTrue(analyzer.EndReachable);
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

            Assert.IsTrue(analyzer.EndReachable);

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

            Assert.IsFalse(analyzer.EndReachable);

            Assert.IsTrue(body.blockStatement().statement(0).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(1).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsTrue(body.blockStatement().statement(2).Reachable, "Failed mark the statement reachability correctly");
            Assert.IsFalse(body.blockStatement().statement(3).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests analyzing a simple branched flow by analyzing an if statement
        /// </summary>
        [TestMethod]
        public void TestSimpleBranchedFlow()
        {
            const string input = "{ var a; var b; if(a) return; var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable);

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
        public void TestBlockedBranchedFlow()
        {
            const string input = "{ var a; var b; if(a) { var d; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable);

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
        public void TestBlockedBranchedFlowInterrupted()
        {
            const string input = "{ var a; var b; if(a) { var d; return; var e; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable);

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

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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

            Assert.IsFalse(analyzer.EndReachable, "Failed to detect correct reachability for end");

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

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(whileLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

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

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var whileLoop1 = blockBody.statement(0).whileStatement().statement().blockStatement();

            Assert.IsTrue(whileLoop1.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(whileLoop1.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(2).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow analysis in for loops
        /// </summary>
        [TestMethod]
        public void TestForLoopInterruptStatement()
        {
            const string input = "{ for(;;) { var a; return; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

            var blockBody = body.blockStatement();

            Assert.IsTrue(blockBody.statement(0).Reachable, "Failed mark the statement reachability correctly");

            var forLoop = blockBody.statement(0).forStatement().statement().blockStatement();

            Assert.IsTrue(forLoop.statement(0).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsTrue(forLoop.statement(1).Reachable, "Failed mark the inner-loop statement reachability correctly");
            Assert.IsFalse(forLoop.statement(2).Reachable, "Failed mark the inner-loop statement reachability correctly");

            Assert.IsTrue(blockBody.statement(1).Reachable, "Failed mark the statement reachability correctly");
        }

        /// <summary>
        /// Tests flow breaking with the break statement
        /// </summary>
        [TestMethod]
        public void TestForLoopBreakStatement()
        {
            const string input = "{ for(;;) { var a; break; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
            const string input = "{ for(;;) { var a; if(a) { break; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
            const string input = "{ for(;;) { var a; continue; var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
            const string input = "{ for(;;) { var a; if(a) { continue; } var b; } var c; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
            const string input = "{ for(;;) { for(;;) { var a; break; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
            const string input = "{ for(;;) { for(;;) { var a; continue; var b; } var c; } var d; }";
            var parser = TestUtils.CreateParser(input);

            var body = parser.functionBody();

            var analyzer = new ControlFlowAnalyzer(new RuntimeGenerationContext(), body);

            analyzer.Analyze();

            Assert.IsTrue(analyzer.EndReachable, "Failed to detect correct reachability for end");

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
    }
}