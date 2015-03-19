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

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of object literal operations
    /// </summary>
    public class ObjectLiteralTests
    {
        [Fact]
        public void TestEmptyObjectLiteral()
        {
            const string input = "var b; func funca(){ b = { }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.True(memory.GetVariable("b") is ZObject, "The VM failed to correctly create the dynamic object");
        }

        [Fact]
        public void TestSimpleObjectLiteral()
        {
            const string input = "var b; func funca(){ b = { x:10, 'abc':11, \"string with spaces\":12 }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(10L, ((ZObject)memory.GetVariable("b"))["x"]);
            Assert.Equal(11L, ((ZObject)memory.GetVariable("b"))["abc"]);
            Assert.Equal(12L, ((ZObject)memory.GetVariable("b"))["string with spaces"]);
        }

        [Fact]
        public void TestComplexObjectLiteral()
        {
            const string input = "var b; func funca(){ b = { x:10 + 5, y:5 * (7 + 1) }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(15L, ((ZObject)memory.GetVariable("b"))["x"]);
            Assert.Equal(40L, ((ZObject)memory.GetVariable("b"))["y"]);
        }
        
        [Fact]
        public void TestNestedObjectLiteral()
        {
            const string input = "var b; func funca(){ b = { a:{ x: 10 }, b:{ y: { z: { a:10 } } }  }; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            // TODO: Deal with this absurd nesting of ZObject casts
            Assert.True(((ZObject)memory.GetVariable("b"))["a"] is ZObject);
            Assert.Equal(10L, ((ZObject)((ZObject)memory.GetVariable("b"))["a"])["x"]);
            Assert.Equal(10L, ((ZObject)((ZObject)((ZObject)((ZObject)memory.GetVariable("b"))["b"])["y"])["z"])["a"]);
        }

        [Fact]
        public void TestObjectSubscript()
        {
            const string input = "var b; func funca(){ var a = { x:10 }; b = a['x']; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(10L, memory.GetVariable("b"));
        }

        [Fact]
        public void TestNestedObjectSubscript()
        {
            const string input = "var b; func funca(){ var a = { x: { x:10 } }; b = a['x']['x']; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(10L, memory.GetVariable("b"));
        }
        
        [Fact]
        public void TestObjectMemberAccess()
        {
            const string input = "var b; func funca(){ var a = { x:10 }; b = a.x; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(10L, memory.GetVariable("b"));
        }
        
        [Fact]
        public void TestNestedObjectMemberAccess()
        {
            const string input = "var b; func funca(){ var a = { x: { x:10 } }; b = a.x.x; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            runtime.CallFunction("funca");

            Assert.Equal(10L, memory.GetVariable("b"));
        }
    }
}