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
            _context = context;

            var funcs = context.BaseScope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

            foreach (var func in funcs)
            {
                // Functions missing bodies are not analyzed
                if (func.BodyContext == null)
                    continue;

                _currentDefinition = func;

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
            _context = context;

            var funcs = context.BaseScope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

            foreach (var func in funcs)
            {
                // Functions missing bodies are not analyzed
                if (func.BodyContext == null)
                    continue;

                //AnalyzeFunction(func);
                NewAnalyzeFunction(func);
            }
        }

        /// <summary>
        /// Analyzes a given function definition
        /// </summary>
        /// <param name="func">The function definition to analyze</param>
        private void NewAnalyzeFunction(FunctionDefinition func)
        {
            _currentDefinition = func;

            var analyzer = new ControlFlowAnalyzer(_context, func.BodyContext);
            analyzer.Analyze();

            // Incomplete return paths
            if (func.HasReturnType && !func.IsVoid && analyzer.IsEndReachable)
            {
                var message = "Not all code paths of non-void function '" + func.Name + "' return a value";
                Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column, message, ErrorCode.IncompleteReturnPaths, func.Context);
            }

            _returnStatements = func.ReturnStatements;

            // Basic loop over returns to verify compatibility
            foreach (var rsc in _returnStatements)
            {
                var context = rsc;

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
                /*else if (!func.HasReturnType && valuedReturn && analyzer.EndReachable)
                {
                    Container.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Function '" + func.Name + "' has inconsistent returns: Not all paths have valued returns.", ErrorCode.IncompleteReturnPathsWithValuedReturn, func.Context);
                }*/
            }

            func.ReturnStatements = _returnStatements;
        }

        /// <summary>
        /// Analyzes a given block statement
        /// </summary>
        private void AnalyzeBlockStatement(ZScriptParser.BlockStatementContext block)
        {
            var statements = block.statement();

            if (statements.Length == 0)
                return;

            foreach (var stmt in statements)
            {
                AnalyzeStatement(stmt);
            }
        }

        /// <summary>
        /// Analyzes a given statement context for return statement state
        /// </summary>
        private void AnalyzeStatement(ZScriptParser.StatementContext context)
        {
            if (context.returnStatement() != null)
                AnalyzeReturnStatement(context.returnStatement());

            if (context.ifStatement() != null)
                AnalyzeIfStatement(context.ifStatement());

            if (context.switchStatement() != null)
                AnalyzeSwitchStatement(context.switchStatement());

            if (context.whileStatement() != null)
                AnalyzeWhileStatement(context.whileStatement());

            if (context.forStatement() != null)
                AnalyzeForStatement(context.forStatement());

            if (context.blockStatement() != null)
                AnalyzeBlockStatement(context.blockStatement());
        }

        /// <summary>
        /// Analyzes a given return statement. Always returns ReturnStatementState.Present
        /// </summary>
        private void AnalyzeReturnStatement(ZScriptParser.ReturnStatementContext context)
        {
            context.ReturnType = CurrentDefinition.ReturnType;

            _returnStatements.Add(context);
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private void AnalyzeIfStatement(ZScriptParser.IfStatementContext context)
        {
            // If's start with their state set as the nested statement, and the
            // final type resolving depends on whether there is an else clause.
            AnalyzeStatement(context.statement());

            var elseStatement = context.elseStatement();
            // In case of missing else clauses, the return is either partial if it is 
            if (elseStatement != null)
            {
                AnalyzeStatement(elseStatement.statement());
            }
        }

        /// <summary>
        /// Analyzes a given WHILE statement context for return statement state
        /// </summary>
        private void AnalyzeWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            AnalyzeStatement(context.statement());
        }

        /// <summary>
        /// Analyzes a given FOR statement context for return statement state
        /// </summary>
        private void AnalyzeForStatement(ZScriptParser.ForStatementContext context)
        {
            AnalyzeStatement(context.statement());
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private void AnalyzeSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            var block = context.switchBlock();
            var cases = block.caseBlock();
            var def = block.defaultBlock();

            foreach (var cbc in cases)
            {
                foreach (var statementContext in cbc.statement())
                {
                    AnalyzeStatement(statementContext);
                }
            }

            // Default case: if it's present, merge the returns
            if (def != null)
            {
                foreach (var statementContext in def.statement())
                {
                     AnalyzeStatement(statementContext);
                }
            }
        }

        /// <summary>
        /// Whether the current return value is void
        /// </summary>
        /// <returns>Whether the current return value is void</returns>
        private bool IsCurrentReturnValueVoid()
        {
            return CurrentDefinition.IsVoid;
        }
    }
}