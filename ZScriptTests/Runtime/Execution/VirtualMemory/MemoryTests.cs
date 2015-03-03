using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Tests the functionarlity of the Memory class
    /// </summary>
    [TestClass]
    public class MemoryTests
    {
        [TestMethod]
        public void TestSetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.AreEqual(10, mem.GetObjectMemory()["var1"]);
            Assert.AreEqual(null, mem.GetObjectMemory()["var2"]);
        }

        [TestMethod]
        public void TestHasVariable()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));
            Assert.IsFalse(mem.HasVariable("var2"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.IsTrue(mem.HasVariable("var1"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true");
            Assert.IsTrue(mem.HasVariable("var2"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true, even with a null value");
        }

        [TestMethod]
        public void TestClearVariable()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.IsTrue(mem.HasVariable("var1"));
            Assert.IsTrue(mem.HasVariable("var2"));

            mem.ClearVariable("var1");

            Assert.IsFalse(mem.HasVariable("var1"), "After a valid ClearVariable() call, calling HasVariable() with the same variable name should return false");
            Assert.IsTrue(mem.HasVariable("var2"));
        }

        [TestMethod]
        public void TestClear()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.IsTrue(mem.HasVariable("var1"));
            Assert.IsTrue(mem.HasVariable("var2"));

            mem.Clear();

            Assert.IsFalse(mem.HasVariable("var1"));
            Assert.IsFalse(mem.HasVariable("var2"));
        }
    }
}