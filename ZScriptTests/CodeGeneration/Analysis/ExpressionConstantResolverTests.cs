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
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            const string input = "10";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Implicitly set it as a integer, and have it converted
            expression.ImplicitCastType = typeProvider.FloatType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(10.0, expression.ConstantValue, "The expander failed to expand the constants correctly");
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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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

        /// <summary>
        /// Tests implicit casting of contents of an array literal
        /// </summary>
        [TestMethod]
        public void TestCastListLiteral()
        {
            const string input = "[10, 1.1]";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Implicitly set it as a float, and have it converted
            expression.arrayLiteral().expressionList().expression(0).ImplicitCastType = typeProvider.FloatType();
            expression.arrayLiteral().expressionList().expression(0).ExpectedType = typeProvider.FloatType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.AreEqual(10.0, expression.arrayLiteral().expressionList().expression(0).ConstantValue, "The expander failed to cast the constants in the list literal");
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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
        /// Tests function call parameter constant propagation
        /// </summary>
        [TestMethod]
        public void TestConstantParameterResolving()
        {
            const string input = "a(10 + 11)";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            var entryList = expression.valueAccess().functionCall().tupleExpression().tupleEntry();

            // Analyze the types
            entryList[0].expression().EvaluatedType = typeProvider.IntegerType();
            entryList[0].expression().expression(0).EvaluatedType = typeProvider.IntegerType();
            entryList[0].expression().expression(1).EvaluatedType = typeProvider.IntegerType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(entryList[0].expression().IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(entryList[0].expression().IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(21L, entryList[0].expression().ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests function call parameter constant propagation
        /// </summary>
        [TestMethod]
        public void TestConstantSubscriptResolving()
        {
            const string input = "a[10 + 11]";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            var exp = expression.valueAccess().arrayAccess().expression();

            // Analyze the types
            exp.expression(0).EvaluatedType = typeProvider.IntegerType();
            exp.expression(1).EvaluatedType = typeProvider.IntegerType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(exp.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(exp.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(21L, exp.ConstantValue, "The expander failed to expand the constants correctly");
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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

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

        /// <summary>
        /// Tests constant propagation
        /// </summary>
        [TestMethod]
        public void TestConstantPropagation()
        {
            const string iConst = "10 + 10";
            const string input = "i";

            var parserA = TestUtils.CreateParser(iConst);
            var parserExp = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider, new TestDefinitionTypeProvider());
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parserExp.expression();
            var iExpr = parserA.expression();

            iExpr.expression(0).EvaluatedType = typeProvider.IntegerType();
            iExpr.expression(1).EvaluatedType = typeProvider.IntegerType();

            expression.memberName().HasDefinition = true;
            expression.memberName().Definition = new LocalVariableDefinition
            {
                IsConstant = true,
                Context = iExpr,
                HasType = true,
                HasValue = true,
                ValueExpression = new Expression(iExpr),
                Name = "a"
            };

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(20L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests constant propagation in a ternary expression that always results in true
        /// </summary>
        [TestMethod]
        public void TestTrueTernaryPropagation()
        {
            const string input = "true ? 0 : 1";

            var parserExp = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider, new TestDefinitionTypeProvider());
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parserExp.expression();

            expression.expression(0).EvaluatedType = typeProvider.BooleanType();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(0L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests constant propagation in a ternary expression that always results in false
        /// </summary>
        [TestMethod]
        public void TestFalseTernaryPropagation()
        {
            const string input = "false ? 0 : 1";

            var parserExp = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider, new TestDefinitionTypeProvider());
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parserExp.expression();

            expression.expression(0).EvaluatedType = typeProvider.BooleanType();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(1L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests null coalescing propagation
        /// </summary>
        [TestMethod]
        public void TestNullCoalescePropagation()
        {
            const string input = "null ?: 1";

            var parserExp = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, null, typeProvider, new TestDefinitionTypeProvider());
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parserExp.expression();

            expression.expression(0).EvaluatedType = typeProvider.IntegerType();
            expression.expression(1).EvaluatedType = typeProvider.IntegerType();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(1L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests null coalescing propagation
        /// </summary>
        [TestMethod]
        public void TestNonNullNullCoalescePropagation()
        {
            const string input = "0 ?: 1";

            var parserExp = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var generationContext = new RuntimeGenerationContext(null, new MessageContainer(), typeProvider, new TestDefinitionTypeProvider());
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parserExp.expression();

            expression.expression(0).EvaluatedType = typeProvider.IntegerType();
            expression.expression(1).EvaluatedType = typeProvider.IntegerType();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual(1L, expression.ConstantValue, "The expander failed to expand the constants correctly");
        }

        /// <summary>
        /// Tests exception raising when executing invalid operations during constant resolving
        /// </summary>
        [TestMethod]
        public void TestExceptionRaising()
        {
            const string input = "10 / 0";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var container = new MessageContainer();
            var generationContext = new RuntimeGenerationContext(null, container, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidConstantOperation), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests raising errors when invalid string escape sequences are met
        /// </summary>
        [TestMethod]
        public void TestInvalidEscapeSequence()
        {
            const string input = "'ab\\dnc'";

            var parser = TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var container = new MessageContainer();
            var generationContext = new RuntimeGenerationContext(null, container, typeProvider);
            var typeResolver = new ExpressionTypeResolver(generationContext);
            var constantResolver = new ExpressionConstantResolver(generationContext, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidEscapeSequence));
        }
    }
}