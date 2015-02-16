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
    /// Helper class used to aid in the tokenization process of an SWITCH statement
    /// </summary>
    public class SwitchStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
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
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        public IntermediateTokenList TokenizeStatement(ZScriptParser.SwitchStatementContext context)
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

            var tokens = new IntermediateTokenList();

            // Add the switch expression
            tokens.AddRange(_context.TokenizeExpression(context.expression()));

            // Add the cases' comparisions
            foreach (var caseStatement in cases)
            {
                // Add a duplicate instruction so we can reuse the top-most value of the stack over many consecutive case comparisions
                tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));

                // Add the case expression now
                tokens.AddRange(caseStatement.ExpressionTokens);
                // Add the comparision operator
                tokens.Add(TokenFactory.CreateOperatorToken(caseStatement.ComparisionOperator));

                // Add the jump token
                tokens.Add(new JumpToken(caseStatement.JumpTarget, true));
            }

            // Add a jump that points to after the switch statement
            tokens.Add(new JumpToken(defaultBlockTarget));

            // Add the cases' statements now
            foreach (var caseStatement in cases)
            {
                tokens.Add(caseStatement.JumpTarget);
                tokens.AddRange(caseStatement.CaseStatementTokens);
            }

            // Add the default block target
            tokens.Add(defaultBlockTarget);

            // Add the default block now
            if (context.switchBlock().defaultBlock() != null)
            {
                foreach (var stmt in context.switchBlock().defaultBlock().statement())
                {
                    tokens.AddRange(_context.TokenizeStatement(stmt));
                }
            }

            // Stick the switch block end target at the end of the list
            tokens.Add(_switchBlockEndTarget);

            return tokens;
        }

        /// <summary>
        /// Tokenizes a given ELSE statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the ELSE statement to tokenize</param>
        private SwitchCaseStatement TokenizeCaseStatement(ZScriptParser.CaseBlockContext context)
        {
            var stmtTokens = new IntermediateTokenList();

            foreach (var statement in context.statement())
            {
                stmtTokens.AddRange(_context.TokenizeStatement(statement));
            }

            return new SwitchCaseStatement(VmInstruction.Equals, _context.TokenizeExpression(context.expression()), stmtTokens);
        }

        /// <summary>
        /// Class that encapsulates a switch case statement
        /// </summary>
        class SwitchCaseStatement
        {
            /// <summary>
            /// Gets the list of tokens that represent the case entry expression
            /// </summary>
            public IntermediateTokenList ExpressionTokens { get; private set; }

            /// <summary>
            /// Gets the list of tokens that represent the statements inside the case block
            /// </summary>
            public IntermediateTokenList CaseStatementTokens { get; private set; }

            /// <summary>
            /// Gets the comparision operator to apply to the switch expression result and this case's expression result
            /// </summary>
            public VmInstruction ComparisionOperator { get; private set; }

            /// <summary>
            /// The jump target token for this case
            /// </summary>
            public JumpTargetToken JumpTarget { get; private set; }

            /// <summary>
            /// Creates a new instance of the SwitchCaseStatement class
            /// </summary>
            /// <param name="comparisionOperator">The comparision operator to apply to the switch expression result and this case's expression result</param>
            /// <param name="expressionTokens">A aist of tokens that represent the case entry expression</param>
            /// <param name="caseStatementTokens">A list of tokens that represent the statements inside the case block</param>
            public SwitchCaseStatement(VmInstruction comparisionOperator, IntermediateTokenList expressionTokens, IntermediateTokenList caseStatementTokens)
            {
                ExpressionTokens = expressionTokens;
                CaseStatementTokens = caseStatementTokens;
                ComparisionOperator = comparisionOperator;
                JumpTarget = new JumpTargetToken();
            }
        }
    }
}