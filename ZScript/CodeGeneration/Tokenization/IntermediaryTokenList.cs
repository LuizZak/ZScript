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
using System.Collections;
using System.Collections.Generic;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization
{
    /// <summary>
    /// Toke list containing tokens in an intermediate format that can be compiled into a TokenList that can be executed by a Function VM
    /// </summary>
    public class IntermediaryTokenList : IList<Token>
    {
        /// <summary>
        /// The internal representation of the token list
        /// </summary>
        private readonly List<Token> _tokens;

        /// <summary>
        /// Initializes a new instance of the IntermediateTokenList class
        /// </summary>
        public IntermediaryTokenList()
        {
            _tokens = new List<Token>();
        }

        /// <summary>
        /// Initializes a new instance of the IntermediateTokenList class with a starting list of tokens
        /// </summary>
        /// <param name="tokens">An enumerable of tokens to add to this intermediate token list</param>
        public IntermediaryTokenList(IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
        }

        /// <summary>
        /// Adds a range of tokens to the end of this intermediate token list
        /// </summary>
        /// <param name="tokens">A range of tokens to add to this intermediate token list</param>
        public void AddRange(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                Add(token);
            }
        }

        /// <summary>
        /// Adds a given token to this intermediate token list
        /// </summary>
        /// <param name="item">The token to add to this list</param>
        public void Add(Token item)
        {
            _tokens.Add(item);
        }

        /// <summary>
        /// Throws an InvalidOperationException.
        /// For removal of tokens, utilize the <see cref="RemoveToken(Token, Token)"/> method instead
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot remove a token with this method. For removal of tokens, utilize the <see cref="RemoveToken(Token, Token)"/> method instead</exception>
        public bool Remove(Token item)
        {
            throw new InvalidOperationException("Tokens must be removed through the RemoveToken() method");
        }

        /// <summary>
        /// Safely removes a token, retargeting any temporary jump instruction point to it
        /// </summary>
        /// <param name="token">The token to remove</param>
        /// <param name="newTarget">A new target for jump instructions that may be pointing to it</param>
        public void RemoveToken(Token token, Token newTarget)
        {
            RemoveToken(this, token, newTarget);
        }

        /// <summary>
        /// Safely removes a token, retargeting any temporary jump instruction point to it
        /// </summary>
        /// <param name="source">The source to remove the token from</param>
        /// <param name="token">The token to remove</param>
        /// <param name="newTarget">A new target for jump instructions that may be pointing to it</param>
        public static void RemoveToken(IList<Token> source, Token token, Token newTarget)
        {
            // Iterate again the jump tokens, now fixing the address of the token pointing
            foreach (Token t in source)
            {
                var j = t as JumpToken;
                if (j == null)
                    continue;

                if (ReferenceEquals(j.TargetToken, token))
                {
                    j.TargetToken = newTarget;
                }
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (ReferenceEquals(source[i], token))
                    source.RemoveAt(i);
            }
        }

        /// <summary>
        /// Converts the contents of this IntermediateTokenList to a TokenList object
        /// </summary>
        /// <returns>A TokenList generated by the tokens in this IntermediateTokenList</returns>
        public TokenList ToTokenList()
        {
            InternalBindJumpTargets(true, VmInstruction.Interrupt);

            var finalList = new IntermediaryTokenList(this);

            // Remove the unreachable tokens
            RemoveUnreachableTokens(finalList);
            // Remove duplicater Clear Stack instructions
            RemoveSequentialInstructions(finalList, VmInstruction.ClearStack);
            RemoveSequentialInstructions(finalList, VmInstruction.Ret);
            RemoveSequentialInstructions(finalList, VmInstruction.Interrupt);

            // Optimize jumps once more
            JumpTokenOptimizer.OptimizeJumps(finalList);

            return new TokenList(CreateExpandedTokenList(finalList));
        }

        /// <summary>
        /// Replaces the jump target tokens on this intermediary token list, leaving any jump target token at the end of the list unexpanded
        /// </summary>
        public void BindJumpTargets()
        {
            InternalBindJumpTargets(false, VmInstruction.Noop);
        }

        /// <summary>
        /// Replaces the jump target tokens on this intermediary token list
        /// </summary>
        /// <param name="endJumpTargetInstruction">The instruction to expand the last jump target as</param>
        public void BindJumpTargets(VmInstruction endJumpTargetInstruction)
        {
            InternalBindJumpTargets(true, endJumpTargetInstruction);
        }

        /// <summary>
        /// Replaces the jump target tokens on this intermediary token list
        /// </summary>
        /// <param name="replaceJumpTargetsAtEnd">
        /// Whether to expand jump targets at the end of the list of tokens as an instruction token specified by the end JumpTargetInstruction parameter
        /// </param>
        /// <param name="endJumpTargetInstruction">The instruction to expand the last jump target as</param>
        private void InternalBindJumpTargets(bool replaceJumpTargetsAtEnd, VmInstruction endJumpTargetInstruction)
        {
            for (int i = 0; i < _tokens.Count; i++)
            {
                if (i == _tokens.Count - 1 && !replaceJumpTargetsAtEnd)
                    break;

                var token = _tokens[i];

                if (!(token is JumpTargetToken))
                    continue;

                Token newTarget;
                bool endJump = false;

                if (i == _tokens.Count - 1)
                {
                    // Expand this jump token as an interrupt
                    newTarget = TokenFactory.CreateInstructionToken(endJumpTargetInstruction);
                    _tokens.Add(newTarget);

                    endJump = true;
                }
                else
                {
                    newTarget = _tokens[i + 1];
                }

                bool hasSource = false;
                // Find all jump tokens that are pointing to this jump target token
                foreach (Token t in _tokens)
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
                    _tokens.Remove(newTarget);
                }

                // Remove jump target token
                _tokens.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Expands the jump tokens on a copy of this list, replacing them with couples of address tokens and jump instruction
        /// tokens, and returning the newly generateed list
        /// </summary>
        /// <exception cref="Exception">One of the jump tokens points to a target token that is not inside the same token list</exception>
        public static List<Token> CreateExpandedTokenList(IList<Token> source)
        {
            var expandedTokens = new List<Token>(source);
            
            // Iterate again the jump tokens, now fixing the address of the token pointing
            for (int i = 0; i < expandedTokens.Count; i++)
            {
                // Remove 'noop' tokens
                if (expandedTokens[i].Type == TokenType.Instruction &&
                    expandedTokens[i].Instruction == VmInstruction.Noop)
                {
                    expandedTokens.RemoveAt(i);
                    i--;
                    continue;
                }

                var jumpToken = expandedTokens[i] as JumpToken;
                // If the jump is pointed at a jump target token, skip the expansion
                if (jumpToken == null || jumpToken.TargetToken is JumpTargetToken)
                    continue;

                // Analyze the jump's validity
                AnalyzeJump(jumpToken);

                // Find address of jump
                int address = OffsetForJump(expandedTokens, jumpToken);
                if (address == -1)
                    throw new Exception("A jump token has a target that is not contained within the same token list");

                var newToken = TokenFactory.CreateInstructionToken(InstructionForJumpToken(jumpToken), address);

                // Replace any jump reference that may be pointing to this jump token
                foreach (var t in expandedTokens)
                {
                    // If this jump token is unconditional, just point the other token to this token's target
                    var token = t as JumpToken;
                    if (token != null && ReferenceEquals(token.TargetToken, jumpToken))
                    {
                        token.TargetToken = newToken;
                    }
                }

                expandedTokens[i] = newToken;
            }

            return expandedTokens;
        }

        /// <summary>
        /// Returns an integer that represents the simulated target offset for a jump at a given index.
        /// If the provided jump token does not exists inside this immediate token list, an exception is raised
        /// </summary>
        /// <param name="jumpToken">The jump to analyze</param>
        /// <returns>The index that represents the jump's target after evaluation</returns>
        /// <exception cref="ArgumentException">The provided jump token is not part of this token list</exception>
        public int OffsetForJump(JumpToken jumpToken)
        {
            return OffsetForJump(_tokens, jumpToken);
        }

        /// <summary>
        /// Returns an integer that represents the simulated target offset for a jump at a given index.
        /// If the provided jump token does not exists inside this immediate token list, an exception is raised
        /// </summary>
        /// <param name="tokens">The list of tokens to use when searching for the offset</param>
        /// <param name="jumpToken">The jump to analyze</param>
        /// <returns>The index that represents the jump's target after evaluation</returns>
        /// <exception cref="ArgumentException">The provided jump token is not part of this token list</exception>
        public static int OffsetForJump(IList<Token> tokens, JumpToken jumpToken)
        {
            if (!tokens.ContainsReference(jumpToken) || !tokens.ContainsReference(jumpToken.TargetToken))
                throw new ArgumentException("The provided token does not exists inside this intermediate token list object", "jumpToken");

            return tokens.IndexOfReference(jumpToken.TargetToken);
        }

        /// <summary>
        /// Analyzes the tokens contained in this intermediary token list, and removes any token that happens to be unreachable by any common code flow
        /// </summary>
        /// <param name="tokenList">The list of token to remove the unreachable tokens from</param>
        /// <param name="entryIndex">The index of the token to start analyzing from</param>
        public static void RemoveUnreachableTokens(IList<Token> tokenList, int entryIndex = 0)
        {
            // Detect the reachability of the tokens
            DetectReachability(tokenList, entryIndex);

            // Remove the tokens marked as unreachable
            for (int i = 0; i < tokenList.Count; i++)
            {
                if (tokenList[i].Reachable)
                    continue;

                // It's safe to remove tokens this way from the list because there are no jumps pointing at it, 
                // as they where not detected during the reachability test
                tokenList.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Analyzes the tokens contained in a given token list, marking the <see cref="Token.Reachable"/> property of the tokens that are reachable from some code flow.
        /// Code flow is marked in a flood-fill-fashion by sweeping the tokens linearly forward, marking visited tokens as reachable, and stopping when an
        /// interruption instruction is met. The analysis takes into consideration conditional and unconditional jumps to mark all possible instruction paths.
        /// The function returns an array of booleans that marks the tokens reachable.
        /// The entry index parameter is used to specify where to start traversing the tokens from.
        /// If the index is out of bounds of the list (&lt; 0 or &gt;= Count), an exception is raised.
        /// </summary>
        /// <param name="tokenList">The token list to detect the reachability on</param>
        /// <param name="entryIndex">The index of the token to start analyzing from</param>
        /// <exception cref="ArgumentOutOfRangeException">The entryIndex points outside the list of tokens</exception>
        public static bool[] DetectReachability(IList<Token> tokenList, int entryIndex = 0)
        {
            if (entryIndex == 0 && tokenList.Count == 0)
                return new bool[0];

            if (entryIndex < 0 || entryIndex >= tokenList.Count)
            {
                throw new ArgumentOutOfRangeException("entryIndex");
            }

            // Reset the reachability of the tokens
            foreach (var token in tokenList)
            {
                token.Reachable = false;
            }

            // Create an array to mark the tokens that have been reached
            bool[] sweepedFlags = new bool[tokenList.Count];

            // Create the queue of tokens to visit
            var visitQueue = new Queue<int>();
            visitQueue.Enqueue(entryIndex);

            // Repeat until there are no more available code paths to dequeue
            while (visitQueue.Count > 0)
            {
                // Dequeue the index to visit
                int index = visitQueue.Dequeue();

                // Linearly scan the tokens, until either A: a conditional jump is reached or B: an interruption (interrupt or ret) instruction is met
                for (; index < tokenList.Count; index++)
                {
                    // Break out of the loop, in case the token has been swept already
                    if (sweepedFlags[index])
                        break;

                    // Mark the token as swept and flag the array at the index as reachable
                    tokenList[index].Reachable = sweepedFlags[index] = true;

                    // Check if the token is an interrupt-type token; break out if it is
                    if (tokenList[index].Instruction == VmInstruction.Interrupt ||
                        tokenList[index].Instruction == VmInstruction.Ret)
                        break;

                    // Check if the token is an unconditional jump; enqueue the jump address
                    var jump = tokenList[index] as JumpToken;
                    if (jump != null)
                    {
                        visitQueue.Enqueue(OffsetForJump(tokenList, jump));

                        // If the jump is not conditional, we break now because the index cannot be advanced forward from this point anyway
                        if(!jump.Conditional)
                            break;
                    }
                }
            }

            return sweepedFlags;
        }

        /// <summary>
        /// Removes all duplicated instances of a given instruction
        /// </summary>
        /// <param name="tokenList">The list of tokens to remove the duplicate instructions from</param>
        /// <param name="instruction">The instruction to remove duplicates of</param>
        private static void RemoveSequentialInstructions(IList<Token> tokenList, VmInstruction instruction)
        {
            for (int i = 1; i < tokenList.Count; i++)
            {
                var last = tokenList[i - 1];
                var cur = tokenList[i];

                if (last.Type == TokenType.Instruction &&
                    last.Instruction == instruction &&
                    cur.Type == TokenType.Instruction &&
                    cur.Instruction == instruction)
                {
                    RemoveToken(tokenList, cur, last);
                    i--;
                }
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
        /// Returns an instruction for the configuration of the jump token
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>One of the VmInstruction jumps that represents the token passed</returns>
        static VmInstruction InstructionForJumpToken(JumpToken token)
        {
            // Unconditional jump
            if (!token.Conditional)
                return VmInstruction.Jump;

            // Null check jump
            if (token.NullCheck)
                return token.ConditionToJump ? VmInstruction.JumpIfNotNull : VmInstruction.JumpIfNull;

            // Jump if true conditional jump
            if (token.ConditionToJump)
                return token.ConsumesStack ? VmInstruction.JumpIfTrue : VmInstruction.JumpIfTruePeek;

            // Jump if false conditional jump
            return token.ConsumesStack ? VmInstruction.JumpIfFalse : VmInstruction.JumpIfFalsePeek;
        }

        #region IList implementation

        public IEnumerator<Token> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _tokens.Clear();
        }

        public bool Contains(Token item)
        {
            return _tokens.Contains(item);
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            _tokens.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _tokens.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(Token item)
        {
            return _tokens.IndexOf(item);
        }

        public void Insert(int index, Token item)
        {
            _tokens.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _tokens.RemoveAt(index);
        }

        public Token this[int index]
        {
            get { return _tokens[index]; }
            set { _tokens[index] = value; }
        }

        #endregion
    }
}