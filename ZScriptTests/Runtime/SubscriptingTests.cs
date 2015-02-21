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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests subscription of different types of values on the VM
    /// </summary>
    [TestClass]
    public class SubscriptingTests
    {
        /// <summary>
        /// Tests parsing and execution of dictionary subscription value fetching
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptGet()
        {
            const string input = "var a = 0; var b; func funca(){ a = b[\"abc\"]; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            memory.SetVariable("b", new Dictionary<string, object> { { "abc", 10L } });

            runtime.CallFunction("funca");

            Assert.AreEqual(10L, memory.GetVariable("a"), "The VM failed to fetch the correct subscription for the dictionary object stored");
        }

        /// <summary>
        /// Tests parsing and execution of dictionary subscription value setting
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptSet()
        {
            const string input = "var b; func funca(){ b[\"abc\"] = 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            var dict = new Dictionary<string, object>();
            memory.SetVariable("b", dict);

            runtime.CallFunction("funca");

            Assert.AreEqual(10L, dict["abc"], "The VM failed to set the correct subscription for the dictionary object stored");
        }

        /// <summary>
        /// Tests utilizing a subscript with a compound operator
        /// </summary>
        [TestMethod]
        public void TestSubscriptCompoundAssign()
        {
            const string input = "var b; func funca(){ b['abc'] += 10; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            runtime.ExpandGlobalVariables();
            var memory = runtime.GlobalMemory;

            // Set the dictionary on memory now
            var dict = new Dictionary<string, object> { { "abc", 10 } };
            memory.SetVariable("b", dict);

            runtime.CallFunction("funca");

            Assert.AreEqual(20L, dict["abc"], "The VM failed to set the correct subscription for the dictionary object stored");
        }
    }
}