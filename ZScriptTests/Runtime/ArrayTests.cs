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

using System.Collections;

using Xunit;

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of array code
    /// </summary>
    public class ArrayTests
    {
        /// <summary>
        /// Tests parsing and execution of array literal creation
        /// </summary>
        [Fact]
        public void TestArrayLiteral()
        {
            const string input = "var a; func funca(){ a = [0, 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.True(memory.GetVariable("a") is IList, "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.Equal(0L, list[0]);
            Assert.Equal(1L, list[1]);
            Assert.Equal(2L, list[2]);
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation
        /// </summary>
        [Fact]
        public void TestNestedArrayLiteral()
        {
            const string input = "var a; func funca(){ a = [[0], 1, 2]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.True(memory.GetVariable("a") is IList, "The list created by the script is not of the expected type");
            var list = (IList)memory.GetVariable("a");

            Assert.True(list[0] is IList, "The list created by the script is not of the expected type");
            Assert.Equal(0L, ((IList)list[0])[0]);
            Assert.Equal(1L, list[1]);
            Assert.Equal(2L, list[2]);
        }

        /// <summary>
        /// Tests parsing and execution of array literal creation and access
        /// </summary>
        [Fact]
        public void TestArrayLiteralAccess()
        {
            const string input = "var a = 0; func funca(){ a = [0, 1, 2][0]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(0L, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal creation and access
        /// </summary>
        [Fact]
        public void TestNestedArrayLiteralAccess()
        {
            const string input = "var a = 0; func funca(){ a = [[0, 1], 1, 2][0][1]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(1L, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests parsing and execution of array literal value set
        /// </summary>
        [Fact]
        public void TestSetArrayIndex()
        {
            const string input = "var a; func funca(){ a = [0, 1, 2]; a[0] = 1; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.Equal(1L, list[0]);
        }

        /// <summary>
        /// Tests parsing and execution of nested array literal value set
        /// </summary>
        [Fact]
        public void TestSetNestedArrayIndex()
        {
            const string input = "var a; func funca(){ a = [[0, 1], 1, 2]; a[0][1] = 2; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("funca");

            // Assert the correct call was made
            var list = (IList)memory.GetVariable("a");

            Assert.Equal(2L, ((IList)list[0])[1]);
        }
    }
}