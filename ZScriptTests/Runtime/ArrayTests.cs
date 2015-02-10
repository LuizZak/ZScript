using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of array code
    /// </summary>
    [TestClass]
    public class ArrayTests
    {
        /// <summary>
        /// Tests parsing and execution of array literal creation
        /// </summary>
        [TestMethod]
        public void TestArrayLiteral()
        {
            const string input = "[ a = 0; ] func funca(){ a = [0, 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(0, list[0], "The list did not have the expected values");
            Assert.AreEqual(1, list[1], "The list did not have the expected values");
            Assert.AreEqual(2, list[2], "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation
        /// </summary>
        [TestMethod]
        public void TestNestedArrayLiteral()
        {
            const string input = "[ a = 0; ] func funca(){ a = [[0], 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.IsInstanceOfType(memory.GetVariable("a"), typeof(IList), "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.IsInstanceOfType(list[0], typeof(IList), "The list created by the script is not of the expected type");
            Assert.AreEqual(0, ((IList)list[0])[0], "The list did not have the expected values");
            Assert.AreEqual(1, list[1], "The list did not have the expected values");
            Assert.AreEqual(2, list[2], "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of array literal creation and access
        /// </summary>
        [TestMethod]
        public void TestArrayLiteralAccess()
        {
            const string input = "[ a = 0; ] func funca(){ a = [0, 1, 2][0]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(0, memory.GetVariable("a"), "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation and access
        /// </summary>
        [TestMethod]
        public void TestNestedArrayLiteralAccess()
        {
            const string input = "[ a = 0; ] func funca(){ a = [[0, 1], 1, 2][0][1]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(1, memory.GetVariable("a"), "The list did not have the expected values");
        }

        /// <summary>
        /// Tests parsing and execution of array literal value set
        /// </summary>
        [TestMethod]
        public void TestSetArrayIndex()
        {
            const string input = "[ a = 0; ] func funca(){ a = [0, 1, 2]; a[0] = 1; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(1, list[0], "The list created by the script is not of the expected type");
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal value set
        /// </summary>
        [TestMethod]
        public void TestSetNestedArrayIndex()
        {
            const string input = "[ a = 0; ] func funca(){ a = [[0, 1], 1, 2]; a[0][1] = 2; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseInputString();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.AreEqual(2, ((IList)list[0])[1], "The list created by the script is not of the expected type");
        }
    }
}