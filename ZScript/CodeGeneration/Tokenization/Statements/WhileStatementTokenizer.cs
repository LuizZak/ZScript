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

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing While statements
    /// </summary>
    public class WhileStatementTokenizer : IParserContextTokenizer<ZScriptParser.WhileStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents a jump target to outside the while loop
        /// </summary>
        private JumpTargetToken _blockEnd;

        /// <summary>
        /// Represents the condition portion of the loop
        /// </summary>
        private JumpTargetToken _conditionTarget;

        /// <summary>
        /// Initializes a new instance of the WhileStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public WhileStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given While loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.WhileStatementContext context)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given While loop statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to tokenize to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.WhileStatementContext context)
        {
            // WHILE loop tokenization:
            // 1 - Condition expression
            // 2 - Conditional jump to End
            // 3 - Body loop
            // 4 - Unconditional jump to Condition
            // 5 - End

            // Create the jump targets
            _conditionTarget = new JumpTargetToken();
            _blockEnd = new JumpTargetToken();

            _context.PushContinueTarget(_conditionTarget);
            _context.PushBreakTarget(_blockEnd);

            var cond = context.expression();

            // 1 - Condition expression
            targetList.Add(_conditionTarget);

            _context.TokenizeExpression(targetList, cond);

            // 2 - Conditional jump to End
            targetList.Add(new JumpToken(_blockEnd, true, false));

            // 3 - Body loop
            _context.TokenizeStatement(targetList, context.statement());

            // 4 - Unconditional jump to Condition
            targetList.Add(new JumpToken(_conditionTarget));

            // 5 - End
            targetList.Add(_blockEnd);

            // Pop the targets
            _context.PopContinueTarget();
            _context.PopBreakTarget();
        }
    }
}