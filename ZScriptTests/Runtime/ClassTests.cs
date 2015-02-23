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
        public void TestBasicClass()
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
        /// Tests a simple class instantiation
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            const string input = "var a:any; func f1() { var f = TestClass(10); a = f.field; } class TestClass { var field:int; func TestClass(i:int) { field = i; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10L, memory.GetVariable("a"));
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

        /// <summary>
        /// Tests usage of the 'this' variable
        /// </summary>
        [TestMethod]
        public void TestThisVariable()
        {
            const string input = "var a:any; func f1() { var inst = TestClass(); a = inst.f(); } class TestClass { var field:int = 10; func f() { return this.field; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10L, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests trying to re-assign the 'this' special constant
        /// </summary>
        [TestMethod]
        public void TestConstantThis()
        {
            const string input = "var a:any; func f1() { var inst = TestClass(); } class TestClass { func TestClass() { this = null; } }";

            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant));
        }

        #region Inheritance tests

        /// <summary>
        /// Tests basic inheritance creation
        /// </summary>
        [TestMethod]
        public void TestInheritance()
        {
            const string input = "class TestBaseClass { } class TestClass : TestBaseClass { var field:int = 10; }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            generator.GenerateRuntime(owner);

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests basic inheritance accessing
        /// </summary>
        [TestMethod]
        public void TestAccessBaseField()
        {
            const string input = "var a:int; func f1() { var inst = TestClass(); a = inst.access(); } class TestBaseClass { var field:int = 10; } class TestClass : TestBaseClass { func access() : int { return field; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(10L, memory.GetVariable("a"));
            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests basic method accessing
        /// </summary>
        [TestMethod]
        public void TestAccessBaseMethod()
        {
            const string input = "var a:int; func f1() { var inst = TestClass(); a = inst.access(); } class TestBaseClass { var field:int = 10; func access() : int { return field; } } class TestClass : TestBaseClass { }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(10L, memory.GetVariable("a"));
            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        #endregion

        #endregion
    }
}