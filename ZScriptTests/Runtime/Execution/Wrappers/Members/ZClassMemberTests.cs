#region License information
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
#endregion
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