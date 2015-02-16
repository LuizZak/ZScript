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