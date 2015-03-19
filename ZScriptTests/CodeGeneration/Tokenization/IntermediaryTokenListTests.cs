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
using System.Linq;

using Xunit;

using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenization
{
    /// <summary>
    /// Tests the functionality of the IntermediateTokenList class and related components
    /// </summary>
    public class IntermediaryTokenListTests
    {
        /// <summary>
        /// Tests automatic removal of 'noop' instruction tokens
        /// </summary>
        [Fact]
        public void TestNoopRemoval()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Noop),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Noop),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens.ToTokenList().Tokens, "The noop removal failed to behave as expected");
        }

        /// <summary>
        /// Tests automatic removal of sequential ClearStack instruction tokens
        /// </summary>
        [Fact]
        public void TestClearStackRemoval()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack)
            };

            var actual = tokens.ToTokenList().Tokens;

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(actual);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, actual, "The sequential ClearStack removal failed to behave as expected");
        }

        /// <summary>
        /// Tests removal of sequential return instructions from a token list
        /// </summary>
        [Fact]
        public void TestSequentialReturnRemoval()
        {
            var tJ1 = new JumpToken(null, true);
            var tJ2 = new JumpToken(null, true);
            var tJ3 = new JumpToken(null, true);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                tJ1,
                TokenFactory.CreateMemberNameToken("b"),
                tJ2,
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                tJ3,
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            tJ1.TargetToken = tokens[7];
            tJ2.TargetToken = tokens[8];
            tJ3.TargetToken = tokens[9];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 7),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 7),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.JumpIfTrue, 7),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            var actual = tokens.ToTokenList().Tokens;

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(actual);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, actual, "The sequential Ret removal failed to behave as expected");
        }

        #region JumpToken/JumpTarget expanding

        /// <summary>
        /// Tests jump expanding on the intermediate token list
        /// </summary>
        [Fact]
        public void TestJumpExpanding()
        {
            var jumpToken = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                jumpToken,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the jump at the 'b' variable set instructions
            jumpToken.TargetToken = tokens[4];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expanded = IntermediaryTokenList.CreateExpandedTokenList(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [Fact]
        public void TestJumpTargetExpanding()
        {
            var jumpTarget = new JumpTargetToken();
            var jumpToken = new JumpToken(jumpTarget);

            var tokens = new IntermediaryTokenList
            {
                jumpToken,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jumpTarget,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the jump at the 'b' variable set instructions
            jumpToken.TargetToken = tokens[4];

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Bind jump targets
            tokens.BindJumpTargets();

            var expanded = IntermediaryTokenList.CreateExpandedTokenList(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [Fact]
        public void TestMultipleJumpTargetExpanding()
        {
            var jumpTarget = new JumpTargetToken();
            var jumpToken = new JumpToken(jumpTarget);

            var tokens = new IntermediaryTokenList
            {
                jumpToken,
                TokenFactory.CreateBoxedValueToken(10),
                new JumpTargetToken(),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jumpTarget,
                new JumpTargetToken(),
                new JumpTargetToken(),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Jump, 4),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Bind jump targets
            tokens.BindJumpTargets();

            var expanded = IntermediaryTokenList.CreateExpandedTokenList(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [Fact]
        public void TestEndTargetlessJumpTargetExpanding()
        {
            var jumpTarget = new JumpTargetToken();

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jumpTarget,
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Bind jump targets
            tokens.BindJumpTargets(VmInstruction.Interrupt);

            var expanded = IntermediaryTokenList.CreateExpandedTokenList(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        #endregion

        #region Unreachable code detection

        /// <summary>
        /// Tests a basic linear reachability detection in a list of tokens
        /// </summary>
        [Fact]
        public void TestLinearReachabilityDetection()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, true, true, true, true, true };
            
            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests a basic linear reachability detection in a list of tokens, starting from a specified instruction index
        /// </summary>
        [Fact]
        public void TestLinearReachabilityDetectionWithOffset()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var reachable = IntermediaryTokenList.DetectReachability(tokens, 2);

            var expected = new[] { false, false, true, true, true, true };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by an interrupt instruction
        /// </summary>
        [Fact]
        public void TestInterruptReachabilityDetection()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt), // Flow interrupted!
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new [] { true, true, true, true, false, false, false };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by an interrupt instruction right at the first step
        /// </summary>
        [Fact]
        public void TestFirstInterruptReachabilityDetection()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt), // Flow interrupted!
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, false, false, false, false, false, false };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by a return instruction
        /// </summary>
        [Fact]
        public void TestRetReachabilityDetection()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret), // Flow interrupted!
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, true, true, true, false, false, false };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding an unconditional jump over a set of tokens
        /// </summary>
        [Fact]
        public void TestJumpReachabilityDetection()
        {
            var jump = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            jump.TargetToken = tokens[5]; // Jump over the 'Set' and '10' tokens

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, true, true, false, false, true, true };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding a conditional jump over a set of tokens
        /// </summary>
        [Fact]
        public void TestConditionalJumpReachabilityDetection()
        {
            var jump = new JumpToken(null, true);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            jump.TargetToken = tokens[5]; // Jump over the 'Set' and '10' tokens

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, true, true, true, true, true, true };

            Assert.True(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding a conditional jump over a set of tokens, and an interrupt in the middle of the token list as well
        /// </summary>
        [Fact]
        public void TestConditionalJumpWithInterruptReachabilityDetection()
        {
            var jump = new JumpToken(null, true);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateInstructionToken(VmInstruction.Interrupt), // Interrupt!
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            jump.TargetToken = tokens[8]; // Jump over to the '10 c set' portion of the token list

            var reachable = IntermediaryTokenList.DetectReachability(tokens);

            var expected = new[] { true, true, true, true, true, true, false, false, true, true, true };

            Assert.True(expected.SequenceEqual(reachable));
        }

        #endregion

        #region Unreachable code removal

        /// <summary>
        /// Tests the unreachable code detection and removal with a plain list of tokens that are all reachable
        /// </summary>
        [Fact]
        public void TestLinearUnreachableCodeRemoval()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            IntermediaryTokenList.RemoveUnreachableTokens(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The unreachable code removal failed to behave as expected");
        }

        /// <summary>
        /// Tests the unreachable code detection and removal with a simple break in the control flow with a return statement
        /// </summary>
        [Fact]
        public void TestReturnUnreachableCodeRemoval()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Ret),
            };

            IntermediaryTokenList.RemoveUnreachableTokens(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The unreachable code removal failed to behave as expected");
        }

        /// <summary>
        /// Tests the unreachable code detection and removal with a jump in the control flow
        /// </summary>
        [Fact]
        public void TestJumpUnreachableCodeRemoval()
        {
            var jump = new JumpToken(null);

            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                jump,
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            jump.TargetToken = tokens[5]; // Jump to the 'b set' instruction

            var eJump = new JumpToken(null);

            var expectedTokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                eJump,
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            eJump.TargetToken = expectedTokens[3];

            IntermediaryTokenList.RemoveUnreachableTokens(tokens);

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, tokens, "The unreachable code removal failed to behave as expected");
        }

        #endregion

        #region Exception checking

        [Fact]
        public void TestRemoveException()
        {
            var jumpToken = new JumpToken(null);

            // ReSharper disable once CollectionNeverQueried.Local
            var tokens = new IntermediaryTokenList { jumpToken };

            Assert.Throws<InvalidOperationException>(() => tokens.Remove(jumpToken));
        }

        [Fact]
        public void TestJumpNotInListException()
        {
            var jumpToken = new JumpToken(null);

            // ReSharper disable once CollectionNeverQueried.Local
            var tokens = new IntermediaryTokenList();

            Assert.Throws<ArgumentException>(() => tokens.OffsetForJump(jumpToken));
        }

        /// <summary>
        /// Tests a basic linear reachability detection in a list of tokens
        /// </summary>
        [Fact]
        public void TestDetectReachabilityRangeCheck()
        {
            var tokens = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => IntermediaryTokenList.DetectReachability(tokens, -1));
        }

        #endregion
    }
}