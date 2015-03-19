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
using ZScript.Elements;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of member accesses
    /// </summary>
    [TestClass]
    public class MemberTests
    {
        /// <summary>
        /// Tests basic member accessing
        /// </summary>
        [TestMethod]
        public void TestBasicMemberAccess()
        {
            const string input = "var a = 0; var b = 10; func funca(){ a = [0].Count; b = \"abc\".Length; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");
            
            // Assert the correct call was made
            Assert.AreEqual(1L, memory.GetVariable("a"), "The member fetch did not occur as expected");
            Assert.AreEqual(3L, memory.GetVariable("b"), "The member fetch did not occur as expected");
        }

        /// <summary>
        /// Tests compound assignments performed with member accesses
        /// </summary>
        [TestMethod]
        public void TestMemberCompoundAssignment()
        {
            const string input = "var a; func funca(){ a = { x:10 }; a.x += 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.AreEqual(20L, ((ZObject)memory.GetVariable("a"))["x"], "The member fetch did not occur as expected");
        }
    }
}