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

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.Runtime.Typing
{
    /// <summary>
    /// Tests the TypeProvider class and related components
    /// </summary>
    [TestClass]
    public class TypeProviderTests
    {
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

            Assert.AreEqual(provider.ListForType(provider.AnyType()),
                provider.FindCommonType(provider.ListForType(provider.AnyType()), provider.ListForType(provider.IntegerType())),
                "Providing the exact same type to the FindCommonTypes should result in the same type being returned");

            Assert.AreEqual(provider.ListForType(provider.VoidType()),
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

            var param2_def = new CallableTypeDef.CallableParameterInfo(provider.IntegerType(), true, true, false);

            var callable1 = new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType(), true);
            var callable2 = new CallableTypeDef(new[] { param1, param2_def }, provider.VoidType(), true);

            Assert.AreEqual(new CallableTypeDef(new[] { param1, param2_def }, provider.VoidType(), true), provider.FindCommonType(callable1, callable2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }

        /// <summary>
        /// Tests the FindCommonType method with callable types
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
            var resolver = new ExpressionTypeResolver(provider, container);

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
            var resolver = new ExpressionTypeResolver(provider, container);

            // Perform the parsing
            var callableType = resolver.ResolveCallableType(parser.callableType());
            var closureType = resolver.ResolveClosureExpression(parser.closureExpression());

            Assert.IsTrue(provider.CanImplicitCast(closureType, callableType),
                "Trying to cast callable types with more total parameters, but same required parameters than the original should not be allowed");
        }
    }
}