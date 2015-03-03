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
using System.Linq;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Analyzes the return statements in the body of a function
    /// </summary>
    public class ReturnStatementAnalyzer
    {
        /// <summary>
        /// Whether to enable constant statements resolving
        /// </summary>
        private readonly bool _constantStatements = false;

        /// <summary>
        /// List of all the return statements of the currently processed function
        /// </summary>
        private List<ZScriptParser.ReturnStatementContext> _returnStatements;

        /// <summary>
        /// The current function definition being analyzed
        /// </summary>
        private FunctionDefinition _currentDefinition;

        /// <summary>
        /// The context for the analysis
        /// </summary>
        private RuntimeGenerationContext _context;

        /// <summary>
        /// Whether the current analysis mode is set to collect or analyze
        /// </summary>
        private bool _collecting;

        /// <summary>
        /// The current message container for the analyzer
        /// </summary>
        private MessageContainer Container
        {
            get { return _context.MessageContainer; }
        }

        /// <summary>
        /// The current function definition being analyzed
        /// </summary>
        private FunctionDefinition CurrentDefinition
        {
            get { return _currentDefinition; }
        }

        /// <summary>
        /// Collects all the returns in the respective function definitions
        /// </summary>
        public void CollectReturnsOnDefinitions(RuntimeGenerationContext context)
        {
            // Check for inconsistent return statement valuation
            _collecting = true;
            _context = context;

            var funcs = context.BaseScope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

            foreach (var func in funcs)
            {
                // Functions missing bodies are not analyzed
                if (func.BodyContext == null)
                    continue;

                var c = func.BodyContext;

                _returnStatements = new List<ZScriptParser.ReturnStatementContext>();

                AnalyzeBlockStatement(c.blockStatement());

                func.ReturnStatements = _returnStatements;
            }
        }

        /// <summary>
        /// Analyzes a given scope for functions with mismatching return statements
        /// </summary>
        /// <param name="context">The context containing the scope to analyze and message container to report errors and warnings to</param>
        public void Analyze(RuntimeGenerationContext context)
        {
            _collecting = false;
            _context = context;

            var funcs = context.BaseScope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

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

            _returnStatements = new List<ZScriptParser.ReturnStatementContext>();
            _currentDefinition = func;

            if (context.blockStatement().statement().Length == 0)
            {
                if (func.HasReturnType && !func.IsVoid)
                {
                    Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, func.Context);
                }

                return;
            }

            var block = context.blockStatement();
            var state = AnalyzeBlockStatement(block);

            if (state == ReturnStatementState.Partial && !func.IsVoid)
            {
                Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                    "Not all code paths of function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, func.Context);
            }
            else if (func.HasReturnType && !func.IsVoid && state != ReturnStatementState.Complete)
            {
                Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                    "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, func.Context);
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
                    Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Function '" + func.Name + "' has inconsistent returns: Some returns are void, some are valued.", ErrorCode.InconsistentReturns, func.Context);
                }
                // Check for early-returns on functions that have a missing return value
                else if (!func.HasReturnType && valuedReturn && state != ReturnStatementState.Complete)
                {
                    Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Function '" + func.Name + "' has inconsistent returns: Not all paths have valued returns.", ErrorCode.IncompleteReturnPathsWithValuedReturn, func.Context);
                }
            }

            func.ReturnStatements = _returnStatements;
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
            if(_collecting)
            {
                _returnStatements.Add(context);
                return ReturnStatementState.Complete;
            }

            if (IsCurrentReturnValueVoid())
            {
                if (context.expression() != null)
                {
                    Container.RegisterError(context.Start.Line, context.Start.Column, "Trying to return a value on a void context", ErrorCode.ReturningValueOnVoidFunction, context);
                }
            }
            else if (CurrentDefinition.HasReturnType && context.expression() == null)
            {
                Container.RegisterError(context.Start.Line, context.Start.Column, "Return value is missing in non-void context", ErrorCode.MissingReturnValueOnNonvoid, context);
            }

            _returnStatements.Add(context);
            return ReturnStatementState.Complete;
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeIfStatement(ZScriptParser.IfStatementContext context)
        {
            var elseStatement = context.elseStatement();
            
            // Constant ifs simply return the state of the statements within; the result is only NotPresent if an else is not present
            if (_constantStatements && context.IsConstant)
            {
                if (context.ConstantValue)
                {
                    return AnalyzeStatement(context.statement());
                }
                if(elseStatement != null)
                {
                    return AnalyzeStatement(elseStatement.statement());
                }

                return ReturnStatementState.NotPresent;
            }
            
            // If's start with their state set as the nested statement, and the
            // final type resolving depends on whether there is an else clause.
            var state = AnalyzeStatement(context.statement());            

            // In case of missing else clauses, the return is either partial if it is 
            if (elseStatement == null)
            {
                return MergeReturnStates(state, ReturnStatementState.NotPresent);
            }

            state = MergeReturnStates(state, AnalyzeStatement(elseStatement.statement()));

            return state;
        }

        /// <summary>
        /// Analyzes a given WHILE statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            // Constant whiles simply return the state of the statements within
            if (_constantStatements && context.IsConstant)
            {
                if (context.ConstantValue)
                {
                    return AnalyzeStatement(context.statement());
                }

                return ReturnStatementState.NotPresent;
            }

            var state = AnalyzeStatement(context.statement());

            // Loop statements are always partial
            return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
        }

        /// <summary>
        /// Analyzes a given FOR statement context for return statement state
        /// </summary>
        private ReturnStatementState AnalyzeForStatement(ZScriptParser.ForStatementContext context)
        {
            // Constant whiles simply return the state of the statements within
            if (_constantStatements && (context.forCondition() == null || context.forCondition().IsConstant))
            {
                if (context.forCondition() == null || (context.forCondition().IsConstant && context.forCondition().ConstantValue))
                {
                    return AnalyzeStatement(context.statement());
                }

                return ReturnStatementState.NotPresent;
            }
            
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

            // Constant switch case
            // TODO: Continue implementing constant switch resolving
            if (_constantStatements && context.IsConstant)
            {
                if (context.ConstantCaseIndex > -1)
                {
                    
                }
            }

            var state = ReturnStatementState.DoesNotApply;
            foreach (var cbc in cases)
            {
                foreach (var statementContext in cbc.statement())
                {
                    state = MergeReturnStates(state, AnalyzeStatement(statementContext));
                }
            }

            // Default case: if it's present, merge the returns
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
            return CurrentDefinition.IsVoid;
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