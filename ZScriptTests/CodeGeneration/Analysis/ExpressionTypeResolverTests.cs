﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Elements.Typing;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScriptTests.Runtime;

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

            var parser = ZRuntimeTests.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.AreEqual(TypeDef.IntegerType, type, "The resolved type did not match the expected type");
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

            var parser = ZRuntimeTests.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

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
            Assert.AreEqual(TypeDef.NullType, resolver.ResolveCompileConstant(nullConst), "The resolved type did not match the expected type");
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

            var parser = ZRuntimeTests.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

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
            Assert.AreEqual(TypeDef.NullType, resolver.ResolveConstantAtom(nullConst), "The resolved type did not match the expected type");
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

            var parser = ZRuntimeTests.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

            var type = parser.arrayLiteral();

            // Compare the result now
            Assert.AreEqual(new ListTypeDef(TypeDef.IntegerType), resolver.ResolveArrayLiteral(type), "The resolved type did not match the expected type");
        }

        #endregion

        #region Type casting

        /// <summary>
        /// Tests basic type casting
        /// </summary>
        [TestMethod]
        public void TestTypeCast()
        {
            // Set up the test
            const string input = "(float)1; (int)1.0; (bool)true; ([int])[0];";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

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

        #region Closure type resolving

        /// <summary>
        /// Tests closure type resolving
        /// </summary>
        [TestMethod]
        public void TestClosureTypeDefinition()
        {
            // Set up the test
            const string input = "(a:int, b:bool) : int => { }";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new TypeProvider(), new MessageContainer());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

            var value = parser.closureExpression();

            // Perform the parsing
            var type = resolver.ResolveClosureExpression(value);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type.ParameterTypes[0], "The resolved type did not match the expected type");
            Assert.AreEqual(provider.ListForType(provider.IntegerType()), type.ParameterTypes[1], "The resolved type did not match the expected type");
        }

        /// <summary>
        /// Tests closure type resolving by execution of callable
        /// </summary>
        [TestMethod]
        public void TestCalledClosureType()
        {
            // Set up the test
            const string input = "(a = 10, b:int...) : int => { }(10, 10)";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, new MessageContainer());

            var value = parser.expression();

            // Perform the parsing
            var type = resolver.ResolveExpression(value);

            // Compare the result now
            Assert.AreEqual(provider.IntegerType(), type, "The resolved type did not match the expected type");
        }

        #endregion

        #region Prefix and postfix expressions

        /// <summary>
        /// Tests resolving the type of a prefix expression
        /// </summary>
        [TestMethod]
        public void TestPrefixExpression()
        {
            // Set up the test
            const string input = "++i; --i; ++f; --f;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container, new TestDefinitionTypeProvider());

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
        /// Tests resolving the type of a postfix expression
        /// </summary>
        [TestMethod]
        public void TestPostfixExpression()
        {
            // Set up the test
            const string input = "i++; i--; f++; f--;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container, new TestDefinitionTypeProvider());

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

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
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [TestMethod]
        public void TestCompoundedArithmeticExpression()
        {
            // Set up the test
            const string input = "(5 + 7) * 8.0; ((5 / 9) + [7][0]) * () : int => { return 0; }(); ((5 / 9) + [7][0]) * 8.0;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

            // Perform the parsing
            var type1 = resolver.ResolveExpression(parser.statement().expression());
            var type2 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(provider.BooleanType(), type1, "Failed to evaluate the result of logical expression correctly");
            Assert.AreEqual(provider.BooleanType(), type2, "Failed to evaluate the result of logical expression correctly");

            Assert.AreEqual(0, container.CodeErrors.Count(), "Errors were detected when not expected");
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests resolving the type of a prefix expression
        /// </summary>
        [TestMethod]
        public void TestFailedPrefixExpression()
        {
            // Set up the test
            const string input = "++b; --s; ++o; --v;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container, new TestDefinitionTypeProvider());

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(4, container.CodeErrors.Count(), "Failed to report expected errors");
        }

        /// <summary>
        /// Tests resolving the type of a postfix expression
        /// </summary>
        [TestMethod]
        public void TestFailedPostfixExpression()
        {
            // Set up the test
            const string input = "b++; o--; s++; v--;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container, new TestDefinitionTypeProvider());

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
            const string input = "(float)true; (bool)1.0; ((->))true; ([int])null;";

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(4, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests resolving the type of an arithmetic expression
        /// </summary>
        [TestMethod]
        public void TestFailedArithmeticExpression()
        {
            // Set up the test
            const string input = "10 + null; true + false; ():void => {}() + 1;true/false;";

            var parser = ZRuntimeTests.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new TypeProvider(), container);

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.AreEqual(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnBinaryExpression), "The expected error failed to be raised");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.VoidOnBinaryExpression), "The expected error failed to be raised");
        }

        /// <summary>
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [TestMethod]
        public void TestFailedCompoundedArithmeticExpression()
        {
            // Set up the test
            const string input = "(5 == 7) * 8.0; ((5 / 9) + [7][0]) * () : bool => { return 0; }(); ((5 / 9) + [true][0]) * 8.0;";

            var parser = ZRuntimeTests.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new TypeProvider(), container);

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now

            Assert.AreEqual(3, container.CodeErrors.Count(), "Failed to raise the expected errors");
        }

        /// <summary>
        /// Tests resolving the type of basic bitwise expressions
        /// </summary>
        [TestMethod]
        public void TestFailedBasicBitwiseExpression()
        {
            // Set up the test
            const string input = "10 | 5.0; 5 ^ true; null & 7;";

            var parser = ZRuntimeTests.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new TypeProvider(), container);

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

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

            var parser = ZRuntimeTests.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(provider, container);

            // Perform the parsing
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.AreEqual(3, container.CodeErrors.Count(), "Failed to raise the expected errors");
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

            var parser = ZRuntimeTests.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, container);

            resolver.ResolveExpression(parser.expression());

            Assert.AreEqual(1, container.Warnings.Count(w => w.Code == WarningCode.TryingToSubscriptNonList));
        }

        /// <summary>
        /// Tests reporting of function call warnings
        /// </summary>
        [TestMethod]
        public void TestFunctionCallWarning()
        {
            // Set up the test
            const string input = "() : int => {}()()";

            var parser = ZRuntimeTests.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(provider, container);

            resolver.ResolveExpression(parser.expression());

            Assert.AreEqual(1, container.Warnings.Count(w => w.Code == WarningCode.TryingToCallNonCallable));
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

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.ParseInputString();
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }
        
        /// <summary>
        /// Tests runtime parsing by assigning a mismatched type from an inferred type
        /// </summary>
        [TestMethod]
        public void TestRuntimeCallableResolve()
        {
            const string input = "func funca(a:int...){ var a = ():float => { return 1.0; }; var aa:int = a(); }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.ParseInputString();
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

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.ParseInputString();
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise the expected errors");
        }

        #endregion

        /// <summary>
        /// Test definition type provider used in tests
        /// </summary>
        class TestDefinitionTypeProvider : IDefinitionTypeProvider
        {
            public TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName)
            {
                if(definitionName == "i")
                    return TypeDef.IntegerType;
                if (definitionName == "f")
                    return TypeDef.FloatType;
                if (definitionName == "b")
                    return TypeDef.BooleanType;
                if (definitionName == "s")
                    return TypeDef.StringType;
                if (definitionName == "o")
                    return new ObjectTypeDef();
                if (definitionName == "v")
                    return TypeDef.VoidType;
                if (definitionName == "a")
                    return TypeDef.AnyType;

                return TypeDef.AnyType;
            }
        }
    }
}