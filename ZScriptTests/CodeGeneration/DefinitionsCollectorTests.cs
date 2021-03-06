﻿#region License information
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
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration
{
    /// <summary>
    /// Tests the functionality of the DefinitionsCollector class and related components
    /// </summary>
    [TestClass]
    public class DefinitionsCollectorTests
    {
        #region Class collection

        /// <summary>
        /// Tests collection of a class definition
        /// </summary>
        [TestMethod]
        public void TestCollectClass()
        {
            const string input = "class Test1 { }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<ClassDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.IsNotNull(parsedClass);
            Assert.IsNotNull(parsedClass.ClassContext);
        }

        /// <summary>
        /// Tests collection of methods on a class definition
        /// </summary>
        [TestMethod]
        public void TestCollectClassMethod()
        {
            const string input = "class Test1 { func f1() { } }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<ClassDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(1, parsedClass.Methods.Count(f => f.Name == "f1"));
        }

        /// <summary>
        /// Tests collection of fields on a class definition
        /// </summary>
        [TestMethod]
        public void TestCollectClassField()
        {
            const string input = "class Test1 { var field1:int = 0; }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<ClassDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(1, parsedClass.Fields.Count(f => f.Name == "field1"));
        }

        /// <summary>
        /// Tests creation of a default class constructor, when none is provided
        /// </summary>
        [TestMethod]
        public void TestDefaultClassConstructor()
        {
            const string input = "class Test1 { }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<ClassDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.IsNotNull(parsedClass.PublicConstructor);
        }

        /// <summary>
        /// Tests creation of a custom class constructor
        /// </summary>
        [TestMethod]
        public void TestCustomConstructor()
        {
            const string input = "class Test1 { func Test1(i:int) { } }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<ClassDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(1, parsedClass.PublicConstructor.Parameters.Length, "Failed to read custom constructor");
        }

        /// <summary>
        /// Tests creation of a custom class constructor
        /// </summary>
        [TestMethod]
        public void TestDuplicatedConstructorError()
        {
            const string input = "class Test1 { func Test1(i:int) { } func Test1(f:float) { } }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition), "Failed to raise expected errors");
        }

        #endregion

        #region Sequence collection

        /// <summary>
        /// Tests collection of a sequence definition
        /// </summary>
        [TestMethod]
        public void TestCollectSequence()
        {
            const string input = "sequence Test1 [ ]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedClass = scope.GetDefinitionByName<SequenceDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.IsNotNull(parsedClass);
            Assert.IsNotNull(parsedClass.SequenceContext);
        }

        /// <summary>
        /// Tests collection of fields on a sequence definition
        /// </summary>
        [TestMethod]
        public void TestCollectSequenceField()
        {
            const string input = "sequence Test1 [ var field1:int = 0; ]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var sequence = scope.GetDefinitionByName<SequenceDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(1, sequence.Fields.Count(f => f.Name == "field1"));
        }

        /// <summary>
        /// Tests collection of sequence frames
        /// </summary>
        [TestMethod]
        public void TestCollectSequenceFrame()
        {
            const string input = "sequence Test1 [ 1 { } 2-3 { } ]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var sequence = scope.GetDefinitionByName<SequenceDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(2, sequence.FrameDefinitions.Length);

            var frame1 = sequence.FrameDefinitions[0];
            var frame2 = sequence.FrameDefinitions[1];

            Assert.AreEqual(new SequenceFrameRange(false, 1),    frame1.FrameRanges[0], "The frame ranges where not read successfully");
            Assert.AreEqual(new SequenceFrameRange(false, 2, 3), frame2.FrameRanges[0], "The frame ranges where not read successfully");
        }

        /// <summary>
        /// Tests collection of labeled sequence frames
        /// </summary>
        [TestMethod]
        public void TestCollectLabeledSequenceFrame()
        {
            const string input = "sequence Test1 [ start: 0 { } ]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var sequence = scope.GetDefinitionByName<SequenceDefinition>("Test1");

            Assert.IsFalse(container.HasErrors);

            Assert.AreEqual("start", sequence.FrameDefinitions[0].Name, "The frame label was not read successfully");
        }

        /// <summary>
        /// Tests error raising when defining duplicated frame labels
        /// </summary>
        [TestMethod]
        public void TestDuplicatedFrameLabelError()
        {
            const string input = "sequence Test1 [ start: 0 { } start: 1 { } ]";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition));
        }

        #endregion

        #region Top-level function collection

        /// <summary>
        /// Tests collection of a top-level function definition
        /// </summary>
        [TestMethod]
        public void TestCollectTopLevelFunction()
        {
            const string input = "func f1() { }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var parsedFunction = scope.GetDefinitionByName<TopLevelFunctionDefinition>("f1");

            Assert.IsFalse(container.HasErrors);
            Assert.IsNotNull(parsedFunction);
            Assert.IsNotNull(parsedFunction.BodyContext);
        }

        /// <summary>
        /// Tests distinction of top-level functions from other types of functions (exports and closures)
        /// </summary>
        [TestMethod]
        public void TestTopLevelFunctionDistinction()
        {
            const string input = "@f1 func f2() { var a = () => { }; }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var exportFunction = scope.GetDefinitionByName<TopLevelFunctionDefinition>("f1");
            var normalFunction = scope.GetDefinitionByName<TopLevelFunctionDefinition>("f2");
            var closureFunction = scope.GetDefinitionByName<TopLevelFunctionDefinition>("$__closure0");

            Assert.IsFalse(container.HasErrors);
            Assert.IsNull(exportFunction);
            Assert.IsNotNull(normalFunction);
            Assert.IsNull(closureFunction);
        }

        #endregion

        #region Local variable collection

        /// <summary>
        /// Tests collection of local variables
        /// </summary>
        [TestMethod]
        public void TestLocalVariableCollection()
        {
            const string input = "var a; func f1() { var b; } class Class { var c; }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var locals = scope.GetDefinitionsByTypeRecursive<LocalVariableDefinition>().ToList();

            Assert.IsFalse(container.HasErrors);
            Assert.AreEqual(1, locals.Count);
            Assert.AreEqual("b", locals[0].Name);
        }

        #endregion
    }
}