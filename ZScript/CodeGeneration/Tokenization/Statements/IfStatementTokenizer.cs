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

using System.Collections.Generic;
using JetBrains.Annotations;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Helper class used to aid in the tokenization process of an IF statement
    /// </summary>
    public class IfStatementTokenizer : IParserContextTokenizer<ZScriptParser.IfStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the IfStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public IfStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        public IntermediaryTokenList TokenizeStatement([NotNull] ZScriptParser.IfStatementContext context)
        {
            var tokens = new IntermediaryTokenList();

            TokenizeStatement(tokens, context);

            return tokens;
        }

        /// <summary>
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to tokenize to</param>
        /// <param name="context">The context containinng</param>
        public void TokenizeStatement(IList<Token> targetList, [NotNull] ZScriptParser.IfStatementContext context)
        {
            // Read first if block of the chain
            TokenizeIfStatement(targetList, context);
        }

        /// <summary>
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="retTokens">The target list to add the tokens to</param>
        /// <param name="context">The context containing the IF statement to tokenize</param>
        private void TokenizeIfStatement(IList<Token> retTokens, [NotNull] ZScriptParser.IfStatementContext context)
        {
            // Constant if statements are evaluated differently
            if (context.IsConstant)
            {
                // If the constant is true, tokenize the statement, if not, tokenize the else statement, if present
                if (context.ConstantValue)
                {
                    _context.TokenizeStatement(retTokens, context.statement());
                }
                else if(context.elseStatement() != null)
                {
                    _context.TokenizeStatement(retTokens, context.elseStatement().statement());
                }
                return;
            }

            // Create the 'else' target
            var elseJump = new JumpTargetToken();
            var endJump = new JumpTargetToken();

            // 1. Read expression
            _context.TokenizeExpression(retTokens, context.expression());

            // 1. Add conditional jump for the else target (changed to an end jump, if no else is present)
            retTokens.Add(new JumpToken(context.elseStatement() == null ? endJump : elseJump, true, false));

            // 2. Add the true statement
            _context.TokenizeStatement(retTokens, context.statement());

            if (context.elseStatement() != null)
            {
                // 3. Pin a jump to the end (which goes before the else statement, and after the
                //    statements of the IF block, and is used to skip over the else statement)
                retTokens.Add(new JumpToken(endJump));

                // 
                // ELSE
                // 
                // 4. Add the else jump target
                retTokens.Add(elseJump);

                // 5. Add the else statement
                if (context.elseStatement() != null)
                {
                    _context.TokenizeStatement(retTokens, context.elseStatement().statement());
                }
            }

            // 6. Pin the end jump target
            retTokens.Add(endJump);
        }
    }
}