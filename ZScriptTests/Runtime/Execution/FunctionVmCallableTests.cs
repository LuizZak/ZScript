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

using ZScript.Elements;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers.Callables;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests method calling functionalities of the FunctionVm class
    /// </summary>
    [TestClass]
    public class FunctionVmCallableTests
    {
        /// <summary>
        /// Tests fetching a callable from a reflected method on the VM
        /// </summary>
        [TestMethod]
        public void TestMethodCallable()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateStringToken("ToString"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ClassMethod));
        }

        /// <summary>
        /// Tests fetching a callable from a reflected method on the VM
        /// </summary>
        [TestMethod]
        public void TestMethodCallableCall()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateStringToken("ToString"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, new ZRuntime(new ZRuntimeDefinition(), new TestRuntimeOwner()));

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual("10", functionVm.Stack.Peek());
        }

        /// <summary>
        /// Tests nesting multiple callables one next to another
        /// </summary>
        [TestMethod]
        public void TestNestedMethodCallablecall()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                // "10".ToString()
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateStringToken("ToString"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                // <res>.IndexOf("0")
                TokenFactory.CreateStringToken("IndexOf"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetCallable),
                TokenFactory.CreateStringToken("0"),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.Call)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, new ZRuntime(new ZRuntimeDefinition(), new TestRuntimeOwner()));

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(1, functionVm.Stack.Peek());
        }
    }
}