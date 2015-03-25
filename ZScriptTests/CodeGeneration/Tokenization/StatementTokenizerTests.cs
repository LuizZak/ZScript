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

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization
{
    [TestClass]
    public class StatementTokenizerTests
    {
        #region Empty statement generation

        [TestMethod]
        public void TestEmptyStatementTokenGeneration()
        {
            const string message = "The tokens generated for the empty statement where not generated as expected";

            const string input = ";";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.statement();
            var generatedTokens = tokenizer.TokenizeStatement(stmt);

            // Empty statements should not generate any tokens
            var expectedTokens = new List<Token>();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #region Variable statement generation and execution

        #region Tokenization tests

        [TestMethod]
        public void TestValuedVarTokenGeneration()
        {
            const string message = "The tokens generated for the variable declare statement where not generated as expected";

            const string input = "var a = 10;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.valueDeclareStatement();
            var generatedTokens = tokenizer.TokenizeValueDeclareStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // Variable initial value
                TokenFactory.CreateBoxedValueToken(10L),
                // Variable name
                TokenFactory.CreateVariableToken("a", true),
                // Variable initial assignment
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestValuelessVarTokenGeneration()
        {
            const string message = "The tokens generated for the variable declare statement where not generated as expected";

            const string input = "var a;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.valueDeclareStatement();
            var generatedTokens = tokenizer.TokenizeValueDeclareStatement(stmt);

            // Valueless initializations generate no instructions
            var expectedTokens = new List<Token>();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestValuedLetTokenGeneration()
        {
            const string message = "The tokens generated for the constant declare statement where not generated as expected";

            const string input = "let a = 10;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.valueDeclareStatement();
            stmt.valueHolderDecl().Definition = new LocalVariableDefinition { IsConstant = true };
            var generatedTokens = tokenizer.TokenizeValueDeclareStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // Constant initial value
                TokenFactory.CreateBoxedValueToken(10L),
                // Constant name
                TokenFactory.CreateVariableToken("a", true),
                // Constant initial assignment
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestValuelessLetTokenGeneration()
        {
            const string message = "The tokens generated for the constant declare statement where not generated as expected";

            const string input = "let a;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.valueDeclareStatement();
            var generatedTokens = tokenizer.TokenizeValueDeclareStatement(stmt);

            // Valueless initializations generate no instructions
            var expectedTokens = new List<Token>();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestConstantLetTokenGeneration()
        {
            const string message = "The tokens generated for the constant declare statement where not generated as expected";

            const string input = "let a = 0;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.valueDeclareStatement();
            stmt.valueHolderDecl().expression().IsConstant = true;
            stmt.valueHolderDecl().expression().IsConstantPrimitive = true;
            stmt.valueHolderDecl().expression().ConstantValue = 0L;
            stmt.valueHolderDecl().Definition = new LocalVariableDefinition { IsConstant = true };

            var generatedTokens = tokenizer.TokenizeValueDeclareStatement(stmt);

            // Valueless initializations generate no instructions
            var expectedTokens = new List<Token>();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #endregion

        #region IF statement generation and execution

        #region Execution tests

        [TestMethod]
        public void TestIfCodeGeneration()
        {
            const string input = "func f() { var a = 0; if(a == 10) { a = 20; } }";

            var generator = TestUtils.CreateGenerator(input);

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

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);
        }

        [TestMethod]
        public void TestIfElseIfCodeGeneration()
        {
            const string input = "var a = 0; var b = 0; var c = 0; func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; } else { c = 10; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(0L, memory.GetVariable("a"));
            Assert.AreEqual(0L, memory.GetVariable("c"));
            Assert.AreEqual(5L, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestNestedIfCodeGeneration()
        {
            const string input = "var a = 0; var b = 0; var c = 0; func f() { if(5 == 10) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual(0L, memory.GetVariable("c"));
            Assert.AreEqual(5L, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestIfExpressionShortCircuit()
        {
            const string input = "var a = 0; var b = 0; var c:int = 0; var d:int = 0; func f() { if(5 == 10 && d > 0) { a = 20; } else if(11 > 10) { b = 5; if(b > 2) { a = 10; } else { c = 10; } } else { c = 10; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("a"), "The statement did not execute as expected");
            Assert.AreEqual(0L, memory.GetVariable("c"));
            Assert.AreEqual(5L, memory.GetVariable("b"), "The statement did not execute as expected");
        }

        #endregion

        #region Tokenization tests

        [TestMethod]
        public void TestIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

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
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }
        
        [TestMethod]
        public void TestIfElseTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else { c; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

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
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestIfElseIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else if(c) { d; } else { e; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd1    = new JumpTargetToken();
            var jEnd2    = new JumpTargetToken();
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
                new JumpToken(jEnd1),

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
                new JumpToken(jEnd2),

                // Else block
                jElse,
                TokenFactory.CreateVariableToken("e", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                // End of inner IF block
                jEnd2,
                // End of IF block
                jEnd1
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestNestedIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else if(c) { d; if(e) { f; } else { g; } } else { h; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.ifStatement();
            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd1 = new JumpTargetToken();
            var jEnd2 = new JumpTargetToken();
            var jElseIf1 = new JumpTargetToken();
            var jElse1 = new JumpTargetToken();

            var jInnerEnd = new JumpTargetToken();
            var jInnerElse = new JumpTargetToken();
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
                    new JumpToken(jInnerElse, true, false),

                        // If body
                        TokenFactory.CreateVariableToken("f", true),
                        TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                        // Jump to skip else block
                        new JumpToken(jInnerEnd),

                    // Else block
                    jInnerElse,

                        TokenFactory.CreateVariableToken("g", true),
                        TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),

                    // End of 2nd if block
                    jInnerEnd,

                    // Jump to skip the else block
                    new JumpToken(jEnd2),

                // Else block
                jElse1,

                    TokenFactory.CreateVariableToken("h", true),
                    TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                
                // End of second IF block
                jEnd2,
                // End of first IF block
                jEnd1
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests the behavior of the tokenizer with if statements that always evaluate to a constant true value
        /// </summary>
        [TestMethod]
        public void TestConstantTrueIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else { c; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.ifStatement();

            // Mark the statement as constantly evaluated as true
            stmt.IsConstant = true;
            stmt.ConstantValue = true;

            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // If body
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack)
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests the behavior of the tokenizer with if statements that always evaluate to a constant false value
        /// </summary>
        [TestMethod]
        public void TestConstantFalseIfTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else { c; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.ifStatement();

            // Mark the statement as constantly evaluated as true
            stmt.IsConstant = true;
            stmt.ConstantValue = false;

            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // Else body
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack)
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests the behavior of the tokenizer with if statements that always evaluate to a constant true/false value
        /// </summary>
        [TestMethod]
        public void TestConstantIfElseTokenGeneration()
        {
            const string message = "The tokens generated for the if statement where not generated as expected";

            const string input = "if(a) { b; } else if(c) { d; } else { e; }";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.ifStatement();

            // Mark the 'else if' statement as constantly evaluated as true
            stmt.elseStatement().statement().ifStatement().IsConstant = true;
            stmt.elseStatement().statement().ifStatement().ConstantValue = true;

            var generatedTokens = tokenizer.TokenizeIfStatement(stmt);

            // Create the expected list
            var jEnd1 = new JumpTargetToken();
            var jElseIf = new JumpTargetToken();
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
                new JumpToken(jEnd1),

                // Else if block
                jElseIf,

                // Else if body
                TokenFactory.CreateVariableToken("d", true),
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
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #endregion

        #region FOR statement generation and execution

        [TestMethod]
        public void TestForGeneration()
        {
            const string input = "var i = 0; var a = 0; func f() { for(i = 0; i < 10; i++) { a = i + 2; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestExpressionInitForGeneration()
        {
            const string input = "var i = 0; var a = 0; var b = 0; func f() { for(f2(); i < 10; i++) { a = i + 2; } } func f2() { b = 10; }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNoInitForGeneration()
        {
            const string input = "var i = 0; var a = 0; func f() { for(; i < 10; i++) { a = i + 2; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNoConditionForGeneration()
        {
            const string input = "var i = 0; var a = 0; func f() { for(i = 0; ; i++) { a = i + 2;  if(i == 9) { i++; break; } } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForBreakStatement()
        {
            const string input = "var i = 0; var a = 0; func f() { for(i = 0; i < 10; i++) { a = i + 2; break; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(2L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestForContinueStatement()
        {
            const string input = "var i = 0; var a = 0; var b = 0; func f() { for(i = 0; i < 10; i++) { a = i + 2; continue; b = a; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(11L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedForStatement()
        {
            const string input = "var i = 0; var a = 0; var b = 0; func f() { for(i = 0; i < 10; i++) { for(var j = 0; j < 10; j++) { a = i + j + 2; continue; b = a; } } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();
            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("i"));
            Assert.AreEqual(20L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region WHILE statement generation and execution

        [TestMethod]
        public void TestWhileCodeGeneration()
        {
            const string input = "var a = 0; func f() { while(a < 10) { a++; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestWhileContinueStatement()
        {
            const string input = "var a = 0; var b = 0; func f() { while(a < 10) { a++; continue; b = 10; } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(0L, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestWhileBreakStatement()
        {
            const string input = "var a = 0; func f() { while(true) { a++; if(a >= 10) { break; } } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
        }

        [TestMethod]
        public void TestNestedWhileStatement()
        {
            const string input = "var a = 0; var b = 0; func f() { while(a < 10) { a++; while(b / a < 10) { b++; } } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(100L, runtime.GlobalMemory.GetVariable("b"));
        }

        [TestMethod]
        public void TestNestedWhileBreakStatement()
        {
            const string input = "var a = 0; var b = 0; func f() { while(a < 10) { a++; while(b / a < 10) { b++; if(b > 6) { break; } } } }";

            var generator = TestUtils.CreateGenerator(input);

            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual(10L, runtime.GlobalMemory.GetVariable("a"));
            Assert.AreEqual(16L, runtime.GlobalMemory.GetVariable("b"));
        }

        #endregion

        #region RETURN statement generation and execution

        [TestMethod]
        public void TestReturnStatement()
        {
            const string input = "func f() : int { return 10+10; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.AreEqual(20L, ret, "The valued return statement did not execute as expected");
        }

        [TestMethod]
        public void TestValuelessReturnStatement()
        {
            const string input = "func f() { return; }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);

            var ret = runtime.CallFunction("f");

            Assert.IsNull(ret, "The valueless return statement did not execute as expected");
        }

        #region Tokenization tests

        [TestMethod]
        public void TestValuelessdReturnTokenGeneration()
        {
            const string message = "The tokens generated for the return statement where not generated as expected";

            const string input = "return;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.returnStatement();
            var generatedTokens = tokenizer.TokenizeReturnStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // Return instruction
                TokenFactory.CreateInstructionToken(VmInstruction.Ret)
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestValuedReturnTokenGeneration()
        {
            const string message = "The tokens generated for the return statement where not generated as expected";

            const string input = "return 10 + 10;";

            var parser = TestUtils.CreateParser(input);
            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.returnStatement();
            var generatedTokens = tokenizer.TokenizeReturnStatement(stmt);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                // Return expression
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                // Return instruction
                TokenFactory.CreateInstructionToken(VmInstruction.Ret)
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #endregion

        #region SWITCH statement generation and execution

        [TestMethod]
        public void TestSwitchStatement()
        {
            const string input = "var a:int = 5; var b:int = 20; var c:int? = null; func f() { switch(a + b) { case 25: c = 10; break; case 30: c = 5; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestDefaultSwitchStatement()
        {
            const string input = "var c:int? = null; var d:int? = null; func f() { var a = 5; var b = 20; switch(a + b) { case 10: c = 5; break; default: c = 10; break; } switch(a + b) { case 25: d = 10; break; default: d = 5; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("c"), "The statement did not execute as expected");
            Assert.AreEqual(10L, memory.GetVariable("d"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestMultipleCasesSwitchStatement()
        {
            const string input = "var a = 5; var b = 20; var c; func f() { switch(a + b) { case 10: case 25: case 30: c = 10; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestValuedSwitchStatement()
        {
            const string input = "var a = 5; var b = 20; var d; func f() { switch(let c = a + b) { case 10: case 25: case 30: d = c; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(25L, memory.GetVariable("d"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestConstantSwitchStatement()
        {
            const string input = "let a = 5; let b = 20; var c:int? = null; func f() { switch(a + b) { case 25: c = 10; break; case 30: c = 5; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(10L, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        [TestMethod]
        public void TestConstantDefaultSwitchStatement()
        {
            const string input = "let a = 5; let b = 20; var c:int? = null; func f() { switch(a + b) { case 30: c = 10; break; case 31: c = 5; break; default: c = 1; break; } }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();

            Assert.IsFalse(generator.HasSyntaxErrors);

            // Generate the runtime now
            var owner = new TestRuntimeOwner();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            Assert.AreEqual(1L, memory.GetVariable("c"), "The statement did not execute as expected");
        }

        #region Tokenization tests

        [TestMethod]
        public void TestBasicSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(a) { case 10: a; break; case 11: b; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();
            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var jtCase1 = new JumpTargetToken();
            var jtCase2 = new JumpTargetToken();
            var jtDefault = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // Switch expression
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #1
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // True condition jump
                new JumpToken(jtCase1, true),

                // Duplicate switch expression before entering case condition #2
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #2
                TokenFactory.CreateBoxedValueToken(11L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // False condition jump
                new JumpToken(jtCase2, true),

                // No case matched: default jump
                new JumpToken(jtDefault),

                // Case body #1
                jtCase1,
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Case body #2
                jtCase2,
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Default body
                jtDefault,

                // End of Switch block
                jtEnd,

                // Stack balancing call
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestStaticTrueSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(a) { case 10: a; break; case 11: b; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();

            // Staticize the switch statement
            stmt.IsConstant = true;
            stmt.ConstantCaseIndex = 0;

            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                new JumpToken(jtEnd),
                jtEnd
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestStaticFalseDefaultlessSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(a) { case 10: a; break; case 11: b; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();

            // Staticize the switch statement
            stmt.IsConstant = true;
            stmt.ConstantCaseIndex = -1;

            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var expectedTokens = new List<Token> { jtEnd };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestStaticFalseDefaultedSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(a) { case 10: a; break; case 11: b; break; default: c; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();

            // Staticize the switch statement
            stmt.IsConstant = true;
            stmt.ConstantCaseIndex = -1;

            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                new JumpToken(jtEnd),
                jtEnd,
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestDefaultSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(a) { case 10: a; break; case 11: b; break; default: c; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();
            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var jtCase1 = new JumpTargetToken();
            var jtCase2 = new JumpTargetToken();
            var jtDefault = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // Switch expression
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #1
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // True condition jump
                new JumpToken(jtCase1, true),

                // Duplicate switch expression before entering case condition #2
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #2
                TokenFactory.CreateBoxedValueToken(11L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // False condition jump
                new JumpToken(jtCase2, true),

                // No case matched: default jump
                new JumpToken(jtDefault),

                // Case body #1
                jtCase1,
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Case body #2
                jtCase2,
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Default body
                jtDefault,
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // End of Switch block
                jtEnd,
                // Stack balancing call
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        [TestMethod]
        public void TestValuedSwitchStatementGeneration()
        {
            const string message = "Faild to produce expected tokens for the switch statement";

            const string input = "switch(let a = b) { case 10: a; break; case 11: b; break; default: c; break; }";
            var parser = TestUtils.CreateParser(input);

            var tokenizer = new StatementTokenizerContext(new RuntimeGenerationContext());

            var stmt = parser.switchStatement();
            var generatedTokens = tokenizer.TokenizeSwitchStatement(stmt);

            // Create the expected list
            var jtEnd = new JumpTargetToken();
            var jtCase1 = new JumpTargetToken();
            var jtCase2 = new JumpTargetToken();
            var jtDefault = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // Switch expression
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #1
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // True condition jump
                new JumpToken(jtCase1, true),

                // Duplicate switch expression before entering case condition #2
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),

                // Case condition #2
                TokenFactory.CreateBoxedValueToken(11L),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                // False condition jump
                new JumpToken(jtCase2, true),

                // No case matched: default jump
                new JumpToken(jtDefault),

                // Case body #1
                jtCase1,
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Case body #2
                jtCase2,
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // Default body
                jtDefault,
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                // End jump
                new JumpToken(jtEnd),

                // End of Switch block
                jtEnd,

                // Stack balancing call
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        #endregion
    }
}