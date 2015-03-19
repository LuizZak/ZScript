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
using System.Collections.Generic;

using Xunit;

using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Tests the functionarlity of the Memory class
    /// </summary>
    public class MemoryTests
    {
        [Fact]
        public void TestSetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.Equal(10, mem.GetObjectMemory()["var1"]);
            Assert.Equal(null, mem.GetObjectMemory()["var2"]);
        }

        [Fact]
        public void TestGetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.Equal(10, mem.GetVariable("var1"));
            Assert.Equal(null, mem.GetVariable("var2"));
        }

        [Fact]
        public void TestFailedGetVariable()
        {
            var mem = new Memory();

            Assert.Throws<KeyNotFoundException>(() => mem.GetVariable("var1"));
        }

        [Fact]
        public void TestTryGetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);

            object obj;
            Assert.True(mem.TryGetVariable("var1", out obj));
            Assert.Equal(10, obj);

            Assert.False(mem.TryGetVariable("var2", out obj));
        }

        [Fact]
        public void TestHasVariable()
        {
            var mem = new Memory();

            Assert.False(mem.HasVariable("var1"));
            Assert.False(mem.HasVariable("var2"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.True(mem.HasVariable("var1"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true");
            Assert.True(mem.HasVariable("var2"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true, even with a null value");
        }

        [Fact]
        public void TestClearVariable()
        {
            var mem = new Memory();

            Assert.False(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.True(mem.HasVariable("var1"));
            Assert.True(mem.HasVariable("var2"));

            mem.ClearVariable("var1");

            Assert.False(mem.HasVariable("var1"), "After a valid ClearVariable() call, calling HasVariable() with the same variable name should return false");
            Assert.True(mem.HasVariable("var2"));
        }

        [Fact]
        public void TestClear()
        {
            var mem = new Memory();

            Assert.False(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.True(mem.HasVariable("var1"));
            Assert.True(mem.HasVariable("var2"));

            mem.Clear();

            Assert.False(mem.HasVariable("var1"));
            Assert.False(mem.HasVariable("var2"));
        }

        [Fact]
        public void TestGetcount()
        {
            var mem = new Memory();

            Assert.False(mem.HasVariable("var1"));

            Assert.Equal(0, mem.GetCount());

            mem.SetVariable("var1", 10);

            Assert.Equal(1, mem.GetCount());

            mem.SetVariable("var2", 10);

            Assert.Equal(2, mem.GetCount());

            mem.SetVariable("var2", 11);

            Assert.Equal(2, mem.GetCount());

            mem.Clear();

            Assert.Equal(0, mem.GetCount());
        }

        [Fact]
        public void TestBasicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new FunctionArgument[0]);

            var mem = Memory.CreateMemoryFromArgs(func);

            Assert.Equal(0, mem.GetCount());
        }

        [Fact]
        public void TestParameteredMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new [] { new FunctionArgument("arg1") });

            var mem = Memory.CreateMemoryFromArgs(func, 1L);

            Assert.Equal(1, mem.GetCount());
            Assert.True(mem.HasVariable("arg1"));
            Assert.Equal(1L, mem.GetVariable("arg1"));
        }

        [Fact]
        public void TestVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var mem = Memory.CreateMemoryFromArgs(func, 1L, 2L);

            Assert.Equal(1, mem.GetCount());
            Assert.True(mem.HasVariable("arg1"));
            Assert.True(mem.GetVariable("arg1") is Memory.VarArgsArrayList);

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.Equal(1L, variadic[0]);
            Assert.Equal(2L, variadic[1]);
        }

        [Fact]
        public void TestEmptyVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var mem = Memory.CreateMemoryFromArgs(func);

            Assert.Equal(1, mem.GetCount());
            Assert.True(mem.HasVariable("arg1"));
            Assert.True(mem.GetVariable("arg1") is ArrayList);

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.Equal(0, variadic.Count);
        }

        [Fact]
        public void TestArrayInVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var array = new Memory.VarArgsArrayList { 1L, 2L, 3L };

            var mem = Memory.CreateMemoryFromArgs(func, array);

            Assert.Equal(1, mem.GetCount());
            Assert.True(mem.HasVariable("arg1"));
            Assert.True(mem.GetVariable("arg1") is ArrayList);

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.Equal(1L, variadic[0]);
            Assert.Equal(2L, variadic[1]);
            Assert.Equal(3L, variadic[2]);
        }
    }
}