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
    /// Tests parsing and execution of top level functions
    /// </summary>
    public class TopLevelFunctionTests
    {
        /// <summary>
        /// Tests basic function calling
        /// </summary>
        [Fact]
        public void TestBasicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); } func funcb() { __trace(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(1L, owner.TraceObjects[0]);
        }

        /// <summary>
        /// Tests function calling with multiple parameters
        /// </summary>
        [Fact]
        public void TestMultiParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(1, 2); } func funcb(a:int, b:int) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(1L, owner.TraceObjects[0]);
            Assert.Equal(2L, owner.TraceObjects[1]);
        }

        /// <summary>
        /// Tests function calling with default parameters
        /// </summary>
        [Fact]
        public void TestDefaultParametered()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(a:int = 0, b:int = 0) { __trace(a, b); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(0L, owner.TraceObjects[0]);
            Assert.Equal(0L, owner.TraceObjects[1]);

            Assert.Equal(1L, owner.TraceObjects[2]);
            Assert.Equal(0L, owner.TraceObjects[3]);

            Assert.Equal(1L, owner.TraceObjects[4]);
            Assert.Equal(2L, owner.TraceObjects[5]);
        }

        /// <summary>
        /// Tests function calling with variadic parameters
        /// </summary>
        [Fact]
        public void TestVariadicFunctionCall()
        {
            const string input = "@__trace(v...) func funca(){ funcb(); funcb(1); funcb(1, 2); } func funcb(args...) { __trace(args); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("funca");

            // Assert the correct call was made
            Assert.Equal(1L, owner.TraceObjects[0]);
            Assert.Equal(1L, owner.TraceObjects[1]);
            Assert.Equal(2L, owner.TraceObjects[2]);
        }

        /// <summary>
        /// Tests shadowing a function by creating a local variable inside another function
        /// </summary>
        [Fact]
        public void TestShadowingFunction()
        {
            const string input = "@__trace(v...) func f1() { f2(); var f2 = 0; } func f2() { __trace(1); }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f1");

            // Assert the correct call was made
            Assert.Equal(1L, owner.TraceObjects[0]);
        }
    }
}