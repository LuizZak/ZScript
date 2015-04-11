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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
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
        /// Tests optional implicit casting omission
        /// </summary>
        [TestMethod]
        public void TestOptionalImplicitCasting()
        {
            // Set up the test
            const string input = "10;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var expression = parser.statement().expression();
            expression.ExpectedType = provider.OptionalTypeForType(provider.IntegerType());

            resolver.ResolveExpression(expression);

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), expression.ImplicitCastType, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();
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
        public void TestComplexOptionalTypeResolving()
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

        /// <summary>
        /// Tests null conditional on optional value
        /// </summary>
        [TestMethod]
        public void TestOptionalNullConditional()
        {
            // Set up the test
            const string input = "ol?.Count;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(new NativeTypeDef(typeof(int))), type1, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();
        }

        /// <summary>
        /// Tests optional unwrapping in left values
        /// </summary>
        [TestMethod]
        public void TestOptionalUnwrappingLeftValue()
        {
            // Set up the test
            const string input = "ol!.Count";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type = resolver.ResolveLeftValue(parser.leftValue());

            // Compare the result now
            Assert.AreEqual(new NativeTypeDef(typeof(int)), type, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests optional unwrapping in left values
        /// </summary>
        [TestMethod]
        public void TestOptionalUnwrappingLeftValueError()
        {
            // Set up the test
            const string input = "l!.Count";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type = resolver.ResolveLeftValue(parser.leftValue());

            // Compare the result now
            Assert.AreEqual(new NativeTypeDef(typeof(int)), type, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TryingToUnwrapNonOptional), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests error raising when trying to use null-conditionals in non-optional values
        /// </summary>
        [TestMethod]
        public void TestNonOptionalNullConditional()
        {
            // Set up the test
            const string input = "l?.Count";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(new NativeTypeDef(typeof(int))), type, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TryingToUnwrapNonOptional), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests type resolving of null-coalesce on mixed optionals and non-optionals
        /// </summary>
        [TestMethod]
        public void TestNullCoalesceOnNonOptional()
        {
            // Set up the test
            const string input = "oi ?: i";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var expression = parser.expression();
            var type = resolver.ResolveExpression(expression);

            container.PrintMessages();

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type);
            Assert.AreEqual(provider.IntegerType(), expression.expression(1).ExpectedType);
        }

        /// <summary>
        /// Tests type resolving of null-coalesce on and modification of expected type of expressions on the left and right side of the coalescing operator
        /// </summary>
        [TestMethod]
        public void TestNullCoalesceExpectedTyping()
        {
            // Set up the test
            const string input = "ooi ?: i";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var expression = parser.expression();
            var type = resolver.ResolveExpression(expression);

            container.PrintMessages();

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), type);
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), expression.expression(1).ExpectedType);
        }

        /// <summary>
        /// Tests type resolving of null-coalesce on and modification of expected type of expressions on the left and right side of the coalescing operator
        /// </summary>
        [TestMethod]
        public void TestNestedNullCoalesceExpectedTyping()
        {
            // Set up the test
            const string input = "oooi ?: ooi ?: i";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Cache this here so we avoid chaining
            Func<TypeDef, TypeDef> opt = provider.OptionalTypeForType;
            var intT = provider.IntegerType();

            // Perform the parsing
            var expression = parser.expression();
            var type = resolver.ResolveExpression(expression);

            container.PrintMessages();

            // Compare the result now
            Assert.AreEqual(opt(opt(intT)), type);
            Assert.AreEqual(opt(opt(intT)), expression.expression(1).ExpectedType);

            Assert.AreEqual(opt(intT), expression.expression(1).expression(0).ExpectedType);
            Assert.AreEqual(opt(intT), expression.expression(1).expression(1).ExpectedType);
        }

        /// <summary>
        /// Tests type resolving of null-coalesce on mixed optionals and non-optionals
        /// </summary>
        [TestMethod]
        public void TestNullCoalesceOnNestedOptional()
        {
            // Set up the test
            const string input = "ooi ?: i; oi ?: ooi; ooi ?: oi ?: i;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());

            container.PrintMessages();

            // Compare the result now
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), type1, "Failed to evaluate the result of expression correctly");
            Assert.AreEqual(provider.OptionalTypeForType(provider.OptionalTypeForType(provider.IntegerType())), type2, "Failed to evaluate the result of expression correctly");
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), type3, "Failed to evaluate the result of expression correctly");
        }

        /// <summary>
        /// Tests error raising when using non-optional types on the left side of null-coalesce operators
        /// </summary>
        [TestMethod]
        public void TestErrorNullCoalesceOnNonOptional()
        {
            // Set up the test
            const string input = "i ?: i";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type, "Failed to evaluate the result of expression correctly");

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NonOptionalNullCoalesceLeftSize));
        }
    }
}