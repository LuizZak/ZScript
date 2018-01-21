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
using System.Linq;
using JetBrains.Annotations;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;

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
        public static void PrintTokens([NotNull] IEnumerable<Token> tokenList)
        {
            int add = 0;

            var list = tokenList as IList<Token> ?? tokenList.ToList();
            foreach (var token in list)
            {
                Console.Write("{0:0000000}", add++);
                Console.Write(": ");

                var jumpToken = token as JumpToken;
                if (jumpToken != null)
                {
                    Console.Write("[");
                    Console.Write(OffsetForJump(list, jumpToken));
                    Console.Write(" JUMP");
                    if (jumpToken.Conditional)
                    {
                        if (jumpToken.NullCheck)
                        {
                            Console.Write(jumpToken.ConditionToJump ? "IfNotTrue" : "IfNull");
                        }
                        else
                        {
                            Console.Write(jumpToken.ConditionToJump ? "IfTrue" : "IfFalse");
                            if (!jumpToken.ConsumesStack)
                                Console.Write("Peek");
                        }
                    }
                    Console.WriteLine("]");
                    continue;
                }
                
                PrintToken(token);

                Console.WriteLine("");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Prints a given list of tokens into the console
        /// </summary>
        /// <param name="tokenList">The list of tokens to print</param>
        public static void PrintTokens([NotNull] TokenList tokenList)
        {
            int add = 0;

            foreach (var token in tokenList.Tokens)
            {
                if(!token.Reachable)
                    Console.Write("UNREACHABLE: ");

                Console.Write("{0:0000000}", add++);
                Console.Write(": ");

                PrintToken(token);

                Console.WriteLine("");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Prints the contents of a given token on the console
        /// </summary>
        /// <param name="token">The token to print</param>
        public static void PrintToken([NotNull] Token token)
        {
            if (token is JumpTargetToken)
            {
                Console.Write("JUMP_TARGET");
                return;
            }

            if (token is TypedToken)
            {
                Console.Write(token.Instruction);
                Console.Write(" ");
                Console.Write(token);
                return;
            }

            switch (token.Type)
            {
                case TokenType.Operator:
                case TokenType.Instruction:
                    Console.Write(token.Instruction);

                    // Print operand for token
                    if (token.TokenObject != null)
                    {
                        Console.Write(" ");
                        Console.Write(token.TokenObject);
                    }
                    break;
                default:
                    Console.Write(token.TokenObject ?? "null");
                    break;
            }
        }

        /// <summary>
        /// Returns an integer that represents the simulated target offset for a jump at a given index
        /// </summary>
        /// <param name="tokenList">The list of tokens to analyze</param>
        /// <param name="jumpToken">The jump to analyze</param>
        /// <returns>The index that represents the jump's target after evaluation</returns>
        public static int OffsetForJump([NotNull] IEnumerable<Token> tokenList, JumpToken jumpToken)
        {
            int i = 0;
            foreach (var token in tokenList)
            {
                if (ReferenceEquals(token, jumpToken.TargetToken))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}