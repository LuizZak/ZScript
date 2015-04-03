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
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing FOR statements
    /// </summary>
    public class ForStatementTokenizer : IParserContextTokenizer<ZScriptParser.ForStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents the last block before the end of the current for blocks
        /// </summary>
        private JumpTargetToken _forBlockEndTarget;

        /// <summary>
        /// Represents the condition portion of the loop
        /// </summary>
        private JumpTargetToken _conditionTarget;

        /// <summary>
        /// Represents the increment portion of the loop
        /// </summary>
        private JumpTargetToken _incrementTarget;

        /// <summary>
        /// Initializes a new instance of the ForStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public ForStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given For loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.ForStatementContext context)
        {
            IntermediaryTokenList tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given For loop statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to tokenize to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.ForStatementContext context)
        {
            // FOR loop tokenization:
            // 1 - Loop start expression
            // 2 - Unconditional jump to Condition
            // 3 - Increment expression
            // 4 - Condition expression
            // 5 - Conditional jump to End
            // 6 - Body loop
            // 7 - Unconditional jump to Increment
            // 8 - End

            // Create the jump targets
            _conditionTarget = new JumpTargetToken();
            _incrementTarget = new JumpTargetToken();
            _forBlockEndTarget = new JumpTargetToken();

            _context.PushContinueTarget(_incrementTarget);
            _context.PushBreakTarget(_forBlockEndTarget);

            // 1 - Loop start expression
            var init = context.forInit();
            if (init != null)
                TokenizeForLoopInit(init, targetList);

            // 2 - Unconditional jump to Condition
            targetList.Add(new JumpToken(_conditionTarget));

            // 3 - Increment expression
            targetList.Add(_incrementTarget);

            var incr = context.forIncrement();
            if (incr != null)
            {
                _context.TokenizeExpression(targetList, incr.expression());
                // Add a clear stack to balance the stack back again
                targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
            }

            // 4 - Condition expression
            targetList.Add(_conditionTarget);

            var cond = context.forCondition();
            if (cond != null)
            {
                _context.TokenizeExpression(targetList, cond.expression());

                // 5 - Conditional jump to End
                targetList.Add(new JumpToken(_forBlockEndTarget, true, false));
            }

            // 6 - Body loop
            _context.TokenizeStatement(targetList, context.statement());

            // 7 - Unconditional jump to Increment
            targetList.Add(new JumpToken(_incrementTarget));

            // 8 - End
            targetList.Add(_forBlockEndTarget);

            // Pop the targets
            _context.PopContinueTarget();
            _context.PopBreakTarget();
        }

        /// <summary>
        /// Tokenizes a given For loop init into a given list of tokens
        /// </summary>
        /// <param name="init">The FOR loop init to tokenize</param>
        /// <param name="tokens">The list of tokens to tokenize to</param>
        private void TokenizeForLoopInit(ZScriptParser.ForInitContext init, IList<Token> tokens)
        {
            if (init.valueHolderDecl() != null)
            {
                _context.TokenizeValueDeclaration(tokens, init.valueHolderDecl());
            }
            else if (init.expression() != null)
            {
                _context.TokenizeExpression(tokens, init.expression());
            }
            else
            {
                _context.TokenizeAssignmentExpression(tokens, init.assignmentExpression());
            }

            // Add a stack balancing instruction
            tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
        }
    }
}