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
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Helper class used to aid in the tokenization process of an SWITCH statement
    /// </summary>
    public class SwitchStatementTokenizer : IParserContextTokenizer<ZScriptParser.SwitchStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a nested switch statement appears inside one of the cases
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents the last block before the end of the current switch blocks
        /// </summary>
        private JumpTargetToken _switchBlockEndTarget;

        /// <summary>
        /// Initializes a new instance of the IfStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public SwitchStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given SWITCH statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.SwitchStatementContext context)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given SWITCH statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to tokenize to</param>
        /// <param name="context">The context containinng</param>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.SwitchStatementContext context)
        {
            // Create the end switch block target
            _switchBlockEndTarget = new JumpTargetToken();
            // Create the default block jump target
            var defaultBlockTarget = new JumpTargetToken();
            // Set the break target now
            _context.PushBreakTarget(_switchBlockEndTarget);

            // Read the cases now
            var cases = new List<SwitchCaseStatement>();

            foreach (var c in context.switchBlock().caseBlock())
            {
                cases.Add(TokenizeCaseStatement(c));
            }

            // Constant switch evaluation
            if (context.IsConstant)
            {
                if (context.ConstantCaseIndex == -1)
                {
                    // Add the default block now
                    if (context.switchBlock().defaultBlock() != null)
                    {
                        foreach (var stmt in context.switchBlock().defaultBlock().statement())
                        {
                            _context.TokenizeStatement(targetList, stmt);
                        }
                    }
                }
                else
                {
                    targetList.AddRange(cases[context.ConstantCaseIndex].CaseStatementTokens);
                }

                // Stick the switch block end target at the end of the list so break statements don't result in invalid jumps
                targetList.Add(_switchBlockEndTarget);

                return;
            }

            // Add the switch expression
            if (context.expression() != null)
            {
                _context.TokenizeExpression(targetList, context.expression());
            }
            else
            {
                _context.TokenizeValueDeclaration(targetList, context.valueHolderDecl());
            }

            // Add the cases' comparisions
            foreach (var caseStatement in cases)
            {
                // Add a duplicate instruction so we can reuse the top-most value of the stack over many consecutive case comparisions
                targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));

                // Add the case expression now
                targetList.AddRange(caseStatement.ExpressionTokens);
                // Add the comparision operator
                targetList.Add(TokenFactory.CreateOperatorToken(caseStatement.ComparisionOperator));

                // Add the jump token
                targetList.Add(new JumpToken(caseStatement.JumpTarget, true));
            }

            // Add a jump that points to after the switch statement
            targetList.Add(new JumpToken(defaultBlockTarget));

            // Add the cases' statements now
            foreach (var caseStatement in cases)
            {
                targetList.Add(caseStatement.JumpTarget);
                targetList.AddRange(caseStatement.CaseStatementTokens);
            }

            // Add the default block target
            targetList.Add(defaultBlockTarget);

            // Add the default block now
            if (context.switchBlock().defaultBlock() != null)
            {
                foreach (var stmt in context.switchBlock().defaultBlock().statement())
                {
                    _context.TokenizeStatement(targetList, stmt);
                }
            }

            // Stick the switch block end target at the end of the list
            targetList.Add(_switchBlockEndTarget);

            // Add a stack balancing instruction
            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
        }

        /// <summary>
        /// Tokenizes a given CASE statement into a switch case statement container
        /// </summary>
        /// <param name="context">The context containing the ELSE statement to tokenize</param>
        private SwitchCaseStatement TokenizeCaseStatement(ZScriptParser.CaseBlockContext context)
        {
            var stmtTokens = new IntermediaryTokenList();

            foreach (var statement in context.statement())
            {
                stmtTokens.AddRange(_context.TokenizeStatement(statement));
            }

            var expTokens = new IntermediaryTokenList();
            _context.TokenizeExpression(expTokens, context.expression());

            return new SwitchCaseStatement(VmInstruction.Equals, expTokens, stmtTokens);
        }

        /// <summary>
        /// Class that encapsulates a switch case statement
        /// </summary>
        class SwitchCaseStatement
        {
            /// <summary>
            /// Gets the list of tokens that represent the case entry expression
            /// </summary>
            public IEnumerable<Token> ExpressionTokens { get; }

            /// <summary>
            /// Gets the list of tokens that represent the statements inside the case block
            /// </summary>
            public IEnumerable<Token> CaseStatementTokens { get; }

            /// <summary>
            /// Gets the comparision operator to apply to the switch expression result and this case's expression result
            /// </summary>
            public VmInstruction ComparisionOperator { get; }

            /// <summary>
            /// The jump target token for this case
            /// </summary>
            public JumpTargetToken JumpTarget { get; }

            /// <summary>
            /// Creates a new instance of the SwitchCaseStatement class
            /// </summary>
            /// <param name="comparisionOperator">The comparision operator to apply to the switch expression result and this case's expression result</param>
            /// <param name="expressionTokens">A aist of tokens that represent the case entry expression</param>
            /// <param name="caseStatementTokens">A list of tokens that represent the statements inside the case block</param>
            public SwitchCaseStatement(VmInstruction comparisionOperator, IEnumerable<Token> expressionTokens, IEnumerable<Token> caseStatementTokens)
            {
                ExpressionTokens = expressionTokens;
                CaseStatementTokens = caseStatementTokens;
                ComparisionOperator = comparisionOperator;
                JumpTarget = new JumpTargetToken();
            }
        }
    }
}