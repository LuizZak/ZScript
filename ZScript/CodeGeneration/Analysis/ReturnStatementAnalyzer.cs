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

using System.Collections.Generic;
using System.Linq;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Analyzes the return statements in the body of a function
    /// </summary>
    public class ReturnStatementAnalyzer
    {
        /// <summary>
        /// List of all the return statements of the currently processed function
        /// </summary>
        private List<ZScriptParser.ReturnStatementContext> _returnStatements;

        /// <summary>
        /// The current message container for the analyzer
        /// </summary>
        private MessageContainer _messageContainer;

        /// <summary>
        /// The current function definition being analyzed
        /// </summary>
        private FunctionDefinition _currentDefinition;

        /// <summary>
        /// Analyzes a given scope for functions with mismatching return statements
        /// </summary>
        /// <param name="scope">The scope to analyze</param>
        /// <param name="messageContainer">The container to report the error messages to</param>
        public void AnalyzeScope(CodeScope scope, MessageContainer messageContainer)
        {
            _messageContainer = messageContainer;

            var funcs = scope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

            foreach (var func in funcs)
            {
                // Functions missing bodies are not analyzed
                if (func.BodyContext == null)
                    continue;

                AnalyzeFunction(func);
            }
        }

        /// <summary>
        /// Analyzes a given function definition
        /// </summary>
        /// <param name="func">The function definition to analyze</param>
        /// <returns>The return statement state for the function</returns>
        private void AnalyzeFunction(FunctionDefinition func)
        {
            var context = func.BodyContext;

            _currentDefinition = func;

            // Check for inconsistent return statement valuation
            _returnStatements = new List<ZScriptParser.ReturnStatementContext>();

            if (context.blockStatement().statement().Length == 0)
            {
                if (func.HasReturnType && !func.IsVoid)
                {
                    _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
                }

                return;
            }

            var block = context.blockStatement();
            var state = AnalyzeBlockStatement(block);

            if (state == ReturnStatementState.Partial)
            {
                _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                    "Not all code paths of function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
            }
            else if (func.HasReturnType && !func.IsVoid && state != ReturnStatementState.Complete)
            {
                _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                    "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
            }

            // Check inconsistencies on return types
            if (!func.HasReturnType && _returnStatements.Count > 0)
            {
                ZScriptParser.ReturnStatementContext last = _returnStatements[0];
                bool valuedReturn = false;
                bool inconsistentReturn = false;
                foreach (var rsc in _returnStatements)
                {
                    valuedReturn = valuedReturn || (rsc.expression() != null);
                    if ((last.expression() == null) != (rsc.expression() == null))
                    {
                        inconsistentReturn = true;
                        break;
                    }

                    last = rsc;
                }

                // Report inconsistent returns
                if (inconsistentReturn)
                {
                    _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Function '" + func.Name + "' has inconsistent returns: Some returns are void, some are valued.", ErrorCode.InconsistentReturns, context);
                }
                // Check for early-returns on functions that have a missing return value
                else if (!func.HasReturnType && valuedReturn && state != ReturnStatementState.Complete)
                {
                    _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Function '" + func.Name + "' has inconsistent returns: Not all paths have valued returns.", ErrorCode.IncompleteReturnPathsWithValuedReturn, context);
                }
            }

            _currentDefinition.ReturnStatements = _returnStatements;
        }

        /// <summary>
        /// Analyzes a given block statement
        /// </summary>
        private ReturnStatementState AnalyzeBlockStatement(ZScriptParser.BlockStatementContext block)
        {
            var statements = block.statement();

            if (statements.Length == 0)
                return ReturnStatementState.NotPresent;

            var state = ReturnStatementState.DoesNotApply;

            foreach (var stmt in statements)
            {
                var blockState = AnalyzeStatement(stmt);

                if (blockState == ReturnStatementState.Complete)
                    return blockState;

                state = MergeReturnStates(state, blockState);
            }

            return state;
        }

        /// <summary>
        /// Analyzes a given statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeStatement(ZScriptParser.StatementContext context)
        {
            if (context.returnStatement() != null)
                return AnalyzeReturnStatement(context.returnStatement());

            if (context.ifStatement() != null)
                return AnalyzeIfStatement(context.ifStatement());

            if (context.switchStatement() != null)
                return AnalyzeSwitchStatement(context.switchStatement());

            if (context.whileStatement() != null)
                return AnalyzeWhileStatement(context.whileStatement());

            if (context.forStatement() != null)
                return AnalyzeForStatement(context.forStatement());

            if (context.blockStatement() != null)
                return AnalyzeBlockStatement(context.blockStatement());

            return ReturnStatementState.DoesNotApply;
        }

        /// <summary>
        /// Analyzes a given return statement. Always returns ReturnStatementState.Present
        /// </summary>
        private ReturnStatementState AnalyzeReturnStatement(ZScriptParser.ReturnStatementContext context)
        {
            if (IsCurrentReturnValueVoid())
            {
                if (context.expression() != null)
                {
                    _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Trying to return a value on a void context", ErrorCode.ReturningValueOnVoidFunction, context);
                }
            }
            else if (_currentDefinition.HasReturnType && context.expression() == null)
            {
                _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Return value is missing in non-void context", ErrorCode.MissingReturnValueOnNonvoid, context);
            }

            _returnStatements.Add(context);
            return ReturnStatementState.Complete;
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeIfStatement(ZScriptParser.IfStatementContext context)
        {
            // If's are always partial
            var state = AnalyzeStatement(context.statement());

            var elseStatement = context.elseStatement();

            if (elseStatement == null)
            {
                return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
            }

            state = MergeReturnStates(state, AnalyzeStatement(elseStatement.statement()));

            return state;
        }

        /// <summary>
        /// Analyzes a given WHILE statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            // If's are always partial
            var state = AnalyzeStatement(context.statement());

            // Loop statements are always partial
            return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
        }

        /// <summary>
        /// Analyzes a given FOR statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeForStatement(ZScriptParser.ForStatementContext context)
        {
            // If's are always partial
            var state = AnalyzeStatement(context.statement());

            // Loop statements are always partial
            return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            var block = context.switchBlock();
            var cases = block.caseBlock();
            var def = block.defaultBlock();

            var state = ReturnStatementState.DoesNotApply;
            foreach (var cbc in cases)
            {
                foreach (var statementContext in cbc.statement())
                {
                    state = MergeReturnStates(state, AnalyzeStatement(statementContext));
                }
            }

            if (def != null)
            {
                foreach (var statementContext in def.statement())
                {
                    state = MergeReturnStates(state, AnalyzeStatement(statementContext));
                }
            }
            else
            {
                return MergeReturnStates(ReturnStatementState.NotPresent, state);
            }

            return state == ReturnStatementState.DoesNotApply ? ReturnStatementState.NotPresent : state;
        }

        /// <summary>
        /// Merges two return statement states
        /// </summary>
        private ReturnStatementState MergeReturnStates(ReturnStatementState state1, ReturnStatementState state2)
        {
            if (state1 == ReturnStatementState.DoesNotApply)
                return state2;

            if (state2 == ReturnStatementState.DoesNotApply)
                return state1;

            if (state1 == ReturnStatementState.Complete && state2 == ReturnStatementState.Complete)
                return ReturnStatementState.Complete;

            if (state1 == ReturnStatementState.NotPresent && state2 == ReturnStatementState.NotPresent)
                return ReturnStatementState.NotPresent;

            return ReturnStatementState.Partial;
        }

        /// <summary>
        /// Whether the current return value is void
        /// </summary>
        /// <returns>Whether the current return value is void</returns>
        private bool IsCurrentReturnValueVoid()
        {
            return _currentDefinition.IsVoid;
        }

        /// <summary>
        /// Defines the return statement state for a block statement
        /// </summary>
        private enum ReturnStatementState
        {
            /// <summary>The state does not apply to the statement</summary>
            DoesNotApply,
            /// <summary>A return statement is present in all sub statements</summary>
            Complete,
            /// <summary>A return statement is not present in any sub statement</summary>
            NotPresent,
            /// <summary>A return statement is partial (or conditional) in one or more of the substatements</summary>
            Partial
        }
    }
}