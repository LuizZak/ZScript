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
using System.Collections.Generic;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.Utils
{
    /// <summary>
    /// Class that contains utility methods related to tokens
    /// </summary>
    public static class TokenUtils
    {
        /// <summary>
        /// Prints a given list of tokens into the console
        /// </summary>
        /// <param name="tokenList">The list of tokens to print</param>
        public static void PrintTokens(IList<Token> tokenList)
        {
            int add = 0;

            foreach (var token in tokenList)
            {
                Console.Write("{0:0000000}", add++);
                Console.Write(": ");

                var jumpToken = token as JumpToken;
                if (jumpToken != null)
                {
                    Console.Write("[");
                    Console.Write(OffsetForJump(tokenList, jumpToken));
                    Console.Write(" JUMP");
                    if (jumpToken.Conditional)
                    {
                        Console.Write(jumpToken.ConditionToJump ? "IfTrue" : "IfFalse");
                        if(!jumpToken.ConsumesStack)
                            Console.Write("Peek");
                    }
                    Console.WriteLine("]");
                    continue;
                }
                if (token is JumpTargetToken)
                {
                    Console.WriteLine("JUMP_TARGET ");
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.Operator:
                    case TokenType.Instruction:
                        Console.Write(token.Instruction);

                        // Print jump address along with jump instructions
                        if (token.Instruction == VmInstruction.Jump || token.Instruction == VmInstruction.JumpIfFalse ||
                            token.Instruction == VmInstruction.JumpIfTrue)
                        {
                            Console.Write(" ");
                            Console.Write(token.TokenObject);
                        }
                        break;
                    default:
                        Console.Write(token.TokenObject);
                        break;
                }

                Console.WriteLine("");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Prints a given list of tokens into the console
        /// </summary>
        /// <param name="tokenList">The list of tokens to print</param>
        public static void PrintTokens(TokenList tokenList)
        {
            int add = 0;

            foreach (var token in tokenList.Tokens)
            {
                if(!token.Reachable)
                    Console.Write("UNREACHABLE: ");

                Console.Write("{0:0000000}", add++);
                Console.Write(": ");

                switch (token.Type)
                {
                    case TokenType.Operator:
                    case TokenType.Instruction:
                        Console.Write(token.Instruction);

                        // Print jump address along with jump instructions
                        if (token.Instruction == VmInstruction.Jump || token.Instruction == VmInstruction.JumpIfFalse ||
                            token.Instruction == VmInstruction.JumpIfTrue)
                        {
                            Console.Write(" ");
                            Console.Write(token.TokenObject);
                        }
                        break;
                    default:
                        Console.Write(token.TokenObject);
                        break;
                }

                Console.WriteLine("");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Returns an integer that represents the simulated target offset for a jump at a given index
        /// </summary>
        /// <param name="tokenList">The list of tokens to analyze</param>
        /// <param name="jumpToken">The jump to analyze</param>
        /// <returns>The index that represents the jump's target after evaluation</returns>
        public static int OffsetForJump(IList<Token> tokenList, JumpToken jumpToken)
        {
            return tokenList.IndexOfReference(jumpToken.TargetToken);
        }
    }
}