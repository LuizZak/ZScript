using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the class generation routines of the runtime generator
    /// </summary>
    [TestClass]
    public class ClassTests
    {
        #region Parsing

        /// <summary>
        /// Tests a simple class parsing
        /// </summary>
        [TestMethod]
        public void TestParseClass()
        {
            const string input = "class TestClass { var field1:int; func f1() { } }";

            var generator = TestUtils.CreateGenerator(input);
            var definitions = generator.CollectDefinitions();

            // Get the class created
            var testClass = definitions.GetDefinitionByName<ClassDefinition>("TestClass");

            Assert.IsNotNull(testClass);

            Assert.AreEqual(1, testClass.Fields.Count(f => f.Name == "field1"), "Failed to collect class fields");
            Assert.AreEqual(1, testClass.Methods.Count(m => m.Name == "f1"), "Failed to collect class methods");
        }

        /// <summary>
        /// Tests a simple class inheritance detection
        /// </summary>
        [TestMethod]
        public void TestInheritanceDetection()
        {
            const string input = "class BaseTest { } class DerivedTest : BaseTest { }";

            var generator = TestUtils.CreateGenerator(input);
            var definitions = generator.CollectDefinitions();

            // Get the class created
            var baseTest = definitions.GetDefinitionByName<ClassDefinition>("BaseTest");
            var derivedTest = definitions.GetDefinitionByName<ClassDefinition>("DerivedTest");

            Assert.AreEqual(baseTest, derivedTest.BaseClass, "Failed to link inherited classes correctly");
        }

        #region Parsing Errors

        /// <summary>
        /// Tests error raising when creating functions on base and derived classes that are not marked as override
        /// </summary>
        [TestMethod]
        public void TestFunctionCollisionError()
        {
            const string input = "class BaseTest { func f1() { } } class DerivedTest : BaseTest { func f1() { } }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests error raising when trying to extend a class that does not exists
        /// </summary>
        [TestMethod]
        public void TestUndefinedBaseClassError()
        {
            const string input = "class DerivedTest : BaseTest { }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.UndeclaredDefinition), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests error raising when trying to create a circular inheritance chain
        /// </summary>
        [TestMethod]
        public void TestCircularInheritanceChainError()
        {
            const string input = "class BaseTest : DerivedTest { } class DerivedTest : BaseTest { }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.CircularInheritanceChain), "Failed to raise expected errors");
        }

        #endregion

        #endregion

        #region Execution

        /// <summary>
        /// Tests a simple class instantiation
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            const string input = "var a:any; func f1() { a = TestClass(); } class TestClass { }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.IsNotNull(memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests getting the value of a class instance's field
        /// </summary>
        [TestMethod]
        public void TestGetField()
        {
            const string input = "var a:any; func f1() { var inst = TestClass(); a = inst.field; } class TestClass { var field:int = 10; }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10L, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests setting the value of a class instance's field
        /// </summary>
        [TestMethod]
        public void TestSetField()
        {
            const string input = "var a:any; func f1() { var inst = TestClass(); inst.field = 20; a = inst.field; } class TestClass { var field:int = 10; }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(20L, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests invoking methods of class definitions
        /// </summary>
        [TestMethod]
        public void TestMethod()
        {
            const string input = "var a:any; func f1() { var inst = TestClass(); a = inst.calc(); } class TestClass { func calc() : int { return 10; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10L, memory.GetVariable("a"));
        }

        #endregion
    }
}