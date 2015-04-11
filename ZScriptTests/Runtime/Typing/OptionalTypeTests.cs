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
using ZScript.Runtime.Typing;

namespace ZScriptTests.Runtime.Typing
{
    /// <summary>
    /// Tests optional types in the type system
    /// </summary>
    [TestClass]
    public class OptionalTypeTests
    {
        /// <summary>
        /// Tests optional type creating
        /// </summary>
        [TestMethod]
        public void TestOptionalCreation()
        {
            var provider = new TypeProvider();

            var optionalInt = provider.OptionalTypeForType(provider.IntegerType());
            var optionalAny = provider.OptionalTypeForType(provider.AnyType());
            var optOptFloat = provider.OptionalTypeForType(provider.OptionalTypeForType(provider.FloatType()));

            Assert.AreEqual(provider.IntegerType(), optionalInt.WrappedType);
            Assert.AreEqual(provider.AnyType(), optionalAny.WrappedType);
            Assert.AreEqual(provider.OptionalTypeForType(provider.FloatType()), optOptFloat.WrappedType);

            Assert.AreEqual(provider.IntegerType(), optionalInt.BaseWrappedType);
            Assert.AreEqual(provider.AnyType(), optionalAny.BaseWrappedType);
            Assert.AreEqual(provider.FloatType(), optOptFloat.BaseWrappedType);

            Assert.AreEqual(0, optionalInt.OptionalDepth);
            Assert.AreEqual(0, optionalAny.OptionalDepth);
            Assert.AreEqual(1, optOptFloat.OptionalDepth);
        }

        /// <summary>
        /// Tests optional type castability
        /// </summary>
        [TestMethod]
        public void TestOptionalCastable()
        {
            var provider = new TypeProvider();

            var optInt = provider.OptionalTypeForType(provider.IntegerType());

            Assert.IsFalse(provider.CanImplicitCast(optInt, provider.IntegerType()));
            Assert.IsFalse(provider.CanExplicitCast(optInt, provider.IntegerType()));

            Assert.IsTrue(provider.CanImplicitCast(optInt, optInt));
            Assert.IsTrue(provider.CanExplicitCast(optInt, optInt));
        }

        /// <summary>
        /// Tests optional type compatibility by testing whether non-optional values are compatible with optional types
        /// </summary>
        [TestMethod]
        public void TestOptionalCompatibility()
        {
            var provider = new TypeProvider();

            var optInt = provider.OptionalTypeForType(provider.IntegerType());
            var optOptInt = provider.OptionalTypeForType(optInt);

            Assert.IsTrue(provider.CanImplicitCast(provider.IntegerType(), optInt));
            Assert.IsTrue(provider.CanExplicitCast(provider.IntegerType(), optInt));

            Assert.IsTrue(provider.CanExplicitCast(provider.IntegerType(), optOptInt));
        }

        /// <summary>
        /// Tests FindCommonType with different configutarion of optional and non-optional types
        /// </summary>
        [TestMethod]
        public void TestOptionalCommonType()
        {
            var provider = new TypeProvider();

            var optInt = provider.OptionalTypeForType(provider.IntegerType());
            var optOptInt = provider.OptionalTypeForType(optInt);

            var optFloat = provider.OptionalTypeForType(provider.FloatType());
            var optOptFloat = provider.OptionalTypeForType(optFloat);

            var optAny = provider.OptionalTypeForType(provider.AnyType());

            // Null values
            Assert.AreEqual(optInt, provider.FindCommonType(optInt, provider.NullType()));
            Assert.AreEqual(optInt, provider.FindCommonType(provider.NullType(), optInt));

            Assert.AreEqual(optInt, provider.FindCommonType(optInt, optInt));
            
            Assert.AreEqual(optInt, provider.FindCommonType(optInt, provider.IntegerType()));
            Assert.AreEqual(optInt, provider.FindCommonType(provider.IntegerType(), optInt));

            Assert.AreEqual(optOptInt, provider.FindCommonType(optOptInt, optInt));
            Assert.AreEqual(optOptInt, provider.FindCommonType(optInt, optOptInt));

            // Unequality
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optInt, optFloat));
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optFloat, optInt));

            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optOptFloat, optInt));
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optInt, optOptFloat));

            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optOptFloat, optOptInt));
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optOptInt, optOptFloat));
        }

        /// <summary>
        /// Tests FindCommonType with optional and compatible non-optional types
        /// </summary>
        [TestMethod]
        public void TestOptionalInternalCommonType()
        {
            var provider = new TypeProvider();

            var optInt = provider.OptionalTypeForType(provider.IntegerType());
            var optFloat = provider.OptionalTypeForType(provider.FloatType());

            // Null values
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optInt, optFloat));
            Assert.AreEqual(provider.AnyType(), provider.FindCommonType(optFloat, optInt));
        }

        /// <summary>
        /// Tests FindCommonType with different configutarion of null and non-optional types
        /// </summary>
        [TestMethod]
        public void TestNullOptionalCommonType()
        {
            var provider = new TypeProvider();

            var intType = provider.IntegerType();

            // Null values
            Assert.AreEqual(provider.OptionalTypeForType(intType), provider.FindCommonType(intType, provider.NullType()));
            Assert.AreEqual(provider.OptionalTypeForType(intType), provider.FindCommonType(provider.NullType(), intType));
        }

        /// <summary>
        /// Tests optional compatibility with null-typed values
        /// </summary>
        [TestMethod]
        public void TestOptionalNullCompatibility()
        {
            var provider = new TypeProvider();

            var optInt = provider.OptionalTypeForType(provider.IntegerType());
            var nullType = provider.NullType();

            Assert.IsTrue(provider.CanImplicitCast(nullType, optInt));
            Assert.IsTrue(provider.CanExplicitCast(nullType, optInt));
        }
    }
}