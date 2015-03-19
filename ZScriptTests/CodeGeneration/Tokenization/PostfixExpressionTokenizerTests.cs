﻿#region License information
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

using Xunit;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization
{
    /// <summary>
    /// Tests the functionality of the PostfixExpressionTokenizer
    /// </summary>
    public class PostfixExpressionTokenizerTests
    {
        #region Primary expression parsing

        /// <summary>
        /// Tests generation of primary 'this' expressions
        /// </summary>
        [Fact]
        public void TestThisPrimary()
        {
            const string message = "The tokens generated for the 'this' primary expression where not generated as expected";

            const string input = "this.field";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("this", true),
                TokenFactory.CreateMemberNameToken("field"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests generation of assignments to fields of 'this' constants
        /// </summary>
        [Fact]
        public void TestAssignThisField()
        {
            const string message = "The tokens generated for the 'this' field assignment where not generated as expected";

            const string input = "this.field = 10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.assignmentExpression();

            var generatedTokens = tokenizer.TokenizeAssignmentExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateVariableToken("this", true),
                TokenFactory.CreateMemberNameToken("field"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
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

        /// <summary>
        /// Tests generation of primary 'base' expressions
        /// </summary>
        [Fact]
        public void TestBasePrimary()
        {
            const string message = "The tokens generated for the 'base' primary expression where not generated as expected";

            const string input = "base()";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("base", true),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
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

        #region New expression

        /// <summary>
        /// Tests generation of a new expression
        /// </summary>
        [Fact]
        public void TestNewExpression()
        {
            const string message = "The tokens generated for the new expression where not generated as expected";

            const string input = "new Test(1, 2)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("Test"),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateBoxedValueToken(2L),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.New),
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

        #region Assignment expression

        /// <summary>
        /// Tests generation of an assignment expression within an expression
        /// </summary>
        [Fact]
        public void TestAssignmentExpression()
        {
            const string message = "The tokens generated for the assignment expression where not generated as expected";

            const string input = "(a = 2 + 2)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(2L),
                TokenFactory.CreateBoxedValueToken(2L),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateVariableToken("a", false),
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

        /// <summary>
        /// Tests generation of a nested assignment expression
        /// </summary>
        [Fact]
        public void TestSequentialAssignment()
        {
            const string message = "The tokens generated for the assignment expression where not generated as expected";

            const string input = "(a = b = c)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("c", true),
                TokenFactory.CreateVariableToken("b", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateVariableToken("a", false),
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

        /// <summary>
        /// Tests generation of a nested assignment expression
        /// </summary>
        [Fact]
        public void TestSequentialCompoundAssignment()
        {
            const string message = "The tokens generated for the assignment expression where not generated as expected";

            const string input = "(a += b -= c)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            /*
                0000000: a
                0000001: Duplicate
                0000002: b
                0000003: Duplicate
                0000004: c
                0000005: Swap
                0000006: Subtract
                0000007: Swap
                0000008: Set
                0000009: Swap
                0000010: Add
                0000011: Swap
                0000012: Set
            */

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),
                TokenFactory.CreateVariableToken("b", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),
                TokenFactory.CreateVariableToken("c", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateOperatorToken(VmInstruction.Subtract),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
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

        #region Left Value expression

        /// <summary>
        /// Tests generation of a left value expression which has a field access
        /// </summary>
        [Fact]
        public void TestLeftValueFieldAccess()
        {
            const string message = "The tokens generated for the left value where not generated as expected";

            const string input = "a.b.c++";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
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
        /// Tests generation of a left value expression which has a function access
        /// </summary>
        [Fact]
        public void TestLeftValueFunctionAccess()
        {
            const string message = "The tokens generated for the left value where not generated as expected";

            const string input = "a().b().c++";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
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
        /// Tests generation of a left value expression which has a function access
        /// </summary>
        [Fact]
        public void TestLeftValueArrayAccess()
        {
            const string message = "The tokens generated for the left value where not generated as expected";

            const string input = "a[0].b()[0]++";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
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

        #region Ternary/binary/unary/prefix/postfix

        #region Prefix/postfix

        /// <summary>
        /// Tests generation of a decrement prefix operation
        /// </summary>
        [Fact]
        public void TestDecrementPrefixOpertion()
        {
            const string message = "The tokens generated for the prefix decrement expression where not generated as expected";

            const string input = "--a";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.DecrementPrefix),
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
        /// Tests generation of a increment prefix operation
        /// </summary>
        [Fact]
        public void TestIncrementPrefixOpertion()
        {
            const string message = "The tokens generated for the prefix increment expression where not generated as expected";

            const string input = "++a";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPrefix),
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
        /// Tests generation of a decrement postfix operation
        /// </summary>
        [Fact]
        public void TestDecrementPostixOpertion()
        {
            const string message = "The tokens generated for the postfix decrement expression where not generated as expected";

            const string input = "a--";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.DecrementPostfix),
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
        /// Tests generation of a increment postfix operation
        /// </summary>
        [Fact]
        public void TestIncrementPostfixOpertion()
        {
            const string message = "The tokens generated for the postfix increment expression where not generated as expected";

            const string input = "a++";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
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

        #region Ternary operator

        /// <summary>
        /// Tests generation of ternary operation
        /// </summary>
        [Fact]
        public void TestSimpleTernaryOperator()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jEnd = new JumpTargetToken();
            var jFalse = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                new JumpToken(jFalse, true, false),
                TokenFactory.CreateBoxedValueToken(1L),
                new JumpToken(jEnd),
                jFalse,
                TokenFactory.CreateBoxedValueToken(2L),
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

        /// <summary>
        /// Tests generation of nested ternary operation
        /// </summary>
        [Fact]
        public void TestNestedTernaryOperatorLeftSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 ? 2 : 3 : 4";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jEnd1 = new JumpTargetToken();
            var jFalse1 = new JumpTargetToken();

            var jEnd2 = new JumpTargetToken();
            var jFalse2 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // 1st ternary expression
                TokenFactory.CreateBoxedValueToken(0L),
                // False jump
                new JumpToken(jFalse1, true, false),

                // 1st ternary true
                    // 2nd ternary expression
                    TokenFactory.CreateBoxedValueToken(1L),
                    // False jump
                    new JumpToken(jFalse2, true, false),
                    
                    // 2nd ternary true
                    TokenFactory.CreateBoxedValueToken(2L),
                    // End jump
                    new JumpToken(jEnd2),

                    // 1st ternary false
                    jFalse2,
                    TokenFactory.CreateBoxedValueToken(3L),
                    jEnd2,

                // End jump
                new JumpToken(jEnd1),

                // 1st ternary false
                jFalse1,
                TokenFactory.CreateBoxedValueToken(4L),
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
        /// Tests generation of nested ternary operation
        /// </summary>
        [Fact]
        public void TestNestedTernaryOperatorRightSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2 ? 3 : 4";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jEnd1 = new JumpTargetToken();
            var jFalse1 = new JumpTargetToken();

            var jEnd2 = new JumpTargetToken();
            var jFalse2 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // 1st ternary's condition
                TokenFactory.CreateBoxedValueToken(0L),
                new JumpToken(jFalse1, true, false),
                // 1st ternary's left side
                TokenFactory.CreateBoxedValueToken(1L),
                new JumpToken(jEnd1),

                // 1st ternary's right side
                jFalse1,
                    // 2nd ternary's condition 
                    TokenFactory.CreateBoxedValueToken(2L),
                    new JumpToken(jFalse2, true, false),

                    // 2nd ternary's left side
                    TokenFactory.CreateBoxedValueToken(3L),
                    new JumpToken(jEnd2),

                    jFalse2,
                    // 2nd ternary's right side
                    TokenFactory.CreateBoxedValueToken(4L),
                    jEnd2,

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
        /// Tests generation of nested ternary operation
        /// </summary>
        [Fact]
        public void TestTernaryExecution()
        {
            const string input = "var a = 1; var b = 2; var c = 3; var d:int; func f1() { d = !true ? a : !true ? b : c;  }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.Equal(0, container.CodeErrors.Length); // "Errors where detected when not expected"
            Assert.Equal(3L, memory.GetVariable("d")); // "Ternary operator did not behave as expected"
        }

        #endregion

        /// <summary>
        /// Tests generation of an expression that is parenthesized
        /// </summary>
        [Fact]
        public void TestParenthesizedExpression()
        {
            const string message = "The tokens generated for the parenthesized expression where not generated as expected";

            const string input = "((5 + 5) * (((7))))";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(5L),
                TokenFactory.CreateBoxedValueToken(5L),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateBoxedValueToken(7L),
                TokenFactory.CreateOperatorToken(VmInstruction.Multiply),
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
        /// Tests generation of an expression that is parenthesized and is followed by an access
        /// </summary>
        [Fact]
        public void TestParenthesizedExpressionAccess()
        {
            const string message = "The tokens generated for the parenthesized expression access where not generated as expected";

            const string input = "(('' + '').Length)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken(""),
                TokenFactory.CreateStringToken(""),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateMemberNameToken("Length"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests generation of an arithmetic negate operation
        /// </summary>
        [Fact]
        public void TestArithmeticNegate()
        {
            const string message = "The tokens generated for the arithmetic negate expression where not generated as expected";

            const string input = "-a";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.ArithmeticNegate),
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
        /// Tests generation of a logical negate operation
        /// </summary>
        [Fact]
        public void TestLogicalNegate()
        {
            const string message = "The tokens generated for the logical negate expression where not generated as expected";

            const string input = "!a";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalNegate),
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

        #region Implicit casting

        /// <summary>
        /// Tests generation of implicit cast operations on expressions containing a non-null ImplicitCastType entry
        /// </summary>
        [Fact]
        public void TestImplicitIntegerCast()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Setup the implicit cast
            exp.EvaluatedType = TypeDef.IntegerType;
            exp.ImplicitCastType = TypeDef.FloatType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Cast, exp.ImplicitCastType),
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
        /// Tests non-generation of implicit cast operations on expressions when an expression has an expected type of 'any'
        /// </summary>
        [Fact]
        public void TestImplicitAnyCast()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Setup the implicit cast
            exp.EvaluatedType = TypeDef.IntegerType;
            exp.ImplicitCastType = TypeDef.AnyType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L)
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
        /// Tests non-generation of implicit cast operations on expressions when an expression is already of the desired type
        /// </summary>
        [Fact]
        public void TestNoImplicitCasting()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Setup the implicit cast
            exp.EvaluatedType = TypeDef.IntegerType;
            exp.ImplicitCastType = TypeDef.IntegerType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L)
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
        /// Tests implicit casts that have expressions nested within
        /// </summary>
        [Fact]
        public void TestNestedImplicitCasting()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "true ? 1 : 0";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Setup the implicit cast
            exp.EvaluatedType = TypeDef.IntegerType;
            exp.ImplicitCastType = TypeDef.FloatType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jtt1 = new JumpTargetToken();
            var jtt2 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(true),
                new JumpToken(jtt1, true, false),
                TokenFactory.CreateBoxedValueToken(1L),
                new JumpToken(jtt2),
                jtt1,
                TokenFactory.CreateBoxedValueToken(0L),
                jtt2,
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Cast, exp.ImplicitCastType),
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
        /// Tests generation of type check against values of 'any' type
        /// </summary>
        [Fact]
        public void TestAnyTypeCheck()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Setup the implicit cast
            exp.EvaluatedType = TypeDef.AnyType;
            exp.ImplicitCastType = TypeDef.IntegerType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CheckType, typeof(long))
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

        #region Array literal

        /// <summary>
        /// Tests tokenization of an array literal literal
        /// </summary>
        [Fact]
        public void TestArrayLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[0, 1, 2]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.arrayLiteral().EvaluatedValueType = TypeDef.IntegerType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateBoxedValueToken(2L),
                TokenFactory.CreateBoxedValueToken(3),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(long))
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
        /// Tests tokenization of an empty array literal
        /// </summary>
        [Fact]
        public void TestEmptyArrayLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.arrayLiteral().EvaluatedValueType = TypeDef.IntegerType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(long))
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
        /// Tests exception raising when tokenizing an array literal with no EvaluatedValueType set
        /// </summary>
        [Fact]
        public void TestArrayMissingValueKeyException()
        {
            const string input = "[0, 1, 2]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            Assert.Throws<InvalidOperationException>(() => tokenizer.TokenizeExpression(exp));
        }

        #endregion

        #region Dictionary literal

        /// <summary>
        /// Tests tokenization of a dictionary literal
        /// </summary>
        [Fact]
        public void TestDictionaryLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[0:'abc', 1:'def']";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedKeyType = TypeDef.IntegerType;
            exp.dictionaryLiteral().EvaluatedValueType = TypeDef.StringType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateStringToken("abc"),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateStringToken("def"),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(long), typeof(string) })
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
        /// Tests tokenization of an empty dictionary literal
        /// </summary>
        [Fact]
        public void TestEmptyDictionaryLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[:]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedKeyType = TypeDef.IntegerType;
            exp.dictionaryLiteral().EvaluatedValueType = TypeDef.StringType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(long), typeof(string) })
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
        /// Tests tokenization of a dictionary literal followed by a subscript access
        /// </summary>
        [Fact]
        public void TestSubscriptDictionaryLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[0:'abc', 1:'def'][0]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedKeyType = TypeDef.IntegerType;
            exp.dictionaryLiteral().EvaluatedValueType = TypeDef.StringType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateStringToken("abc"),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateStringToken("def"),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(long), typeof(string) }),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests tokenization of a dictionary literal followed by a member access
        /// </summary>
        [Fact]
        public void TestMemberAccessDictionaryLiteral()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "[0:'abc', 1:'def'].Count";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedKeyType = TypeDef.IntegerType;
            exp.dictionaryLiteral().EvaluatedValueType = TypeDef.StringType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateStringToken("abc"),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateStringToken("def"),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(long), typeof(string) }),
                TokenFactory.CreateMemberNameToken("Count"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests exception raising when tokenizing a dictionary literal with no EvaluatedKeyType set
        /// </summary>
        [Fact]
        public void TestDictionaryMissingKeyTypeException()
        {
            const string input = "[0:'abc', 1:'def']";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedValueType = TypeDef.StringType;

            Assert.Throws<InvalidOperationException>(() => tokenizer.TokenizeExpression(exp));
        }

        /// <summary>
        /// Tests exception raising when tokenizing a dictionary literal with no EvaluatedValueType set
        /// </summary>
        [Fact]
        public void TestDictionaryMissingValueTypeException()
        {
            const string input = "[0:'abc', 1:'def']";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Provide the type for the expression
            exp.dictionaryLiteral().EvaluatedKeyType = TypeDef.StringType;

            Assert.Throws<InvalidOperationException>(() => tokenizer.TokenizeExpression(exp));
        }

        #endregion

        #region Cast instruction and 'is' operator

        /// <summary>
        /// Tests generation of cast operation
        /// </summary>
        [Fact]
        public void TestCastOperation()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "(bool)10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Cast, exp.type()),
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
        /// Tests generation of 'is' operator
        /// </summary>
        [Fact]
        public void TestIsOperator()
        {
            const string message = "The tokens generated for the 'is' operation where not generated as expected";

            const string input = "10 is int";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, exp.type()),
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

        #region Logical operator short-circuit

        /// <summary>
        /// Tests OR logical operator short circuiting
        /// </summary>
        [Fact]
        public void TestSimpleOrShortcircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a || b";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jt1 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // a
                TokenFactory.CreateVariableToken("a", true),
                // OR short-circuit jump
                new JumpToken(jt1, true, true, false),
                // b
                TokenFactory.CreateVariableToken("b", true),
                // OR operator
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalOr),
                jt1
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
        /// Tests AND logical operator short circuiting
        /// </summary>
        [Fact]
        public void TestSimpleAndShortcircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a && b";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jt1 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // a
                TokenFactory.CreateVariableToken("a", true),
                // OR short-circuit jump
                new JumpToken(jt1, true, false, false),
                // b
                TokenFactory.CreateVariableToken("b", true),
                // OR operator
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalAnd),
                jt1
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
        /// Tests generation of mixed AND and OR logical operator short circuiting
        /// </summary>
        [Fact]
        public void TestMixedOrAndShortCircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a || b && c";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jt1 = new JumpTargetToken();
            var jt2 = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                // a
                TokenFactory.CreateVariableToken("a", true),
                // OR short-circuit jump
                new JumpToken(jt1, true, true, false),
                // b
                TokenFactory.CreateVariableToken("b", true),
                // OR short-circuit jump
                new JumpToken(jt2, true, false, false),
                // c
                TokenFactory.CreateVariableToken("c", true),
                // AND operator
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalAnd),
                jt2,
                // OR operator
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalOr),
                jt1
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

        #region Constant propagation

        /// <summary>
        /// Tests postfix expression parsing analyzing constants in expression nodes to generate optimized token lists
        /// </summary>
        [Fact]
        public void TestNumberConstantPropagation()
        {
            const string message = "The constant was not propagated as expected";

            const string input = "10 + 10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Manually insert a constant on the expression
            exp.IsConstant = true;
            exp.IsConstantPrimitive = true;
            exp.ConstantValue = 20L;
            exp.EvaluatedType = TypeDef.IntegerType;
            
            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(20L),
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
        /// Tests postfix expression parsing analyzing constants in expression nodes to generate optimized token lists
        /// </summary>
        [Fact]
        public void TestStringConstantPropagation()
        {
            const string message = "The constant was not propagated as expected";

            const string input = "'abc' + 'abc'";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            // Manually insert a constant on the expression
            exp.IsConstant = true;
            exp.IsConstantPrimitive = true;
            exp.ConstantValue = "abcabc";
            exp.EvaluatedType = TypeDef.StringType;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("abcabc"),
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

        #region Value access

        /// <summary>
        /// Tests member access-type access token generation
        /// </summary>
        [Fact]
        public void TestMemberAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.Length";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateMemberNameToken("Length"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get)
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
        /// Tests chained member access-type access token generation
        /// </summary>
        [Fact]
        public void TestChainedMemberAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.Length.Length.A";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateMemberNameToken("Length"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateMemberNameToken("Length"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateMemberNameToken("A"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests function call-type access token generation
        /// </summary>
        [Fact]
        public void TestFunctionCall()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.ToString()";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateMemberNameToken("ToString"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call)
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
        /// Tests chained function call-type access token generation
        /// </summary>
        [Fact]
        public void TestChainedFunctionCall()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.ToString().IndexOf('1')";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateMemberNameToken("ToString"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateMemberNameToken("IndexOf"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateStringToken("1"),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.Call)
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
        /// Tests subscript-type access token generation
        /// </summary>
        [Fact]
        public void TestSubscriptAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'[0]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
        /// Tests chained subscript-type access token generation
        /// </summary>
        [Fact]
        public void TestChainedSubscriptAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'[0][1]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateStringToken("a"),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
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
    }
}