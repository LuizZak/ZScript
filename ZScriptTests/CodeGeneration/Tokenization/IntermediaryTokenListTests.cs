using System;
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
    }
}