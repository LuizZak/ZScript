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
            generator.ParseInputString();
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
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual(10, ((ZObject)memory.GetVariable("b"))["x"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual(11, ((ZObject)memory.GetVariable("b"))["abc"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual(12, ((ZObject)memory.GetVariable("b"))["string with spaces"], "The VM failed to correctly create the dynamic object");
        }

        [TestMethod]
        public void TestComplexObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { x:10 + 5, y:5 * (7 + 1) }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.AreEqual(15, ((ZObject)memory.GetVariable("b"))["x"], "The VM failed to correctly create the dynamic object");
            Assert.AreEqual(40, ((ZObject)memory.GetVariable("b"))["y"], "The VM failed to correctly create the dynamic object");
        }
        
        [TestMethod]
        public void TestNestedObjectLiteral()
        {
            const string input = "[ b; ] func funca(){ b = { a:{ x: 10 }, b:{ y: { z: { a:10 } } }  }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            // TODO: Deal with this absurd nesting of ZObject casts
            Assert.IsInstanceOfType(((ZObject)memory.GetVariable("b"))["a"], typeof(ZObject), "The VM failed to correctly create the nested dynamic objects");
            Assert.AreEqual(10, ((ZObject)((ZObject)memory.GetVariable("b"))["a"])["x"], "The VM failed to correctly create the nested dynamic objects");
            Assert.AreEqual(10, ((ZObject)((ZObject)((ZObject)((ZObject)memory.GetVariable("b"))["b"])["y"])["z"])["a"], "The VM failed to correctly create the nested dynamic objects");
        }
    }
}