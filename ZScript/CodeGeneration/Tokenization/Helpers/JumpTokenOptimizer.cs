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

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Class responsible for expanding jump tokens into address and instruction tokens
    /// </summary>
    public class JumpTokenOptimizer
    {
        /// <summary>
        /// Expands the jump tokens associated with the given token list
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        public static void OptimizeJumps(IntermediaryTokenList tokens)
        {
            tokens.BindJumpTargets(VmInstruction.Noop);

            OptimizeJumpPointing(tokens);
        }

        /// <summary>
        /// Expands the jump tokens associated with the given token list, expanding the last jump target instruction at the end of the token list,
        /// if it is present, as a given instruction
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        /// <param name="endJumpTargetInstruction">The instruction to expand the last jump target as</param>
        public static void OptimizeJumps(IntermediaryTokenList tokens, VmInstruction endJumpTargetInstruction)
        {
            return;

            tokens.BindJumpTargets(endJumpTargetInstruction);

            // Optimize the jump flow
            bool optimize = true;
            while (optimize)
            {
                optimize = OptimizeJumpPointing(tokens);
            }
        }

        /// <summary>
        /// Optimizes the jump flow of the given list of tokens by re-pointing jumps so chained jumps can be avoided.
        /// The method returns a boolean value specifying if optimizations where realized on the token list or not
        /// </summary>
        /// <param name="tokens">The list of tokens to optimize</param>
        /// <returns>true when optimizations where realized; false otherwise</returns>
        public static bool OptimizeJumpPointing(IntermediaryTokenList tokens)
        {
            bool optimized = false;

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

                    optimized = OptimizeJumpRelationship(jumpToken, otherJump, tokens);
                }
            }

            // Remove jump tokens that point immediately forward or that are preceeded by equivalent jumps
            for (int i = 0; i < tokens.Count; i++)
            {
                var jumpToken = tokens[i] as JumpToken;
                if (jumpToken == null)
                    continue;

                // Conditional jumps that always fail should be removed completely
                if (i > 0 && i < tokens.Count - 1 && jumpToken.Conditional && !jumpToken.NullCheck)
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
                                tokens.RemoveToken(valueToken, newTarget);
                                i--;
                            }
                            i--;
                            continue;
                        }

                        newTarget = new JumpToken(jumpToken.TargetToken);

                        tokens.Insert(i + 1, newTarget);

                        // Transform the jump into an unconditional jump
                        TryRemoveJumpInstruction(jumpToken, tokens, true, newTarget);

                        // Also remove the true constant from the stack
                        if (jumpToken.ConsumesStack)
                        {
                            tokens.RemoveToken(valueToken, jumpToken.TargetToken);
                            i--;
                        }
                        i--;

                        optimized = true;

                        continue;
                    }
                }

                int jumpOffset = tokens.OffsetForJump(jumpToken);
                
                // Immediate unconditional/peeking jumps
                if (i + 1 == jumpOffset && TryRemoveJumpInstruction(jumpToken, tokens))
                {
                    i--;
                    optimized = true;
                    continue;
                }
                
                // If an unconditional jump points at a Return or Interrupt instruction, replace the jump with the instruction itself
                if (!jumpToken.Conditional && !jumpToken.NullCheck &&
                    (jumpToken.TargetToken.Instruction == VmInstruction.Ret ||
                     jumpToken.TargetToken.Instruction == VmInstruction.Interrupt))
                {
                    var replaceToken = TokenFactory.CreateInstructionToken(jumpToken.TargetToken.Instruction);
                    TryRemoveJumpInstruction(jumpToken, tokens);
                    tokens.Insert(i, replaceToken);

                    // If the previous token is a 'ClearStack', remove it, since clearing a stack before finishing the VM is useless
                    if (jumpToken.TargetToken.Instruction != VmInstruction.Ret && i > 0 && tokens[i - 1].Instruction == VmInstruction.ClearStack)
                    {
                        tokens.RemoveToken(tokens[i - 1], replaceToken);
                        i--;
                    }

                    optimized = true;
                    continue;
                }

                // If a conditional peek type jump both preceedes and points to the same interrupt-type instruction, replace the token with the instruction itself
                if (i < tokens.Count - 1 && jumpToken.Conditional && !jumpToken.NullCheck && !jumpToken.ConsumesStack &&
                    (jumpToken.TargetToken.Instruction == VmInstruction.Ret ||
                     jumpToken.TargetToken.Instruction == VmInstruction.Interrupt) &&
                    (tokens[i + 1].Instruction == jumpToken.TargetToken.Instruction))
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

                    optimized = true;
                    continue;
                }

                // If a conditional jump jumps immediately after an immediate unconditional jump, remove the unconditional jump and flip the conditional jump's condition
                if (i < tokens.Count - 2 && jumpToken.Conditional && !jumpToken.NullCheck && jumpToken.ConsumesStack && tokens[i + 1] is JumpToken)
                {
                    var immediateJump = (JumpToken)tokens[i + 1];

                    // Can only apply optimization to unconditional immediate jumps, and when the conditional jump jumps over this immediate jump
                    if (immediateJump.Conditional || jumpOffset != i + 2)
                        continue;

                    // Flip the conditionality of the current jump
                    var newJump = new JumpToken(immediateJump.TargetToken, true, !jumpToken.ConditionToJump);
                    TryRemoveJumpInstruction(jumpToken, tokens, true);
                    tokens.Insert(i, newJump);

                    // Remove the immediate jump
                    TryRemoveJumpInstruction(immediateJump, tokens);

                    continue;
                }

                // Sequential equivalent jump detection
                if (i < tokens.Count - 1 && tokens[i + 1] is JumpToken)
                {
                    // Remove sequential equivalent jumps
                    var nextJump = (JumpToken)tokens[i + 1];
                    if (nextJump.TargetToken == jumpToken.TargetToken &&
                        nextJump.Conditional == jumpToken.Conditional &&
                        nextJump.ConsumesStack == jumpToken.ConsumesStack &&
                        nextJump.ConditionToJump == jumpToken.ConditionToJump &&
                        nextJump.NullCheck == jumpToken.NullCheck)
                    {
                        TryRemoveJumpInstruction(jumpToken, tokens);
                        i--;
                    }
                }
            }

            return optimized;
        }

        /// <summary>
        /// Analyzes and performs optimizations, when possible, on two jumps, returning a boolean value specifying if any optimization was made
        /// </summary>
        /// <param name="pointedJump">The first jump token which is being pointed at</param>
        /// <param name="pointingJump">The second jump token which is pointing to the first jump token</param>
        /// <param name="owningList">The list of tokens that own the two tokens</param>
        /// <returns>true if any optimization was made, false otherwise</returns>
        private static bool OptimizeJumpRelationship(JumpToken pointedJump, JumpToken pointingJump, IntermediaryTokenList owningList)
        {
            // Unconditional jump, forward the other jump to this jump's target
            if (!pointedJump.Conditional)
            {
                // Cyclic references of jumps
                if (!pointingJump.Conditional && ReferenceEquals(pointedJump.TargetToken, pointingJump))
                {
                    throw new Exception("Two unconditional jumps pointing at each other generates infinite loops");
                }

                pointingJump.TargetToken = pointedJump.TargetToken;
                return true;
            }

            // Conditional stack peek jump
            if (!pointedJump.ConsumesStack && !pointingJump.ConsumesStack && !pointedJump.NullCheck && !pointingJump.NullCheck)
            {
                // Direct jumps forward, if they have the same condition
                if (pointingJump.ConditionToJump == pointedJump.ConditionToJump)
                {
                    pointingJump.TargetToken = pointedJump.TargetToken;
                    return true;
                }

                // Point the jump forward, because if the jumps have different conditions,
                // the second jump will always fail when comming from the first
                int pointedJumpIndex = owningList.IndexOfReference(pointedJump);
                if (pointedJumpIndex < owningList.Count - 2)
                {
                    pointingJump.TargetToken = owningList[pointedJumpIndex + 1];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a jump token from a list of tokens, moving all jumps that point to it to it's target point.
        /// The removal is only realized when the jump is unconditional or does not modify the stack.
        /// This operation must be made before expanding jump tokens
        /// </summary>
        /// <param name="jmp">The jump token to optimize</param>
        /// <param name="tokens">The list of tokens containing the jump</param>
        /// <param name="newTarget">A new target for jump tokens that aim at the given jump. Leave null to re-target to the jump's current target</param>
        /// <param name="force">Whether to force the removal, even if it is a conditional jump</param>
        /// <returns>Whether the method successfully removed the jump token</returns>
        private static bool TryRemoveJumpInstruction(JumpToken jmp, IntermediaryTokenList tokens, bool force = false, Token newTarget = null)
        {
            if (jmp.Conditional && (jmp.ConsumesStack || jmp.NullCheck) && !force)
                return false;

            tokens.RemoveToken(jmp, newTarget ?? jmp.TargetToken);

            return true;
        }
    }
}