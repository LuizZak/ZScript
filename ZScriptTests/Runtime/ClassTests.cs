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

        /// <summary>
        /// Tests parsing and collecting a base constructor call
        /// </summary>
        [TestMethod]
        public void TestBaseConstructor()
        {
            const string input = "class Base { } class Derived : Base { func Derived() { base(); } }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            // Get the class created
            Assert.IsFalse(collector.HasErrors);
        }

        /// <summary>
        /// Tests having a field that has a type matching the parent class
        /// </summary>
        [TestMethod]
        public void TestSelfReference()
        {
            const string input = "class TestClass { var a:TestClass; }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            // Get the class created
            Assert.IsFalse(collector.HasErrors);
        }

        #region Parsing Errors

        /// <summary>
        /// Tests error raising when trying to call base from inside a closure that is contained within an overriden method
        /// </summary>
        [TestMethod]
        public void TestBaseInClosure()
        {
            const string input = "class Base { func f() { } } class Derived : Base { override func f() { () => { base(); }; } }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            // Get the class created
            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoBaseTarget));
        }

        /// <summary>
        /// Tests error raising when parsing a constructor that contains a return type
        /// </summary>
        [TestMethod]
        public void TestReturnTypedConstructor()
        {
            const string input = "class TestClass { func TestClass() : int { } }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            // Get the class created
            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ReturnTypeOnConstructor));
            Assert.AreEqual(0, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteReturnPaths),
                "No return type errors can be raised on a constructor because it cannot return a value");
        }

        /// <summary>
        /// Tests error raising when parsing a base constructor call that contains a mismatched argument count
        /// </summary>
        [TestMethod]
        public void TestMissmatchedBaseCall()
        {
            const string input = "class Base { func Base(i:int) { } } class Derived : Base { func Derived() { base(); } }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            // Get the class created
            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.TooFewArguments));
        }

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
        /// Tests error raising when creating variables on base and derived classes
        /// </summary>
        [TestMethod]
        public void TestFieldCollisionError()
        {
            const string input = "class BaseTest { var f1; } class DerivedTest : BaseTest { var f1; }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests error raising when creating same variables on the same class
        /// </summary>
        [TestMethod]
        public void TestDuplicatedFieldError()
        {
            const string input = "class DerivedTest { var f1; var f1; }";

            var generator = TestUtils.CreateGenerator(input);
            var collector = generator.MessageContainer;
            generator.CollectDefinitions();

            collector.PrintMessages();

            Assert.AreEqual(1, collector.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests error raising when creating same methods on the same class
        /// </summary>
        [TestMethod]
        public void TestDuplicatedMethodError()
        {
            const string input = "class DerivedTest { func f1() { } func f1() { } }";

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

        /// <summary>
        /// Tests trying to override methods with no matching base method to override
        /// </summary>
        [TestMethod]
        public void TestOverrideNoTarget()
        {
            const string input = "class TestBaseClass { func access1() { } }" +
                                 "class TestClass : TestBaseClass { override func access2() { } }";

            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoOverrideTarget));
        }

        /// <summary>
        /// Tests overriding a method with a different signature
        /// </summary>
        [TestMethod]
        public void TestMismatchedOverride()
        {
            const string input = "class TestBaseClass { func f1() : int { return 0; } }" +
                                 "class TestClass : TestBaseClass { override func f1() : void { } }";

            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.MismatchedOverrideSignatures));
        }

        /// <summary>
        /// Tests trying to call 'base' on a method or function that has no base
        /// </summary>
        [TestMethod]
        public void TestMissingBaseMethod()
        {
            const string input = "class TestClass { func f2() { base(); } }";

            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.NoBaseTarget));
        }

        #endregion

        #endregion

        #region Type resolving

        /// <summary>
        /// Tests raising of errors related to missing types on fields
        /// </summary>
        [TestMethod]
        public void TestMissingTypeOnFieldError()
        {
            const string input = "class Test1 { var field3; var field1:int; var field2 = 0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.MissingFieldType), "Failed to raise expected errors");
        }

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
            const string input = "var a:any; func f1() { var inst = TestClass(); a = inst.f(); } class TestClass { var field:int = 10; func f() : int { return this.field; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10L, memory.GetVariable("a"));
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

        /// <summary>
        /// Tests override of methods
        /// </summary>
        [TestMethod]
        public void TestOverridenMethod()
        {
            const string input = "var a:int; func f1() { var inst = TestClass(); inst.access(); }" +
                                 "class TestBaseClass { func access() { a = 10; } }" +
                                 "class TestClass : TestBaseClass { override func access() { a = 20; } }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(20L, memory.GetVariable("a"));
            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests calling 'base' on an overriden method
        /// </summary>
        [TestMethod]
        public void TestCallBaseMethod()
        {
            const string input = "var a:int; func f1() { var inst = TestClass(); inst.access(); }" +
                                 "class TestBaseClass { func access() { a = 10; } }" +
                                 "class TestClass : TestBaseClass { override func access() { base(); } }";

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
        /// Tests the 'is' operator on generated class types
        /// </summary>
        [TestMethod]
        public void TestIsOperator()
        {
            const string input = "@__trace(v...)" +
                                 "func f1()" +
                                 "{" +
                                 "  __trace(TestBaseClass() is TestBaseClass);" +
                                 "  __trace(TestClass() is TestClass);" +
                                 "  __trace(TestClass() is TestBaseClass);" +
                                 "  __trace(TestBaseClass() is TestClass);" +
                                 "}" +
                                 "class TestBaseClass { }" +
                                 "class TestClass : TestBaseClass { }";

            var owner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f1");

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);

            Assert.AreEqual(true, owner.TraceObjects[0], "Failed TestBaseClass() is TestBaseClass");
            Assert.AreEqual(true, owner.TraceObjects[1], "Failed TestClass() is TestClass");
            Assert.AreEqual(true, owner.TraceObjects[2], "Failed TestClass() is TestBaseClass");
            Assert.AreEqual(false, owner.TraceObjects[3], "Failed TestBaseClass() is TestClass");
        }

        #endregion

        #endregion
    }
}