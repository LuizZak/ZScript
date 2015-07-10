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
using ZScript.Runtime.Typing.Elements;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests tuple type resolving on the ExpressionTypeResolver
    /// </summary>
    [TestClass]
    public class ExpressionTypeResolverTupleTests
    {
        /// <summary>
        /// Tests resolving the type of tuples with one value
        /// </summary>
        [TestMethod]
        public void TestResolveTupleTypeWithOneType()
        {
            // Set up the test
            const string input = "(0); (true); (1.1); (i);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            var type4 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.BooleanType(), type2, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.FloatType(), type3, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.IntegerType(), type4, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests resolving the type of tuples with two values
        /// </summary>
        [TestMethod]
        public void TestResolveTupleTypeWithTwoTypes()
        {
            // Set up the test
            const string input = "(0, 1); (true, 0.0);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var tupleExpression = parser.statement().expression().tupleExpression();
            var expression = parser.statement().expression();

            var type1 = (TupleTypeDef)resolver.ResolveTupleExpression(tupleExpression);
            var type2 = (TupleTypeDef)resolver.ResolveExpression(expression);

            // Compare the result now
            var tupleType1 = provider.TupleForTypes(provider.IntegerType(), provider.IntegerType());
            var tupleType2 = provider.TupleForTypes(provider.BooleanType(), provider.FloatType());

            Assert.AreEqual(tupleType1, tupleExpression.TupleType);
            Assert.AreEqual(tupleType2, expression.tupleExpression().TupleType);

            Assert.AreEqual(tupleType1, type1, "The resolved type did not match the expected type");
            Assert.AreEqual(tupleType2, type2, "The resolved type did not match the expected type");

            Assert.AreEqual("0", type1.InnerTypeNames[0]);
            Assert.AreEqual("1", type1.InnerTypeNames[1]);

            Assert.AreEqual("0", type2.InnerTypeNames[0]);
            Assert.AreEqual("1", type2.InnerTypeNames[1]);
        }

        /// <summary>
        /// Tests resolving the type of tuples with identifiers
        /// </summary>
        [TestMethod]
        public void TestResolveTupleTypeWithIdentifiers()
        {
            // Set up the test
            const string input = "(x: float, y: float, float)";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveTupleType(parser.tupleType());

            // Compare the result now
            Assert.AreEqual(provider.TupleForTypes(provider.FloatType(), provider.FloatType(), provider.FloatType()), type1, "The resolved type did not match the expected type");
            Assert.AreEqual("x", type1.InnerTypeNames[0]);
            Assert.AreEqual("y", type1.InnerTypeNames[1]);
            Assert.AreEqual("2", type1.InnerTypeNames[2]);
        }

        /// <summary>
        /// Tests resolving of a tuple literal initialization expression
        /// </summary>
        [TestMethod]
        public void TestResolveTupleLiteralInitialization()
        {
            // Set up the test
            const string input = "(x: float, y: float, float)(0, 0, 0) (x: float, y: float, float)(0, 0, 0)";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveTupleLiteralInit(parser.tupleLiteralInit());
            var type2 = (TupleTypeDef)resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(provider.TupleForTypes(provider.FloatType(), provider.FloatType(), provider.FloatType()), type1, "The resolved type did not match the expected type");
            Assert.AreEqual("x", type1.InnerTypeNames[0]);
            Assert.AreEqual("y", type1.InnerTypeNames[1]);
            Assert.AreEqual("2", type1.InnerTypeNames[2]);

            Assert.AreEqual(provider.TupleForTypes(provider.FloatType(), provider.FloatType(), provider.FloatType()), type1, "The resolved type did not match the expected type");
            Assert.AreEqual("x", type2.InnerTypeNames[0]);
            Assert.AreEqual("y", type2.InnerTypeNames[1]);
            Assert.AreEqual("2", type2.InnerTypeNames[2]);
        }

        /// <summary>
        /// Tests raising errors when incorrect argument counts are passed to tuple initializers
        /// </summary>
        [TestMethod]
        public void TestResolveIncorrectArgumentCountInTupleLiteralInitialization()
        {
            // Set up the test
            const string input = "(float, float, float)(0, 0); (float, float)(0, 0, 0);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooFewArguments));
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooManyArguments));
        }

        /// <summary>
        /// Tests raising errors when incorrect argument types are passed to tuple initializers
        /// </summary>
        [TestMethod]
        public void TestResolveIncorrectTypesInTupleLiteralInitialization()
        {
            // Set up the test
            const string input = "(int, bool)(0.0, 0);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests passing of expected types in tuple expressions
        /// </summary>
        [TestMethod]
        public void TestPassExpectedTypeInTuples()
        {
            // Set up the test
            const string input = "(0, 1);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var exp = parser.statement().expression();
            var tupleType = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());
            exp.ExpectedType = tupleType;
            resolver.ResolveExpression(exp);

            // Compare the result now
            Assert.AreEqual(tupleType, exp.tupleExpression().TupleType, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.FloatType(), exp.tupleExpression().tupleEntry(1).expression().ExpectedType, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests failed implicit conversion of tuple entries
        /// </summary>
        [TestMethod]
        public void TestInvalidTupleEntryImplicitConvertion()
        {
            // Set up the test
            const string input = "(0.0, 1);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            var exp = parser.statement().expression();
            exp.ExpectedType = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());
            resolver.ResolveExpression(exp);

            // Compare the result now
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests creating tuples with labeled entries
        /// </summary>
        [TestMethod]
        public void TestCreateLabeledTuple()
        {
            // Set up the test
            const string input = "(x: 0, y: false);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var type1 = (TupleTypeDef)resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual("x", type1.InnerTypeNames[0], "The resolved type did not match the expected type");
            Assert.AreEqual("y", type1.InnerTypeNames[1], "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests accessing a tuple's entry by name
        /// </summary>
        [TestMethod]
        public void TestAccessLabeledTupleEntry()
        {
            // Set up the test
            const string input = "(x: 0, y: false).y;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var exp = parser.statement().expression();
            var type1 = resolver.ResolveExpression(exp);

            // Compare the result now
            Assert.IsTrue(exp.objectAccess().fieldAccess().IsTupleAccess);
            Assert.AreEqual(1, exp.objectAccess().fieldAccess().TupleIndex);
            Assert.AreEqual(provider.BooleanType(), type1, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests accessing a tuple's entry by index
        /// </summary>
        [TestMethod]
        public void TestAccessTupleIndex()
        {
            // Set up the test
            const string input = "(0, false).0; (true, 1.0).1; (x: 0, 1).0;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.FloatType(), type2, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.IntegerType(), type3, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests accessing a tuple's entry by index
        /// </summary>
        [TestMethod]
        public void TestAccessInvalidTupleIndex()
        {
            // Set up the test
            const string input = "(0, false).2;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            resolver.ResolveExpression(parser.statement().expression());

            container.PrintMessages();

            // Compare the result now
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UnrecognizedMember));
        }

        /// <summary>
        /// Tests raising errors when trying to access tuples with invalid value accesses
        /// </summary>
        [TestMethod]
        public void TestInvalidTupleAccessTypeError()
        {
            // Set up the test
            const string input = "(0, false)[0];";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TryingToSubscriptNonList));
        }
        
        /// <summary>
        /// Tests implicit casting within tuples, where the expected tuple type is optional
        /// </summary>
        [TestMethod]
        public void TestOptionalTupleInnerImplicitCasting()
        {
            // Set up the test
            const string input = "(0, 1);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var exp = parser.statement().expression();
            var tupleType = provider.TupleForTypes(provider.OptionalTypeForType(provider.IntegerType()), provider.FloatType());
            exp.ExpectedType = provider.OptionalTypeForType(tupleType);

            resolver.ResolveExpression(exp);

            // Compare the result now
            Assert.AreEqual(tupleType, exp.tupleExpression().TupleType, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.OptionalTypeForType(provider.IntegerType()), exp.tupleExpression().tupleEntry(0).expression().ExpectedType, "The resolved type did not match the expected type");
        }
    }
}