using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

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
        /// The context of the body of the function to analyze
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// Gets a value specifying whether the end of the function that was analyzed is reachable by any code path
        /// </summary>
        public bool EndReachable { get; private set; }

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
            // Visit the statements, resetting their reachability
            var walker = new ParseTreeWalker();
            walker.Walk(this, _bodyContext);

            // Analyze the reachability now
            EndReachable = false;

            if (_bodyContext.blockStatement().statement().Length == 0)
            {
                EndReachable = true;
                return;
            }

            var statementStack = new Stack<ControlFlowPointer>();

            statementStack.Push(new ControlFlowPointer(_bodyContext.blockStatement().statement(), 0));

            while (statementStack.Count > 0)
            {
                var flow = statementStack.Pop();
                var index = flow.StatementIndex;

                var stmts = flow.Statements;

                bool quitBranch = false;
                for (int i = index; i < stmts.Length; i++)
                {
                    var breakTarget = flow.BreakTarget;
                    var continueTarget = flow.ContinueTarget;

                    var stmt = stmts[i];

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
                        break;
                    }
                    // Continue statement
                    if (stmt.continueStatement() != null && continueTarget != null)
                    {
                        statementStack.Push(continueTarget);
                        break;
                    }

                    // Block statement
                    if (stmt.blockStatement() != null)
                    {
                        statementStack.Push(new ControlFlowPointer(stmt.blockStatement().statement(), 0, breakTarget, continueTarget, flow.BackTarget));
                        quitBranch = true;
                        break;
                    }

                    // Branching for loop
                    var forStatement = stmt.forStatement();
                    if (forStatement != null)
                    {
                        // Queue the next statement before the loop, along with a break statement
                        statementStack.Push(new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget) { Context = forStatement });

                        breakTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = forStatement };
                        continueTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = forStatement };

                        // For statekemtn
                        statementStack.Push(new ControlFlowPointer(new[] { forStatement.statement() }, 0, breakTarget, continueTarget));

                        quitBranch = true;
                        break;
                    }

                    // Branching while loop
                    var whileStatement = stmt.whileStatement();
                    if (whileStatement != null)
                    {
                        // Queue the next statement before the loop, along with a break statement
                        statementStack.Push(new ControlFlowPointer(stmts, i + 1, breakTarget, continueTarget, flow.BackTarget) { Context = whileStatement });

                        breakTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = whileStatement };
                        continueTarget = new ControlFlowPointer(stmts, i + 1, backTarget: flow.BackTarget) { Context = whileStatement };

                        statementStack.Push(new ControlFlowPointer(new[] { whileStatement.statement() }, 0, breakTarget, continueTarget));

                        quitBranch = true;
                        break;
                    }

                    // Branching if
                    var ifStatement = stmt.ifStatement();
                    if (ifStatement != null)
                    {
                        if (ifStatement.elseStatement() != null)
                        {
                            // Queue the else
                            statementStack.Push(
                                new ControlFlowPointer(new[] { ifStatement.elseStatement().statement() }, 0, breakTarget, continueTarget,
                                    new ControlFlowPointer(stmts, i + 1)));

                            // Queue the if
                            statementStack.Push(new ControlFlowPointer(new[] { ifStatement.statement() }, 0, breakTarget, continueTarget));

                            quitBranch = true;
                            break;
                        }

                        // Queue the if
                        statementStack.Push(new ControlFlowPointer(new[] { ifStatement.statement() }, 0, breakTarget, continueTarget,
                            new ControlFlowPointer(stmts, i + 1)));
                    }
                }

                if (quitBranch)
                    continue;

                // Fall back to the top flow
                if (flow.Statements[0].Parent != null)
                {
                    // End of function - mark end as reachable
                    if (flow.BackTarget == null)
                    {
                        EndReachable = true;
                        continue;
                    }

                    statementStack.Push(flow.BackTarget);
                }
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