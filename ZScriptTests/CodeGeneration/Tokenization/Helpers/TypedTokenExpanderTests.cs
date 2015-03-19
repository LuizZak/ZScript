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

using System.Collections.Generic;

using Xunit;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;

using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Tests the functionality of the TypedTokenExpander class
    /// </summary>
    public class TypedTokenExpanderTests
    {
        /// <summary>
        /// Tests expanding an any type
        /// </summary>
        [Fact]
        public void TestExpandAnyType()
        {
            // Setup the test
            const string input = "any";

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
            Assert.Equal(typeof(object), tokenList.Tokens[0].TokenObject); // "The type was not expanded as expected"
        }

        /// <summary>
        /// Tests expanding a primitive type
        /// </summary>
        [Fact]
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
            Assert.Equal(typeof(long), tokenList.Tokens[0].TokenObject); // "The type was not expanded as expected"
        }

        /// <summary>
        /// Tests expanding a list type
        /// </summary>
        [Fact]
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
            Assert.Equal(typeof(List<long>), tokenList.Tokens[0].TokenObject); // "The type was not expanded as expected"
        }

        /// <summary>
        /// Tests expanding an object type
        /// </summary>
        [Fact]
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
            Assert.Equal(typeof(ZObject), tokenList.Tokens[0].TokenObject);
        }
    }
}