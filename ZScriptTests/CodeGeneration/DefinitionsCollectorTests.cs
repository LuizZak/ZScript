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
        #region Class parsing

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

        #region Sequence parsing

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
    }
}