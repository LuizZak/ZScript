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

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ZScript.CodeGeneration.Messages;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Represents a class capable of analyzing the control flow of a function
    /// </summary>
    public class ControlFlowAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// The context for the current runtime generation this control flow is analyzing on
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// The current break statement depth - used during pre-analysis do detect invalid break statements
        /// </summary>
        private int _breakDepth;

        /// <summary>
        /// The current continue statement depth - used during pre-analysis do detect invalid continue statements
        /// </summary>
        private int _continueDepth;

        /// <summary>
        /// The context of the body of the function to analyze
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// Gets a value specifying whether the end of the function that was analyzed is reachable by any code path
        /// </summary>
        public bool IsEndReachable { get; private set; }

        /// <summary>
        /// Gets a list of all the return statements of the currently processed function
        /// </summary>
        public List<ZScriptParser.ReturnStatementContext> ReturnStatements { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ControlFlowAnalyzer class
        /// </summary>
        /// <param name="generationContext">The context for the current runtime generation this control flow is analyzing on</param>
        /// <param name="bodyContext">The context of the body of the function to analyze</param>
        public ControlFlowAnalyzer(RuntimeGenerationContext generationContext, ZScriptParser.FunctionBodyContext bodyContext)
        {
            _generationContext = generationContext;
            _bodyContext = bodyContext;
            ReturnStatements = new List<ZScriptParser.ReturnStatementContext>();
        }

        /// <summary>
        /// Analyzes the control flow of the current body context
        /// </summary>
        public void Analyze()
        {
            // TODO: Break this method into smaller, more manageable methods

            // Visit the statements, resetting their reachability
            var walker = new ParseTreeWalker();
            walker.Walk(this, _bodyContext);

            // Analyze the reachability now
            IsEndReachable = false;

            if (_bodyContext.blockStatement().statement().Length == 0)
            {
                IsEndReachable = true;
                return;
            }

            var endReturnFlow = new ControlFlowPointer(new ZScriptParser.StatementContext[0], 0);

            var visitedExpressions = new List<ZScriptParser.StatementContext>();
            var statementStack = new Stack<ControlFlowPointer>();

            statementStack.Push(new ControlFlowPointer(_bodyContext.blockStatement().statement(), 0, backTarget: endReturnFlow));

            while (statementStack.Count > 0)
            {
                var flow = statementStack.Pop();

                // Reachable end detected
                if (flow == endReturnFlow)
                {
                    IsEndReachable = true;
                    continue;
                }

                var stmts = flow.Statements;
                var index = flow.StatementIndex;

                bool quitBranch = false;
                for (int i = index; i < stmts.Length; i++)
                {
                    quitBranch = false;
                    var breakTarget = flow.BreakTarget;
                    var continueTarget = flow.ContinueTarget;
                    var returnFlow = flow.BackTarget;

                    var stmt = stmts[i];

                    if (visitedExpressions.ContainsReference(stmt))
                    {
                        quitBranch = true;
                        if(stmt.breakStatement() == null && stmt.returnStatement() == null && stmt.continueStatement() == null)
                            continue;

                        break;
                    }

                    visitedExpressions.Add(stmt);

                    stmt.Reachable = true;

                    // Return statement
                    if (stmt.returnStatement() != null)
                    {
                        ReturnStatements.Add(stmt.returnStatement());

                        quitBranch = true;
                        break;
                    }

                    // Break statement
                    if (stmt.breakStatement() != null && breakTarget != null)
                    {
                        statementStack.Push(breakTarget);
                        quitBranch = true;
                        break;
                    }
                    // Continue statement
                    if (stmt.continueStatement() != null && continueTarget != null)
                    {
                        statementStack.Push(continueTarget);
                        quitBranch = true;
                        break;
                    }

                    // Block statement
                    if (stmt.blockStatement() != null)
                    {
                        statementStack.Push(new ControlFlowPointer(stmt.blockStatement().statement(), 0, breakTarget, continueTarget, flow.BackTarget));
                        quitBranch = true;
                        break;
                    }

                    // Branching if
                    var ifStatement = stmt.ifStatement();
                    if (ifStatement != null)
                    {
                        // Constant if
                        if (ifStatement.IsConstant)
                        {
                            if(ifStatement.ConstantValue)
                            {
                                // Queue the if
                                statementStack.Push(new ControlFlowPointer(new[] { ifStatement.statement() }, 0, breakTarget, continueTarget));
                            }
                            else if(ifStatement.elseStatement() != null)
                            {
                                var elseStatements = new[] { ifStatement.elseStatement().statement() };

                                // Queue the else
                                statementStack.Push(new ControlFlowPointer(elseStatements, 0, breakTarget, continueTarget));
                            }
                            else
                            {
                                continue;
                            }

                            quitBranch = true;
                            break;
                        }

                        returnFlow = new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, returnFlow);

                        if (ifStatement.elseStatement() != null)
                        {
                            var elseStatements = new[] { ifStatement.elseStatement().statement() };

                            // Queue the else
                            statementStack.Push(new ControlFlowPointer(elseStatements, 0, breakTarget, continueTarget, returnFlow));
                        }
                        else
                        {
                            statementStack.Push(new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget));
                        }

                        // Queue the if
                        statementStack.Push(new ControlFlowPointer(new[] { ifStatement.statement() }, 0, breakTarget, continueTarget, returnFlow));

                        quitBranch = true;
                        break;
                    }

                    // Switch statement
                    var switchStatement = stmt.switchStatement();
                    if (switchStatement != null)
                    {
                        // Set the break target
                        breakTarget = new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget)
                        {
                            Context = switchStatement
                        };

                        bool hasDefault = switchStatement.switchBlock().defaultBlock() != null;
                        var defaultBlock = switchStatement.switchBlock().defaultBlock();

                        // Build the array of case label statements now
                        var caseBlocks = switchStatement.switchBlock().caseBlock();
                        var offsets = new int[caseBlocks.Length + (hasDefault ? 1 : 0)];
                        var caseStatements = new List<ZScriptParser.StatementContext>();

                        for (int ci = 0; ci < caseBlocks.Length; ci++)
                        {
                            offsets[ci] = caseStatements.Count;
                            caseStatements.AddRange(caseBlocks[ci].statement());
                        }

                        // Add default block
                        if (switchStatement.switchBlock().defaultBlock() != null)
                        {
                            offsets[caseBlocks.Length] = caseStatements.Count;
                            caseStatements.AddRange(defaultBlock.statement());
                        }

                        // Build the case control flows now
                        var caseStatementsArray = caseStatements.ToArray();
                        var caseControlFlows = new List<ControlFlowPointer>();

                        for (int ci = 0; ci < caseBlocks.Length; ci++)
                        {
                            var caseFlow = new ControlFlowPointer(caseStatementsArray, offsets[ci], breakTarget, continueTarget);
                            caseControlFlows.Add(caseFlow);
                        }

                        // Constant switch
                        if (switchStatement.IsConstant)
                        {
                            if (switchStatement.ConstantCaseIndex > -1)
                            {
                                statementStack.Push(caseControlFlows[switchStatement.ConstantCaseIndex]);
                            }
                            else if (hasDefault)
                            {
                                returnFlow = new ControlFlowPointer(stmts, i + 1, flow.BackTarget, continueTarget, returnFlow);
                                statementStack.Push(new ControlFlowPointer(caseStatementsArray, offsets[offsets.Length - 1], breakTarget, continueTarget, returnFlow));
                            }
                            else
                            {
                                continue;
                            }

                            quitBranch = true;
                            break;
                        }

                        // Deal with default: if it exists, ommit the after-switch control flow resume, if not, append it to the statements
                        if (hasDefault)
                        {
                            returnFlow = new ControlFlowPointer(stmts, i + 1, flow.BackTarget, continueTarget, returnFlow);
                            var defaultFlow = new ControlFlowPointer(caseStatementsArray, offsets[offsets.Length - 1], breakTarget, continueTarget, returnFlow);
                            caseControlFlows.Add(defaultFlow);

                            // Dump the flows on the statements stack
                            foreach (var caseFlow in caseControlFlows)
                            {
                                statementStack.Push(caseFlow);
                            }

                            quitBranch = true;
                            break;
                        }

                        // Push the outer switch
                        statementStack.Push(new ControlFlowPointer(stmts, i + 1, flow.BackTarget, continueTarget, flow.BackTarget));

                        // Dump the flows on the statements stack
                        foreach (var caseFlow in caseControlFlows)
                        {
                            statementStack.Push(caseFlow);
                        }

                        quitBranch = true;
                        break;
                    }

                    // Branching while loop
                    var whileStatement = stmt.whileStatement();
                    if (whileStatement != null)
                    {
                        breakTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = whileStatement };
                        continueTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = whileStatement };

                        if (whileStatement.IsConstant)
                        {
                            if (whileStatement.ConstantValue)
                            {
                                // Push the while
                                statementStack.Push(new ControlFlowPointer(new[] {whileStatement.statement()}, 0, breakTarget, continueTarget));

                                quitBranch = true;
                                break;
                            }

                            continue;
                        }

                        // Push the next statement after the loop, along with a break statement
                        statementStack.Push(new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget));

                        returnFlow = new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, returnFlow);
                        statementStack.Push(new ControlFlowPointer(new[] { whileStatement.statement() }, 0, breakTarget, continueTarget, returnFlow));

                        quitBranch = true;
                        break;
                    }

                    // Branching for loop
                    var forStatement = stmt.forStatement();
                    if (forStatement != null)
                    {
                        breakTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = forStatement };
                        continueTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = forStatement };

                        if (forStatement.forCondition() == null || forStatement.forCondition().IsConstant)
                        {
                            bool constValue = forStatement.forCondition() == null || forStatement.forCondition().ConstantValue;
                            if (constValue)
                            {
                                // Push the for
                                statementStack.Push(new ControlFlowPointer(new[] { forStatement.statement() }, 0, breakTarget, continueTarget));

                                quitBranch = true;
                                break;
                            }

                            continue;
                        }

                        // Push the next statement after the loop, along with a break statement
                        statementStack.Push(new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget));

                        // For statement
                        returnFlow = new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, returnFlow);
                        statementStack.Push(new ControlFlowPointer(new[] { forStatement.statement() }, 0, breakTarget, continueTarget, returnFlow));

                        quitBranch = true;
                        break;
                    }
                }

                if (quitBranch)
                    continue;

                
                // End of function - mark end as reachable
                if (flow.BackTarget == null)
                {
                    //IsEndReachable = true;
                    continue;
                }

                statementStack.Push(flow.BackTarget);
            }
        }

        /// <summary>
        /// Enters the given statement, marking it as unreachable by default.
        /// This listener override is used during the beginning of the analysis to mark all statements of the block as unreachable
        /// </summary>
        /// <param name="context">The context to reset</param>
        public override void EnterStatement(ZScriptParser.StatementContext context)
        {
            context.Reachable = false;
        }

        #region Continue/break mismatched statement detection

        /// <summary>
        /// Enters a given break statement, analyzing whether it has a current valid target
        /// </summary>
        /// <param name="context">The context for the break statement</param>
        public override void EnterBreakStatement(ZScriptParser.BreakStatementContext context)
        {
            if (_breakDepth <= 0 && _generationContext.MessageContainer != null)
            {
                _generationContext.MessageContainer.RegisterError(context, "No target for break statement", ErrorCode.NoTargetForBreakStatement);
            }
        }

        /// <summary>
        /// Enters a given continue statement, analyzing whether it has a current valid target
        /// </summary>
        /// <param name="context">The context for the continue statement</param>
        public override void EnterContinueStatement(ZScriptParser.ContinueStatementContext context)
        {
            if (_continueDepth <= 0 && _generationContext.MessageContainer != null)
            {
                _generationContext.MessageContainer.RegisterError(context, "No target for continue statement", ErrorCode.NoTargetForContinueStatement);
            }
        }

        /// <summary>
        /// Enters the given for statement, increasing the break and continue depth along the way
        /// </summary>
        /// <param name="context">The for statement context to enter</param>
        public override void EnterForStatement(ZScriptParser.ForStatementContext context)
        {
            _breakDepth++;
            _continueDepth++;
        }

        /// <summary>
        /// Exits the given for statement, decreasing the break and continue depth along the way
        /// </summary>
        /// <param name="context">The for statement context to exit</param>
        public override void ExitForStatement(ZScriptParser.ForStatementContext context)
        {
            _breakDepth--;
            _continueDepth--;
        }

        /// <summary>
        /// Enters the given while statement, increasing the break and continue depth along the way
        /// </summary>
        /// <param name="context">The while statement context to enter</param>
        public override void EnterWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            _breakDepth++;
            _continueDepth++;
        }

        /// <summary>
        /// Exits the given while statement, decreasing the break and continue depth along the way
        /// </summary>
        /// <param name="context">The while statement context to exit</param>
        public override void ExitWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            _breakDepth--;
            _continueDepth--;
        }

        /// <summary>
        /// Enters the given switch statement, increasing the break depth along the way
        /// </summary>
        /// <param name="context">The switch statement context to enter</param>
        public override void EnterSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            _breakDepth++;
        }

        /// <summary>
        /// Exits the given switch statement, decreasing the break depth along the way
        /// </summary>
        /// <param name="context">The switch statement context to exit</param>
        public override void ExitSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            _breakDepth--;
        }

        #endregion

        /// <summary>
        /// Class used to represent a control flow head
        /// </summary>
        private class ControlFlowPointer
        {
            /// <summary>
            /// Special context that can be attributed to the control flow pointer
            /// </summary>
            public ParserRuleContext Context;

            /// <summary>
            /// The statements the control flow is flowing throgh
            /// </summary>
            public readonly ZScriptParser.StatementContext[] Statements;

            /// <summary>
            /// The target for a break statement
            /// </summary>
            public readonly ControlFlowPointer BreakTarget;

            /// <summary>
            /// The target for a continue statement
            /// </summary>
            public readonly ControlFlowPointer ContinueTarget;

            /// <summary>
            /// The target to point the back jump to, when returning up the code flow
            /// </summary>
            public readonly ControlFlowPointer BackTarget;

            /// <summary>
            /// The index of the statement the control flow is pointing to
            /// </summary>
            public readonly int StatementIndex;

            /// <summary>
            /// Initializes a new instance of the ControlFlowPointer class
            /// </summary>
            /// <param name="statements">The array of statements this control flow is flowing through</param>
            /// <param name="statementIndex">The index of the statement the control flow is pointing to</param>
            /// <param name="breakTarget">The control flow to jump to when a break is reached</param>
            /// <param name="continueTarget">The control flow to jump to when a continue is reached</param>
            /// <param name="backTarget">The target to point the back jump to, when returning up the code flow</param>
            public ControlFlowPointer(ZScriptParser.StatementContext[] statements, int statementIndex,
                ControlFlowPointer breakTarget = null, ControlFlowPointer continueTarget = null,
                ControlFlowPointer backTarget = null)
            {
                Statements = statements;
                StatementIndex = statementIndex;
                BackTarget = backTarget;
                BreakTarget = breakTarget;
                ContinueTarget = continueTarget;
            }
        }
    }
}