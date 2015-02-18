using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    [TestClass]
    public class IntermediaryTokenListTests
    {
        #region JumpToken/JumpTarget expanding

        /// <summary>
        /// Tests jump expanding on the intermediate token list
        /// </summary>
        [TestMethod]
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
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expanded = tokens.CreateExpandedTokenList();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [TestMethod]
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
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Bind jump targets
            tokens.BindJumpTargets();

            var expanded = tokens.CreateExpandedTokenList();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [TestMethod]
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
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Bind jump targets
            tokens.BindJumpTargets();

            var expanded = tokens.CreateExpandedTokenList();

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(expanded);

            // Now verify the results
            TestUtils.AssertTokenListEquals(expectedTokens, expanded, "The intermediary token list failed to produce the expected results");
        }

        [TestMethod]
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

            var expanded = tokens.CreateExpandedTokenList();

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
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, true, true, true, true, true };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests a basic linear reachability detection in a list of tokens, starting from a specified instruction index
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability(2);

            var expected = new[] { false, false, true, true, true, true };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by an interrupt instruction
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new [] { true, true, true, true, false, false, false };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by an interrupt instruction right at the first step
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, false, false, false, false, false, false };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by interruption of the sweeping by a return instruction
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, true, true, true, false, false, false };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding an unconditional jump over a set of tokens
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, true, true, false, false, true, true };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding a conditional jump over a set of tokens
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, true, true, true, true, true, true };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        /// <summary>
        /// Tests detection of unreachable tokens by adding a conditional jump over a set of tokens, and an interrupt in the middle of the token list as well
        /// </summary>
        [TestMethod]
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

            var reachable = tokens.DetectReachability();

            var expected = new[] { true, true, true, true, true, true, false, false, true, true, true };

            Assert.IsTrue(expected.SequenceEqual(reachable));
        }

        #endregion

        #region Unreachable code removal

        /// <summary>
        /// Tests the unreachable code detection and removal with a plain list of tokens that are all reachable
        /// </summary>
        [TestMethod]
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

            tokens.RemoveUnreachableTokens();

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
        [TestMethod]
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

            tokens.RemoveUnreachableTokens();

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
        [TestMethod]
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

            tokens.RemoveUnreachableTokens();

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

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Trying to remove tokens with IntermediaryTokenList.Remove() must raise an InvalidOperationException")]
        public void TestRemoveException()
        {
            var jumpToken = new JumpToken(null);

            // ReSharper disable once CollectionNeverQueried.Local
            var tokens = new IntermediaryTokenList { jumpToken };

            tokens.Remove(jumpToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to get the offset for a jump not in the called IntermediaryTokenList must raise an ArgumentException")]
        public void TestJumpNotInListException()
        {
            var jumpToken = new JumpToken(null);

            // ReSharper disable once CollectionNeverQueried.Local
            var tokens = new IntermediaryTokenList();

            tokens.OffsetForJump(jumpToken);
        }

        /// <summary>
        /// Tests a basic linear reachability detection in a list of tokens
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Trying to start an unreachable code detection with an out of bounds index must raise an ArgumentOutOfRangeException exception")]
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

            tokens.DetectReachability(-1);
        }

        #endregion
    }
}