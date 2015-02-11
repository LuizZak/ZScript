using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Tests the ZObjectMember class functionality
    /// </summary>
    [TestClass]
    public class ZObjectMemberTests
    {
        [TestMethod]
        public void TestCreation()
        {
            const string memberName = "member";

            var obj = new ZObject { { memberName, 10 } };
            var wrap = new ZObjectMember(obj, memberName);

            Assert.AreEqual(memberName, wrap.MemberName, "The member name returned by the MemberName must match the member name the ZObjectMember was created with");
            Assert.AreEqual(typeof(object), wrap.MemberType, "ZObjectMember.MemberType must always return typeof(object)");
        }

        [TestMethod]
        public void TestMemberGet()
        {
            const string memberName = "member";

            var obj = new ZObject { { memberName, 10 } };
            var wrap = new ZObjectMember(obj, memberName);

            Assert.AreEqual(obj[memberName], wrap.GetValue(), "The value returned by the ZObjectMember must match the underlying field in the ZObject");
        }

        [TestMethod]
        public void TestMemberSet()
        {
            const string memberName = "member";

            var obj = new ZObject();
            var wrap = new ZObjectMember(obj, memberName);

            wrap.SetValue(10);

            Assert.AreEqual(obj[memberName], wrap.GetValue(), "The SetValue() method did not se the value correctly");
        }
    }
}