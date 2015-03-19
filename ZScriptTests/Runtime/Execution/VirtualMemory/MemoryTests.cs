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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution.VirtualMemory
{
    /// <summary>
    /// Tests the functionarlity of the Memory class
    /// </summary>
    [TestClass]
    public class MemoryTests
    {
        [TestMethod]
        public void TestSetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.AreEqual(10, mem.GetObjectMemory()["var1"]);
            Assert.AreEqual(null, mem.GetObjectMemory()["var2"]);
        }

        [TestMethod]
        public void TestGetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.AreEqual(10, mem.GetVariable("var1"));
            Assert.AreEqual(null, mem.GetVariable("var2"));
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException), "Trying to fetch a variable that does not exists must raise a KeyNotFoundException")]
        public void TestFailedGetVariable()
        {
            var mem = new Memory();

            mem.GetVariable("var1");
        }

        [TestMethod]
        public void TestTryGetVariable()
        {
            var mem = new Memory();

            mem.SetVariable("var1", 10);

            object obj;
            Assert.IsTrue(mem.TryGetVariable("var1", out obj));
            Assert.AreEqual(10, obj);

            Assert.IsFalse(mem.TryGetVariable("var2", out obj));
        }

        [TestMethod]
        public void TestHasVariable()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));
            Assert.IsFalse(mem.HasVariable("var2"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", null);

            Assert.IsTrue(mem.HasVariable("var1"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true");
            Assert.IsTrue(mem.HasVariable("var2"), "After a valid SetVariable() call, calling HasVariable() with the same variable name should return true, even with a null value");
        }

        [TestMethod]
        public void TestClearVariable()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.IsTrue(mem.HasVariable("var1"));
            Assert.IsTrue(mem.HasVariable("var2"));

            mem.ClearVariable("var1");

            Assert.IsFalse(mem.HasVariable("var1"), "After a valid ClearVariable() call, calling HasVariable() with the same variable name should return false");
            Assert.IsTrue(mem.HasVariable("var2"));
        }

        [TestMethod]
        public void TestClear()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));

            mem.SetVariable("var1", 10);
            mem.SetVariable("var2", 10);

            Assert.IsTrue(mem.HasVariable("var1"));
            Assert.IsTrue(mem.HasVariable("var2"));

            mem.Clear();

            Assert.IsFalse(mem.HasVariable("var1"));
            Assert.IsFalse(mem.HasVariable("var2"));
        }

        [TestMethod]
        public void TestGetcount()
        {
            var mem = new Memory();

            Assert.IsFalse(mem.HasVariable("var1"));

            Assert.AreEqual(0, mem.GetCount());

            mem.SetVariable("var1", 10);

            Assert.AreEqual(1, mem.GetCount());

            mem.SetVariable("var2", 10);

            Assert.AreEqual(2, mem.GetCount());

            mem.SetVariable("var2", 11);

            Assert.AreEqual(2, mem.GetCount());

            mem.Clear();

            Assert.AreEqual(0, mem.GetCount());
        }

        [TestMethod]
        public void TestBasicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new FunctionArgument[0]);

            var mem = Memory.CreateMemoryFromArgs(func);

            Assert.AreEqual(0, mem.GetCount());
        }

        [TestMethod]
        public void TestParameteredMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new [] { new FunctionArgument("arg1") });

            var mem = Memory.CreateMemoryFromArgs(func, 1L);

            Assert.AreEqual(1, mem.GetCount());
            Assert.IsTrue(mem.HasVariable("arg1"));
            Assert.AreEqual(1L, mem.GetVariable("arg1"));
        }

        [TestMethod]
        public void TestVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var mem = Memory.CreateMemoryFromArgs(func, 1L, 2L);

            Assert.AreEqual(1, mem.GetCount());
            Assert.IsTrue(mem.HasVariable("arg1"));
            Assert.IsInstanceOfType(mem.GetVariable("arg1"), typeof(Memory.VarArgsArrayList));

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.AreEqual(1L, variadic[0], "Failed to generate expected variadic array");
            Assert.AreEqual(2L, variadic[1], "Failed to generate expected variadic array");
        }

        [TestMethod]
        public void TestEmptyVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var mem = Memory.CreateMemoryFromArgs(func);

            Assert.AreEqual(1, mem.GetCount());
            Assert.IsTrue(mem.HasVariable("arg1"));
            Assert.IsInstanceOfType(mem.GetVariable("arg1"), typeof(ArrayList));

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.AreEqual(0, variadic.Count);
        }

        [TestMethod]
        public void TestArrayInVariadicMemoryFromFunctionArgs()
        {
            var func = new ZFunction("Abc", new TokenList(), new[] { new FunctionArgument("arg1", true) });

            var array = new Memory.VarArgsArrayList { 1L, 2L, 3L };

            var mem = Memory.CreateMemoryFromArgs(func, array);

            Assert.AreEqual(1, mem.GetCount());
            Assert.IsTrue(mem.HasVariable("arg1"));
            Assert.IsInstanceOfType(mem.GetVariable("arg1"), typeof(ArrayList));

            var variadic = (Memory.VarArgsArrayList)mem.GetVariable("arg1");

            Assert.AreEqual(1L, variadic[0], "Failed to generate expected variadic array");
            Assert.AreEqual(2L, variadic[1], "Failed to generate expected variadic array");
            Assert.AreEqual(3L, variadic[2], "Failed to generate expected variadic array");
        }
    }
}