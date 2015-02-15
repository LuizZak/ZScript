using System;
using System.Collections.Generic;

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