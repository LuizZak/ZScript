using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Tests the functionality of the TypedTokenExpander class
    /// </summary>
    [TestClass]
    public class TypedTokenExpanderTests
    {
        /// <summary>
        /// Tests expanding a primitive type
        /// </summary>
        [TestMethod]
        public void TestExpandPrimitiveType()
        {
            // Setup the test
            const string input = "int";

            var parser = TestUtils.CreateParser(input);
            var type = parser.type();

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, type),
            };

            var tokenList = tokens.ToTokenList();
            var typeProvider = new TypeProvider();
            var context = new RuntimeGenerationContext(null, new MessageContainer(), typeProvider);
            context.ContextTypeProvider = new ExpressionTypeResolver(context);

            // Expand the tokens
            TypedTokenExpander expander = new TypedTokenExpander(context);

            expander.ExpandInList(tokenList);

            // Assert the results
            Assert.AreEqual(typeof(long), tokenList.Tokens[0].TokenObject, "The type was not expanded as expected");
        }

        /// <summary>
        /// Tests expanding a list type
        /// </summary>
        [TestMethod]
        public void TestExpandListType()
        {
            // Setup the test
            const string input = "[int]";

            var parser = TestUtils.CreateParser(input);
            var type = parser.type();

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, type)
            };

            var tokenList = tokens.ToTokenList();
            var typeProvider = new TypeProvider();
            var context = new RuntimeGenerationContext(null, new MessageContainer(), typeProvider);
            context.ContextTypeProvider = new ExpressionTypeResolver(context);

            // Expand the tokens
            TypedTokenExpander expander = new TypedTokenExpander(context);

            expander.ExpandInList(tokenList);

            // Assert the results
            Assert.AreEqual(typeof(List<long>), tokenList.Tokens[0].TokenObject, "The type was not expanded as expected");
        }

        /// <summary>
        /// Tests expanding an object type
        /// </summary>
        [TestMethod]
        public void TestExpandObjectType()
        {
            // Setup the test
            const string input = "object";

            var parser = TestUtils.CreateParser(input);
            var type = parser.type();

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, type)
            };

            var tokenList = tokens.ToTokenList();
            var typeProvider = new TypeProvider();
            var context = new RuntimeGenerationContext(null, new MessageContainer(), typeProvider);
            context.ContextTypeProvider = new ExpressionTypeResolver(context);

            // Expand the tokens
            TypedTokenExpander expander = new TypedTokenExpander(context);

            expander.ExpandInList(tokenList);

            // Assert the results
            Assert.AreEqual(typeof(ZObject), tokenList.Tokens[0].TokenObject, "The type was not expanded as expected");
        }
    }
}