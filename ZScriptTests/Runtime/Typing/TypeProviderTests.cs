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
using System.Collections.Generic;

using Rhino.Mocks;

using Xunit;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Elements;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime.Typing
{
    /// <summary>
    /// Tests the TypeProvider class and related components
    /// </summary>
    public class TypeProviderTests
    {
        #region Custom type support

        /// <summary>
        /// Tests storaging and fetching of custom types
        /// </summary>
        [Fact]
        public void TestCustomType()
        {
            var typeProvider = new TypeProvider();
            var expectedType = TypeDef.StringType;

            var typeSource = MockRepository.Mock<ICustomTypeSource>();

            typeSource.Stub(x => x.HasType("CustomType")).Return(true);
            typeSource.Stub(x => x.TypeNamed("CustomType")).Return(expectedType);

            typeProvider.RegisterCustomTypeSource(typeSource);

            var type = typeProvider.TypeNamed("CustomType");

            Assert.Equal(expectedType, type);
        }

        #endregion

        #region NativeTypeForTypeDf

        [Fact]
        public void TestNativeTypeInteger()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.IntegerType());

            Assert.Equal(typeof(long), native);
        }

        [Fact]
        public void TestNativeTypeBoolean()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.BooleanType());

            Assert.Equal(typeof(bool), native);
        }

        [Fact]
        public void TestNativeTypeString()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.StringType());

            Assert.Equal(typeof(string), native);
        }

        [Fact]
        public void TestNativeTypeFloat()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.FloatType());

            Assert.Equal(typeof(double), native);
        }

        [Fact]
        public void TestNativeTypeVoid()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.VoidType());

            Assert.Equal(typeof(void), native);
        }

        [Fact]
        public void TestNativeTypeNull()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.NullType());

            Assert.Equal(typeof(object), native);
        }

        [Fact]
        public void TestNativeTypeList()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ListForType(provider.IntegerType()));

            Assert.Equal(typeof(List<long>), native);
        }

        [Fact]
        public void TestNativeTypeAnyList()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ListForType(provider.AnyType()));

            Assert.Equal(typeof(List<object>), native);
        }

        [Fact]
        public void TestNativeTypeDictionary()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.DictionaryForTypes(provider.StringType(), provider.FloatType()));

            Assert.Equal(typeof(Dictionary<string, double>), native);
        }

        [Fact]
        public void TestNativeTypeObject()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ObjectType());

            Assert.Equal(typeof(ZObject), native);
        }

        [Fact]
        public void TestNativeTypeAny()
        {
            var provider = new TypeProvider();

            var native1 = provider.NativeTypeForTypeDef(provider.AnyType());
            var native2 = provider.NativeTypeForTypeDef(provider.AnyType(), true);

            Assert.Equal(null, native1);
            Assert.Equal(typeof(object), native2);
        }

        [Fact]
        public void TestNativeTypeClass()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(new ClassTypeDef("class"));

            Assert.Equal(native, native);
        }

        #endregion

        #region FindCommonType

        /// <summary>
        /// Tests the FindCommonType method with the same types
        /// </summary>
        [Fact]
        public void TestSameTypesFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.Equal(provider.IntegerType(),
                provider.FindCommonType(provider.IntegerType(), provider.IntegerType()));

            Assert.Equal(provider.ListForType(provider.IntegerType()),
                provider.FindCommonType(provider.ListForType(provider.IntegerType()), provider.ListForType(provider.IntegerType())));
        }

        /// <summary>
        /// Tests the FindCommonType method with void and any types
        /// </summary>
        [Fact]
        public void TestVoidAnyPropagationFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.Equal(provider.VoidType(), provider.FindCommonType(provider.VoidType(), provider.AnyType()));

            Assert.Equal(provider.AnyType(),
                provider.FindCommonType(provider.AnyType(), provider.IntegerType()));
            
            Assert.Equal(provider.VoidType(),
                provider.FindCommonType(provider.VoidType(), provider.IntegerType()));

            Assert.Equal(provider.AnyType(),
                provider.FindCommonType(provider.ListForType(provider.AnyType()), provider.ListForType(provider.IntegerType())));

            Assert.Equal(provider.AnyType(),
                provider.FindCommonType(provider.ListForType(provider.AnyType()), provider.ListForType(provider.VoidType())));
        }

        /// <summary>
        /// Tests the FindCommonType method with convertible primitive types
        /// </summary>
        [Fact]
        public void TestPrimitiveConversionFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.Equal(provider.FloatType(),
                provider.FindCommonType(provider.IntegerType(), provider.FloatType()));
        }

        /// <summary>
        /// Tests the FindCommonType method with callable types
        /// </summary>
        [Fact]
        public void TestSimpleCallableFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);

            var callable1 = new CallableTypeDef(new [] { param1, param2 }, provider.IntegerType(), true);
            var callable2 = new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true);

            Assert.Equal(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true), provider.FindCommonType(callable1, callable2));
        }
        
        /// <summary>
        /// Tests the FindCommonType method with callable types, with one of the values containing a default value with the other not
        /// </summary>
        [Fact]
        public void TestCallableWithDefaultFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);

            var defaulParam2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);

            var callable1 = new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType(), true);
            var callable2 = new CallableTypeDef(new[] { param1, defaulParam2 }, provider.VoidType(), true);

            Assert.Equal(new CallableTypeDef(new[] { param1, defaulParam2 }, provider.VoidType(), true), provider.FindCommonType(callable1, callable2));
        }

        /// <summary>
        /// Tests the FindCommonType method with array of callable types
        /// </summary>
        [Fact]
        public void TestArrayFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);

            var array1 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType(), true));
            var array2 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true));

            Assert.Equal(new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true)), provider.FindCommonType(array1, array2));
        }

        /// <summary>
        /// Tests the FindCommonType method with dictionary types
        /// </summary>
        [Fact]
        public void TestDictionaryFindCommonType()
        {
            var provider = new TypeProvider();

            var dict1 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());
            var dict2 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());

            var expected1 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());

            Assert.Equal(expected1, provider.FindCommonType(dict1, dict2));

            var dict3 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());
            var dict4 = provider.DictionaryForTypes(provider.IntegerType(), provider.IntegerType());

            var expected2 = provider.AnyType();

            Assert.Equal(expected2, provider.FindCommonType(dict3, dict4));
        }

        #endregion

        #region Implicit cast

        /// <summary>
        /// Tests failed callable type cast checking by providing callables with different required argument count
        /// </summary>
        [Fact]
        public void TestFailedDefaultArgumentClosureTypeImplicitCast()
        {
            // Set up the test
            const string input = "(int->int) (i:int, j:int) => {}";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var callableType = resolver.ResolveCallableType(parser.callableType());
            var closureType = resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.False(provider.CanImplicitCast(closureType, callableType), "Trying to cast callable types with less required parameters than the original should not be allowed");
        }

        /// <summary>
        /// Tests successfull callable type cast checking by providing callables with same required argument count
        /// </summary>
        [Fact]
        public void TestDefaultArgumentClosureTypeImplicitCast()
        {
            // Set up the test
            const string input = "(int->int) (i:int, j:int=0):int => {}";

            var parser = TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var callableType = resolver.ResolveCallableType(parser.callableType());
            var closureType = resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.True(provider.CanImplicitCast(closureType, callableType),
                "Trying to cast callable types with more total parameters, but same required parameters than the original should not be allowed");
        }

        #endregion

        #region Casting

        [Fact]
        public void TestPassingPrimitiveCastOperation()
        {
            var typeProvider = new TypeProvider();

            Assert.Equal(10, typeProvider.CastObject(10L, typeof(int)));
            Assert.Equal(10.0, typeProvider.CastObject(10L, typeof(double)));
            Assert.Equal(10.0f, typeProvider.CastObject(10L, typeof(float)));
        }

        [Fact]
        public void TestPassingReferenceCastOperator()
        {
            var typeProvider = new TypeProvider();
            TestBaseClass testBaseClass = new TestDerivedClass();

            Assert.True(typeProvider.CastObject(testBaseClass, typeof(TestDerivedClass)) is TestDerivedClass);
        }

        [Fact]
        public void TestFailingCastOperation()
        {
            var typeProvider = new TypeProvider();

            Assert.Throws<InvalidCastException>(() => typeProvider.CastObject("SneakyString", typeof(TestDerivedClass)));
        }

        #endregion
    }
}