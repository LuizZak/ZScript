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

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Typing;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests generic typing behavior of the FunctionVm class
    /// </summary>
    [TestClass]
    public class FunctionVmGenericsTests
    {
        /// <summary>
        /// Tests a Cast operation instruction that points to a dynamically-bound type in the VM context
        /// </summary>
        [TestMethod]
        public void TestGenericCast()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0f),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, 0),
                TokenFactory.CreateBoxedValueToken(10.0f),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, 1)
            };

            // Create the type list
            var typeList = new TypeList(new [] { typeof(long), typeof(int) });
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, typeProvider, typeList); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            Assert.AreEqual(10, functionVm.Stack.Pop());
            Assert.AreEqual(10L, functionVm.Stack.Pop());
        }

        /// <summary>
        /// Tests an Is operation instruction that points to a dynamically-bound type in the VM context
        /// </summary>
        [TestMethod]
        public void TestGenericIs()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0f),
                TokenFactory.CreateOperatorToken(VmInstruction.Is, 0),
                TokenFactory.CreateBoxedValueToken(10.0f),
                TokenFactory.CreateOperatorToken(VmInstruction.Is, 1)
            };

            // Create the type list
            var typeList = new TypeList(new[] { typeof(float), typeof(long) });
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, typeProvider, typeList); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            Assert.AreEqual(false, functionVm.Stack.Pop());
            Assert.AreEqual(true, functionVm.Stack.Pop());
        }

        /// <summary>
        /// Tests a CheckType operation instruction that points to a dynamically-bound type in the VM context
        /// </summary>
        [TestMethod]
        public void TestGenericTypeCheck()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0),
                TokenFactory.CreateInstructionToken(VmInstruction.CheckType, 0),
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CheckType, 1)
            };

            // Create the type list
            var typeList = new TypeList(new[] { typeof(double), typeof(long) });
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, typeProvider, typeList); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();
        }

        /// <summary>
        /// Tests a CheckType operation instruction that points to a dynamically-bound type in the VM context
        /// </summary>
        [TestMethod]
        public void TestGenericCreateArray()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateBoxedValueToken(11L),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, 0)
            };

            // Create the type list
            var typeList = new TypeList(new[] { typeof(long) });
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, typeProvider, typeList); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            var list = (List<long>)functionVm.Stack.Pop();

            Assert.AreEqual(10L, list[0]);
            Assert.AreEqual(11L, list[1]);
        }

        /// <summary>
        /// Tests a CheckType operation instruction that points to a dynamically-bound type in the VM context
        /// </summary>
        [TestMethod]
        public void TestGenericCreateDictionary()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("0"),
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateBoxedValueToken("1"),
                TokenFactory.CreateBoxedValueToken(11L),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { 0, 1 })
            };

            // Create the type list
            var typeList = new TypeList(new[] { typeof(string), typeof(long) });
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, typeProvider, typeList); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            var list = (Dictionary<string, long>)functionVm.Stack.Pop();

            Assert.AreEqual(10L, list["0"]);
            Assert.AreEqual(11L, list["1"]);
        }

        /// <summary>
        /// Tests a generic function call that passes generic parameter types
        /// </summary>
        [TestMethod]
        public void TestGenericFunctionCall()
        {
            // Create the dummy generic function
            var tFunc = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, 0),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            var function = new ZFunction("func1", tFunc.ToTokenList(), new FunctionArgument[0]);
            var runtimeDef = new ZRuntimeDefinition(); runtimeDef.AddFunctionDef(function);
            var runtime = new ZRuntime(runtimeDef, null);

            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("func1"),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.CallGeneric, new [] { typeof(long) })
            };

            // Create the type list
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, runtime, null, typeProvider);

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            var list = (List<long>)functionVm.Stack.Pop();

            Assert.AreEqual(10L, list[0]);
        }

        /// <summary>
        /// Tests a generic function call that passes generic parameter types from its own list of generic types
        /// </summary>
        [TestMethod]
        public void TestGenericGenericFunctionCall()
        {
            // Create the dummy generic function
            var tFunc1 = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, 0),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            // Create the dummy generic function
            var tFunc2 = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("func1"),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.CallGeneric, new [] { 0 }),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            var func1 = new ZFunction("func1", tFunc1.ToTokenList(), new FunctionArgument[0]);
            var func2 = new ZFunction("func2", tFunc2.ToTokenList(), new FunctionArgument[0]);
            var runtimeDef = new ZRuntimeDefinition(); runtimeDef.AddFunctionDefs(new []{ func1, func2 });
            var runtime = new ZRuntime(runtimeDef, null);

            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("func2"),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.CallGeneric, new [] { typeof(long) })
            };

            // Create the type list
            var typeProvider = new TypeProvider();

            var memory = new Memory();
            var context = new VmContext(memory, null, runtime, null, typeProvider);

            var functionVm = new FunctionVM(new TokenList(t), context);

            functionVm.Execute();

            var list = (List<long>)functionVm.Stack.Pop();

            Assert.AreEqual(10L, list[0]);
        }
    }
}