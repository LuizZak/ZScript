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

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Tokenization;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Typing;

namespace ZScriptTests.Runtime.Execution
{
    [TestClass]
    public class FunctionVmTests
    {
        #region General instructions

        [TestMethod]
        public void TestNoop()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Noop),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Noop),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Noop),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Noop)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestInterrupt()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"), "The Interrupt instruction did not interrupt the virtual machine as expected");
        }

        [TestMethod]
        public void TestValuelessReturn()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"), "The Return instruction did not interrupt the virtual machine as expected");
        }

        [TestMethod]
        public void TestValuedReturn()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsTrue(functionVm.HasReturnValue, "The Return instruction did not return expected");
            Assert.AreEqual(10, functionVm.ReturnValue, "The Return instruction did not return the correct value expected");
            Assert.IsFalse(memory.HasVariable("a"), "The Return instruction did not interrupt the virtual machine as expected");
        }

        [TestMethod]
        public void TestSet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
            
            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestMultipleSet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(20),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variables that were set by the instructions");
            Assert.AreEqual(20, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Trying to call set with less than two items in the stack should raise an exception", AllowDerivedTypes = true)]
        public void TestFailedSet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [TestMethod]
        public void TestGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            memory.SetVariable("b", 5);

            functionVm.Execute();

            Assert.AreEqual(5, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestImplicitGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            memory.SetVariable("b", 5);

            functionVm.Execute();

            Assert.AreEqual(5, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestClearStack()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [TestMethod]
        public void TestSwapStack()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap), // Swapping the stack should get the '5' at the top, making the Set instruction set a as 5
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(5, memory.GetVariable("a"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestDuplicateStack()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate), // Duplicating the stack should result in two '5's at the top
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestJump()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4), // Jumping to the 5th instruction should skip to the 'b' variable set instructions
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestJumpIfTrue()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 5), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 11), // Jumping to the end
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestJumpIfFalse()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse, 5), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse, 11), // Jumping to the end
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.AreEqual(5, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestPeekJumpIfTrue()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTruePeek, 4), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTruePeek, 11), // Jumping to the end
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.AreEqual(false, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestPeekJumpIfFalse()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalsePeek, 4), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalsePeek, 11), // Jumping to the end
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsFalse(memory.HasVariable("a"));
            Assert.AreEqual(true, memory.GetVariable("b"), "The memory should contain the variables that were set by the instructions");
        }

        #endregion

        #region Increment/decrement

        [TestMethod]
        public void TestPrefixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPrefix),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(11, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestPostfixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestPrefixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.DecrementPrefix),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(9, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestPostfixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.DecrementPostfix),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        #endregion

        #region Arithmetic Operations

        [TestMethod]
        public void TestMultiply()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Multiply),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(100, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestDivide()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(50),
                TokenFactory.CreateBoxedValueToken(10.0),
                TokenFactory.CreateOperatorToken(VmInstruction.Divide),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(5.0, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestModulo()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateOperatorToken(VmInstruction.Modulo),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(0, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestAdd()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(20, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestSubtract()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(15),
                TokenFactory.CreateBoxedValueToken(7),
                TokenFactory.CreateOperatorToken(VmInstruction.Subtract),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(8, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestEquals()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),

                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(80.0),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),

                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.Equals),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, memory.GetVariable("a"), "The 'Equals' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("b"), "The 'Equals' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("c"), "The 'Equals' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestUnequals()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Unequals),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),

                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(80.0),
                TokenFactory.CreateOperatorToken(VmInstruction.Unequals),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),

                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.Unequals),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Unequals' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Unequals' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("c"), "The 'Unequals' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestLess()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(80),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, memory.GetVariable("a"), "The 'Less' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Less' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestLessOrEquals()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.LessOrEquals),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateOperatorToken(VmInstruction.LessOrEquals),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(71),
                TokenFactory.CreateOperatorToken(VmInstruction.LessOrEquals),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, memory.GetVariable("a"), "The 'Less or equals' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Less or equals' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("c"), "The 'Less or equals' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestGreater()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Greater),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(80),
                TokenFactory.CreateOperatorToken(VmInstruction.Greater),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Greater' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("b"), "The 'Greater' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestGreaterOrEquals()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(100),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.GreaterOrEquals),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateOperatorToken(VmInstruction.GreaterOrEquals),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(70),
                TokenFactory.CreateBoxedValueToken(71),
                TokenFactory.CreateOperatorToken(VmInstruction.GreaterOrEquals),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Greater or equals' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Greater or equals' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("c"), "The 'Greater or equals' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestLogicalAnd()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalAnd),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalAnd),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalAnd),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, memory.GetVariable("a"), "The 'Logical And' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Logical And' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("c"), "The 'Logical And' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestLogicalOr()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalOr),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalOr),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalOr),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Logical Or' instruction is not behaving correctly");
            Assert.AreEqual(true, memory.GetVariable("b"), "The 'Logical Or' instruction is not behaving correctly");
            Assert.AreEqual(false, memory.GetVariable("c"), "The 'Logical Or' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestUnaryNegate()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.ArithmeticNegate),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(-10, memory.GetVariable("a"), "The 'Unary Negate' instruction is not behaving correctly");
        }

        [TestMethod]
        public void TestLogicalNegate()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateOperatorToken(VmInstruction.LogicalNegate),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Logical Negate' instruction is not behaving correctly");
        }

        #endregion

        #region Type operations

        #region Value/primitive types

        [TestMethod]
        public void TestPassingPrimitiveIsOperator()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Is, typeof(long))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, functionVm.Stack.Peek(), "Operator failed to produce the expected results");
        }

        [TestMethod]
        public void TestFailingPrimitiveIsOperator()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateStringToken("StringyString"),
                TokenFactory.CreateOperatorToken(VmInstruction.Is, typeof(long))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, functionVm.Stack.Peek(), "Operator failed to produce the expected results");
        }

        [TestMethod]
        public void TestPassingPrimitiveCastOperation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, typeof(int))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, functionVm.Stack.Peek(), "Operator failed to produce the expected results");
        }

        #endregion

        #region Reference types

        [TestMethod]
        public void TestPassingReferenceIsOperator()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(new TestDerivedClass()),
                TokenFactory.CreateOperatorToken(VmInstruction.Is, typeof(TestBaseClass))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, functionVm.Stack.Peek(), "Operator failed to produce the expected results");
        }

        [TestMethod]
        public void TestPassingReferenceCastOperator()
        {
            TestBaseClass bc = new TestDerivedClass();

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(bc),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, typeof(TestDerivedClass))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Trying to cast an object from one type to an invalid type must raise an InvalidCastException at runtime")]
        public void TestFailingCastOperation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateStringToken("StringyString"),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, typeof(TestDerivedClass))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        #endregion

        #endregion

        #region Bitwise Operations

        [TestMethod]
        public void TestBitwiseAnd()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0xFF00),
                TokenFactory.CreateBoxedValueToken(0xF000),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseAnd),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(0xF000, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBitwiseOr()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0xF000),
                TokenFactory.CreateBoxedValueToken(0x0F00),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseOr),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(0xFF00, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBitwiseXOr()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0xFAE01),
                TokenFactory.CreateBoxedValueToken(0xED10),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseXOr),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(1000209, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBooleanBitwiseAnd()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseAnd),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseAnd),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
            Assert.AreEqual(true, memory.GetVariable("b"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBooleanBitwiseOr()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseOr),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseOr),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
            Assert.AreEqual(false, memory.GetVariable("b"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBooleanBitwiseXOr()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(false),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseXOr),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateBoxedValueToken(true),
                TokenFactory.CreateOperatorToken(VmInstruction.BitwiseXOr),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
            Assert.AreEqual(false, memory.GetVariable("b"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestShiftLeft()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateOperatorToken(VmInstruction.ShiftLeft),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateOperatorToken(VmInstruction.ShiftLeft),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(4, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
            Assert.AreEqual(20, memory.GetVariable("b"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestShiftRight()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(32),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateOperatorToken(VmInstruction.ShiftRight),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(40),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateOperatorToken(VmInstruction.ShiftRight),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(16, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
            Assert.AreEqual(10, memory.GetVariable("b"), "The memory should contain the variable that was set by the instructions");
        }

        #endregion

        #region Manual memory addressing

        /// <summary>
        /// Tests addressing of memory via manual indexing
        /// </summary>
        [TestMethod]
        public void TestNamedVariableFetching()
        {
            /*
                0000000: 0
                0000001: i
                0000002: Set
                0000003: 7
                0000004: Jump
                0000005: i
                0000006: IncrementPostfix
                0000007: i
                0000008: 100000
                0000009: Less
                0000010: 14
                0000011: JumpIfFalse
                0000012: 5
                0000013: Jump
                0000014: Interrupt
            */

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateVariableToken("i", true),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 6),
                TokenFactory.CreateVariableToken("i", true),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
                TokenFactory.CreateVariableToken("i", true),
                TokenFactory.CreateBoxedValueToken(100000),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse, 12),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            // Test the script time
            var sw = Stopwatch.StartNew();

            functionVm.Execute();

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            //Assert.AreEqual(100, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        /// <summary>
        /// Tests addressing of memory via manual indexing
        /// </summary>
        [TestMethod]
        public void TestManualAddressing()
        {
            /*
                0000000: 0
                0000001: i
                0000002: Set
                0000003: 7
                0000004: Jump
                0000005: i
                0000006: IncrementPostfix
                0000007: i
                0000008: GAA
                0000009: 100000
                0000010: Less
                0000011: 15
                0000012: JumpIfFalse
                0000013: 5
                0000014: Jump
                0000015: Interrupt
            */

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.SetAtAddress),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 6),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.GetAtAddress),
                TokenFactory.CreateBoxedValueToken(100000),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse, 14),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, new LocalAddressed(12), null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            // Test the script time
            var sw = Stopwatch.StartNew();

            functionVm.Execute();

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            //Assert.AreEqual(100, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        #endregion

        class LocalAddressed : IMemory<int>
        {
            /// <summary>
            /// Array of memory addresses available
            /// </summary>
            private readonly object[] _memory;

            private readonly int _count;

            public LocalAddressed(int count)
            {
                _count = count;
                _memory = new object[_count];

                for (int i = 0; i < _count; i++)
                {
                    _memory[i] = null;
                }
            }

            public bool HasVariable(int identifier)
            {
                return _count < identifier;
            }

            public bool TryGetVariable(int identifier, out object value)
            {
                if (!HasVariable(identifier))
                {
                    value = null;
                    return false;
                }

                value = GetVariable(identifier);
                return true;
            }

            public object GetVariable(int identifier)
            {
                return _memory[identifier];
            }

            public void SetVariable(int identifier, object value)
            {
                _memory[identifier] = value;
            }

            public void Clear()
            {
                for (int i = 0; i < _count; i++)
                {
                    _memory[i] = null;
                }
            }

            public int GetCount()
            {
                return _count;
            }
        }

        class TestBaseClass
        {
            
        }

        class TestDerivedClass : TestBaseClass
        {
            
        }
    }
}