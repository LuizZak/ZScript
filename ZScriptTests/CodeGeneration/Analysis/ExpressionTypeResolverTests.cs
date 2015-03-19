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

using Xunit;
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
    public class ExpressionTypeResolverTests
    {
        #region General Type Resolving

        /// <summary>
        /// Tests callable type resolving
        /// </summary>
        [Fact]
        public void TestCallableTypeResolving()
        {
            // Set up the test
            const string input = "(int, bool->any)";

            var parser = TestUtils.CreateParser(input);
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider()));

            var value = parser.callableType();

            var type = resolver.ResolveCallableType(value);

            // Compare the result now
            // "The resolved type did not match the expected type"
            Assert.Equal(TypeDef.IntegerType, type.ParameterTypes[0]);
            // "The resolved type did not match the expected type"
            Assert.Equal(TypeDef.BooleanType, type.ParameterTypes[1]);
            // "The resolved type did not match the expected type"
            Assert.Equal(TypeDef.AnyType, type.ReturnType);
        }

        /// <summary>
        /// Tests list type resolving
        /// </summary>
        [Fact]
        public void TestListType()
        {
            // Set up the test
            const string input = "[int] [int] [[int]]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveListType(parser.listType());
            var type2 = (ListTypeDef)resolver.ResolveType(parser.type(), false);
            var type3 = (ListTypeDef)resolver.ResolveType(parser.type(), false);

            // Compare the result now
            Assert.Equal(TypeDef.IntegerType, type1.EnclosingType); // "The resolved type did not match the expected type"
            Assert.Equal(TypeDef.IntegerType, type2.EnclosingType); // "The resolved type did not match the expected type"
            Assert.Equal(provider.ListForType(TypeDef.IntegerType), type3.EnclosingType); // "The resolved type did not match the expected type"
        }

        /// <summary>
        /// Tests dictionary type resolving
        /// </summary>
        [Fact]
        public void TestDictionaryType()
        {
            // Set up the test
            const string input = "[int:string] [[int]:[string]]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveDictionaryType(parser.dictionaryType());
            var type2 = resolver.ResolveDictionaryType(parser.dictionaryType());

            // Compare the result now
            Assert.Equal(TypeDef.IntegerType, type1.SubscriptType); // "The resolved type did not match the expected type"
            Assert.Equal(TypeDef.StringType, type1.EnclosingType); // "The resolved type did not match the expected type"

            Assert.Equal(provider.ListForType(TypeDef.IntegerType), type2.SubscriptType); // "The resolved type did not match the expected type"
            Assert.Equal(provider.ListForType(TypeDef.StringType), type2.EnclosingType); // "The resolved type did not match the expected type"
        }

        /// <summary>
        /// Tests subscripting a list-type object
        /// </summary>
        [Fact]
        public void TestListSubscript()
        {
            // Set up the test
            const string input = "() : [int] => {}()[0]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.Equal(TypeDef.IntegerType, type);
        }

        /// <summary>
        /// Tests chaining calling of return values
        /// </summary>
        [Fact]
        public void TestCallableType()
        {
            // Set up the test
            const string input = "(int,int->int) (int,int->void) (->int) (->)";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type1 = resolver.ResolveCallableType(parser.callableType());
            var type2 = resolver.ResolveCallableType(parser.callableType());
            var type3 = resolver.ResolveCallableType(parser.callableType());
            var type4 = resolver.ResolveCallableType(parser.callableType());

            // Compare the result now
            Assert.Equal(
                new CallableTypeDef(new[]
                {
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false),
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false)
                }, TypeDef.IntegerType, true),
                type1);

            Assert.Equal(
                new CallableTypeDef(new[]
                {
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false),
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false)
                }, TypeDef.VoidType, true),
                type2);

            Assert.Equal(
                new CallableTypeDef(new CallableTypeDef.CallableParameterInfo[0], TypeDef.IntegerType, true),
                type3);

            Assert.Equal(
                new CallableTypeDef(new CallableTypeDef.CallableParameterInfo[0], TypeDef.VoidType, false),
                type4);
        }

        /// <summary>
        /// Tests chaining calling of return values
        /// </summary>
        [Fact]
        public void TestChainedCallable()
        {
            // Set up the test
            const string input = "() : (->(->int)) => {}()()()";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var type = resolver.ResolveExpression(parser.expression());

            // Compare the result now
            Assert.Equal(TypeDef.IntegerType, type);
        }

        #endregion

        #region Assignment expression type resolving

        /// <summary>
        /// Tests resolving of types in an assignment expression
        /// </summary>
        [Fact]
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
            Assert.Equal(TypeDef.IntegerType, type1);
            Assert.Equal(TypeDef.BooleanType, type2);
            Assert.Equal(TypeDef.FloatType,   type3);
            Assert.Equal(TypeDef.StringType,  type4);
            Assert.Equal(TypeDef.FloatType,   type5);
        }

        #endregion

        #region Atoms and compile-time constants

        /// <summary>
        /// Tests compile constant type resolving
        /// </summary>
        [Fact]
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
            Assert.Equal(TypeDef.IntegerType, resolver.ResolveCompileConstant(intConst));
            Assert.Equal(TypeDef.IntegerType, resolver.ResolveCompileConstant(negativeIntConst));
            Assert.Equal(TypeDef.FloatType, resolver.ResolveCompileConstant(floatConst));
            Assert.Equal(TypeDef.FloatType, resolver.ResolveCompileConstant(negativeFloatConst));
            Assert.Equal(TypeDef.BooleanType, resolver.ResolveCompileConstant(boolTrueConst));
            Assert.Equal(TypeDef.BooleanType, resolver.ResolveCompileConstant(boolFalseConst));
            Assert.Equal(TypeDef.AnyType, resolver.ResolveCompileConstant(nullConst));
            Assert.Equal(TypeDef.StringType, resolver.ResolveCompileConstant(stringConst));
        }

        /// <summary>
        /// Tests constant atom type resolving
        /// </summary>
        [Fact]
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
            Assert.Equal(TypeDef.IntegerType, resolver.ResolveConstantAtom(intConst));
            Assert.Equal(TypeDef.FloatType, resolver.ResolveConstantAtom(floatConst));
            Assert.Equal(TypeDef.BooleanType, resolver.ResolveConstantAtom(boolTrueConst));
            Assert.Equal(TypeDef.BooleanType, resolver.ResolveConstantAtom(boolFalseConst));
            Assert.Equal(TypeDef.AnyType, resolver.ResolveConstantAtom(nullConst));
            Assert.Equal(TypeDef.StringType, resolver.ResolveConstantAtom(stringConst));
        }

        #endregion

        #region Literal resolving

        /// <summary>
        /// Tests object literal type solving
        /// </summary>
        [Fact]
        public void TestObjectLiteral()
        {
            // Set up the test
            const string input = "{ x:10 }";

            var parser = TestUtils.CreateParser(input);
            var typeProvider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), typeProvider));

            var type = parser.objectLiteral();

            // Compare the result now
            Assert.Equal(typeProvider.ObjectType(), resolver.ResolveObjectLiteral(type));
        }

        /// <summary>
        /// Tests array literal type resolving
        /// </summary>
        [Fact]
        public void TestArrayLiteral()
        {
            // Set up the test
            const string input = "[0, 1, 2]";

            var parser = TestUtils.CreateParser(input);
            var typeProvider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), typeProvider));

            var type = parser.arrayLiteral();
            var resolvedType = resolver.ResolveArrayLiteral(type);

            // Compare the result now
            Assert.Equal(typeProvider.ListForType(typeProvider.IntegerType()), resolvedType);
            Assert.Equal(typeProvider.IntegerType(), type.EvaluatedValueType);
        }

        /// <summary>
        /// Tests dictionary literal type resolving
        /// </summary>
        [Fact]
        public void TestDictionaryLiteral()
        {
            // Set up the test
            const string input = "[0: 'apples', 1: 'oranges']";

            var parser = TestUtils.CreateParser(input);
            var typeProvider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), typeProvider));

            var type = parser.dictionaryLiteral();

            var resolvedType = resolver.ResolveDictionaryLiteral(type);

            // Compare the result now
            Assert.Equal(typeProvider.DictionaryForTypes(typeProvider.IntegerType(), typeProvider.StringType()), resolvedType);
            Assert.Equal(typeProvider.IntegerType(), type.EvaluatedKeyType);
            Assert.Equal(typeProvider.StringType(), type.EvaluatedValueType);
        }

        #endregion

        #region Callable function call resolving

        /// <summary>
        /// Tests callable argument type checking
        /// </summary>
        [Fact]
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
            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests callable argument count checking
        /// </summary>
        [Fact]
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
            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooFewArguments));
            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooManyArguments));
        }

        #endregion

        #region Type casting/checking

        /// <summary>
        /// Tests basic type casting
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.FloatType(), resolver.ResolveExpression(exp1));
            Assert.Equal(provider.IntegerType(), resolver.ResolveExpression(exp2));
            Assert.Equal(provider.BooleanType(), resolver.ResolveExpression(exp3));
            Assert.Equal(provider.ListForType(provider.IntegerType()), resolver.ResolveExpression(exp4));
        }

        /// <summary>
        /// Tests implicit array casting
        /// </summary>
        [Fact]
        public void TestImplicitArray()
        {
            // Set up the test
            const string input = "lf = [0]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            var assignment = parser.assignmentExpression();

            resolver.ResolveAssignmentExpression(assignment);
            container.PrintMessages();

            Assert.False(container.HasErrors);
            // Compare the result now
            Assert.Equal(provider.FloatType(), assignment.expression().arrayLiteral().expressionList().expression(0).ImplicitCastType);
            Assert.Equal(provider.ListForType(provider.FloatType()), assignment.expression().arrayLiteral().ExpectedType);
            Assert.Equal(provider.ListForType(provider.FloatType()), assignment.expression().arrayLiteral().ImplicitCastType);
        }

        /// <summary>
        /// Tests multiple sequential type casting
        /// </summary>
        [Fact]
        public void TestMultipleCasting()
        {
            // Set up the test
            const string input = "([int])(any)[0];";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider));

            var exp1 = parser.statement().expression();

            // Compare the result now
            Assert.Equal(provider.ListForType(provider.IntegerType()), resolver.ResolveExpression(exp1));
        }

        #endregion

        #region Subscription/function call argument type resolving

        /// <summary>
        /// Tests list subscription type checking
        /// </summary>
        [Fact]
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

            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests checking of dictionary types
        /// </summary>
        [Fact]
        public void TestDictionarySubscriptionTypeChecking()
        {
            // Set up the test
            const string input = "[0:'']['invalid!']; [0:0][1.0]; [0.0:1][0]; [:]['anyType'];";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, new TypeProvider()));

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());

            Assert.Equal(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        #endregion

        #region Class Type resolving

        /// <summary>
        /// Tests resolving the type of a class field
        /// </summary>
        [Fact]
        public void TestClassFieldTypeResolving()
        {
            // Set up the test
            const string input = "tClass.field1";

            // Create the test class
            var testClass = TestUtils.CreateTestClassDefinition();

            var definitionTypeProvider = new TestDefinitionTypeProvider();
            definitionTypeProvider.CustomTypes["tClass"] = testClass.ClassTypeDef;

            var container = new MessageContainer();
            var context = new RuntimeGenerationContext(null, container, new TypeProvider())
            {
                DefinitionTypeProvider = definitionTypeProvider
            };

            var resolver = new ExpressionTypeResolver(context);

            var parser = TestUtils.CreateParser(input);

            var type = resolver.ResolveExpression(parser.expression());

            Assert.Equal(context.TypeProvider.IntegerType(), type);
        }

        /// <summary>
        /// Tests resolving the type of a class method call
        /// </summary>
        [Fact]
        public void TestClassMethodTypeResolving()
        {
            // Set up the test
            const string input = "tClass.func1()";

            // Create the test class
            var testClass = TestUtils.CreateTestClassDefinition();

            var definitionTypeProvider = new TestDefinitionTypeProvider();
            definitionTypeProvider.CustomTypes["tClass"] = testClass.ClassTypeDef;

            var container = new MessageContainer();
            var context = new RuntimeGenerationContext(null, container, new TypeProvider())
            {
                DefinitionTypeProvider = definitionTypeProvider
            };

            var resolver = new ExpressionTypeResolver(context);

            var parser = TestUtils.CreateParser(input);

            var type = resolver.ResolveExpression(parser.expression());

            Assert.Equal(context.TypeProvider.IntegerType(), type);
        }

        #endregion

        #region Closure type resolving

        /// <summary>
        /// Tests closure type resolving
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type.ParameterTypes[0]);
            Assert.Equal(provider.BooleanType(), type.ParameterTypes[1]);
            Assert.Equal(provider.IntegerType(), type.ReturnType);
        }

        /// <summary>
        /// Tests complex closure type resolving
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type.ParameterTypes[0]);
            Assert.Equal(provider.ListForType(provider.IntegerType()), type.ParameterTypes[1]);
            Assert.Equal(true, type.ParameterInfos[1].IsVariadic);
        }

        /// <summary>
        /// Tests closure type resolving by providing a closure with different total argument count but equal required argument count
        /// </summary>
        [Fact]
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

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests closure type resolving by execution of callable
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type);
        }

        #endregion

        #region Prefix, postfix and unary expressions

        /// <summary>
        /// Tests resolving the type of prefix expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.IntegerType(), type2);
            Assert.Equal(provider.FloatType(), type3);
            Assert.Equal(provider.FloatType(), type4);

            Assert.Equal(0, container.CodeErrors.Count());
        }

        /// <summary>
        /// Tests resolving the type of postfix expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.IntegerType(), type2);
            Assert.Equal(provider.FloatType(), type3);
            Assert.Equal(provider.FloatType(), type4);

            Assert.Equal(0, container.CodeErrors.Count());
        }

        /// <summary>
        /// Tests resolving the type of unary expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.BooleanType(), type2);
            Assert.Equal(provider.FloatType(), type3);
            Assert.Equal(provider.BooleanType(), type4);

            Assert.Equal(0, container.CodeErrors.Count());
        }

        #endregion

        #region Binary expression type resolving

        /// <summary>
        /// Tests resolving the type of basic arithmetic expressions
        /// </summary>
        [Fact]
        public void TestBasicArithmeticExpression()
        {
            // Set up the test
            const string input = "10 + 5; 5.5 + 6; 6.0 + 7.0;" +
                                 "10 - 5; 5.5 - 6; 6.0 - 7.0;" +
                                 "10 * 5; 5.5 * 6; 6.0 * 7.0;" +
                                 "10 / 5; 5.5 / 6; 6.0 / 7.0;" +
                                 "10 % 5; 5.5 % 6; 6.0 % 7.0;" +
                                 "a + a; a / a; a - a; a * a;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

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

            var type16 = resolver.ResolveExpression(parser.statement().expression());
            var type17 = resolver.ResolveExpression(parser.statement().expression());
            var type18 = resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.FloatType(), type2);
            Assert.Equal(provider.FloatType(), type3);

            Assert.Equal(provider.IntegerType(), type4);
            Assert.Equal(provider.FloatType(), type5);
            Assert.Equal(provider.FloatType(), type6);

            Assert.Equal(provider.IntegerType(), type7);
            Assert.Equal(provider.FloatType(), type8);
            Assert.Equal(provider.FloatType(), type9);

            Assert.Equal(provider.IntegerType(), type10);
            Assert.Equal(provider.FloatType(), type11);
            Assert.Equal(provider.FloatType(), type12);

            Assert.Equal(provider.IntegerType(), type13);
            Assert.Equal(provider.FloatType(), type14);
            Assert.Equal(provider.FloatType(), type15);

            Assert.Equal(provider.AnyType(), type16);
            Assert.Equal(provider.AnyType(), type17);
            Assert.Equal(provider.AnyType(), type18);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests string concatenation
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.StringType(), type1);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests string concatenation with any types
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.StringType(), type1);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.FloatType(), type1);
            Assert.Equal(provider.IntegerType(), type2);
            Assert.Equal(provider.FloatType(), type3);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic bitwise expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.IntegerType(), type2);
            Assert.Equal(provider.IntegerType(), type3);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic comparision expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.BooleanType(), type1);
            Assert.Equal(provider.BooleanType(), type2);
            Assert.Equal(provider.BooleanType(), type3);
            Assert.Equal(provider.BooleanType(), type4);
            Assert.Equal(provider.BooleanType(), type5);
            Assert.Equal(provider.BooleanType(), type6);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic logical expressions
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.BooleanType(), type1);
            Assert.Equal(provider.BooleanType(), type2);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests utilizing the '|', '&' and '^' operators with boolean types for standard (non-short circuited) boolean operations
        /// </summary>
        [Fact]
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
            Assert.Equal(provider.BooleanType(), type1);
            Assert.Equal(provider.BooleanType(), type2);
            Assert.Equal(provider.BooleanType(), type3);

            Assert.Equal(0, container.CodeErrors.Length);
        }

        #endregion

        #region Ternary type resolving

        /// <summary>
        /// Tests ternary expression type resolving
        /// </summary>
        [Fact]
        public void TestTernaryResolving()
        {
            // Set up the test
            const string input = "i == i ? 0 : 1; i == i ? [0] : [1]; ";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, new MessageContainer(), provider, new TestDefinitionTypeProvider()));

            var value1 = parser.statement().expression();
            var value2 = parser.statement().expression();

            // Perform the parsing
            var type1 = resolver.ResolveExpression(value1);
            var type2 = resolver.ResolveExpression(value2);

            // Compare the result now
            Assert.Equal(provider.IntegerType(), type1);
            Assert.Equal(provider.ListForType(provider.IntegerType()), type2);
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests error raising when using 'void' type in invalid contexts
        /// </summary>
        [Fact]
        public void TestInvalidVoid()
        {
            // Set up the test
            const string input = "[void: int] [void]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveDictionaryType(parser.dictionaryType());
            resolver.ResolveListType(parser.listType());

            // Compare the result now
            Assert.Equal(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidVoidType));
        }

        /// <summary>
        /// Tests error raising for failed 'new' expressions
        /// </summary>
        [Fact]
        public void TestFailedNewExpression()
        {
            // Set up the test
            const string input = "new Unknown();";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.statement().expression());

            // Compare the result now
            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UnkownType));
        }

        /// <summary>
        /// Tests error raising for basic type casting
        /// </summary>
        [Fact]
        public void TestUnkownType()
        {
            // Set up the test
            const string input = "(unknown)1; 1 is unknown; unknown";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveExpression(parser.statement().expression());
            resolver.ResolveType(parser.type(), false);

            // Compare the result now
            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UnkownType));
        }

        /// <summary>
        /// Tests dictionary literal type resolving
        /// </summary>
        [Fact]
        public void TestFailedDictionaryLiteralCasting()
        {
            // Set up the test
            const string input = "[0: 'apples', 1: 'oranges']";

            var parser = TestUtils.CreateParser(input);
            var typeProvider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, typeProvider));

            var dictionary = parser.dictionaryLiteral();

            dictionary.ExpectedType = typeProvider.DictionaryForTypes(typeProvider.IntegerType(), typeProvider.IntegerType());

            resolver.ResolveDictionaryLiteral(dictionary);

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests error raising for invalid implicit array casting
        /// </summary>
        [Fact]
        public void TestFailedImplicitArray()
        {
            // Set up the test
            const string input = "lf = [0, true]";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider, new TestDefinitionTypeProvider()));

            var assignment = parser.assignmentExpression();

            resolver.ResolveAssignmentExpression(assignment);
            container.PrintMessages();

            Assert.Equal(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests resolving of types in an assignment expression
        /// </summary>
        [Fact]
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
            Assert.Equal(6, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid prefix expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(4, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid postfix expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(4, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests error raising when resolving the type of invalid unary expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(4, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests basic type casting
        /// </summary>
        [Fact]
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
            
            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests resolving the type of an arithmetic expression
        /// </summary>
        [Fact]
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
            Assert.Equal(3, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnOperation));
            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.VoidOnBinaryExpression));
        }

        /// <summary>
        /// Tests resolving the type of arithmetic expressions that are evaluated from complex scenarios (compounded, in parenthesis, evaluated from arrays, etc.)
        /// </summary>
        [Fact]
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

            Assert.Equal(4, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic bitwise expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(3, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic comparision expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(6, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests resolving the type of basic logical expressions
        /// </summary>
        [Fact]
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

            Assert.Equal(4, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests error raising on closure type resolving by providing an argument type that does not match the default value
        /// </summary>
        [Fact]
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

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests failed ternary expression type resolving
        /// </summary>
        [Fact]
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

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        #endregion

        #region Warning raising

        /// <summary>
        /// Tests reporting of subscription warnings
        /// </summary>
        [Fact]
        public void TestSubscriptWarning()
        {
            // Set up the test
            const string input = "() : int => {}[0]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.expression());

            Assert.Equal(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToSubscriptNonList));
        }

        /// <summary>
        /// Tests reporting of function call warnings
        /// </summary>
        [Fact]
        public void TestFunctionCallWarning()
        {
            // Set up the test
            const string input = "() : int => {}()()";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var provider = new TypeProvider();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            resolver.ResolveExpression(parser.expression());

            Assert.Equal(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable));
        }

        /// <summary>
        /// Tests raising warnings when trying to call non callables
        /// </summary>
        [Fact]
        public void TestCallingUncallable()
        {
            const string input = "func f() { var a = 10; a(); }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.Equal(1, generator.MessageContainer.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable));
            Assert.Equal(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests raising warnings when trying to call non callables
        /// </summary>
        [Fact]
        public void TestShadowedCallingUncallable()
        {
            const string input = "func f() { var a = 10; a(); } func a() { }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.Equal(1, generator.MessageContainer.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable));
            Assert.Equal(0, generator.MessageContainer.CodeErrors.Length);
        }

        /// <summary>
        /// Tests handling using definitions before shadowing
        /// </summary>
        [Fact]
        public void TestUsageBeforeShadowing()
        {
            const string input = "func f() { a(); var a = 10; } func a() { }";
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.Equal(0, generator.MessageContainer.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable));
            Assert.Equal(0, generator.MessageContainer.CodeErrors.Length);
        }

        #endregion

        #region Exception raising

        /// <summary>
        /// Tests exception raising during 'this' type resolving with no definition type provider defined
        /// </summary>
        [Fact]
        public void TestFailedThisResolvingException()
        {
            // Set up the test
            const string input = "this;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            // "Trying to resolve a 'this' type with no definition type provider speciifed should result in an exception"
            Assert.Throws<Exception>(() => { resolver.ResolveExpression(parser.statement().expression()); });
        }

        /// <summary>
        /// Tests exception raising during 'base' type resolving with no definition type provider defined
        /// </summary>
        [Fact]
        public void TestFailedBaseResolvingException()
        {
            // Set up the test
            const string input = "base;";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            // "Trying to resolve a 'base' type with no definition type provider speciifed should result in an exception"
            Assert.Throws<Exception>(() => { resolver.ResolveExpression(parser.statement().expression()); });
        }

        #endregion

        #region Runtime Tests

        /// <summary>
        /// Tests runtime parsing by assigning a value from a parameter with mismatching types
        /// </summary>
        [Fact]
        public void TestRuntimeVariadicParameter()
        {
            const string input = "func funca(a:int...){ var b:int = a; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }
        
        /// <summary>
        /// Tests runtime parsing by assigning a mismatched type from an inferred type
        /// </summary>
        [Fact]
        public void TestRuntimeCallableResolve()
        {
            const string input = "func funca(a:int...){ var b = ():float => { return 1.0; }; var aa:int = b(); }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        /// <summary>
        /// Tests runtime parsing by returning a mismatched value in a function
        /// </summary>
        [Fact]
        public void TestRuntimeMismatchedReturnTypes()
        {
            const string input = "func funca() : int { return 1.0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;

            generator.CollectDefinitions();

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast));
        }

        #endregion
    }
}