using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization
{
    /// <summary>
    /// Tests tokenization of tuple types
    /// </summary>
    [TestClass]
    public class PostfixExpressionTokenizerTuplesTests
    {
        #region Tuples

        /// <summary>
        /// Tests tokenization of a tuple with only one value contained within
        /// </summary>
        [TestMethod]
        public void TestTupleWithOneValue()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            const string input = "(0)";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.expression();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
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
        /// Tests tokenization of a tuple with two values contained within
        /// </summary>
        [TestMethod]
        public void TestTupleWithTwoValues()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            // Input
            const string input = "(0, true)";


            // Boilerplate
            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: provider)));

            var tupleType = provider.TupleForTypes(provider.IntegerType(), provider.BooleanType());

            var exp = parser.expression();

            exp.tupleExpression().TupleType = tupleType;
            exp.tupleExpression().tupleEntry(0).expression().EvaluatedType = tupleType.InnerTypes[0];
            exp.tupleExpression().tupleEntry(1).expression().EvaluatedType = tupleType.InnerTypes[1];


            // Test action
            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateTypeToken(TokenType.Instruction, VmInstruction.CreateTuple, tupleType)
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
        /// Tests tokenization of a tuple creation literal
        /// </summary>
        [TestMethod]
        public void TestCreateTupleLiteral()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            // Input
            const string input = "(int, bool)(0, true)";


            // Boilerplate
            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: provider)));

            var tupleType = provider.TupleForTypes(provider.IntegerType(), provider.BooleanType());

            var exp = parser.expression();

            exp.tupleLiteralInit().tupleType().TupleType = tupleType;
            exp.tupleLiteralInit().functionCall().funcCallArguments().expressionList().expression(0).EvaluatedType = tupleType.InnerTypes[0];
            exp.tupleLiteralInit().functionCall().funcCallArguments().expressionList().expression(1).EvaluatedType = tupleType.InnerTypes[1];


            // Test action
            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateTypeToken(TokenType.Instruction, VmInstruction.CreateTuple, tupleType)
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
        /// Tests tokenization of a tuple access by index
        /// </summary>
        [TestMethod]
        public void TestAccessTupleByIndex()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            const string input = "(0, true).0";
            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: provider)));

            var tupleType = provider.TupleForTypes(provider.IntegerType(), provider.BooleanType());

            var exp = parser.expression();

            exp.EvaluatedType = provider.TupleForTypes(provider.IntegerType(), provider.BooleanType());
            exp.tupleExpression().TupleType = tupleType;
            exp.tupleExpression().tupleEntry(0).expression().EvaluatedType = provider.IntegerType();
            exp.tupleExpression().tupleEntry(1).expression().EvaluatedType = provider.BooleanType();

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateTypeToken(TokenType.Instruction, VmInstruction.CreateTuple, tupleType),
                TokenFactory.CreateMemberNameToken("Field0"),
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
        /// Tests tokenization of a tuple access by label
        /// </summary>
        [TestMethod]
        public void TestAccessTupleByLabel()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            const string input = "(0, x: true).x";
            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: provider)));

            var tupleType = provider.TupleForTypes(provider.IntegerType(), provider.BooleanType());

            var exp = parser.expression();

            exp.tupleExpression().TupleType = tupleType;
            exp.tupleExpression().tupleEntry(0).expression().EvaluatedType = provider.IntegerType();
            exp.tupleExpression().tupleEntry(1).expression().EvaluatedType = provider.BooleanType();

            exp.objectAccess().fieldAccess().IsTupleAccess = true;
            exp.objectAccess().fieldAccess().TupleIndex = 1;

            var generatedTokens = tokenizer.TokenizeExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateTypeToken(TokenType.Instruction, VmInstruction.CreateTuple, tupleType),
                TokenFactory.CreateMemberNameToken("Field1"),
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
        /// Tests tokenization of a tuple with only one value contained within
        /// </summary>
        [TestMethod]
        public void TestAssignTupleByIndex()
        {
            const string message = "The tokens generated for the tuple expression where not generated as expected";

            const string input = "tuple.0 = 1";
            var parser = TestUtils.CreateParser(input);
            var tokenizer = new PostfixExpressionTokenizer(new StatementTokenizerContext(new RuntimeGenerationContext(typeProvider: new TypeProvider())));

            var exp = parser.assignmentExpression();

            var generatedTokens = tokenizer.TokenizeAssignmentExpression(exp);

            // Create the expected list
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(1L),
                TokenFactory.CreateVariableToken("tuple", true),
                TokenFactory.CreateMemberNameToken("Field0"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
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
    }
}