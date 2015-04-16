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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime;
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
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, typeof(AnonTuple1<int,string,long>))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null);

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(AnonTuple1<int, string, long>));

            var tuple = (AnonTuple1<int, string, long>)functionVm.Stack.Pop();

            Assert.AreEqual(10, tuple.Field0);
            Assert.AreEqual("abc", tuple.Field1);
            Assert.AreEqual(10L, tuple.Field2);
        }

        /// <summary>
        /// Tests exception raising when trying to create a tuple with mismatched types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCreateTupleTypeException()
        {
            // Create the set of tokens
            var t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10L),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, typeof(AnonTuple1<,,>))
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
                TokenFactory.CreateInstructionToken(VmInstruction.CreateTuple, typeof(AnonTuple2<string,long>))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null);
            // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);
            
            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(AnonTuple2<string, long>));

            var array = (AnonTuple2<string, long>)functionVm.Stack.Pop();

            Assert.AreEqual("abc", array.Field0);
            Assert.AreEqual(10L, array.Field1);

            Assert.AreEqual(10, functionVm.Stack.Pop());
        }

        /// <summary>
        /// Anonymous tuple declaration used for inner tests
        /// </summary>
        protected struct AnonTuple1<T1, T2, T3> : ITuple
        {
            public T1 Field0;
            public T2 Field1;
            public T3 Field2;

            public AnonTuple1(T1 field0, T2 field1, T3 field2)
            {
                Field0 = field0;
                Field1 = field1;
                Field2 = field2;
            }
        }

        /// <summary>
        /// Anonymous tuple declaration used for inner tests
        /// </summary>
        protected struct AnonTuple2<T1, T2> : ITuple
        {
            public T1 Field0;
            public T2 Field1;

            public AnonTuple2(T1 field0, T2 field1)
            {
                Field0 = field0;
                Field1 = field1;
            }
        }
    }
}