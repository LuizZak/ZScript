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

using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization
{
    /// <summary>
    /// Tests the functionality of the PostfixExpressionTokenizer
    /// </summary>
    [TestClass]
    public class PostfixExpressionTokenizerTests
    {
        #region Implicit casting

        /// <summary>
        /// Tests generation of implicit cast operations on expressions containing a non-null ImplicitCastType entry
        /// </summary>
        [TestMethod]
        public void TestImplicitIntegerCast()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestImplicitAnyCast()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestNoImplicitCasting()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestNestedImplicitCasting()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "true ? 1 : 0";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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

        #endregion

        #region Ternary operator

        /// <summary>
        /// Tests generation of ternary operation
        /// </summary>
        [TestMethod]
        public void TestSimpleTernaryOperator()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestNestedTernaryOperatorLeftSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 ? 2 : 3 : 4";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestNestedTernaryOperatorRightSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2 ? 3 : 4";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestTernaryExecution()
        {
            const string input = "var a = 1; var b = 2; var c = 3; var d:int; func f1() { d = !true ? a : !true ? b : c;  }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where detected when not expected");
            Assert.AreEqual(3L, memory.GetVariable("d"), "Ternary operator did not behave as expected");
        }

        #endregion

        #region cast instruction and 'is' operator

        /// <summary>
        /// Tests generation of cast operation
        /// </summary>
        [TestMethod]
        public void TestCastOperation()
        {
            const string message = "The tokens generated for the 'cast' operation where not generated as expected";

            const string input = "(bool)10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestIsOperator()
        {
            const string message = "The tokens generated for the 'is' operation where not generated as expected";

            const string input = "10 is int";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestSimpleOrShortcircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a || b";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestSimpleAndShortcircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a && b";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestMixedOrAndShortCircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a || b && c";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestNumberConstantPropagation()
        {
            const string message = "The constant was not propagated as expected";

            const string input = "10 + 10";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestStringConstantPropagation()
        {
            const string message = "The constant was not propagated as expected";

            const string input = "'abc' + 'abc'";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestMemberAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.Length";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestChainedMemberAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.Length.Length.A";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestFunctionCall()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.ToString()";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestChainedFunctionCall()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'.ToString().IndexOf('1')";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestSubscriptAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'[0]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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
        [TestMethod]
        public void TestChainedSubscriptAccess()
        {
            const string message = "Failed to generate expected tokens";

            const string input = "'a'[0][1]";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

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