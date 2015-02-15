using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScriptTests.Runtime;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Tokenizers.Helpers
{
    /// <summary>
    /// Tests the JumpTokenExpander class and related methods
    /// </summary>
    [TestClass]
    public class JumpTokenExpanderTests
    {
        [TestMethod]
        public void TestJumpExpanding()
        {
            var jumpToken = new JumpToken(null);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(5, tokens[0].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.Jump, tokens[1].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestJumpTargetExpanding()
        {
            var jumpTarget = new JumpTargetToken();
            var jumpToken = new JumpToken(jumpTarget);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(5, tokens[0].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.Jump, tokens[1].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestMultipleJumpTargetExpanding()
        {
            var jumpTarget = new JumpTargetToken();
            var jumpToken = new JumpToken(jumpTarget);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(5, tokens[0].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.Jump, tokens[1].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestSequentialJumpExpanding()
        {
            var jump1 = new JumpToken(null);
            var jump2 = new JumpToken(null);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(3, tokens.Count, "The sequential immediate jumps where not optimized out");
        }

        [TestMethod]
        public void TestSequentialJumpWithNonSequentialJumpExpanding()
        {
            var inter = TokenFactory.CreateInstructionToken(VmInstruction.Interrupt);
            var jump2 = new JumpToken(inter);
            var jump1 = new JumpToken(jump2);

            List<Token> tokens = new List<Token>
            {
                jump1,
                jump2,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                inter
            };

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(6, tokens.Count, "The sequential immediate jumps where not correctly optimized out");
        }

        [TestMethod]
        public void TestImmediateJumpExpanding()
        {
            var jump1 = new JumpToken(null);
            var inter = TokenFactory.CreateInstructionToken(VmInstruction.Interrupt);
            var jump2 = new JumpToken(inter);

            List<Token> tokens = new List<Token>
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                inter
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = tokens[1];

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.IsFalse(tokens.Any(t => t.Instruction == VmInstruction.Jump), "Jump instructions that point immediately forward should be removed during optimization");
        }

        [TestMethod]
        public void TestImmediatePeekingJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, true, false);

            List<Token> tokens = new List<Token>
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = tokens[1];

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreNotEqual(VmInstruction.JumpIfTruePeek, tokens[1].Instruction, "Jump instructions that point immediately forward should be removed during optimization");
        }

        [TestMethod]
        public void TestSequentialEquivalentJumpExclusion()
        {
            var jump1 = new JumpToken(null);
            var jump2 = new JumpToken(null);

            List<Token> tokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump1,
                jump2,
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = tokens[0];
            jump2.TargetToken = tokens[0];

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(1, tokens.Count(t => t.Instruction == VmInstruction.Jump),
                "Sequential jump instructions that point to the same location should be optimized out to a single jump");
        }

        [TestMethod]
        public void TestChainedJumpExpanding()
        {
            var jump1 = new JumpToken(null);
            var jump2 = new JumpToken(null);

            List<Token> tokens = new List<Token>
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                jump2,
                TokenFactory.CreateBoxedValueToken(11),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(12),
                TokenFactory.CreateMemberNameToken("c"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Point the first jump to the second, and the second jump to the 'set c' instruction
            jump1.TargetToken = jump2;
            jump2.TargetToken = tokens[8];

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(10, tokens[0].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.Jump, tokens[1].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestChainedPeekJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, true, false);
            var jump2 = new JumpToken(null, true, true, false);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(12, tokens[1].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.JumpIfTruePeek, tokens[2].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestChainedUnequalPeekJumpExpanding()
        {
            var jump1 = new JumpToken(null, true, false, false);
            var jump2 = new JumpToken(null, true, true, false);

            List<Token> tokens = new List<Token>
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

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(9, tokens[1].TokenObject, "The address for the jump that was created is not valid");
            Assert.AreEqual(VmInstruction.JumpIfFalsePeek, tokens[2].Instruction, "The address for the jump that was created is not valid");
        }

        [TestMethod]
        public void TestUnconditionalJumpToReturnOptimization()
        {
            var ret = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump1 = new JumpToken(ret);

            List<Token> tokens = new List<Token>
            {
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                ret
            };

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(VmInstruction.Ret, tokens[0].Instruction, "Unconditional jumps that point to returns should be replaced with returns themselves");
        }

        [TestMethod]
        public void TestChainedUnconditionalJumpToReturnOptimization()
        {
            var ret = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump2 = new JumpToken(ret);
            var jump1 = new JumpToken(jump2);

            List<Token> tokens = new List<Token>
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
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(VmInstruction.Ret, tokens[0].Instruction, "Chained unconditional jumps that point to returns should be replaced with returns themselves");
        }

        [TestMethod]
        public void TestAlwaysFalseConditionalJumpOptimization()
        {
            var target = TokenFactory.CreateInstructionToken(VmInstruction.Ret);
            var jump1 = new JumpToken(target, true, false);

            List<Token> tokens = new List<Token>
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
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            Assert.AreEqual(0, tokens.Count(t => t.Instruction == VmInstruction.JumpIfFalse), "Jumps that are detected to never happen should be removed completely");
        }

        [TestMethod]
        public void TestConstantFalseJumpOptimization()
        {
            var target = new JumpTargetToken();
            var jump1 = new JumpToken(target, true, false);

            var tokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(false),
                jump1,
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                target,
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateInstructionToken(VmInstruction.Jump),
                TokenFactory.CreateBoxedValueToken(10),
                TokenFactory.CreateMemberNameToken("a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateBoxedValueToken(5),
                TokenFactory.CreateMemberNameToken("b"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            // Expand the jumps
            JumpTokenExpander.ExpandInList(tokens);

            // Now verify the results
            TestUtils.AssertTokenListEquals(tokens, expectedTokens, "The jump expander failed to produce the expected results");
        }
    }
}