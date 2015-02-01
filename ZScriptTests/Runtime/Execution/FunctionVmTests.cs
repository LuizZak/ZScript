﻿using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Noop),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Instruction, null, VmInstruction.Noop)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Interrupt),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.Value, 20),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [TestMethod]
        public void TestGet()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Get),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.ClearStack),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }

        [TestMethod]
        public void TestSwapStack()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Instruction, null, VmInstruction.Swap), // Swapping the stack should get the '5' at the top, making the Set instruction set a as 5
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(5, memory.GetVariable("a"), "The memory should contain the variables that were set by the instructions");
        }

        [TestMethod]
        public void TestJump()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Instruction, null, VmInstruction.Jump), // Jumping to the 5th instruction should skip to the 'b' variable set instructions
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "b"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.IncrementPrefix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.IncrementPostfix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.DecrementPrefix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.DecrementPostfix),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Multiply),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 50),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Operator, null, VmInstruction.Divide),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Operator, null, VmInstruction.Modulo),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, 10),
                new Token(TokenType.Operator, null, VmInstruction.Add),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 15),
                new Token(TokenType.Value, 7),
                new Token(TokenType.Operator, null, VmInstruction.Subtract),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 0xFF00),
                new Token(TokenType.Value, 0xF000),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseAnd),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 0xF000),
                new Token(TokenType.Value, 0x0F00),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 0xFAE01),
                new Token(TokenType.Value, 0xED10),
                new Token(TokenType.Operator, null, VmInstruction.BitwiseXOr),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(1000209, memory.GetVariable("a"), "The memory should contain the variable that was set by the instructions");
        }

        [TestMethod]
        public void TestEquals()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
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

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.UnaryNegate),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
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
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, false),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Operator, null, VmInstruction.LogicalNegate),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Instruction, null, VmInstruction.Set)
            };

            var tokenList = new TokenList { Tokens = t.ToArray() };
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(true, memory.GetVariable("a"), "The 'Logical Negate' instruction is not behaving correctly");
        }

        #endregion
    }
}