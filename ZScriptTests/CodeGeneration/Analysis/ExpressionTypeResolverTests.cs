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
    /// Tests the ExpressionTypeResolver class and related components
    /// </summary>
    [TestClass]
    public class ExpressionTypeResolverTests
    {
        #region General Type Resolving

        /// <summary>
        /// Tests callable type resolving
        /// </summary>
        [TestMethod]
        public void TestCallableTypeResolving()
        {
            // Set up the test
            const string input = "(int, bool->any)";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var value = parser.callableType();

            var type = resolver.ResolveCallableType(value);

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type.ParameterTypes[0], "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, type.ParameterTypes[1], "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.AnyType, type.ReturnType, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests list type resolving
        /// </summary>
        [TestMethod]
        public void TestListType()
        {
            // Set up the test
            const string input = "[int] [int] [[int]]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveListType(parser.listType());
            var type2 = (ListTypeDef)resolver.ResolveType(parser.type());
            var type3 = (ListTypeDef)resolver.ResolveType(parser.type());

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type1.EnclosingType, "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.IntegerType, type2.EnclosingType, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.ListForType(TypeDef.IntegerType), type3.EnclosingType, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests subscripting a list-type object
        /// </summary>
        [TestMethod]
        public void TestListSubscript()
        {
            // Set up the test
            const string input = "() : [int] => {}()[0]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests chaining calling of return values
        /// </summary>
        [TestMethod]
        public void TestChainedCallable()
        {
            // Set up the test
            const string input = "() : (->(->int)) => {}()()()";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type, "The resolved type did not match the expected type");
        }

        #endregion

        #region Assignment expression type resolving

        /// <summary>
        /// Tests resolving of types in an assignment expression
        /// </summary>
        [TestMethod]
        public void TestAssignmentExpressionResolving()
        {
            // Set up the test
            const string input = "i = 1; b = true; f -= 5.0; s = 'abc'; llf[0][0] = 0;";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider(), new TestDefinitionTypeProvider()));

            var type1 = resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            var type2 = resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            var type3 = resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            var type4 = resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            var type5 = resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type1, "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, type2, "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.FloatType,   type3, "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.StringType,  type4, "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.FloatType,   type5, "The resolved type did not match the expected type");
        }

        #endregion

        #region Atoms and compile-time constants

        /// <summary>
        /// Tests compile constant type resolving
        /// </summary>
        [TestMethod]
        public void TestCompileConstantResolving()
        {
            // Set up the test
            const string input = "10 -10 11.1 -15.251 true false null \"abc\"";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var intConst = parser.compileConstant();
            var negativeIntConst = parser.compileConstant();
            var floatConst = parser.compileConstant();
            var negativeFloatConst = parser.compileConstant();
            var boolTrueConst = parser.compileConstant();
            var boolFalseConst = parser.compileConstant();
            var nullConst = parser.compileConstant();
            var stringConst = parser.compileConstant();
            
            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, resolver.ResolveCompileConstant(intConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.IntegerType, resolver.ResolveCompileConstant(negativeIntConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.FloatType, resolver.ResolveCompileConstant(floatConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.FloatType, resolver.ResolveCompileConstant(negativeFloatConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, resolver.ResolveCompileConstant(boolTrueConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, resolver.ResolveCompileConstant(boolFalseConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.AnyType, resolver.ResolveCompileConstant(nullConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.StringType, resolver.ResolveCompileConstant(stringConst), "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests constant atom type resolving
        /// </summary>
        [TestMethod]
        public void TestConstantAtomResolving()
        {
            // Set up the test
            const string input = "10 11.1 true false null \"abc\"";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var intConst = parser.constantAtom();
            var floatConst = parser.constantAtom();
            var boolTrueConst = parser.constantAtom();
            var boolFalseConst = parser.constantAtom();
            var nullConst = parser.constantAtom();
            var stringConst = parser.constantAtom();

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, resolver.ResolveConstantAtom(intConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.FloatType, resolver.ResolveConstantAtom(floatConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, resolver.ResolveConstantAtom(boolTrueConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.BooleanType, resolver.ResolveConstantAtom(boolFalseConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.AnyType, resolver.ResolveConstantAtom(nullConst), "The resolved type did not match the expected type");
            Assert.AreEqual(TypeDef.StringType, resolver.ResolveConstantAtom(stringConst), "The resolved type did not match the expected type");
        }

        #endregion

        #region Literal resolving

        /// <summary>
        /// Tests object literal type solving
        /// </summary>
        [TestMethod]
        public void TestObjectLiteral()
        {
            // Set up the test
            const string input = "{ x:10 }";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var type = parser.objectLiteral();

            // Compare the result now
            Assert.AreEqual(new ObjectTypeDef(), resolver.ResolveObjectLiteral(type), "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests array literal type resolving
        /// </summary>
        [TestMethod]
        public void TestArrayLiteral()
        {
            // Set up the test
            const string input = "[0, 1, 2]";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var type = parser.arrayLiteral();

            // Compare the result now
            Assert.AreEqual(new ListTypeDef(TypeDef.IntegerType), resolver.ResolveArrayLiteral(type), "The resolved type did not match the expected type");
        }

        #endregion

        #region Callable function call resolving

        /// <summary>
        /// Tests callable argument type checking
        /// </summary>
        [TestMethod]
        public void TestCallableArgumentTypeChecking()
        {
            // Set up the test
            const string input = "(a:int, b:bool) : int => { }(0, 0); (a:int, b:bool...) : int => { }(0, true, false, 0, 'sneakyString'); ";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to report expected errors about invalid type in arguments");
        }

        /// <summary>
        /// Tests callable argument count checking
        /// </summary>
        [TestMethod]
        public void TestCallableArgumentCountCheck()
        {
            // Set up the test
            const string input = "(a:int, b:bool) => { }(0);" +
                                 "(a:int, b:bool) => { }(0, true, 0);" +
                                 "(a:int, b:bool=false) => { }(0);" +
                                 "(a:int, b:bool...) => { }(0, true, false, true);" +
                                 "(a...) => { }();";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooFewArguments),  "Failed to report expected errors about mismatched argument count");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooManyArguments), "Failed to report expected errors about mismatched argument count");
        }

        #endregion

        #region Type casting/checking

        /// <summary>
        /// Tests basic type casting
        /// </summary>
        [TestMethod]
        public void TestTypeCast()
        {
            // Set up the test
            const string input = "(float)1; (int)1.0; (bool)true; ([int])[0];";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var exp1 = parser.statement().expression();
            var exp2 = parser.statement().expression();
            var exp3 = parser.statement().expression();
            var exp4 = parser.statement().expression();

            // Compare the result now
            Assert.AreEqual(provider.FloatType(), resolver.ResolveExpression(exp1), "The resolved type did not match the expected type");
            Assert.AreEqual(provider.IntegerType(), resolver.ResolveExpression(exp2), "The resolved type did not match the expected type");
            Assert.AreEqual(provider.BooleanType(), resolver.ResolveExpression(exp3), "The resolved type did not match the expected type");
            Assert.AreEqual(provider.ListForType(provider.IntegerType()), resolver.ResolveExpression(exp4), "The resolved type did not match the expected type");
        }

        #endregion

        #region Subscription/function call argument type resolving

        /// <summary>
        /// Tests list subscription type checking
        /// </summary>
        [TestMethod]
        public void TestListSubscriptionTypeChecking()
        {
            // Set up the test
            const string input = "[0]['invalid!']; [0][1]; 'string'['invalid!']; 'string'[0]; { x:10 }['valid']; { x:10 }[0];";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider()));

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        #endregion

        #region Closure type resolving

        /// <summary>
        /// Tests closure type resolving
        /// </summary>
        [TestMethod]
        public void TestClosureTypeDefinition()
        {
            // Set up the test
            const string input = "(a:int, b:bool) : int => { }";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var value = parser.closureExpression();

            // Perform the parsing
            var type = resolver.ResolveClosureExpression(value);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type.ParameterTypes[0], "The resolved type did not match the expected type");
            Assert.AreEqual(provider.BooleanType(), type.ParameterTypes[1], "The resolved type did not match the expected type");
            Assert.AreEqual(provider.IntegerType(), type.ReturnType, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests complex closure type resolving
        /// </summary>
        [TestMethod]
        public void TestVariadicAndInferredClosureTypeDefinition()
        {
            // Set up the test
            const string input = "(a = 10, b:int...) : int => { }";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var value = parser.closureExpression();

            // Perform the parsing
            var type = resolver.ResolveClosureExpression(value);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type.ParameterTypes[0], "The resolved type did not match the expected type");
            Assert.AreEqual(provider.ListForType(provider.IntegerType()), type.ParameterTypes[1], "The resolved type did not match the expected type");
            Assert.AreEqual(true, type.ParameterInfos[1].IsVariadic, "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests closure type resolving by providing a closure with different total argument count but equal required argument count
        /// </summary>
        [TestMethod]
        public void TestDefaultArgumentClosureTypeDefinition()
        {
            // Set up the test
            const string input = "(a = 10, b:int...) : (int->int) => { return (i:int, j:int=10) => { }; }";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where detected when not expected");
        }

        /// <summary>
        /// Tests closure type resolving by execution of callable
        /// </summary>
        [TestMethod]
        public void TestCalledClosureType()
        {
            // Set up the test
            const string input = "(a = 10, b:int...) : int => { }(10, 10)";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var value = parser.expression();

            // Perform the parsing
            var type = resolver.ResolveExpression(value);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type, "The resolved type did not match the expected type");
        }

        #endregion

        #region Prefix, postfix and unary expressions

        /// <summary>
        /// Tests resolving the type of prefix expressions
        /// </summary>
        [TestMethod]
        public void TestPrefixExpression()
        {
            // Set up the test
            const string input = "++i; --i; ++f; --f;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            var type4 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of Prefix expression correctly");
            Assert.AreEqual(provider.IntegerType(), type2, "Failed to evaluate the result of Prefix expression correctly");
            Assert.AreEqual(provider.FloatType(), type3, "Failed to evaluate the result of Prefix expression correctly");
            Assert.AreEqual(provider.FloatType(), type4, "Failed to evaluate the result of Prefix expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of postfix expressions
        /// </summary>
        [TestMethod]
        public void TestPostfixExpression()
        {
            // Set up the test
            const string input = "i++; i--; f++; f--;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            var type4 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of Postfix expression correctly");
            Assert.AreEqual(provider.IntegerType(), type2, "Failed to evaluate the result of Postfix expression correctly");
            Assert.AreEqual(provider.FloatType(), type3, "Failed to evaluate the result of Postfix expression correctly");
            Assert.AreEqual(provider.FloatType(), type4, "Failed to evaluate the result of Postfix expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of unary expressions
        /// </summary>
        [TestMethod]
        public void TestUnaryExpression()
        {
            // Set up the test
            const string input = "-i; !b; -f; !(true)";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            var type4 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of Unary expression correctly");
            Assert.AreEqual(provider.BooleanType(), type2, "Failed to evaluate the result of Unary expression correctly");
            Assert.AreEqual(provider.FloatType(), type3, "Failed to evaluate the result of Unary expression correctly");
            Assert.AreEqual(provider.BooleanType(), type4, "Failed to evaluate the result of Unary expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        #endregion

        #region Binary expression type resolving

        /// <summary>
        /// Tests resolving the type of basic arithmetic expressions
        /// </summary>
        [TestMethod]
        public void TestBasicArithmeticExpression()
        {
            // Set up the test
            const string input = "10 + 5; 5.5 + 6; 6.0 + 7.0;" +
                                 "10 - 5; 5.5 - 6; 6.0 - 7.0;" +
                                 "10 * 5; 5.5 * 6; 6.0 * 7.0;" +
                                 "10 / 5; 5.5 / 6; 6.0 / 7.0;" +
                                 "10 % 5; 5.5 % 6; 6.0 % 7.0;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            
            var type4 = resolver.ResolveExpression(parser.statement().expression());
            var type5 = resolver.ResolveExpression(parser.statement().expression());
            var type6 = resolver.ResolveExpression(parser.statement().expression());
            
            var type7 = resolver.ResolveExpression(parser.statement().expression());
            var type8 = resolver.ResolveExpression(parser.statement().expression());
            var type9 = resolver.ResolveExpression(parser.statement().expression());

            var type10 = resolver.ResolveExpression(parser.statement().expression());
            var type11 = resolver.ResolveExpression(parser.statement().expression());
            var type12 = resolver.ResolveExpression(parser.statement().expression());

            var type13 = resolver.ResolveExpression(parser.statement().expression());
            var type14 = resolver.ResolveExpression(parser.statement().expression());
            var type15 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of SUM expression correctly");
            Assert.AreEqual(provider.FloatType(), type2, "Failed to evaluate the result of SUM expression correctly");
            Assert.AreEqual(provider.FloatType(), type3, "Failed to evaluate the result of SUM expression correctly");

            Assert.AreEqual(provider.IntegerType(), type4, "Failed to evaluate the result of SUBTRACTION expression correctly");
            Assert.AreEqual(provider.FloatType(), type5, "Failed to evaluate the result of SUBTRACTION expression correctly");
            Assert.AreEqual(provider.FloatType(), type6, "Failed to evaluate the result of SUBTRACTION expression correctly");

            Assert.AreEqual(provider.IntegerType(), type7, "Failed to evaluate the result of MULTIPLICATION expression correctly");
            Assert.AreEqual(provider.FloatType(), type8, "Failed to evaluate the result of MULTIPLICATION expression correctly");
            Assert.AreEqual(provider.FloatType(), type9, "Failed to evaluate the result of MULTIPLICATION expression correctly");

            Assert.AreEqual(provider.IntegerType(), type10, "Failed to evaluate the result of DIVISION expression correctly");
            Assert.AreEqual(provider.FloatType(), type11, "Failed to evaluate the result of DIVISION expression correctly");
            Assert.AreEqual(provider.FloatType(), type12, "Failed to evaluate the result of DIVISION expression correctly");

            Assert.AreEqual(provider.IntegerType(), type13, "Failed to evaluate the result of MODULO expression correctly");
            Assert.AreEqual(provider.FloatType(), type14, "Failed to evaluate the result of MODULO expression correctly");
            Assert.AreEqual(provider.FloatType(), type15, "Failed to evaluate the result of MODULO expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests string concatenation
        /// </summary>
        [TestMethod]
        public void TestStringConcatenation()
        {
            // Set up the test
            const string input = "'a' + 10;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.StringType(), type1, "Failed to evaluate the result of expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests string concatenation with any types
        /// </summary>
        [TestMethod]
        public void TestStringAnyConcatenation()
        {
            // Set up the test
            const string input = "a + 'string';";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.StringType(), type1, "Failed to evaluate the result of expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [TestMethod]
        public void TestCompoundedArithmeticExpression()
        {
            // Set up the test
            const string input = "(5 + 7) * 8.0; ((5 / 9) + [7][0]) * () : int => { return 0; }(); ((5 / 9) + [7][0]) * 8.0;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.FloatType(), type1, "Failed to evaluate the result of compound expression correctly");
            Assert.AreEqual(provider.IntegerType(), type2, "Failed to evaluate the result of compound expression correctly");
            Assert.AreEqual(provider.FloatType(), type3, "Failed to evaluate the result of compound expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of basic bitwise expressions
        /// </summary>
        [TestMethod]
        public void TestBasicBitwiseExpression()
        {
            // Set up the test
            const string input = "10 | 5; 5 ^ 6; 6 & 7;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "Failed to evaluate the result of BITWISE OR expression correctly");
            Assert.AreEqual(provider.IntegerType(), type2, "Failed to evaluate the result of BITWISE XOR expression correctly");
            Assert.AreEqual(provider.IntegerType(), type3, "Failed to evaluate the result of BITWISE AND expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of basic comparision expressions
        /// </summary>
        [TestMethod]
        public void TestBasicComparisionExpression()
        {
            // Set up the test
            const string input = "true == false; null != 0; 10 > 5; 5.0 < 1; 7 >= 5; 8 <= 9;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());
            var type4 = resolver.ResolveExpression(parser.statement().expression());
            var type5 = resolver.ResolveExpression(parser.statement().expression());
            var type6 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.BooleanType(), type1, "Failed to evaluate the result of EQUALS expression correctly");
            Assert.AreEqual(provider.BooleanType(), type2, "Failed to evaluate the result of UNEQUALS expression correctly");
            Assert.AreEqual(provider.BooleanType(), type3, "Failed to evaluate the result of GREATER expression correctly");
            Assert.AreEqual(provider.BooleanType(), type4, "Failed to evaluate the result of LESS expression correctly");
            Assert.AreEqual(provider.BooleanType(), type5, "Failed to evaluate the result of GREATER OR EQUALS expression correctly");
            Assert.AreEqual(provider.BooleanType(), type6, "Failed to evaluate the result of LESS OR EQUALS expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests resolving the type of basic logical expressions
        /// </summary>
        [TestMethod]
        public void TestBasicLogicalExpressions()
        {
            // Set up the test
            const string input = "(9 == 7) && false; true && (false || true && false);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.BooleanType(), type1, "Failed to evaluate the result of logical expression correctly");
            Assert.AreEqual(provider.BooleanType(), type2, "Failed to evaluate the result of logical expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        /// <summary>
        /// Tests utilizing the '|', '&' and '^' operators with boolean types for standard (non-short circuited) boolean operations
        /// </summary>
        [TestMethod]
        public void TestStandardBooleanOperations()
        {
            // Set up the test
            const string input = "true ^ false; false | true; true & false;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());
            var type3 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.BooleanType(), type1, "Failed to evaluate the result of standard boolean expression correctly");
            Assert.AreEqual(provider.BooleanType(), type2, "Failed to evaluate the result of standard boolean expression correctly");
            Assert.AreEqual(provider.BooleanType(), type3, "Failed to evaluate the result of standard boolean expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        #endregion

        #region Ternary type resolving

        /// <summary>
        /// Tests ternary expression type resolving
        /// </summary>
        [TestMethod]
        public void TestTernaryResolving()
        {
            // Set up the test
            const string input = "i == i ? 0 : 1; i == i ? [0] : [true]; ";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var value1 = parser.statement().expression();
            var value2 = parser.statement().expression();

            // Perform the parsing
            var type1 = resolver.ResolveExpression(value1);
            var type2 = resolver.ResolveExpression(value2);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type1, "The resolved type did not match the expected type");
            Assert.AreEqual(provider.ListForType(provider.AnyType()), type2, "The resolved type did not match the expected type");
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests resolving of types in an assignment expression
        /// </summary>
        [TestMethod]
        public void TestFailedAssignmentExpressionResolving()
        {
            // Set up the test
            const string input = "i = false; b = 10; f -= 'abc'; s -= 'abc'; llf[0] = 0; s *= 10;";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider(), new TestDefinitionTypeProvider()));

            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());
            resolver.ResolveAssignmentExpression(parser.statement().assignmentExpression());

            // Compare the result now
            Assert.AreEqual(6, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid prefix expressions
        /// </summary>
        [TestMethod]
        public void TestFailedPrefixExpression()
        {
            // Set up the test
            const string input = "++b; --s; ++o; --v;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to report expected errors");
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid postfix expressions
        /// </summary>
        [TestMethod]
        public void TestFailedPostfixExpression()
        {
            // Set up the test
            const string input = "b++; o--; s++; v--;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to report expected errors");
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid unary expressions
        /// </summary>
        [TestMethod]
        public void TestFailedUnaryExpression()
        {
            // Set up the test
            const string input = "-b; !f; !s; !(i = 10);";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to report expected errors");
        }

        /// <summary>
        /// Tests basic type casting
        /// </summary>
        [TestMethod]
        public void TestFailedTypeCast()
        {
            // Set up the test
            const string input = "(float)true; (bool)1.0; ((->))true;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            
            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests resolving the type of an arithmetic expression
        /// </summary>
        [TestMethod]
        public void TestFailedArithmeticExpression()
        {
            // Set up the test
            const string input = "10 + null; true + false; ():void => {}() + 1;true/false;";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnOperation), "The expected error failed to be raised");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.VoidOnBinaryExpression), "The expected error failed to be raised");
        }

        /// <summary>
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [TestMethod]
        public void TestFailedCompoundedArithmeticExpression()
        {
            // Set up the test
            const string input = "(5 == 7) * 8.0;" +
                                 "((5 / 9) + [7][0]) * () : bool => { return true; }();" +
                                 "((5 / 9) + [true][0]) * 8.0;";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now

            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests resolving the type of basic bitwise expressions
        /// </summary>
        [TestMethod]
        public void TestFailedBasicBitwiseExpression()
        {
            // Set up the test
            const string input = "10 | 5.0; 5 ^ true; null & 7;";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(3, container.CodeErrors.Count(), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests resolving the type of basic comparision expressions
        /// </summary>
        [TestMethod]
        public void TestFailedBasicComparisionExpression()
        {
            // Set up the test
            const string input = "true == ():void => { }(); ():void => { }() != 0; true > 5; null < 1; 7 >= false; ():void => { }() <= 9;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(6, container.CodeErrors.Count(), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests resolving the type of basic logical expressions
        /// </summary>
        [TestMethod]
        public void TestFailedBasicLogicalExpressions()
        {
            // Set up the test
            const string input = "(9) && false; true && (null || true && false);12 && 15;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            
            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests error raising on closure type resolving by providing an argument type that does not match the default value
        /// </summary>
        [TestMethod]
        public void TestFailedImplicitArgumentCast()
        {
            // Set up the test
            const string input = "(a:bool = 10) => { }";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests failed ternary expression type resolving
        /// </summary>
        [TestMethod]
        public void TestFailedTernaryOperation()
        {
            // Set up the test
            const string input = "10 ? 0 : 1;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        #endregion

        #region Warning raising

        /// <summary>
        /// Tests reporting of subscription warnings
        /// </summary>
        [TestMethod]
        public void TestSubscriptWarning()
        {
            // Set up the test
            const string input = "() : int => {}[0]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.expression());

            Assert.AreEqual(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToSubscriptNonList));
        }

        /// <summary>
        /// Tests reporting of function call warnings
        /// </summary>
        [TestMethod]
        public void TestFunctionCallWarning()
        {
            // Set up the test
            const string input = "() : int => {}()()";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.expression());

            Assert.AreEqual(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable));
        }

        #endregion

        #region Runtime Tests

        /// <summary>
        /// Tests runtime parsing by assigning a value from a parameter with mismatching types
        /// </summary>
        [TestMethod]
        public void TestRuntimeVariadicParameter()
        {
            const string input = "func funca(a:int...){ var b:int = a; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }
        
        /// <summary>
        /// Tests runtime parsing by assigning a mismatched type from an inferred type
        /// </summary>
        [TestMethod]
        public void TestRuntimeCallableResolve()
        {
            const string input = "func funca(a:int...){ var b = ():float => { return 1.0; }; var aa:int = b(); }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests runtime parsing by returning a mismatched value in a function
        /// </summary>
        [TestMethod]
        public void TestRuntimeMismatchedReturnTypes()
        {
            const string input = "func funca() : int { return 1.0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        #endregion
    }
}