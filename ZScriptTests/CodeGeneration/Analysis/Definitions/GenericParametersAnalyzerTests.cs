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
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;

namespace ZScriptTests.CodeGeneration.Analysis.Definitions
{
    /// <summary>
    /// Tests the functionality of the GenericParametersAnalyzer class and related components
    /// </summary>
    [TestClass]
    public class GenericParametersAnalyzerTests
    {
        /// <summary>
        /// Tests verification of an empty signature
        /// </summary>
        [TestMethod]
        public void TestEmptySignature()
        {
            var signature = new GenericSignatureInformation();
            var container = new MessageContainer();
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests a simple signature with a single generic type
        /// </summary>
        [TestMethod]
        public void TestSingleType()
        {
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T") });
            var container = new MessageContainer();
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests a simple signature with two generic types
        /// </summary>
        [TestMethod]
        public void TestDualType()
        {
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") });
            var container = new MessageContainer();
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsFalse(container.HasErrors);
        }

        /// <summary>
        /// Tests resolving of simple constraining of two types via inheritance
        /// </summary>
        [TestMethod]
        public void TestSimpleConstraint()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U", null) };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") }, constraints);
            var container = new MessageContainer();
            var typeProvider = new TypeProvider();
            var genericTypeSource = new GenericTypeSource();
            genericTypeSource.PushGenericContext(signature);
            typeProvider.RegisterCustomTypeSource(genericTypeSource);
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container, typeProvider: typeProvider));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsFalse(container.HasErrors);
            Assert.IsTrue(signature.GenericTypes[0].BaseType == signature.GenericTypes[1]);
        }

        /// <summary>
        /// Tests detecting unexisting type names in constraints
        /// </summary>
        [TestMethod]
        public void TestInvalidTypeNameInConstriant()
        {
            var constraints = new[] { new GenericTypeConstraint("N", "U", null) };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T") }, constraints);
            var container = new MessageContainer();
            var typeProvider = new TypeProvider();
            var genericTypeSource = new GenericTypeSource();
            genericTypeSource.PushGenericContext(signature);
            typeProvider.RegisterCustomTypeSource(genericTypeSource);
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container, typeProvider: typeProvider));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsTrue(container.HasErrors);
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UnkownGenericTypeName));
        }

        /// <summary>
        /// Tests detecting unexisting type names in base types in constraints
        /// </summary>
        [TestMethod]
        public void TestUnknownBaseTypeConstraint()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U", null) };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T") }, constraints);
            var container = new MessageContainer();
            var typeProvider = new TypeProvider();
            var genericTypeSource = new GenericTypeSource();
            genericTypeSource.PushGenericContext(signature);
            typeProvider.RegisterCustomTypeSource(genericTypeSource);
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container, typeProvider: typeProvider));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsTrue(container.HasErrors);
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UnkownType));
        }

        /// <summary>
        /// Tests detecting cyclical generic type constraint errors
        /// </summary>
        [TestMethod]
        public void TestCyclicalConstraintError()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U", null), new GenericTypeConstraint("U", "T", null) };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") }, constraints);
            var container = new MessageContainer();
            var typeProvider = new TypeProvider();
            var genericTypeSource = new GenericTypeSource();
            genericTypeSource.PushGenericContext(signature);
            typeProvider.RegisterCustomTypeSource(genericTypeSource);
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container, typeProvider: typeProvider));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsTrue(container.HasErrors);
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.CyclicalGenericConstraint));
        }

        /// <summary>
        /// Tests detecting duplicated generic type constraint errors
        /// </summary>
        [TestMethod]
        public void TestDuplicatedGenericConstraint()
        {
            var constraints = new[] { new GenericTypeConstraint("T", "U", null), new GenericTypeConstraint("T", "V", null) };
            var signature = new GenericSignatureInformation(new[] { new GenericTypeDefinition("T"), new GenericTypeDefinition("U") }, constraints);
            var container = new MessageContainer();
            var typeProvider = new TypeProvider();
            var genericTypeSource = new GenericTypeSource();
            genericTypeSource.PushGenericContext(signature);
            typeProvider.RegisterCustomTypeSource(genericTypeSource);
            var analyzer = new GenericSignatureAnalyzer(new RuntimeGenerationContext(messageContainer: container, typeProvider: typeProvider));

            analyzer.AnalyzeSignature(signature);

            container.PrintMessages();

            Assert.IsTrue(container.HasErrors);
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DupliatedGenericConstraint));
        }
    }
}