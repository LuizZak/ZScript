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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests the parsing and execution of tuple functionality
    /// </summary>
    [TestClass]
    public class TupleTests
    {
        #region Parsing

        [TestMethod]
        public void TestParseTupleExpression()
        {
            const string input = "var tuple = (0, 1);";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(container.HasErrors);
        }

        [TestMethod]
        public void TestIncompleteTupleTypeError()
        {
            const string input = "var tuple = (0, null);";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteType));
        }

        [TestMethod]
        public void TestTupleTyping()
        {
            const string input = "var tuple:(int, bool) = (0, true);";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(container.HasErrors);
        }

        #endregion

        #region Execution

        [TestMethod]
        public void TestTupleCreation()
        {
            const string input = "var tuple:(int, bool) = (1, true);";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.ExpandGlobalVariables();

            // Assert the correct call was made
            var tuple = memory.GetVariable("tuple");
            Assert.AreEqual(1L, tuple.GetType().GetField("Field0").GetValue(tuple));
            Assert.AreEqual(true, tuple.GetType().GetField("Field1").GetValue(tuple));
        }

        [TestMethod]
        public void TestIndexedTupleAccess()
        {
            const string input = "var v = (0, true).0;";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.ExpandGlobalVariables();

            // Assert the correct call was made
            Assert.AreEqual(0L, memory.GetVariable("v"));
        }

        [TestMethod]
        public void TestLabeledTupleAccess()
        {
            const string input = "var v = (0, x: true).x;";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.ExpandGlobalVariables();

            // Assert the correct call was made
            Assert.AreEqual(true, memory.GetVariable("v"));
        }

        [TestMethod]
        public void TestIndexedTupleAssign()
        {
            const string input = "var v = (0, x: true); func f() { v.1 = false; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            // Assert the correct call was made
            var tuple = memory.GetVariable("v");
            Assert.AreEqual(false, tuple.GetType().GetField("Field1").GetValue(tuple));
        }

        [TestMethod]
        public void TestLabeledTupleAssign()
        {
            const string input = "var v = (0, x: true); func f() { v.x = false; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            // Assert the correct call was made
            var tuple = memory.GetVariable("v");
            Assert.AreEqual(false, tuple.GetType().GetField("Field1").GetValue(tuple));
        }

        [TestMethod]
        public void TestNestedLabeledTupleAssign()
        {
            const string input = "var v = (0, (1, x: true)); func f() { v.1.x = false; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            // Assert the correct call was made
            var tuple = memory.GetVariable("v");
            var innerTuple = tuple.GetType().GetField("Field1").GetValue(tuple);
            Assert.AreEqual(false, innerTuple.GetType().GetField("Field1").GetValue(innerTuple));
        }

        [TestMethod]
        public void TestAssignIndexOnConstantTuple()
        {
            const string input = "let v = (0, 1); func f() { v.1 = 0; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            // Assert the correct call was made
            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant));
        }

        [TestMethod]
        public void TestAssignNestedIndexOnConstantTuple()
        {
            const string input = "let v = (0, (1, 1)); func f() { v.1 .0 = 0; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            // Assert the correct call was made
            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant));
        }

        [TestMethod]
        public void TestAssignNestedLabelOnConstantTuple()
        {
            const string input = "let v = (0, y: (1, 1)); func f() { v.y.0 = 0; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            // Assert the correct call was made
            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant));
        }

        [TestMethod]
        public void TestTuplePassByValue()
        {
            const string input = "var v = (0, true); func f() { var b = v; b.0 = 1; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            // Assert the correct call was made
            var tuple = memory.GetVariable("v");
            Assert.AreEqual(0L, tuple.GetType().GetField("Field0").GetValue(tuple));
        }

        [TestMethod]
        public void TestTuplePassByValueArgument()
        {
            const string input = "var v = (0, true); func f() { f1(v); } func f1(b:(int, bool)) { b.0 = 1; }";

            // Setup owner call
            var owner = new TestRuntimeOwner();

            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f");

            // Assert the correct call was made
            var tuple = memory.GetVariable("v");
            Assert.AreEqual(0L, tuple.GetType().GetField("Field0").GetValue(tuple));
        }

        #endregion
    }
}