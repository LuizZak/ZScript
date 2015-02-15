using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Runtime;

namespace ZScriptTests.CodeGeneration.Tokenizers
{
    [TestClass]
    public class StatementTokenizerTests
    {
        #region IF statement generation and execution

        #region Execution tests

        [TestMethod]
        public void TestIfCodeGeneration()
        {
            const string input = "func f() { var a = 0; if(a == 10) { a = 20; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseCodeGeneration()
        {
            const string input = "func f() { var a = 0; var b = 0; if(a == 10) { a = 20; } else { b = 5; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseIfCodeGeneration()
        {
            const string input = "[ a = 0; b = 0; c = 0; ] func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)0, memory.GetVariable("a"));
            Assert.AreEqual((long)0, memory.GetVariable("c"));
            Assert.AreEqual((long)5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestNestedIfCodeGeneration()
        {
            const string input = "[ a = 0; b = 0; c = 0; ] func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual((long)0, memory.GetVariable("c"));
            Assert.AreEqual((long)5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestIfExpressionShortCircuit()
        {
            const string input = "[ a = 0; b = 0; c = 0; d = null; ] func f() { if(5 == 10 && d > 0) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual((long)0, memory.GetVariable("c"));
            Assert.AreEqual((long)5, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        #endregion

        #region Tokenization tests

        [TestMethod]
        public void TestIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; }";

            var parser = ZRuntimeTests.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(null, null);

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jt = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // If expression
                TokenFactory.CreateVariableToken("a", true),
                // False condition jump
                new JumpToken(jt, true, false),
                // If body
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End of IF block
                jt
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            PostfixExpressionTokenizerTests.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }
        
        [TestMethod]
        public void TestIfElseTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else { c; }";

            var parser = ZRuntimeTests.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(null, null);

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd = new JumpTargetToken();
            var jElse = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // If expression
                TokenFactory.CreateVariableToken("a", true),
                // False condition jump
                new JumpToken(jElse, true, false),

                // If body
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                // Jump to skip the else block
                new JumpToken(jEnd),

                // Else block
                jElse,
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End of IF block
                jEnd
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            PostfixExpressionTokenizerTests.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestIfElseIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else if(c) { d; } else { e; }";

            var parser = ZRuntimeTests.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(null, null);

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd    = new JumpTargetToken();
            var jElseIf = new JumpTargetToken();
            var jElse   = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // If expression
                TokenFactory.CreateVariableToken("a", true),
                // False condition jump
                new JumpToken(jElseIf, true, false),

                // If body
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // Jump to skip the else block
                new JumpToken(jEnd),

                // Else if block
                jElseIf,
                // Else if expression
                TokenFactory.CreateVariableToken("c", true),
                // False condition jump
                new JumpToken(jElse, true, false),
                // Else if body
                TokenFactory.CreateVariableToken("d", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // Jump to skip the else block
                new JumpToken(jEnd),

                // Else block
                jElse,
                TokenFactory.CreateVariableToken("e", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                // End of IF block
                jEnd
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            PostfixExpressionTokenizerTests.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestNestedIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else if(c) { d; if(e) { f; } else { g; } } else { h; }";

            var parser = ZRuntimeTests.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(null, null);

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd1 = new JumpTargetToken();
            var jElseIf1 = new JumpTargetToken();
            var jElse1 = new JumpTargetToken();

            var jEnd2 = new JumpTargetToken();
            var jElse2 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // If expression
                TokenFactory.CreateVariableToken("a", true),
                // False condition jump
                new JumpToken(jElseIf1, true, false),

                // If body
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // Jump to skip the else block
                new JumpToken(jEnd1),

                // Else if block
                jElseIf1,
                // Else if expression
                TokenFactory.CreateVariableToken("c", true),
                // False condition jump
                new JumpToken(jElse1, true, false),
                // Else if body
                
                    TokenFactory.CreateVariableToken("d", true),
                    TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                    // 2nd if expression
                    TokenFactory.CreateVariableToken("e", true),
                    // False condition jump
                    new JumpToken(jElse2, true, false),
                    // If body
                    TokenFactory.CreateVariableToken("f", true),
                    TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                    // Jump to skip else block
                    new JumpToken(jEnd2),

                    // Else block
                    jElse2,

                    TokenFactory.CreateVariableToken("g", true),
                    TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                    // End of 2nd if block
                    jEnd2,

                // Jump to skip the else block
                new JumpToken(jEnd1),

                // Else block
                jElse1,
                TokenFactory.CreateVariableToken("h", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                // End of IF block
                jEnd1
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            PostfixExpressionTokenizerTests.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #endregion

        #region FOR statement generation and execution

        [TestMethod]
        public void TestForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNoInitForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(; i < 10; i++) { a = i + 2; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNoConditionForGeneration()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; ; i++) { a = i + 2;  if(i == 9) { i++; break; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)11, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForBreakStatement()
        {
            const string input = "[ i = 0; a = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; break; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)0, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)2, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForContinueStatement()
        {
            const string input = "[ i = 0; a = 0; b = 0; ] func f() { for(i = 0; i < 10; i++) { a = i + 2; continue; b = a; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)11, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual((long)0, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedForStatement()
        {
            const string input = "[ i = 0; a = 0; b = 0; ] func f() { for(i = 0; i < 10; i++) { for(var j = 0; j < 10; j++) { a = i + j + 2; continue; b = a; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual((long)20, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual((long)0, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region WHILE statement generation and execution

        [TestMethod]
        public void TestWhileCodeGeneration()
        {
            const string input = "[ a = 0; ] func f() { while(a < 10) { a++; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestWhileContinueStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; continue; b = 10; } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual((long)0, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestWhileBreakStatement()
        {
            const string input = "[ a = 0; ] func f() { while(true) { a++; if(a >= 10) { break; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNestedWhileStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; while(b / a < 10) { b++; } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual((long)100, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedWhileBreakStatement()
        {
            const string input = "[ a = 0; b = 0; ] func f() { while(a < 10) { a++; while(b / a < 10) { b++; if(b > 6) { break; } } } }";

            var generator = ZRuntimeTests.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual((long)16, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region RETURN statement generation and execution

        [TestMethod]
        public void TestReturnStatement()
        {
            const string input = "func f() { return 10+10; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.AreEqual((long)20, ret, "The valued return statement did not execute as expected");
        }

        [TestMethod]
        public void TestValuelessReturnStatement()
        {
            const string input = "func f() { return; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.IsNull(ret, "The valueless return statement did not execute as expected");
        }

        #endregion

        #region SWITCH statement generation and execution

        [TestMethod]
        public void TestSwitchStatement()
        {
            const string input = "[ a = 5; b = 20; c = null; ] func f() { switch(a + b) { case 25: c = 10; break; case 30: c = 5; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestDefaultSwitchStatement()
        {
            const string input = "[ c = null; d = null; ] func f() { var a = 5; var b = 20; switch(a + b) { case 10: c = 5; break; default: c = 10; break; } switch(a + b) { case 25: d = 10; break; default: d = 5; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, memory.GetVariable("c"), "The statement did not execute as expected");
            Assert.AreEqual((long)10, memory.GetVariable("d"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestMultipleCasesSwitchStatement()
        {
            const string input = "[ a = 5; b = 20; c; ] func f() { switch(a + b) { case 10: case 25: case 30: c = 10; break; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        #endregion

        #region BREAK/CONTINUE statement error raising

        [TestMethod]
        public void TestMismatchedBreakStatementError()
        {
            const string input = "[ a = 5; b = 20; c = null; ] func f() { break; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;

            // Collect definitions now
            generator.CollectDefinitions();

            Assert.IsFalse(generator.HasSyntaxErrors);
            Assert.AreEqual(0, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForBreakStatement), "Faild to raise expected errors");
        }

        [TestMethod]
        public void TestMismatchedContinueStatementError()
        {
            const string input = "[ a = 5; b = 20; c = null; ] func f() { continue; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;

            // Collect definitions now
            generator.CollectDefinitions();

            Assert.IsFalse(generator.HasSyntaxErrors);
            Assert.AreEqual(0, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoTargetForContinueStatement), "Faild to raise expected errors");
        }

        #endregion
    }
}