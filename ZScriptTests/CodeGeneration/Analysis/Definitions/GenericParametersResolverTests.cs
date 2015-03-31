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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Definitions;

namespace ZScriptTests.CodeGeneration.Analysis.Definitions
{
    /// <summary>
    /// Tests the functionality of the GenericParametersResolver class and related components
    /// </summary>
    [TestClass]
    public class GenericParametersResolverTests
    {
        /// <summary>
        /// Tests verification of an empty signature
        /// </summary>
        [TestMethod]
        public void TestEmptySignature()
        {
            var signature = new GenericSignatureInformation();
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Assert.IsTrue(result.IsValid);
        }

        /// <summary>
        /// Tests a simple signature with a single generic type
        /// </summary>
        [TestMethod]
        public void TestSingleType()
        {
            var signature = new GenericSignatureInformation(new []{ new GenericTypeDefinition("T") });
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Assert.IsTrue(result.IsValid);
        }

        /// <summary>
        /// Tests a simple signature with two generic types
        /// </summary>
        [TestMethod]
        public void TestDualType()
        {
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") });
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Assert.IsTrue(result.IsValid);
        }

        /// <summary>
        /// Tests resolving of simple constraining of two types via inheritance
        /// </summary>
        [TestMethod]
        public void TestSimpleConstraint()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U") };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") }, constraints);
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Console.WriteLine(result.Message);

            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(signature.GenericTypes[0].BaseType == signature.GenericTypes[1]);
        }

        /// <summary>
        /// Tests detecting unexisting type names in constraints
        /// </summary>
        [TestMethod]
        public void TestInvalidTypeNameInConstriant()
        {
            var constraints = new[] { new GenericTypeConstraint("N", "U") };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T") }, constraints);
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Assert.IsFalse(result.IsValid);
        }

        /// <summary>
        /// Tests detecting cyclical generic type constraint errors
        /// </summary>
        [TestMethod]
        public void TestCyclicalConstraintError()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U"), new GenericTypeConstraint("U", "T") };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") }, constraints);
            var resolver = new GenericSignatureResolver();

            var result = resolver.Resolve(signature);

            Assert.IsFalse(result.IsValid);
        }
    }
}