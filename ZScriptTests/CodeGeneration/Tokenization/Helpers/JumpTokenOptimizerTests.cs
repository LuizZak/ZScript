﻿#region License information
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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Tokenization;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Tests the JumpTokenExpander class and related methods
    /// </summary>
    [TestClass]
    public class JumpTokenOptimizerTests
    {
        [TestMethod]
        public void TestSequentialJumpOptimizing()
        {
            var jump1 = new JumpToken(null);
            var jump2 = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                jump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            // Point the first jump to the second, and the second jump immediately forward instruction
            jump1.TargetToken = jump2;
            jump2.TargetToken = tokens[2];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestSequentialJumpWithNonSequentialJumpExpanding()
        {
            var inter = TokenFactory.CreateInstructionToken(VmInstruction.Interrupt);
            var jump2 = new JumpToken(inter);
            var jump1 = new JumpToken(jump2);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                jump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                inter
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestImmediateJumpExpanding()
        {
            var jump1 = new JumpToken(null);
            var inter = TokenFactory.CreateInstructionToken(VmInstruction.Interrupt);
            var jump2 = new JumpToken(inter);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                inter
            };

            // Point the first jump to the second, and the second jump to the 'set a' instruction
            jump1.TargetToken = tokens[1];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt)
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestImmediatePeekingJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, true, false);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set a' instruction
            jump1.TargetToken = tokens[1];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestSequentialEquivalentJumpExclusion()
        {
            var tJump1 = new JumpToken(null);
            var tJump2 = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tJump1,
                tJump2,
            };

            // Point the first jump to the second, and the second jump to the 'set a' instruction
            tJump1.TargetToken = tokens[0];
            tJump2.TargetToken = tokens[0];

            var eJump1 = new JumpToken(null);
            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJump1
            };

            eJump1.TargetToken = expectedTokens[0];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestChainedJumpOptimizing()
        {
            // Create tokens to be optimized
            var tJump1 = new JumpToken(null);
            var tJump2 = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                tJump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tJump2,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(12),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            tJump1.TargetToken = tJump2;
            tJump2.TargetToken = tokens[8];
            
            // Create expected tokens
            var eJump1 = new JumpToken(null);
            var eJump2 = new JumpToken(null);

            var expectedTokens = new IntermediaryTokenList
            {
                eJump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJump2,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(12),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            eJump1.TargetToken = expectedTokens[8];
            eJump2.TargetToken = expectedTokens[8];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            //Assert.AreEqual(10, tokens[0].TokenObject, "The address for the jump that was created is not valid");
            //Assert.AreEqual(VmInstruction.Jump, tokens[1].Instruction, "The address for the jump that was created is not valid");

            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "Failed to generate expected tokens");
        }

        [TestMethod]
        public void TestChainedPeekJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, true, false);
            var jump2 = new JumpToken(null, true, true, false);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("fake_false"),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken("fake_true"),
                jump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = jump2;
            jump2.TargetToken = tokens[10];

            var eJump1 = new JumpToken(null, true, true, false);
            var eJump2 = new JumpToken(null, true, true, false);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("fake_false"),
                eJump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken("fake_true"),
                eJump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            eJump1.TargetToken = expectedTokens[10];
            eJump2.TargetToken = expectedTokens[10];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumpPointing(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestChainedUnequalPeekJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, false, false);
            var jump2 = new JumpToken(null, true, true, false);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("fake_true"),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken("fake_true"),
                jump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = jump2;
            jump2.TargetToken = tokens[10];

            var eJump1 = new JumpToken(null, true, false, false);
            var eJump2 = new JumpToken(null, true, true, false);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken("fake_true"),
                eJump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken("fake_true"),
                eJump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            eJump1.TargetToken = expectedTokens[7];
            eJump2.TargetToken = expectedTokens[10];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumpPointing(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        [TestMethod]
        public void TestUnconditionalJumpToReturnOptimization()
        {
            var ret = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump1 = new JumpToken(ret);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                ret
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            // Now verify the results
            Assert.AreEqual(VmInstruction.Ret, tokens[0].Instruction, "Unconditional jumps that point to returns should be replaced with returns themselves");
        }

        [TestMethod]
        public void TestChainedUnconditionalJumpToReturnOptimization()
        {
            var ret = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump2 = new JumpToken(ret);
            var jump1 = new JumpToken(jump2);

            var tokens = new IntermediaryTokenList
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                ret
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            // Now verify the results
            Assert.AreEqual(VmInstruction.Ret, tokens[0].Instruction, "Chained unconditional jumps that point to returns should be replaced with returns themselves");
        }

        [TestMethod]
        public void TestAlwaysFalseConditionalJumpOptimization()
        {
            var target = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump1 = new JumpToken(target, true, false);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                target
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            // Now verify the results
            Assert.AreEqual(0, tokens.Count(t => t.Instruction == VmInstruction.JumpIfFalse), "Jumps that are detected to never happen should be removed completely");
        }

        [TestMethod]
        public void TestConstantFalseJumpOptimization()
        {
            var tTarget = new JumpTargetToken();
            var tJump = new JumpToken(tTarget, true, false);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(false),
                tJump,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tTarget,
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var eJump = new JumpToken(null);

            var expectedTokens = new IntermediaryTokenList
            {
                eJump,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            eJump.TargetToken = expectedTokens[4];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests optimization of jump pointing when a jump points to an always-failing conditional jump
        /// </summary>
        [TestMethod]
        public void TestJumpPointedAtFailedConditionalJump()
        {
            var jump1 = new JumpToken(null, true, false);
            var jump2 = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            jump1.TargetToken = tokens[6];
            jump2.TargetToken = tokens[0];

            // Expected tokens
            var eJump1 = new JumpToken(null);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJump1,
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            eJump1.TargetToken = expectedTokens[0];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests optimization of jump pointing when a jump points to an always-failing conditional jump
        /// </summary>
        [TestMethod]
        public void TestJumpPointedAtSuceededConditionalJump()
        {
            var jump1 = new JumpToken(null, true);
            var jump2 = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(true),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            jump1.TargetToken = tokens[6];
            jump2.TargetToken = tokens[0];

            // Expected tokens
            var eJump1 = new JumpToken(null);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            eJump1.TargetToken = expectedTokens[0];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests re-forwarding of jumps when always-true conditional jumps are optimized
        /// </summary>
        [TestMethod]
        public void TestConditionalJumpToJumpTargetToConditionalJumpOptimization()
        {
            /*
                0000000: a
                0000001: [11 JUMPIfFalse]
                0000002: c
                0000003: [8 JUMPIfFalse]
                0000004: b
                0000005: 0
                0000006: Greater
                0000007: [9 JUMP]
                0000008: False
                0000009: JUMP_TARGET 
                0000010: [12 JUMP]
                0000011: False
                0000012: JUMP_TARGET 
                0000013: [16 JUMPIfFalse]
                0000014: d
                0000015: [17 JUMP]
                0000016: False
                0000017: JUMP_TARGET 
                0000018: a
                0000019: Set
                0000020: ClearStack
            */

            var tTarget1 = new JumpTargetToken();
            var tJump1 = new JumpToken(tTarget1, true, false);

            var tTarget2 = new JumpTargetToken();
            var tJump2 = new JumpToken(tTarget2, true, false);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                tJump1,
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tTarget1,
                TokenFactory.CreateBoxedValueToken(false),
                tJump2,
                TokenFactory.CreateBoxedValueToken(6),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tTarget2,
                TokenFactory.CreateBoxedValueToken(7),
                TokenFactory.CreateMemberNameToken("d"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var eTarget2 = TokenFactory.CreateBoxedValueToken(7);
            var eJump1 = new JumpToken(eTarget2, true, false);
            var eJump2 = new JumpToken(eTarget2);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                eJump1,
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJump2,
                TokenFactory.CreateBoxedValueToken(6),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eTarget2,
                TokenFactory.CreateMemberNameToken("d"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests optimizing out conditional jumps that precede and point to the same interrupt-type instructions
        /// </summary>
        [TestMethod]
        public void TestDeadEndConditionalPeekingJump()
        {
            var jump = new JumpToken(null, true, true, false);

            var tokens = new IntermediaryTokenList
            {
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            jump.TargetToken = tokens[5];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests failed optimization of dead-ended conditional peeking jumps because they precede and point to different type of interrupt instructions
        /// </summary>
        [TestMethod]
        public void TestFailedDeadEndConditionalPeekingJump()
        {
            var jump = new JumpToken(null, true, true, false);

            var tokens = new IntermediaryTokenList
            {
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            jump.TargetToken = tokens[5];

            var jump2 = new JumpToken(null, true, true, false);

            var expectedTokens = new IntermediaryTokenList
            {
                jump2,
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            jump2.TargetToken = expectedTokens[5];

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests optimizing a conditional jump that jumps over an immediate jump token by replacing both jumps with a reversed conditional jump
        /// </summary>
        [TestMethod]
        public void TestConditionalJumpOverUnconditionalJumpHopping()
        {
            var tJt1 = TokenFactory.CreateBoxedValueToken(12);

            var tJump1 = new JumpToken(null, true);
            var tJump2 = new JumpToken(tJt1);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                tJump1,
                tJump2,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tJt1,
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            tJump1.TargetToken = tokens[3];

            var eJt1 = TokenFactory.CreateBoxedValueToken(12);

            var eJump1 = new JumpToken(eJt1, true, false);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                eJump1,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJt1,
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }

        /// <summary>
        /// Tests optimizing a conditional jump that jumps over an immediate jump token by replacing both jumps with a reversed conditional jump.
        /// This unit test covers a Jump If False case
        /// </summary>
        [TestMethod]
        public void TestReverseConditionalJumpOverUnconditionalJumpHopping()
        {
            var tJt1 = TokenFactory.CreateBoxedValueToken(12);

            var tJump1 = new JumpToken(null, true, false);
            var tJump2 = new JumpToken(tJt1);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                tJump1,
                tJump2,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tJt1,
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            tJump1.TargetToken = tokens[3];

            var eJt1 = TokenFactory.CreateBoxedValueToken(12);

            var eJump1 = new JumpToken(eJt1, true);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                eJump1,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                eJt1,
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Expand the jumps
            JumpTokenOptimizer.OptimizeJumps(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The jump optimizer failed to produce the expected results");
        }
    }
}