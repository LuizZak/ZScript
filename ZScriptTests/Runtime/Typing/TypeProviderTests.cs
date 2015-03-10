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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;

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
    [TestClass]
    public class TypeProviderTests
    {
        #region Custom type support

        /// <summary>
        /// Tests storaging and fetching of custom types
        /// </summary>
        [TestMethod]
        public void TestCustomType()
        {
            var typeProvider = new TypeProvider();
            var expectedType = TypeDef.StringType;

            var typeSource = MockRepository.Mock<ICustomTypeSource>();

            typeSource.Stub(x => x.HasType("CustomType")).Return(true);
            typeSource.Stub(x => x.TypeNamed("CustomType")).Return(expectedType);

            typeProvider.RegisterCustomTypeSource(typeSource);

            var type = typeProvider.TypeNamed("CustomType");

            Assert.AreEqual(expectedType, type);
        }

        #endregion

        #region NativeTypeForTypeDf

        [TestMethod]
        public void TestNativeTypeInteger()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.IntegerType());

            Assert.AreEqual(typeof(long), native);
        }

        [TestMethod]
        public void TestNativeTypeBoolean()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.BooleanType());

            Assert.AreEqual(typeof(bool), native);
        }

        [TestMethod]
        public void TestNativeTypeString()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.StringType());

            Assert.AreEqual(typeof(string), native);
        }

        [TestMethod]
        public void TestNativeTypeFloat()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.FloatType());

            Assert.AreEqual(typeof(double), native);
        }

        [TestMethod]
        public void TestNativeTypeVoid()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.VoidType());

            Assert.AreEqual(typeof(void), native);
        }

        [TestMethod]
        public void TestNativeTypeNull()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.NullType());

            Assert.AreEqual(typeof(object), native);
        }

        [TestMethod]
        public void TestNativeTypeList()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ListForType(provider.IntegerType()));

            Assert.AreEqual(typeof(List<long>), native);
        }

        [TestMethod]
        public void TestNativeTypeAnyList()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ListForType(provider.AnyType()));

            Assert.AreEqual(typeof(List<object>), native);
        }

        [TestMethod]
        public void TestNativeTypeDictionary()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.DictionaryForTypes(provider.StringType(), provider.FloatType()));

            Assert.AreEqual(typeof(Dictionary<string, double>), native);
        }

        [TestMethod]
        public void TestNativeTypeObject()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(provider.ObjectType());

            Assert.AreEqual(typeof(ZObject), native);
        }

        [TestMethod]
        public void TestNativeTypeAny()
        {
            var provider = new TypeProvider();

            var native1 = provider.NativeTypeForTypeDef(provider.AnyType());
            var native2 = provider.NativeTypeForTypeDef(provider.AnyType(), true);

            Assert.AreEqual(null, native1);
            Assert.AreEqual(typeof(object), native2);
        }

        [TestMethod]
        public void TestNativeTypeClass()
        {
            var provider = new TypeProvider();

            var native = provider.NativeTypeForTypeDef(new ClassTypeDef("class"));

            Assert.AreEqual(native, native);
        }

        #endregion

        #region FindCommonType

        /// <summary>
        /// Tests the FindCommonType method with the same types
        /// </summary>
        [TestMethod]
        public void TestSameTypesFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.AreEqual(provider.IntegerType(),
                provider.FindCommonType(provider.IntegerType(), provider.IntegerType()),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");

            Assert.AreEqual(provider.ListForType(provider.IntegerType()),
                provider.FindCommonType(provider.ListForType(provider.IntegerType()), provider.ListForType(provider.IntegerType())),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");
        }

        /// <summary>
        /// Tests the FindCommonType method with void and any types
        /// </summary>
        [TestMethod]
        public void TestVoidAnyPropagationFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.AreEqual(provider.VoidType(), provider.FindCommonType(provider.VoidType(), provider.AnyType()), "Void always has preceedence over any in common type finding");

            Assert.AreEqual(provider.AnyType(),
                provider.FindCommonType(provider.AnyType(), provider.IntegerType()),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");
            
            Assert.AreEqual(provider.VoidType(),
                provider.FindCommonType(provider.VoidType(), provider.IntegerType()),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");

            Assert.AreEqual(provider.AnyType(),
                provider.FindCommonType(provider.ListForType(provider.AnyType()), provider.ListForType(provider.IntegerType())),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");

            Assert.AreEqual(provider.AnyType(),
                provider.FindCommonType(provider.ListForType(provider.AnyType()), provider.ListForType(provider.VoidType())),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");
        }

        /// <summary>
        /// Tests the FindCommonType method with convertible primitive types
        /// </summary>
        [TestMethod]
        public void TestPrimitiveConversionFindCommonType()
        {
            var provider = new TypeProvider();

            Assert.AreEqual(provider.FloatType(),
                provider.FindCommonType(provider.IntegerType(), provider.FloatType()),
                "Integers should be converted to floats when trying to find a common time between integers and floats mixedly");
        }

        /// <summary>
        /// Tests the FindCommonType method with callable types
        /// </summary>
        [TestMethod]
        public void TestSimpleCallableFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);

            var callable1 = new CallableTypeDef(new [] { param1, param2 }, provider.IntegerType(), true);
            var callable2 = new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true);

            Assert.AreEqual(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true), provider.FindCommonType(callable1, callable2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }
        
        /// <summary>
        /// Tests the FindCommonType method with callable types, with one of the values containing a default value with the other not
        /// </summary>
        [TestMethod]
        public void TestCallableWithDefaultFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, false, false);

            var defaulParam2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);

            var callable1 = new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType(), true);
            var callable2 = new CallableTypeDef(new[] { param1, defaulParam2 }, provider.VoidType(), true);

            Assert.AreEqual(new CallableTypeDef(new[] { param1, defaulParam2 }, provider.VoidType(), true), provider.FindCommonType(callable1, callable2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }

        /// <summary>
        /// Tests the FindCommonType method with array of callable types
        /// </summary>
        [TestMethod]
        public void TestArrayFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);
            var param2 = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);

            var array1 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType(), true));
            var array2 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true));

            Assert.AreEqual(new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType(), true)), provider.FindCommonType(array1, array2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }

        /// <summary>
        /// Tests the FindCommonType method with dictionary types
        /// </summary>
        [TestMethod]
        public void TestDictionaryFindCommonType()
        {
            var provider = new TypeProvider();

            var dict1 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());
            var dict2 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());

            var expected1 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());

            Assert.AreEqual(expected1, provider.FindCommonType(dict1, dict2));

            var dict3 = provider.DictionaryForTypes(provider.StringType(), provider.IntegerType());
            var dict4 = provider.DictionaryForTypes(provider.IntegerType(), provider.IntegerType());

            var expected2 = provider.AnyType();

            Assert.AreEqual(expected2, provider.FindCommonType(dict3, dict4));
        }

        #endregion

        #region Implicit cast

        /// <summary>
        /// Tests failed callable type cast checking by providing callables with different required argument count
        /// </summary>
        [TestMethod]
        public void TestFailedDefaultArgumentClosureTypeImplicitCast()
        {
            // Set up the test
            const string input = "(int->int) (i:int, j:int) => {}";

            var parser = Utils.TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var callableType = resolver.ResolveCallableType(parser.callableType());
            var closureType = resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.IsFalse(provider.CanImplicitCast(closureType, callableType), "Trying to cast callable types with less required parameters than the original should not be allowed");
        }

        /// <summary>
        /// Tests successfull callable type cast checking by providing callables with same required argument count
        /// </summary>
        [TestMethod]
        public void TestDefaultArgumentClosureTypeImplicitCast()
        {
            // Set up the test
            const string input = "(int->int) (i:int, j:int=0) => {}";

            var parser = Utils.TestUtils.CreateParser(input);
            var provider = new TypeProvider();
            var container = new MessageContainer();
            var resolver = new ExpressionTypeResolver(new RuntimeGenerationContext(null, container, provider));

            // Perform the parsing
            var callableType = resolver.ResolveCallableType(parser.callableType());
            var closureType = resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.IsTrue(provider.CanImplicitCast(closureType, callableType),
                "Trying to cast callable types with more total parameters, but same required parameters than the original should not be allowed");
        }

        #endregion

        #region Casting

        [TestMethod]
        public void TestPassingPrimitiveCastOperation()
        {
            var typeProvider = new TypeProvider();

            Assert.AreEqual(10, typeProvider.CastObject(10L, typeof(int)), "Type provider failed to cast to expected type");
            Assert.AreEqual(10.0, typeProvider.CastObject(10L, typeof(double)), "Type provider failed to cast to expected type");
            Assert.AreEqual(10.0f, typeProvider.CastObject(10L, typeof(float)), "Type provider failed to cast to expected type");
        }

        [TestMethod]
        public void TestPassingReferenceCastOperator()
        {
            var typeProvider = new TypeProvider();
            TestBaseClass testBaseClass = new TestDerivedClass();

            Assert.IsInstanceOfType(typeProvider.CastObject(testBaseClass, typeof(TestDerivedClass)), typeof(TestDerivedClass), "Type provider failed to cast to expected type");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Trying to cast an object from one type to an invalid type must raise an InvalidCastException at runtime")]
        public void TestFailingCastOperation()
        {
            var typeProvider = new TypeProvider();

            typeProvider.CastObject("SneakyString", typeof(TestDerivedClass));
        }

        #endregion
    }
}