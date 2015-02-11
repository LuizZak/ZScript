using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers;

namespace ZScriptTests.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Tests the MemberWrapperHelper class and related components
    /// </summary>
    [TestClass]
    public class MemberWrapperHelperTests
    {
        /// <summary>
        /// Tests wrapping the field of an ordinary object
        /// </summary>
        [TestMethod]
        public void TestWrappingObject()
        {
            const string target = "target";

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "Length");

            Assert.AreEqual(target.Length, member.GetValue(), "The value returned by the member is incorrect");
        }

        /// <summary>
        /// Tests wrapping the field of a ZObject instance
        /// </summary>
        [TestMethod]
        public void TestWrappingZObject()
        {
            var target = new ZObject();
            target["abc"] = 0;

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "abc");

            Assert.AreEqual(target["abc"], member.GetValue(), "The value returned by the member is incorrect");
        }
    }
}