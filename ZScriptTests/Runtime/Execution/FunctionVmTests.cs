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
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Instruction, null, VmInstruction.Noop)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Interrupt),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 20),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Get),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.ClearStack),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Instruction, null, VmInstruction.Swap), // Swapping the stack should get the '5' at the top, making the Set instruction set a as 5
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 5),
                new Token(TokenType.Instruction, null, VmInstruction.Duplicate), // Duplicating the stack should result in two '5's at the top
                new Token(TokenType.Operator, null, VmInstruction.Add),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Instruction, null, VmInstruction.Jump), // Jumping to the 5th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 6),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfTrue), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, 12),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfTrue), // Jumping to the end
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, 6),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfFalse), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 12),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfFalse), // Jumping to the end
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfTruePeek), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, 12),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfTruePeek), // Jumping to the end
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfFalsePeek), // Jumping to the 6th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 12),
                new Token(TokenType.Instruction, null, VmInstruction.JumpIfFalsePeek), // Jumping to the end
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.IncrementPrefix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.IncrementPostfix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.DecrementPrefix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.DecrementPostfix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Multiply),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 50),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Operator, null, VmInstruction.Divide),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Operator, null, VmInstruction.Modulo),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Add),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 15),
                new Token(TokenType.Value, 7),
                new Token(TokenType.Operator, null, VmInstruction.Subtract),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(8, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestBitwiseAnd()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 0xFF00),
                new Token(TokenType.Value, 0xF000),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseAnd),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 0xF000),
                new Token(TokenType.Value, 0x0F00),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 0xFAE01),
                new Token(TokenType.Value, 0xED10),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseXOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseAnd),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseAnd),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseOr),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseXOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseXOr),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
        public void TestEquals()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Equals),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),

                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 80.0),
                new Token(TokenType.Operator, null, VmInstruction.Equals),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),

                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.Equals),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Unequals),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),

                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 80.0),
                new Token(TokenType.Operator, null, VmInstruction.Unequals),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),

                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.Unequals),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Less),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 80),
                new Token(TokenType.Operator, null, VmInstruction.Less),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.LessOrEquals),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Operator, null, VmInstruction.LessOrEquals),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 71),
                new Token(TokenType.Operator, null, VmInstruction.LessOrEquals),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Greater),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 80),
                new Token(TokenType.Operator, null, VmInstruction.Greater),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 100),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.GreaterOrEquals),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Operator, null, VmInstruction.GreaterOrEquals),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 70),
                new Token(TokenType.Value, 71),
                new Token(TokenType.Operator, null, VmInstruction.GreaterOrEquals),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.LogicalAnd),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.LogicalAnd),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.LogicalAnd),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.LogicalOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, true),
                new Token(TokenType.Operator, null, VmInstruction.LogicalOr),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, false),
                new Token(TokenType.Value, false),
                new Token(TokenType.Operator, null, VmInstruction.LogicalOr),
                new Token(TokenType.MemberName, "c"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.ArithmeticNegate),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
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
                new Token(TokenType.Value, false),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.LogicalNegate),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Logical Negate' instruction is not behaving correctly");
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
                TokenFactory.CreateBoxedValueToken(7),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateVariableToken("i", true),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
                TokenFactory.CreateVariableToken("i", true),
                TokenFactory.CreateBoxedValueToken(100000),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateBoxedValueToken(14),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
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
                TokenFactory.CreateBoxedValueToken(7),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix),
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateInstructionToken(VmInstruction.GetAtAddress),
                TokenFactory.CreateBoxedValueToken(100000),
                TokenFactory.CreateOperatorToken(VmInstruction.Less),
                TokenFactory.CreateBoxedValueToken(15),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfFalse),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
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
    }
}