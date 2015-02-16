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
        public void TestWrappingObjectMember()
        {
            const string target = "target";

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "Length");

            Assert.AreEqual(target.Length, member.GetValue(), "The value returned by the member is incorrect");
        }

        /// <summary>
        /// Tests wrapping the field of a ZObject instance
        /// </summary>
        [TestMethod]
        public void TestWrappingZObjectMember()
        {
            var target = new ZObject();
            target["abc"] = 0;

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "abc");

            Assert.AreEqual(target["abc"], member.GetValue(), "The value returned by the member is incorrect");
        }

        /// <summary>
        /// Tests wrapping the method of an object instance
        /// </summary>
        [TestMethod]
        public void TestWrappingObjectMethod()
        {
            const long target = 10;

            var member = MemberWrapperHelper.CreateCallableWrapper(target, "ToString");

            Assert.AreEqual(target.ToString(), member.Call(), "The value returned by the callable is incorrect");
        }
    }
}