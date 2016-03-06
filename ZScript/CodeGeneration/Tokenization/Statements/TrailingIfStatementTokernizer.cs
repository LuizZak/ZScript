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
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Helper class to aid in the tokenization process of a trailing if statement
    /// </summary>
    class TrailingIfStatementTokernizer : IParserContextTokenizer<ZScriptParser.TrailingIfStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the TrailingIfStatementTokernizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public TrailingIfStatementTokernizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given trailing IF statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.TrailingIfStatementContext context)
        {
            var tokens = new IntermediaryTokenList();

            TokenizeStatement(tokens, context);

            return tokens;
        }

        /// <summary>
        /// Tokenizes a given trailing IF statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to tokenize to</param>
        /// <param name="context">The context containinng</param>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.TrailingIfStatementContext context)
        {
            // Read first if block of the chain
            TokenizeTrailingIfStatement(targetList, context);
        }

        /// <summary>
        /// Tokenizes a given trailing IF statement into a list of tokens
        /// </summary>
        /// <param name="retTokens">The target list to add the tokens to</param>
        /// <param name="context">The context containing the trailing IF statement to tokenize</param>
        private void TokenizeTrailingIfStatement(IList<Token> retTokens, ZScriptParser.TrailingIfStatementContext context)
        {
            // Constant if statements are evaluated differently
            if (context.IsConstant)
            {
                // If the constant is false, do not tokenize the statement
                if (!context.ConstantValue)
                    return;

                if (context.returnStatement() != null)
                {
                    _context.TokenizeReturnStatement(retTokens, context.returnStatement());
                }
                else
                {
                    _context.TokenizeExpression(retTokens, context.expression(0));

                    // Add a pop to balance the stack
                    retTokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Pop));
                }

                return;
            }

            // Create the 'false' target
            var endJump = new JumpTargetToken();

            // 1. Read expression
            _context.TokenizeExpression(retTokens, context.exp);

            // 1. Add conditional jump for the end target
            retTokens.Add(new JumpToken(endJump, true, false));

            // 2. Add the true statement
            if (context.returnStatement() != null)
            {
                _context.TokenizeReturnStatement(retTokens, context.returnStatement());
            }
            else
            {
                _context.TokenizeExpression(retTokens, context.expression(0));

                // Add a pop to balance the stack
                retTokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Pop));
            }

            // 6. Pin the end jump target
            retTokens.Add(endJump);
        }
    }
}
