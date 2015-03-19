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

using System;

using Xunit;

using ZScript.Elements;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers;

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Tests the MemberWrapperHelper class and related components
    /// </summary>
    public class MemberWrapperHelperTests
    {
        /// <summary>
        /// Tests wrapping the field of an ordinary object
        /// </summary>
        [Fact]
        public void TestWrappingObjectMember()
        {
            const string target = "target";

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "Length");

            Assert.Equal(target.Length, member.GetValue());
        }

        /// <summary>
        /// Tests wrapping the field of a ZObject instance
        /// </summary>
        [Fact]
        public void TestWrappingZObjectMember()
        {
            var target = new ZObject();
            target["abc"] = 0;

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "abc");

            Assert.Equal(target["abc"], member.GetValue());
        }

        /// <summary>
        /// Tests wrapping the field of a ZClassInstance object
        /// </summary>
        [Fact]
        public void TestWrappingZClassInstanceMember()
        {
            var target = TestUtils.CreateTestClassInstance();

            var member = MemberWrapperHelper.CreateMemberWrapper(target, "field1");

            Assert.Equal(target.LocalMemory.GetVariable("field1"), member.GetValue());
        }

        /// <summary>
        /// Tests wrapping the method of an object instance
        /// </summary>
        [Fact]
        public void TestWrappingObjectMethod()
        {
            const long target = 10;

            var member = MemberWrapperHelper.CreateCallableWrapper(target, "ToString");

            Assert.Equal("ToString", member.CallableName);
            Assert.Equal(target.ToString(), member.Call(null));
        }

        /// <summary>
        /// Tests a failure case when trying to wrap an unexisting or non-public method of an object
        /// </summary>
        [Fact]
        public void TestFailedWrappingObjectMethod()
        {
            const long target = 10;

            Assert.Throws<ArgumentException>(() => MemberWrapperHelper.CreateCallableWrapper(target, "InvalidMethod"));
        }

        /// <summary>
        /// Tests wrapping the method of a ZClassInstance object
        /// </summary>
        [Fact]
        public void TestWrappingClassInstanceMethod()
        {
            var target = TestUtils.CreateTestClassInstance();

            var member = MemberWrapperHelper.CreateCallableWrapper(target, "func1");
            var runtime = new ZRuntime(new ZRuntimeDefinition(), null);
            var context = new VmContext(new Memory(), null, runtime, null, null);

            Assert.Equal("func1", member.CallableName);
            Assert.Equal(10L, member.Call(context));
        }
    }
}