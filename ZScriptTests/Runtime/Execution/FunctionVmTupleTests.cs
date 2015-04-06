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
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests Tuple creation/access functionality on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmTupleTests
    {
        /// <summary>
        /// Tests the VmInstruction.CreateTuple instruction
        /// </summary>
        [TestMethod]
        public void TestCreateTuple()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, new[] { typeof(object), typeof(string), typeof(long) })
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null);
                // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(List<object>));

            var array = (List<object>)functionVm.Stack.Pop();

            Assert.AreEqual(10, array[0]);
            Assert.AreEqual("abc", array[1]);
            Assert.AreEqual(10L, array[2]);
        }

        /// <summary>
        /// Tests exception raising when trying to create a tuple with mismatched types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(VirtualMachineException))]
        public void TestCreateTupleTypeException()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, new[] { typeof(long), typeof(string) })
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null);
            // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        /// <summary>
        /// Tests tuple creation with less values than the stack has
        /// </summary>
        [TestMethod]
        public void TestCreateTupleShorterThanStack()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, new[] { typeof(string), typeof(long) })
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null);
            // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(List<object>));

            var array = (List<object>)functionVm.Stack.Pop();

            Assert.AreEqual("abc", array[0]);
            Assert.AreEqual(10L, array[1]);

            Assert.AreEqual(10, functionVm.Stack.Pop());
        }
    }
}