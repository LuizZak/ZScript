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

using System;
using System.Collections.Generic;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Tokenizer context, used to help direct tokenization calls to different specialized tokenizers
    /// </summary>
    public class StatementTokenizerContext
    {
        /// <summary>
        /// The stack of continue targets
        /// </summary>
        private readonly Stack<Token> _continueTargetStack = new Stack<Token>();

        /// <summary>
        /// The stack of break targets
        /// </summary>
        private readonly Stack<Token> _breakTargetStack = new Stack<Token>();

        /// <summary>
        /// The code scope that is expose to the statements tokenizers
        /// </summary>
        private readonly CodeScope _scope;

        /// <summary>
        /// The message container to report errors and warnings to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// The current target for continue statements.
        /// May be null, if no targets are currently registered
        /// </summary>
        public Token CurrentContinueTarget
        {
            get { return _continueTargetStack.Count == 0 ? null : _continueTargetStack.Peek(); }
        }

        /// <summary>
        /// The current target for break statements.
        /// May be null, if no targets are currently registered
        /// </summary>
        public Token CurrentBreakTarget
        {
            get { return _breakTargetStack.Count == 0 ? null : _breakTargetStack.Peek(); }
        }

        /// <summary>
        /// Gets the code scope that contains the definitions that were pre-parsed
        /// </summary>
        public CodeScope Scope
        {
            get { return _scope; }
        }

        /// <summary>
        /// The message container to report errors and warnings to
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
        }

        /// <summary>
        /// Initializes a new instance of the StatementTokenizerContext class
        /// </summary>
        /// <param name="scope">A code scope containing definitions that were pre-parsed</param>
        /// <param name="messageContainer">A message container to report errors and warnings to</param>
        public StatementTokenizerContext(CodeScope scope, MessageContainer messageContainer)
        {
            _scope = scope;
            _messageContainer = messageContainer;
        }

        /// <summary>
        /// Tokenizes a given block statement
        /// </summary>
        /// <param name="context">The context containing the block statement to tokenize</param>
        public IntermediateTokenList TokenizeBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            var statements = context.statement();

            IntermediateTokenList statementTokens = new IntermediateTokenList();

            foreach (var statement in statements)
            {
                statementTokens.AddRange(TokenizeStatement(statement));
            }

            return new IntermediateTokenList(statementTokens);
        }

        /// <summary>
        /// Tokenizes a given statement into a list of tokens
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        public IntermediateTokenList TokenizeStatement(ZScriptParser.StatementContext statement)
        {
            IntermediateTokenList statementTokens;

            if (statement.expression() != null || statement.assignmentExpression() != null)
            {
                statementTokens = TokenizeExpressionStatement(statement);
                statementTokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
            }
            else if (statement.ifStatement() != null)
            {
                statementTokens = TokenizeIfStatement(statement.ifStatement());
            }
            else if (statement.forStatement() != null)
            {
                statementTokens = TokenizeForStatement(statement.forStatement());
            }
            else if (statement.whileStatement() != null)
            {
                statementTokens = TokenizeWhileStatement(statement.whileStatement());
            }
            else if (statement.switchStatement() != null)
            {
                statementTokens = TokenizeSwitchStatement(statement.switchStatement());
            }
            else if (statement.blockStatement() != null)
            {
                statementTokens = TokenizeBlockStatement(statement.blockStatement());
            }
            else if (statement.breakStatement() != null)
            {
                statementTokens = TokenizeBreakStatement(statement.breakStatement());
            }
            else if (statement.continueStatement() != null)
            {
                statementTokens = TokenizeContinueStatement(statement.continueStatement());
            }
            else if (statement.valueDeclareStatement() != null)
            {
                statementTokens = TokenizeValueDeclareStatement(statement.valueDeclareStatement());
            }
            else if (statement.returnStatement() != null)
            {
                statementTokens = TokenizeReturnStatement(statement.returnStatement());
            }
            else if (statement.GetText() == ";")
            {
                statementTokens = new IntermediateTokenList();
            }
            else
            {
                throw new Exception("Unkown statement that cannot be tokenized: " + statement.GetType().Name);
            }

            return statementTokens;
        }

        #region Statements

        /// <summary>
        /// Tokenizes a given IF statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the if statement</returns>
        public IntermediateTokenList TokenizeIfStatement(ZScriptParser.IfStatementContext statement)
        {
            var tokenizer = new IfStatementTokenizer(this);
            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given FOR statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the for statement</returns>
        public IntermediateTokenList TokenizeForStatement(ZScriptParser.ForStatementContext statement)
        {
            var tokenizer = new ForStatementTokenizer(this);
            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given WHILE statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the while statement</returns>
        public IntermediateTokenList TokenizeWhileStatement(ZScriptParser.WhileStatementContext statement)
        {
            var tokenizer = new WhileStatementTokenizer(this);
            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given SWITCH statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the switch statement</returns>
        public IntermediateTokenList TokenizeSwitchStatement(ZScriptParser.SwitchStatementContext statement)
        {
            var tokenizer = new SwitchStatementTokenizer(this);
            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given BREAK statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the for statement</returns>
        public IntermediateTokenList TokenizeBreakStatement(ZScriptParser.BreakStatementContext statement)
        {
            BreakStatementTokenizer tokenizer = new BreakStatementTokenizer(this);

            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given CONTINUE statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the for statement</returns>
        public IntermediateTokenList TokenizeContinueStatement(ZScriptParser.ContinueStatementContext statement)
        {
            ContinueStatementTokenizer tokenizer = new ContinueStatementTokenizer(this);

            return tokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given return statement on this statement tokenizer context
        /// </summary>
        /// <param name="returnStatement">The statement to tokenize</param>
        /// <returns>A tokenized version of the given statement</returns>
        public IntermediateTokenList TokenizeReturnStatement(ZScriptParser.ReturnStatementContext returnStatement)
        {
            ReturnStatementTokenizer tokenizer = new ReturnStatementTokenizer(this);
            return tokenizer.TokenizeStatement(returnStatement);
        }

        #endregion

        /// <summary>
        /// Tokenizes a given expression statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement containing the expression to tokenize</param>
        /// <returns>A list of tokens corresponding to the expression statement that was tokenized</returns>
        public IntermediateTokenList TokenizeExpressionStatement(ZScriptParser.StatementContext statement)
        {
            //new InfixExpressionPrinter().PrintStatement(statement);
            //new PostfixExpressionPrinter().PrintStatement(statement);

            var postfixTokenizer = new PostfixExpressionTokenizer(this);

            return postfixTokenizer.TokenizeStatement(statement);
        }

        /// <summary>
        /// Tokenizes a given expression on this statement tokenizer context
        /// </summary>
        /// <param name="expression">The expression to tokenize</param>
        /// <returns>A list of tokens corresponding to the expression that was tokenized</returns>
        public IntermediateTokenList TokenizeExpression(ZScriptParser.ExpressionContext expression)
        {
            var postfixTokenizer = new PostfixExpressionTokenizer(this);

            return postfixTokenizer.TokenizeExpression(expression);
        }

        /// <summary>
        /// Tokenizes a given assignment expression on this statement tokenizer context
        /// </summary>
        /// <param name="expression">The assignment expression to tokenize</param>
        /// <returns>A list of tokens corresponding to the assignment expression that was tokenized</returns>
        public IntermediateTokenList TokenizeAssignmentExpression(ZScriptParser.AssignmentExpressionContext expression)
        {
            var postfixTokenizer = new PostfixExpressionTokenizer(this);

            return postfixTokenizer.TokenizeAssignmentExpression(expression);
        }

        /// <summary>
        /// Tokenizes a given variable declaration
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediateTokenList TokenizeValueDeclareStatement(ZScriptParser.ValueDeclareStatementContext context)
        {
            return TokenizeValueDeclaration(context.valueHolderDecl());
        }

        /// <summary>
        /// Tokenizes a given variable declaration
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediateTokenList TokenizeValueDeclaration(ZScriptParser.ValueHolderDeclContext context)
        {
            var tokenizer = new VariableDeclarationStatementTokenizer(this);
            return tokenizer.TokenizeValueHolderDeclaration(context);
        }

        /// <summary>
        /// Pushes a jump target token as the target for a continue statement
        /// </summary>
        /// <param name="target">The target for the next continue statement</param>
        public void PushContinueTarget(Token target)
        {
            _continueTargetStack.Push(target);
        }

        /// <summary>
        /// Pops a continue target from the continue target stack
        /// </summary>
        public void PopContinueTarget()
        {
            _continueTargetStack.Pop();
        }

        /// <summary>
        /// Pushes a jump target token as the target for a break statement
        /// </summary>
        /// <param name="target">The target for the next break statement</param>
        public void PushBreakTarget(Token target)
        {
            _breakTargetStack.Push(target);
        }

        /// <summary>
        /// Pops a break target from the break target stack
        /// </summary>
        public void PopBreakTarget()
        {
            _breakTargetStack.Pop();
        }
    }
}