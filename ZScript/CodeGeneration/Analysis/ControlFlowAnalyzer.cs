using System.Collections.Generic;
using Antlr4.Runtime.Tree;
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
        /// The context of the body of the function to analyze
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// List of all the return statements of the currently processed function
        /// </summary>
        private List<ZScriptParser.ReturnStatementContext> _returnStatements;

        /// <summary>
        /// Gets a value specifying whether the end of the function that was analyzed is reachable by any code path
        /// </summary>
        public bool EndReachable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ControlFlowAnalyzer class
        /// </summary>
        /// <param name="generationContext">The context for the current runtime generation this control flow is analyzing on</param>
        /// <param name="bodyContext">The context of the body of the function to analyze</param>
        public ControlFlowAnalyzer(RuntimeGenerationContext generationContext, ZScriptParser.FunctionBodyContext bodyContext)
        {
            _generationContext = generationContext;
            _bodyContext = bodyContext;
            _returnStatements = new List<ZScriptParser.ReturnStatementContext>();
        }

        /// <summary>
        /// Analyzes the control flow of the current body context
        /// </summary>
        public void Analyze()
        {
            //AnalyzeBlockStatement(_bodyContext.blockStatement());

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

            var statementQueue = new Queue<ControlFlowPointer>();

            statementQueue.Enqueue(new ControlFlowPointer(_bodyContext.blockStatement().statement(), 0));

            while (statementQueue.Count > 0)
            {
                var flow = statementQueue.Dequeue();
                var index = flow.StatementIndex;

                var stmts = flow.Statements;

                bool quitBranch = false;
                for (int i = index; i < stmts.Length; i++)
                {
                    var stmt = stmts[i];
                    var t = stmt.GetText();

                    stmt.Reachable = true;

                    if (stmt.returnStatement() != null)
                    {
                        quitBranch = true;
                        break;
                    }

                    // Block statement
                    if (stmt.blockStatement() != null)
                    {
                        statementQueue.Enqueue(new ControlFlowPointer(stmt.blockStatement().statement(), 0));
                        quitBranch = true;
                        break;
                    }

                    // Branching if
                    var ifStatement = stmt.ifStatement();
                    if (ifStatement != null)
                    {
                        if (ifStatement.elseStatement() != null)
                        {
                            // Queue the if
                            statementQueue.Enqueue(new ControlFlowPointer(new[] {ifStatement.statement()}, 0));

                            // Queue the else
                            statementQueue.Enqueue(
                                new ControlFlowPointer(new[] {ifStatement.elseStatement().statement()}, 0,
                                    new ControlFlowPointer(stmts, i + 1)));

                            quitBranch = true;
                            break;
                        }

                        // Queue the if
                        statementQueue.Enqueue(new ControlFlowPointer(new[] {ifStatement.statement()}, 0,
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

                    statementQueue.Enqueue(flow.BackTarget);
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
        /// Analyzes a given block statement
        /// </summary>
        private void AnalyzeBlockStatement(ZScriptParser.BlockStatementContext block)
        {
            var statements = block.statement();

            AnalyzeStatements(statements);
        }

        /// <summary>
        /// Analyzes a given array of statements
        /// </summary>
        private void AnalyzeStatements(ZScriptParser.StatementContext[] statements)
        {
            
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
            _returnStatements.Add(context);
        }

        /// <summary>
        /// Analyzes a given IF statement context for return statement state
        /// </summary>
        private void AnalyzeIfStatement(ZScriptParser.IfStatementContext context)
        {
            var elseStatement = context.elseStatement();

            // Constant ifs simply return the state of the statements within; the result is only NotPresent if an else is not present
            if (context.IsConstant)
            {
                if (context.ConstantValue)
                {
                    AnalyzeStatement(context.statement());
                }
                if (elseStatement != null)
                {
                    AnalyzeStatement(elseStatement.statement());
                }
            }

            // If's start with their state set as the nested statement, and the
            // final type resolving depends on whether there is an else clause.
            AnalyzeStatement(context.statement());
        }

        /// <summary>
        /// Analyzes a given WHILE statement context for return statement state
        /// </summary>
        private void AnalyzeWhileStatement(ZScriptParser.WhileStatementContext context)
        {
            // Constant whiles simply return the state of the statements within
            if (context.IsConstant)
            {
                if (context.ConstantValue)
                {
                    AnalyzeStatement(context.statement());
                }
            }

            AnalyzeStatement(context.statement());
        }

        /// <summary>
        /// Analyzes a given FOR statement context for return statement state
        /// </summary>
        private void AnalyzeForStatement(ZScriptParser.ForStatementContext context)
        {
            // Constant whiles simply return the state of the statements within
            if ((context.forCondition() == null || context.forCondition().IsConstant))
            {
                if (context.forCondition() == null || (context.forCondition().IsConstant && context.forCondition().ConstantValue))
                {
                    AnalyzeStatement(context.statement());
                }
            }

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

            // Constant switch case
            // TODO: Continue implementing constant switch resolving
            if (context.IsConstant)
            {
                if (context.ConstantCaseIndex > -1)
                {

                }
            }

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
        /// Class used to represent a control flow head
        /// </summary>
        private class ControlFlowPointer
        {
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
            /// <param name="backTarget">The target to point the back jump to, when returning up the code flow</param>
            /// <param name="breakTarget">The control flow to jump to when a break is reached</param>
            /// <param name="continueTarget">The control flow to jump to when a continue is reached</param>
            public ControlFlowPointer(ZScriptParser.StatementContext[] statements, int statementIndex,
                ControlFlowPointer backTarget = null, ControlFlowPointer breakTarget = null,
                ControlFlowPointer continueTarget = null)
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