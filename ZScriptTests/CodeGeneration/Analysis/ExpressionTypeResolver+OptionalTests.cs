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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Extension to the ExpressionTypeResolverTests class to support optional type resolving
    /// </summary>
    public partial class ExpressionTypeResolverTests
    {
        /// <summary>
        /// Tests binary optional binary expression errors
        /// </summary>
        [TestMethod]
        public void TestOptionalBinaryExpression()
        {
            // Set up the test
            const string input = "oi + 10;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.AnyType(), type1, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnOperation), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests optional unwrapping
        /// </summary>
        [TestMethod]
        public void TestOptionalUnwrapping()
        {
            // Set up the test
            const string input = "oi!;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();
        }

        /// <summary>
        /// Tests trying to unwrap a non-optional value
        /// </summary>
        [TestMethod]
        public void TestOptionalUnwrappingError()
        {
            // Set up the test
            const string input = "10!;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.AnyType(), type1, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TryingToUnwrapNonOptional), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests resolving a simple primitive an optional type context
        /// </summary>
        [TestMethod]
        public void TestOptionalTypeResolving()
        {
            // Set up the test
            const string input = "int?";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveType(parser.type(), false);

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), type1, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests resolving a complex optional type context
        /// </summary>
        [TestMethod]
        public void TestCimplexOptionalTypeResolving()
        {
            // Set up the test
            const string input = "[float?]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type = resolver.ResolveType(parser.type(), false);

            // Compare the result now
            Assert.AreEqual(provider.ListForType(provider.OptionalTypeForType(provider.FloatType())), type, "The resolved type did not match the expected type");
        }
    }
}