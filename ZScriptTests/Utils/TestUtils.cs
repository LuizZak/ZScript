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
using Antlr4.Runtime;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;

namespace ZScriptTests.Utils
{
    /// <summary>
    /// Static class that contains utillity methods utilized through the unit tests
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Returns whether the two given token lists are equivalent.
        /// The equivalence takes in consideration the token types, instructions,
        /// values, and in case of jump tokens, the equivalence of the jump
        /// </summary>
        /// <param name="expected">The expected token</param>
        /// <param name="actual">The actual token</param>
        /// <param name="message">The message to display in case the tokens mismatch</param>
        /// <returns>true if the token lists are equivalent, false otherwise</returns>
        /// <exception cref="Exception">The token lists did not match</exception>
        public static void AssertTokenListEquals(IList<Token> expected, IList<Token> actual, string message)
        {
            // Compare the tokens one by one
            for (int i = 0; i < Math.Min(expected.Count, actual.Count); i++)
            {
                Token t1 = expected[i];
                Token t2 = actual[i];

                // Unequality of types
                if (t1.GetType() != t2.GetType())
                    throw new Exception(message + "; Tokens at index " + i + " have different types: expected " + t1.GetType() + " actual: " + t2.GetType());

                var jt1 = t1 as JumpToken;
                var jt2 = t2 as JumpToken;
                if (jt1 != null && jt2 != null)
                {
                    if (jt1.ConditionToJump != jt2.ConditionToJump ||
                        jt1.Conditional != jt2.Conditional ||
                        jt1.ConsumesStack != jt2.ConsumesStack ||
                        expected.IndexOf(jt1.TargetToken) != expected.IndexOf(jt2.TargetToken))
                    {
                        throw new Exception(message + "; Jump tokens at index " + i + " do not have the same configuration: expected " + t1 + " actual: " + t2);
                    }
                }

                if (!t1.Equals(t2))
                    throw new Exception(message + "; Tokens at index " + i + " have different values: expected " + t1 + " actual: " + t2 + " (watch out for int->long conversions in numeric atoms!)");
            }

            if (expected.Count != actual.Count)
                throw new Exception(message + "; Token lists have different token counts");
        }

        /// <summary>
        /// Creates the default generator to use in tests
        /// </summary>
        /// <param name="input">The input string to use in the generator</param>
        /// <returns>A default runtime generator to use in tests</returns>
        public static ZRuntimeGenerator CreateGenerator(string input)
        {
            return new ZRuntimeGenerator(input) { Debug = true };
        }

        /// <summary>
        /// Creates a new ZScriptParser object from a given string
        /// </summary>
        /// <param name="input">The input string to generate the ZScriptParser from</param>
        /// <returns>A ZScriptParser created from the given string</returns>
        public static ZScriptParser CreateParser(string input)
        {
            AntlrInputStream stream = new AntlrInputStream(input);
            ITokenSource lexer = new ZScriptLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            return new ZScriptParser(tokens);
        }
    }
}