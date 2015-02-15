using System;
using System.Collections.Generic;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Class responsible for expanding jump tokens into address and instruction tokens
    /// </summary>
    public class JumpTokenExpander
    {
        /// <summary>
        /// Expands the jump tokens associated with the given token list
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        public static void ExpandInList(List<Token> tokens)
        {
            BindJumpTargets(tokens, false, VmInstruction.Noop);
            ExpandJumpTokens(tokens);
        }

        /// <summary>
        /// Expands the jump tokens associated with the given token list, expanding the last jump target instruction at the end of the token list,
        /// if it is present, as a given instruction
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        /// <param name="endJumpTargetInstruction">The instruction to expand the last jump target as</param>
        public static void ExpandInList(List<Token> tokens, VmInstruction endJumpTargetInstruction)
        {
            BindJumpTargets(tokens, true, endJumpTargetInstruction);
            RemoveSequentialInterrupts(tokens);
            ExpandJumpTokens(tokens);
        }

        /// <summary>
        /// Replaces the jump target tokens on the given list
        /// </summary>
        /// <param name="tokens">The list of jump target tokens to replace</param>
        /// <param name="replaceJumpTargetsAtEnd">
        /// Whether to expand jump targets at the end of the list of tokens as an instruction token specified by the endJumpTargetInstruction parameter
        /// </param>
        /// <param name="endJumpTargetInstruction">The instruction to expand the last jump target as</param>
        static void BindJumpTargets(IList<Token> tokens, bool replaceJumpTargetsAtEnd, VmInstruction endJumpTargetInstruction)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == tokens.Count - 1 && !replaceJumpTargetsAtEnd)
                    break;

                var token = tokens[i];

                if (!(token is JumpTargetToken))
                    continue;

                Token newTarget;
                bool endJump = false;

                if (i == tokens.Count - 1)
                {
                    // Expand this jump token as an interrupt
                    newTarget = TokenFactory.CreateInstructionToken(endJumpTargetInstruction);
                    tokens.Add(newTarget);

                    endJump = true;
                }
                else
                {
                    newTarget = tokens[i + 1];
                }

                bool hasSource = false;
                // Find all jump tokens that are pointing to this jump target token
                foreach (Token t in tokens)
                {
                    var jumpToken = t as JumpToken;
                    if (jumpToken != null && ReferenceEquals(jumpToken.TargetToken, token))
                    {
                        jumpToken.TargetToken = newTarget; // Token next to the target token
                        hasSource = true;
                    }
                }

                // If the target has no source, exclude the newly created target
                if (!hasSource && endJump)
                {
                    tokens.Remove(newTarget);
                }

                // Remove jump target token
                tokens.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Expands the jump tokens on the given list, replacing them with couples of address tokens and jump instruction tokens
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        /// <exception cref="Exception">One of the jump tokens points to a target token that is not inside the same token list</exception>
        static void ExpandJumpTokens(List<Token> tokens)
        {
            // Optimize the jump flow
            OptimizeJumpPointing(tokens);

            // Iterate over jump tokens and add jump instructions in front of them
            for (int i = 0; i < tokens.Count; i++)
            {
                var jumpToken = tokens[i] as JumpToken;
                // If the jump is pointed at a jump target token, skip the expansion
                if (jumpToken == null || jumpToken.TargetToken is JumpTargetToken)
                    continue;

                AnalyzeJump(jumpToken);

                // Add a jump token in front of the jump token
                Token t = TokenFactory.CreateInstructionToken(InstructionForJumpToken(jumpToken));
                tokens.Insert(i + 1, t);

                // Skip over the jump instruction token that was just added
                i++;
            }

            // Iterate again the jump tokens, now fixing the address of the token pointing
            for (int i = 0; i < tokens.Count; i++)
            {
                var jumpToken = tokens[i] as JumpToken;
                // If the jump is pointed at a jump target token, skip the expansion
                if (jumpToken == null || jumpToken.TargetToken is JumpTargetToken)
                    continue;
                
                // Find address of jump
                int address = OffsetForJump(tokens, jumpToken);
                if(address == -1)
                    throw new Exception("A jump token has a target that is not contained within the same token list");

                var newToken = TokenFactory.CreateBoxedValueToken(address);

                // Replace any jump reference that may be pointing to this jump token
                foreach (Token t in tokens)
                {
                    // If this jump token is unconditional, just point the other token to this token's target
                    var token = t as JumpToken;
                    if (token != null && ReferenceEquals(token.TargetToken, jumpToken))
                    {
                        token.TargetToken = newToken;
                    }
                }

                tokens[i] = newToken;
            }
        }

        /// <summary>
        /// Analyzes a given jump token for errors
        /// </summary>
        /// <param name="jumpToken">The jump token</param>
        // ReSharper disable once UnusedParameter.Local
        static void AnalyzeJump(JumpToken jumpToken)
        {
            if (ReferenceEquals(jumpToken.TargetToken, jumpToken))
                throw new Exception("Jump instruction is pointing at itself!");

            if (jumpToken.TargetToken == null)
                throw new Exception("Jump instruction has a null target!");
        }

        /// <summary>
        /// Optimizes the jump flow of the given list of tokens by re-pointing jumps so chained jumps can be avoided.
        /// </summary>
        /// <param name="tokens">The list of tokens to optimize</param>
        static void OptimizeJumpPointing(List<Token> tokens)
        {
            // Iterate again the jump tokens, now fixing the address of the token pointing
            foreach (Token token in tokens)
            {
                var jumpToken = token as JumpToken;
                // If the jump is pointed at an address token, skip the expansion
                if (jumpToken == null)
                    continue;

                // Iterate over and find any jumps pointing at the jump token
                foreach (Token t in tokens)
                {
                    var otherJump = t as JumpToken;
                    if (otherJump == null || !ReferenceEquals(otherJump.TargetToken, jumpToken))
                        continue;

                    OptimizeJumpRelationship(jumpToken, otherJump, tokens);
                }
            }

            // Remove jump tokens that point immediately forward or that are preceeded by equivalent jumps
            for (int i = 0; i < tokens.Count; i++)
            {
                var jumpToken = tokens[i] as JumpToken;
                if (jumpToken == null)
                    continue;

                // Conditional jumps that always fail should be removed completely
                if (i > 0 && i < tokens.Count - 1 && jumpToken.Conditional)
                {
                    var valueToken = tokens[i - 1];

                    if (valueToken.Type == TokenType.Value && valueToken.TokenObject is bool)
                    {
                        var newTarget = tokens[i + 1];

                        // Compare the condition with the token object which might be a constant boolean value
                        if (valueToken.TokenObject.Equals(!jumpToken.ConditionToJump))
                        {
                            TryRemoveJumpInstruction(jumpToken, tokens, true, newTarget);

                            // Also remove the true constant from the stack
                            if (jumpToken.ConsumesStack)
                            {
                                RemoveToken(valueToken, tokens, newTarget);
                                i--;
                            }
                            i--;
                            continue;
                        }

                        newTarget = new JumpToken(newTarget);

                        tokens.Insert(i + 1, newTarget);

                        // Transform the jump into an unconditional jump
                        TryRemoveJumpInstruction(jumpToken, tokens, true, newTarget);

                        // Also remove the true constant from the stack
                        if (jumpToken.ConsumesStack)
                        {
                            RemoveToken(valueToken, tokens, newTarget);
                            i--;
                        }
                        i--;
                        continue;
                    }
                }

                int nextToken = OffsetForJump(tokens, jumpToken);
                if (i + 1 == nextToken && TryRemoveJumpInstruction(jumpToken, tokens))
                {
                    i--;
                    continue;
                }
                
                // If an unconditional jump points at a Return or Interrupt instruction, replace the jump with the instruction itself
                if (!jumpToken.Conditional &&
                    (jumpToken.TargetToken.Instruction == VmInstruction.Ret ||
                     jumpToken.TargetToken.Instruction == VmInstruction.Interrupt))
                {
                    var replaceToken = TokenFactory.CreateInstructionToken(jumpToken.TargetToken.Instruction);
                    TryRemoveJumpInstruction(jumpToken, tokens);
                    tokens.Insert(i, replaceToken);

                    // If the previous token is a 'ClearStack', remove it, since clearing a stack before finishing the VM is useless
                    if (i > 0 && tokens[i - 1].Instruction == VmInstruction.ClearStack)
                    {
                        tokens.RemoveAt(i - 1);
                        i--;
                    }
                    continue;
                }

                if (i < tokens.Count - 1 && tokens[i + 1] is JumpToken)
                {
                    // Remove sequential equivalent jumps
                    var nextJump = (JumpToken)tokens[i + 1];
                    if (nextJump.TargetToken == jumpToken.TargetToken &&
                        nextJump.Conditional == jumpToken.Conditional &&
                        nextJump.ConsumesStack == jumpToken.ConsumesStack &&
                        nextJump.ConditionToJump == jumpToken.ConditionToJump)
                    {
                        TryRemoveJumpInstruction(jumpToken, tokens);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Analyzes and performs optimizations, when possible, on two jumps
        /// </summary>
        /// <param name="pointedJump">The first jump token which is being pointed at</param>
        /// <param name="pointingJump">The second jump token which is pointing to the first jump token</param>
        /// <param name="owningList">The list of tokens that own the two tokens</param>
        private static void OptimizeJumpRelationship(JumpToken pointedJump, JumpToken pointingJump, IList<Token> owningList)
        {
            // Unconditional jump, forward the other jump to this jump's target
            if (!pointedJump.Conditional)
            {
                // Cyclic references of jumps
                if (!pointingJump.Conditional && ReferenceEquals(pointedJump.TargetToken, pointingJump))
                {
                    throw new Exception("Two inconditional jumps pointing at each other generates infinite loops");
                }

                pointingJump.TargetToken = pointedJump.TargetToken;
                return;
            }

            // Conditional stack peek jump
            if (!pointedJump.ConsumesStack && !pointingJump.ConsumesStack)
            {
                // Direct jumps forward, if they have the same condition
                if (pointingJump.ConditionToJump == pointedJump.ConditionToJump)
                {
                    pointingJump.TargetToken = pointedJump.TargetToken;
                    return;
                }

                // Point the jump forward, because if the jumps have different conditions,
                // the second jump will always fail when comming from the first
                int pointedJumpIndex = owningList.IndexOfReference(pointedJump);
                if (pointedJumpIndex < owningList.Count - 2)
                {
                    pointingJump.TargetToken = owningList[pointedJumpIndex + 1];
                }
            }
        }

        /// <summary>
        /// Removes a jump token from a list of tokens, moving all jumps that point to it to it's target point.
        /// The removal is only realized when the jump is unconditional or does not modify the stack.
        /// This optimization must be made before expanding jump tokens
        /// </summary>
        /// <param name="jmp">The jump token to optimize</param>
        /// <param name="tokens">The list of tokens containing the jump</param>
        /// <param name="newTarget">A new target for jump tokens that aim at the given jump. Leave null to re-target to the jump's current target</param>
        /// <param name="force">Whether to force the removal, even if it is a conditional jump</param>
        /// <returns>Whether the method successfully removed the jump token</returns>
        private static bool TryRemoveJumpInstruction(JumpToken jmp, IList<Token> tokens, bool force = false, Token newTarget = null)
        {
            if (jmp.Conditional && jmp.ConsumesStack && !force)
                return false;

            RemoveToken(jmp, tokens, newTarget ?? jmp.TargetToken);

            return true;
        }

        /// <summary>
        /// Removes any multiple trailing Interrupt instructions located at the end of the given list of tokens.
        /// This operation must be performed before and jump token expansion
        /// </summary>
        /// <param name="tokens">The list of tokens to remove the sequential trailing interrupts from</param>
        private static void RemoveSequentialInterrupts(List<Token> tokens)
        {
            if (tokens.Count < 2 || tokens[tokens.Count - 1].Instruction != VmInstruction.Interrupt)
                return;

            while (tokens[tokens.Count - 2].Instruction != VmInstruction.Interrupt)
            {
                if (tokens[tokens.Count - 2].Instruction != VmInstruction.Interrupt)
                {
                    break;
                }

                // Remove the token
                RemoveToken(tokens[tokens.Count - 2], tokens, tokens[tokens.Count - 1]);
            }
        }

        /// <summary>
        /// Safely removes a token, retargeting any temporary jump instruction point to it
        /// </summary>
        /// <param name="token">The token to remove</param>
        /// <param name="tokens">The list of tokens to remove the token from</param>
        /// <param name="newTarget">A new target for jump instructions that may be pointing to it</param>
        private static void RemoveToken(Token token, IList<Token> tokens, Token newTarget)
        {
            // Iterate again the jump tokens, now fixing the address of the token pointing
            foreach (Token t in tokens)
            {
                var j = t as JumpToken;
                if (j == null)
                    continue;

                if (ReferenceEquals(j.TargetToken, token))
                {
                    j.TargetToken = newTarget;
                }
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                if(ReferenceEquals(tokens[i], token))
                    tokens.RemoveAt(i);
            }
        }

        /// <summary>
        /// Returns an integer that represents the simulated target offset for a jump at a given index
        /// </summary>
        /// <param name="tokenList">The list of tokens to analyze</param>
        /// <param name="jumpToken">The jump to analyze</param>
        /// <returns>The index that represents the jump's target after evaluation</returns>
        public static int OffsetForJump(List<Token> tokenList, JumpToken jumpToken)
        {
            return tokenList.IndexOfReference(jumpToken.TargetToken);
        }

        /// <summary>
        /// Returns an instruction for the configuration of the jump token
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>One of the VmInstruction jumps that represents the token passed</returns>
        static VmInstruction InstructionForJumpToken(JumpToken token)
        {
            // Unconditional jump
            if(!token.Conditional)
                return VmInstruction.Jump;

            // Jump if true conditional jump
            if (token.ConditionToJump)
                return token.ConsumesStack ? VmInstruction.JumpIfTrue : VmInstruction.JumpIfTruePeek;
            
            // Jump if false conditional jump
            return token.ConsumesStack ? VmInstruction.JumpIfFalse : VmInstruction.JumpIfFalsePeek;
        }
    }
}