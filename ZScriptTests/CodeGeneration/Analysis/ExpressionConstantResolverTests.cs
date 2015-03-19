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
using Xunit;
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
    public class ExpressionConstantResolverTests
    {
        #region Implicit casting tests

        /// <summary>
        /// Tests a simple constant resolving with an integer that is caster to a float
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(10.0, expression.ConstantValue);
        }

        /// <summary>
        /// Tests a simple constant resolving with a float that is caster to an integer
        /// </summary>
        [Fact]
        public void TestCastedFloatResolving()
        {
            const string input = "10.0";

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
            expression.ImplicitCastType = typeProvider.IntegerType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(10L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests implicit casting of expressions
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(20.0, expression.ConstantValue);
        }

        /// <summary>
        /// Tests implicit casting of contents of an array literal
        /// </summary>
        [Fact]
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

            // "The expander failed to cast the constants in the list literal"
            Assert.Equal(10.0, expression.arrayLiteral().expressionList().expression(0).ConstantValue);
        }

        #endregion

        /// <summary>
        /// Tests a simple constant resolving with a single constant atom
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(10L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests ignoring resolving with a single constant atom that contains a value access
        /// </summary>
        [Fact]
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

            Assert.False(expression.IsConstant, "The expander expanded a constant that was not supposed to be expanded due to value access");
        }

        /// <summary>
        /// Tests a simple binary expression constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(21L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests function call parameter constant propagation
        /// </summary>
        [Fact]
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

            var expressionList = expression.valueAccess().functionCall().funcCallArguments().expressionList();

            // Analyze the types
            expressionList.expression(0).expression(0).EvaluatedType = typeProvider.IntegerType();
            expressionList.expression(0).expression(1).EvaluatedType = typeProvider.IntegerType();

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.True(expressionList.expression(0).IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expressionList.expression(0).IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(21L, expressionList.expression(0).ConstantValue);
        }

        /// <summary>
        /// Tests function call parameter constant propagation
        /// </summary>
        [Fact]
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

            Assert.True(exp.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(exp.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(21L, exp.ConstantValue);
        }

        /// <summary>
        /// Tests binary expression containing constant string concatenations constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal("10abc", expression.ConstantValue);
        }

        /// <summary>
        /// Tests a compounded binary expression constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(64L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests a compounded binary expression containing logical operations constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(true, expression.ConstantValue);
        }

        /// <summary>
        /// Tests a compounded binary expression containing comparision operations constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(false, expression.ConstantValue);
        }

        /// <summary>
        /// Tests a compounded binary expression constant resolving
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(-10L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests constant propagation
        /// </summary>
        [Fact]
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

            Assert.True(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.True(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            // "The expander failed to expand the constants correctly"
            Assert.Equal(20L, expression.ConstantValue);
        }

        /// <summary>
        /// Tests exception raising when executing invalid operations during constant resolving
        /// </summary>
        [Fact]
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

            // "Failed to raise expected errors"
            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidConstantOperation));
        }
    }
}