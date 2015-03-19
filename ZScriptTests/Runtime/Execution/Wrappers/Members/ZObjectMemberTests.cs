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

using Xunit;

using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Tests the ZObjectMember class functionality
    /// </summary>
    public class ZObjectMemberTests
    {
        [Fact]
        public void TestCreation()
        {
            const string memberName = "member";

            var obj = new ZObject { { memberName, 10 } };
            var wrap = new ZObjectMember(obj, memberName);

            Assert.Equal(memberName, wrap.MemberName);
            Assert.Equal(typeof(object), wrap.MemberType);
        }

        [Fact]
        public void TestMemberGet()
        {
            const string memberName = "member";

            var obj = new ZObject { { memberName, 10 } };
            var wrap = new ZObjectMember(obj, memberName);

            Assert.Equal(obj[memberName], wrap.GetValue());
        }

        [Fact]
        public void TestMemberSet()
        {
            const string memberName = "member";

            var obj = new ZObject();
            var wrap = new ZObjectMember(obj, memberName);

            wrap.SetValue(10);

            Assert.Equal(obj[memberName], wrap.GetValue());
        }
    }
}