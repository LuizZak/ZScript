using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Runtime;

namespace ZScriptTests.CodeGeneration.Tokenizers
{
    /// <summary>
    /// Tests the functionality of the PostfixExpressionTokenizer
    /// </summary>
    [TestClass]
    public class PostfixExpressionTokenizerTests
    {
        #region Ternary operator

        /// <summary>
        /// Tests generation of ternary operation
        /// </summary>
        [TestMethod]
        public void TestSimpleTernaryOperator()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2";
            var parser = ZRuntimeTests.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(null);

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var jEnd = new JumpTargetToken();
            var jFalse = new JumpTargetToken();
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken((long)0),
                new JumpToken(jFalse, true, false),
                TokenFactory.CreateBoxedValueToken((long)1),
                new JumpToken(jEnd),
                jFalse,
                TokenFactory.CreateBoxedValueToken((long)2),
                jEnd
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests generation of nested ternary operation
        /// </summary>
        [TestMethod]
        public void TestNestedTernaryOperatorLeftSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 ? 2 : 3 : 4";
            var parser = ZRuntimeTests.CreateParser(input);
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
                TokenFactory.CreateBoxedValueToken((long)0),
                // False jump
                new JumpToken(jEnd1, true, false),

                // 1st ternary true
                    // 2nd ternary expression
                    TokenFactory.CreateBoxedValueToken((long)1),
                    // False jump
                    new JumpToken(jEnd2, true, false),
                    
                    // 2nd ternary true
                    TokenFactory.CreateBoxedValueToken((long)2),
                    // End jump
                    new JumpToken(jEnd2),

                    // 1st ternary false
                    jFalse2,
                    TokenFactory.CreateBoxedValueToken((long)3),
                    jEnd2,

                // End jump
                new JumpToken(jEnd1),

                // 1st ternary false
                jFalse1,
                TokenFactory.CreateBoxedValueToken((long)4),
                jEnd1
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests generation of nested ternary operation
        /// </summary>
        [TestMethod]
        public void TestNestedTernaryOperatorRightSide()
        {
            const string message = "The tokens generated for the ternary expression where not generated as expected";

            const string input = "0 ? 1 : 2 ? 3 : 4";
            var parser = ZRuntimeTests.CreateParser(input);
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
                TokenFactory.CreateBoxedValueToken((long)0),
                new JumpToken(jEnd1, true, false),
                // 1st ternary's left side
                TokenFactory.CreateBoxedValueToken((long)1),
                new JumpToken(jEnd1),

                // 1st ternary's right side
                jFalse1,
                    // 2nd ternary's condition 
                    TokenFactory.CreateBoxedValueToken((long)2),
                    new JumpToken(jEnd2, true, false),

                    // 2nd ternary's left side
                    TokenFactory.CreateBoxedValueToken((long)3),
                    new JumpToken(jEnd2),

                    jFalse2,
                    // 2nd ternary's right side
                    TokenFactory.CreateBoxedValueToken((long)4),
                    jEnd2,

                jEnd1
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);
            
            // Assert the tokens where generated correctly
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests generation of nested ternary operation
        /// </summary>
        [TestMethod]
        public void TestTernaryExecution()
        {
            const string input = "[ a = 1; b = 2; c = 3; d; ] func f1() { d = !true ? a : !true ? b : c;  }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;
            var runtime = generator.GenerateRuntime(null);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where detected when not expected");
            Assert.AreEqual((long)3, memory.GetVariable("d"), "Ternary operator did not behave as expected");
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
            var parser = ZRuntimeTests.CreateParser(input);
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
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests AND logical operator short circuiting
        /// </summary>
        [TestMethod]
        public void TestSimpleAndShortcircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a && b";
            var parser = ZRuntimeTests.CreateParser(input);
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
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        /// <summary>
        /// Tests generation of mixed AND and OR logical operator short circuiting
        /// </summary>
        [TestMethod]
        public void TestMixedOrAndShortCircuit()
        {
            const string message = "Failed to generate tokens containing expected short-circuit";

            const string input = "a || b && c";
            var parser = ZRuntimeTests.CreateParser(input);
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
            AssertTokenListEquals(expectedTokens, generatedTokens, message);
        }

        #endregion

        /// <summary>
        /// Returns whether the two given token lists are equivalent.
        /// The equivalence takes in consideration the token types, instructions,
        /// values, and in case of jump tokens, the equivalence of the jump
        /// </summary>
        /// <param name="expected">The expected token</param>
        /// <param name="actual">The actual token</param>
        /// <param name="message">The message to display in case the tokens mismatch</param>
        /// <returns>true if the token lists are equivalent, false otherwise</returns>
        /// <exception cref="Exception">The token lists did not match</exception>
        public static void AssertTokenListEquals(List<Token> expected, List<Token> actual, string message)
        {
            if (expected.Count != actual.Count)
                throw new Exception(message + "; Token lists have different token counts");

            // Compare the tokens one by one
            for (int i = 0; i < expected.Count; i++)
            {
                Token t1 = expected[i];
                Token t2 = actual[i];

                // Unequality of types
                if (t1.GetType() != t2.GetType())
                    throw new Exception(message + "; Tokens at index " + i + " have different types: expected " + t1.GetType() + " actual: " + t2.GetType());

                var jt1 = t1 as JumpToken;
                var jt2 = t2 as JumpToken;
                if (jt1 != null && jt2 != null)
                {
                    if (jt1.ConditionToJump != jt2.ConditionToJump ||
                        jt1.Conditional != jt2.Conditional ||
                        jt1.ConsumesStack != jt2.ConsumesStack ||
                        expected.IndexOf(jt1.TargetToken) != expected.IndexOf(jt2.TargetToken))
                    {
                        throw new Exception(message + "; Jump tokens at index " + i + " do not have the same configuration: expected " + t1 + " actual: " + t2);
                    }
                }

                if (!t1.Equals(t2))
                    throw new Exception(message + "; Tokens at index " + i + " have different values: expected " + t1 + " actual: " + t2);
            }
        }
    }
}