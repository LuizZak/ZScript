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

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of function accesses
    /// </summary>
    public class FunctionAccessTests
    {
        /// <summary>
        /// Tests basic function calling
        /// </summary>
        [Fact]
        public void TestBasicFunctionCall()
        {
            const string input = "var a = ''; var b = 10; func funca(){ a = b.ToString(); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal("10", memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests chained function calling
        /// </summary>
        [Fact]
        public void TestChainedFunctionCall()
        {
            const string input = "var a; func funca(){ a = 1234.ToString().IndexOf('4'); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(3, memory.GetVariable("a"));
        }
    }
}