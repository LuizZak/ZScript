using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Runtime.Execution.Wrappers.Members;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Tests the functionality of the ZClassMember class and related components
    /// </summary>
    [TestClass]
    public class ZClassMemberTests
    {
        [TestMethod]
        public void TestCreation()
        {
            const string memberName = "field1";

            var obj = TestUtils.CreateTestClassInstance();
            var wrap = new ZClassMember(obj, memberName);

            Assert.AreEqual(memberName, wrap.MemberName, "The member name returned by the MemberName must match the member name the ZClassMember was created with");
            Assert.AreEqual(typeof(long), wrap.MemberType, "ZClassMember.MemberType must always return typeof(object)");
        }

        [TestMethod]
        public void TestMemberGet()
        {
            const string memberName = "field1";

            var obj = TestUtils.CreateTestClassInstance();
            var wrap = new ZClassMember(obj, memberName);

            Assert.AreEqual(obj.LocalMemory.GetVariable(memberName), wrap.GetValue(), "The value returned by the ZClassMember must match the underlying field in the ZClassInstance");
        }

        [TestMethod]
        public void TestMemberSet()
        {
            const string memberName = "field1";

            var obj = TestUtils.CreateTestClassInstance();
            var wrap = new ZClassMember(obj, memberName);

            wrap.SetValue(10);

            Assert.AreEqual(obj.LocalMemory.GetVariable(memberName), wrap.GetValue(), "The SetValue() method did not se the value correctly");
        }
    }
}