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

using Xunit;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers.Callables;
using ZScript.Runtime.Typing;

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// General test cases for the FunctionVM class
    /// </summary>
    public class FunctionVmTests
    {
        #region General instructions

        [Fact]
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

            Assert.Equal(10, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"), "The Interrupt instruction did not interrupt the virtual machine as expected");
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"), "The Return instruction did not interrupt the virtual machine as expected");
        }

        [Fact]
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

            Assert.True(functionVm.HasReturnValue, "The Return instruction did not return expected");
            Assert.Equal(10, functionVm.ReturnValue);
            Assert.False(memory.HasVariable("a"), "The Return instruction did not interrupt the virtual machine as expected");
        }

        [Fact]
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

            Assert.Equal(10, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(10, memory.GetVariable("a"));
            Assert.Equal(20, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.ThrowsAny<Exception>(() => functionVm.Execute());
        }

        [Fact]
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

            Assert.Equal(5, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(5, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.ThrowsAny<Exception>(() => functionVm.Execute());
        }

        [Fact]
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

            Assert.Equal(5, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(10, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"));
            Assert.Equal(5, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"));
            Assert.Equal(5, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"));
            Assert.Equal(5, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"));
            Assert.Equal(false, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.False(memory.HasVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
        }

        #endregion

        #region Increment/decrement

        #region Integer

        [Fact]
        public void TestIntegerPrefixIncrement()
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

            Assert.Equal(11, memory.GetVariable("a"));
        }

        [Fact]
        public void TestIntegerPostfixIncrement()
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

            Assert.Equal(10, memory.GetVariable("a"));
        }

        [Fact]
        public void TestIntegerPrefixDecrement()
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

            Assert.Equal(9, memory.GetVariable("a"));
        }

        [Fact]
        public void TestIntegerPostfixDecrement()
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

            Assert.Equal(10, memory.GetVariable("a"));
        }

        #endregion

        #region Long

        [Fact]
        public void TestLongPrefixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
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

            Assert.Equal(11L, memory.GetVariable("a"));
        }

        [Fact]
        public void TestLongPostfixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
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

            Assert.Equal(10L, memory.GetVariable("a"));
        }

        [Fact]
        public void TestLongPrefixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
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

            Assert.Equal(9L, memory.GetVariable("a"));
        }

        [Fact]
        public void TestLongPostfixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
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

            Assert.Equal(10L, memory.GetVariable("a"));
        }

        #endregion

        #region Double

        [Fact]
        public void TestDoublePrefixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0),
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

            Assert.Equal(11.0, memory.GetVariable("a"));
        }

        [Fact]
        public void TestDoublePostfixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0),
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

            Assert.Equal(10.0, memory.GetVariable("a"));
        }

        [Fact]
        public void TestDoublePrefixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0),
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

            Assert.Equal(9.0, memory.GetVariable("a"));
        }

        [Fact]
        public void TestDoublePostfixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10.0),
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

            Assert.Equal(10.0, memory.GetVariable("a"));
        }

        #endregion

        #region Others

        [Fact]
        public void TestUnsignedPrefixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10U),
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

            Assert.Equal(11U, memory.GetVariable("a"));
        }

        [Fact]
        public void TestUnsignedPostfixIncrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10U),
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

            Assert.Equal(10U, memory.GetVariable("a"));
        }

        [Fact]
        public void TestUnsignedPrefixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10U),
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

            Assert.Equal(9U, memory.GetVariable("a"));
        }

        [Fact]
        public void TestUnsignedPostfixDecrement()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10U),
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

            Assert.Equal(10U, memory.GetVariable("a"));
        }

        #endregion

        #endregion

        #region Arithmetic Operations

        [Fact]
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

            Assert.Equal(100, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(5.0, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(0, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(20, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(8, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(false, memory.GetVariable("a"));
            Assert.Equal(false, memory.GetVariable("b"));
            Assert.Equal(true, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
            Assert.Equal(false, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(false, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(false, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
            Assert.Equal(true, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(false, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
            Assert.Equal(false, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(false, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
            Assert.Equal(false, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
            Assert.Equal(false, memory.GetVariable("c"));
        }

        [Fact]
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

            Assert.Equal(-10, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
        }

        #endregion

        #region Type operations

        #region Value/primitive types

        [Fact]
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

            Assert.Equal(true, functionVm.Stack.Peek());
        }

        [Fact]
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

            Assert.Equal(false, functionVm.Stack.Peek());
        }

        [Fact]
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

            Assert.Equal(10, functionVm.Stack.Peek());
        }

        #endregion

        #region Reference types

        [Fact]
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

            Assert.Equal(true, functionVm.Stack.Peek());
        }

        [Fact]
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

        [Fact]
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

            Assert.Throws<InvalidCastException>(() => functionVm.Execute());
        }

        #endregion

        [Fact]
        public void TestTypeCheck()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(new TestDerivedClass()),
                TokenFactory.CreateInstructionToken(VmInstruction.CheckType, typeof(TestBaseClass))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [Fact]
        public void TestFailedTypeCheck()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(new Object()),
                TokenFactory.CreateInstructionToken(VmInstruction.CheckType, typeof(TestBaseClass))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null, null, null, new TypeProvider()); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            Assert.ThrowsAny<Exception>(() => functionVm.Execute());
        }

        #endregion

        #region Bitwise Operations

        [Fact]
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

            Assert.Equal(0xF000, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(0xFF00, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(1000209, memory.GetVariable("a"));
        }

        [Fact]
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

            Assert.Equal(false, memory.GetVariable("a"));
            Assert.Equal(true, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(false, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(true, memory.GetVariable("a"));
            Assert.Equal(false, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(4, memory.GetVariable("a"));
            Assert.Equal(20, memory.GetVariable("b"));
        }

        [Fact]
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

            Assert.Equal(16, memory.GetVariable("a"));
            Assert.Equal(10, memory.GetVariable("b"));
        }

        #endregion

        #region Manual memory addressing

        /// <summary>
        /// Tests addressing of memory via manual indexing
        /// </summary>
        [Fact]
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
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 4),
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

            //Assert.AreEqual(100, memory.GetVariable("a"));
        }

        /// <summary>
        /// Tests addressing of memory via manual indexing
        /// </summary>
        [Fact]
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
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 4),
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

            //Assert.AreEqual(100, memory.GetVariable("a"));
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Tests the NumberTypeForBoxedNumber method
        /// </summary>
        [Fact]
        public void TestNumberTypeForBoxedNumber()
        {
            Assert.Equal(FunctionVM.NumberType.Byte, FunctionVM.NumberTypeForBoxedNumber((byte)0));
            Assert.Equal(FunctionVM.NumberType.SByte, FunctionVM.NumberTypeForBoxedNumber((sbyte)0));

            Assert.Equal(FunctionVM.NumberType.Short, FunctionVM.NumberTypeForBoxedNumber((short)0));
            Assert.Equal(FunctionVM.NumberType.UShort, FunctionVM.NumberTypeForBoxedNumber((ushort)0));

            Assert.Equal(FunctionVM.NumberType.UShort, FunctionVM.NumberTypeForBoxedNumber((ushort)0));

            Assert.Equal(FunctionVM.NumberType.Integer, FunctionVM.NumberTypeForBoxedNumber(0));
            Assert.Equal(FunctionVM.NumberType.UInteger, FunctionVM.NumberTypeForBoxedNumber(0U));

            Assert.Equal(FunctionVM.NumberType.Long, FunctionVM.NumberTypeForBoxedNumber(0L));
            Assert.Equal(FunctionVM.NumberType.ULong, FunctionVM.NumberTypeForBoxedNumber(0UL));

            Assert.Equal(FunctionVM.NumberType.Decimal, FunctionVM.NumberTypeForBoxedNumber(0M));

            Assert.Equal(FunctionVM.NumberType.Float, FunctionVM.NumberTypeForBoxedNumber(0.0f));
            Assert.Equal(FunctionVM.NumberType.Double, FunctionVM.NumberTypeForBoxedNumber(0.0));

            Assert.Equal(FunctionVM.NumberType.Unspecified, FunctionVM.NumberTypeForBoxedNumber(true));
        }

        /// <summary>
        /// Tests the IsCallable method
        /// </summary>
        [Fact]
        public void TestIsCallable()
        {
            // Test valid inputs
            Assert.True(FunctionVM.IsCallable(new ZFunction(null, null, null)));
            Assert.True(FunctionVM.IsCallable(new ZClassMethod(null, null)));
            Assert.True(FunctionVM.IsCallable(new ClassMethod(null, null)));

            // Test some junk data
            Assert.False(FunctionVM.IsCallable(null));
            Assert.False(FunctionVM.IsCallable(false));
            Assert.False(FunctionVM.IsCallable(10));
        }

        #endregion

        #region Error raising

        /// <summary>
        /// Tests exception raising when trying to fetch a return value from a function VM that didn't hit a ret instruction
        /// </summary>
        [Fact]
        public void TestNoReturnValue()
        {
            var vm = new FunctionVM(new TokenList(), new VmContext(new Memory(), null, null));

            Assert.Throws<InvalidOperationException>(() => vm.ReturnValue);
        }

        [Fact]
        public void TestInvalidOperation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateOperatorToken(VmInstruction.Noop),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            Assert.Throws<ArgumentException>(() => functionVm.Execute());
        }

        [Fact]
        public void TestInvalidInstruction()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Add),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            Assert.Throws<ArgumentException>(() => functionVm.Execute());
        }

        [Fact]
        public void TestInvalidGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("Thing1"),
                TokenFactory.CreateBoxedValueToken("Thing2"),
                TokenFactory.CreateInstructionToken(VmInstruction.Get),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            Assert.Throws<VirtualMachineException>(() => functionVm.Execute());
        }

        [Fact]
        public void TestInvalidSet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("Thing1"),
                TokenFactory.CreateBoxedValueToken("Thing2"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            Assert.Throws<VirtualMachineException>(() => functionVm.Execute());
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