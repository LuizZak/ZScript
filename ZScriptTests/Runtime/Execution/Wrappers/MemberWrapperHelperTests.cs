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

        /// <summary>
        /// Tests a failure case when trying to wrap an unexisting or non-public method of an object
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Wehn trying to access non-existing or non-public methods with CreateCallableWrapper(), an ArgumentException must be thrown")]
        public void TestFailedWrappingObjectMethod()
        {
            const long target = 10;

            MemberWrapperHelper.CreateCallableWrapper(target, "InvalidMethod");
        }
    }
}