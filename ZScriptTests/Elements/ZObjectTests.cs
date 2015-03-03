using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;

namespace ZScriptTests.Elements
{
    /// <summary>
    /// Tests the functionality of the ZObject class
    /// </summary>
    [TestClass]
    public class ZObjectTests
    {
        /// <summary>
        /// Tests the CanSubscriptWithType implementation
        /// </summary>
        [TestMethod]
        public void TestCanSubscriptWithType()
        {
            var obj = new ZObject();

            Assert.IsTrue(obj.CanSubscriptWithIndexType(typeof(string)));
            Assert.IsFalse(obj.CanSubscriptWithIndexType(typeof(void)));
            Assert.IsFalse(obj.CanSubscriptWithIndexType(typeof(int)));
        }

        /// <summary>
        /// Tests subscription with ZObjects
        /// </summary>
        [TestMethod]
        public void TestSubcript()
        {
            var obj = new ZObject();

            obj["a"] = 10;
            obj["b"] = "bcd";

            Assert.AreEqual(10, obj["a"]);
            Assert.AreEqual("bcd", obj["b"]);
            Assert.AreEqual(null, obj["c"]);
        }

        /// <summary>
        /// Tests subscription with non string objects on ZObjects
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to subscripts a ZObjcect with a non-string value should raise an exception")]
        public void TestSubcriptWithNonString()
        {
            var obj = new ZObject();
            obj[10] = 10;
        }

        /// <summary>
        /// Tests subscription with null objects on ZObjects
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Trying to subscripts a ZObjcect with a null value should raise an exception")]
        public void TestSubcriptWithNullException()
        {
            var obj = new ZObject();

            obj[null] = 10;
        }
    }
}