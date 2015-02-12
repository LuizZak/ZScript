using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Elements.Typing;
using ZScript.Runtime.Typing;

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
        public void TestCallableFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = provider.IntegerType();
            var param2 = provider.IntegerType();

            var callable1 = new CallableTypeDef(new [] { param1, param2 }, provider.IntegerType());
            var callable2 = new CallableTypeDef(new [] { param1, param2 }, provider.VoidType());

            Assert.AreEqual(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType()), provider.FindCommonType(callable1, callable2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }

        /// <summary>
        /// Tests the FindCommonType method with callable types
        /// </summary>
        [TestMethod]
        public void TestArrayFindCommonType()
        {
            var provider = new TypeProvider();

            var param1 = provider.IntegerType();
            var param2 = provider.IntegerType();

            var array1 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.IntegerType()));
            var array2 = new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType()));

            Assert.AreEqual(new ListTypeDef(new CallableTypeDef(new[] { param1, param2 }, provider.VoidType())), provider.FindCommonType(array1, array2),
                "Trying to find a common type between two callables of same parameters but with a void type should result in a callable with a void return type");
        }
    }
}