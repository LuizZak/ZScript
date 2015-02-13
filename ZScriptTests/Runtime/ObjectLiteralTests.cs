using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of object literal operations
    /// </summary>
    [TestClass]
    public class ObjectLiteralTests
    {
        [TestMethod]
        public void TestEmptyObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.IsInstanceOfType(memory.GetVariable("b"), typeof(ZObject), "The VM failed to correctly create the dynamic object");
        }

        [TestMethod]
        public void TestSimpleObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { x:10, 'abc':11, \"string with spaces\":12 }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)10, ((ZObject)memory.GetVariable("b"))["x"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual((long)11, ((ZObject)memory.GetVariable("b"))["abc"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual((long)12, ((ZObject)memory.GetVariable("b"))["string with spaces"], "The VM failed to correctly create the dynamic object");
        }

        [TestMethod]
        public void TestComplexObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { x:10 + 5, y:5 * (7 + 1) }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)15, ((ZObject)memory.GetVariable("b"))["x"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual((long)40, ((ZObject)memory.GetVariable("b"))["y"], "The VM failed to correctly create the dynamic object");
        }
        
        [TestMethod]
        public void TestNestedObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { a:{ x: 10 }, b:{ y: { z: { a:10 } } }  }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            // TODO: Deal with this absurd nesting of ZObject casts
            Assert.IsInstanceOfType(((ZObject)memory.GetVariable("b"))["a"], typeof(ZObject), "The VM failed to correctly create the nested dynamic objects");
            Assert.AreEqual((long)10, ((ZObject)((ZObject)memory.GetVariable("b"))["a"])["x"], "The VM failed to correctly create the nested dynamic objects");
            Assert.AreEqual((long)10, ((ZObject)((ZObject)((ZObject)((ZObject)memory.GetVariable("b"))["b"])["y"])["z"])["a"], "The VM failed to correctly create the nested dynamic objects");
        }

        [TestMethod]
        public void TestObjectSubscript()
        {
            const string input = "[ b; ] func funca(){ var a = { x:10 }; b = a['x']; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)10, memory.GetVariable("b"), "The VM failed to perform the correct subscript operation on the ZObject");
        }

        [TestMethod]
        public void TestNestedObjectSubscript()
        {
            const string input = "[ b; ] func funca(){ var a = { x: { x:10 } }; b = a['x']['x']; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)10, memory.GetVariable("b"), "The VM failed to perform the correct subscript operation on the ZObject");
        }
        
        [TestMethod]
        public void TestObjectMemberAccess()
        {
            const string input = "[ b; ] func funca(){ var a = { x:10 }; b = a.x; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)10, memory.GetVariable("b"), "The VM failed to perform the correct member fetch operation on the ZObject");
        }
        
        [TestMethod]
        public void TestNestedObjectMemberAccess()
        {
            const string input = "[ b; ] func funca(){ var a = { x: { x:10 } }; b = a.x.x; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual((long)10, memory.GetVariable("b"), "The VM failed to perform the correct member fetch operation on the ZObject");
        }
    }
}