using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Analysis;
using ZScript.Runtime.Typing;

using TestUtils = ZScriptTests.Utils.TestUtils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the ExpressionConstantResolver class and related components
    /// </summary>
    [TestClass]
    public class ExpressionConstantResolverTests
    {
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
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

            // Generate the expression
            var expression = parser.expression();

            // Analyze the types
            typeResolver.ResolveExpression(expression);

            // Resolve the constants now
            constantResolver.ExpandConstants(expression);

            Assert.IsTrue(expression.IsConstant, "The expander failed to modify the 'IsConstant' flag on the expression context");
            Assert.IsTrue(expression.IsConstantPrimitive, "The expander failed to modify the 'IsConstantPrimitive' flag on the expression context");
            Assert.AreEqual((long)10, expression.ConstantValue, "The expander failed to expand the constants correctly");
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
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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
            
            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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

            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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

            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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

            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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

            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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

            var parser = Utils.TestUtils.CreateParser(input);

            // Create the analyzer for expanding the types of the expression so the constant expander knows what to do with them
            var typeProvider = new TypeProvider();
            var binaryProvider = new BinaryExpressionTypeProvider(typeProvider);
            var typeResolver = new ExpressionTypeResolver(typeProvider, null);
            var constantResolver = new ExpressionConstantResolver(binaryProvider, new TypeOperationProvider());

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