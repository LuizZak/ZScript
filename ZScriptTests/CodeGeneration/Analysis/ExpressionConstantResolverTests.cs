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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.Runtime.Typing;

using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the ExpressionConstantResolver class and related components
    /// </summary>
    [TestClass]
    public class ExpressionConstantResolverTests
    {
        #region Implicit casting tests

        /// <summary>
        /// Tests a simple constant resolving with an integer that is caster to a float
        /// </summary>
        [TestMethod]
        public void TestCastedIntegerResolving()
        {
            const string input = "10";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Implicitly set it as a float, and have it converted
            expression.ImplicitCastType = typeProvider.FloatType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(10.0, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests a simple constant resolving with a float that is caster to an integer
        /// </summary>
        [TestMethod]
        public void TestCastedFloatResolving()
        {
            const string input = "10.0";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Implicitly set it as a integer, and have it converted
            expression.ImplicitCastType = typeProvider.IntegerType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(10L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests implicit casting of expressions
        /// </summary>
        [TestMethod]
        public void TestExpressionImplicitCast()
        {
            const string input = "10 + 10";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Implicitly set it as a float, and have it converted
            expression.ImplicitCastType = typeProvider.FloatType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(20.0, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        #endregion

        /// <summary>
        /// Tests a simple constant resolving with a single constant atom
        /// </summary>
        [TestMethod]
        public void TestConstantAtomResolving()
        {
            const string input = "10";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(10L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests ignoring resolving with a single constant atom that contains a value access
        /// </summary>
        [TestMethod]
        public void TestConstantAtomWithAccessResolving()
        {
            const string input = "10.ToString()";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsFalse(expression.IsConstant, "The expander expanded a constant that was not supposed to be expanded due to value access");
        }

        /// <summary>
        /// Tests a simple binary expression constant resolving
        /// </summary>
        [TestMethod]
        public void TestSimpleBinaryExpressionResolving()
        {
            const string input = "10 + 11";
            
            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(21L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests binary expression containing constant string concatenations constant resolving
        /// </summary>
        [TestMethod]
        public void TestStringResolving()
        {
            const string input = "10 + 'abc'";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual("10abc", expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests a compounded binary expression constant resolving
        /// </summary>
        [TestMethod]
        public void TestCompoundBinaryExpression()
        {
            const string input = "10 + 11 * 5 - 1";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(64L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests a compounded binary expression containing logical operations constant resolving
        /// </summary>
        [TestMethod]
        public void TestLogicalBinaryOperations()
        {
            const string input = "true || false && false";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(true, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests a compounded binary expression containing comparision operations constant resolving
        /// </summary>
        [TestMethod]
        public void TestComparisionBinaryOperations()
        {
            const string input = "10 > 5 + 7";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(false, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests a compounded binary expression constant resolving
        /// </summary>
        [TestMethod]
        public void TestUnaryExpression()
        {
            const string input = "-10";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var typeResolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, null, typeProvider));
            var constantResolver = new ExpressionConstantResolver(typeProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(-10L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }
    }
}