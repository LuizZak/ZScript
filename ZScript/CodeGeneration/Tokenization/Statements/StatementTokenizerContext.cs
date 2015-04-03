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

using System;
using System.Collections.Generic;
using System.Linq;

using ZScript.CodeGeneration.Analysis;
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Tokenizer context, used to help direct tokenization calls to different specialized tokenizers
    /// </summary>
    public class StatementTokenizerContext : IParserContextTokenizer<ZScriptParser.StatementContext>
    {
        /// <summary>
        /// The inner temporary variable creator for this statement tokenizer context
        /// </summary>
        private readonly InnerTemporaryCreator _tempCreator;

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
        /// The generation context for this statement tokenizer
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// The current target for continue statements.
        /// May be null, if no targets are currently registered
        /// </summary>
        public Token CurrentContinueTarget
        {
            get { return _continueTargetStack.FirstOrDefault(); }
        }

        /// <summary>
        /// The current target for break statements.
        /// May be null, if no targets are currently registered
        /// </summary>
        public Token CurrentBreakTarget
        {
            get { return _breakTargetStack.FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the code scope that contains the definitions that were pre-parsed
        /// </summary>
        public CodeScope Scope
        {
            get { return _scope; }
        }

        /// <summary>
        /// Gets the generation context for this statement tokenizer
        /// </summary>
        public RuntimeGenerationContext GenerationContext
        {
            get { return _generationContext; }
        }

        /// <summary>
        /// Gets the temporary definition creator for this StatementTokenizerContext instance
        /// </summary>
        public ITemporaryDefinitionCreator TemporaryDefinitionCreator
        {
            get { return _tempCreator; }
        }

        /// ;<summary>
        /// Initializes a new instance of the StatementTokenizerContext class
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        public StatementTokenizerContext(RuntimeGenerationContext context)
        {
            _scope = context.BaseScope;

            _generationContext = context;
            _tempCreator = new InnerTemporaryCreator();
        }

        /// <summary>
        /// Tokenizes a given block statement
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="context">The context containing the block statement to tokenize</param>
        public void TokenizeBlockStatement(IList<Token> targetList, ZScriptParser.BlockStatementContext context)
        {
            var statements = context.statement();
            foreach (var statement in statements)
            {
                TokenizeStatement(targetList, statement);
            }
        }

        /// <summary>
        /// Tokenizes a given statement into a list of tokens
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <exception cref="ArgumentException">The statement context contains a statement not recognized by this statement tokenizer</exception>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.StatementContext statement)
        {
            var statementTokens = new IntermediaryTokenList();

            TokenizeStatement(statementTokens, statement);

            return statementTokens;
        }

        /// <summary>
        /// Tokenizes a given statement into a list of tokens
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        /// <exception cref="ArgumentException">The statement context contains a statement not recognized by this statement tokenizer</exception>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.StatementContext statement)
        {
            if (statement.expression() != null || statement.assignmentExpression() != null)
            {
                TokenizeExpressionStatement(targetList, statement);
                targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
            }
            else if (statement.ifStatement() != null)
            {
                TokenizeIfStatement(targetList, statement.ifStatement());
            }
            else if (statement.forStatement() != null)
            {
                TokenizeForStatement(targetList, statement.forStatement());
            }
            else if (statement.forEachStatement() != null)
            {
                TokenizeForEachStatement(targetList, statement.forEachStatement());
            }
            else if (statement.whileStatement() != null)
            {
                TokenizeWhileStatement(targetList, statement.whileStatement());
            }
            else if (statement.switchStatement() != null)
            {
                TokenizeSwitchStatement(targetList, statement.switchStatement());
            }
            else if (statement.blockStatement() != null)
            {
                TokenizeBlockStatement(targetList, statement.blockStatement());
            }
            else if (statement.breakStatement() != null)
            {
                TokenizeBreakStatement(targetList, statement.breakStatement());
            }
            else if (statement.continueStatement() != null)
            {
                TokenizeContinueStatement(targetList, statement.continueStatement());
            }
            else if (statement.valueDeclareStatement() != null)
            {
                TokenizeValueDeclareStatement(targetList, statement.valueDeclareStatement());

                if (targetList.Count > 0)
                    targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));
            }
            else if (statement.returnStatement() != null)
            {
                TokenizeReturnStatement(targetList, statement.returnStatement());
            }
            else if (statement.GetText() != ";")
            {
                throw new ArgumentException("Unkown statement that cannot be tokenized: " + statement.GetType().Name);
            }
        }

        #region Statements

        /// <summary>
        /// Tokenizes a given IF statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the if statement</returns>
        public IntermediaryTokenList TokenizeIfStatement(ZScriptParser.IfStatementContext statement)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeIfStatement(tokens, statement);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given IF statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeIfStatement(IList<Token> targetList, ZScriptParser.IfStatementContext statement)
        {
            var tokenizer = new IfStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given FOR statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeForStatement(IList<Token> targetList, ZScriptParser.ForStatementContext statement)
        {
            var tokenizer = new ForStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given FOR EACH statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        public IntermediaryTokenList TokenizeForEachStatement(ZScriptParser.ForEachStatementContext statement)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeForEachStatement(tokens, statement);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given FOR EACH statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeForEachStatement(IList<Token> targetList, ZScriptParser.ForEachStatementContext statement)
        {
            var tokenizer = new ForEachStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given WHILE statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeWhileStatement(IList<Token> targetList, ZScriptParser.WhileStatementContext statement)
        {
            var tokenizer = new WhileStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given SWITCH statement on this statement tokenizer context
        /// </summary>
        /// <param name="statement">The statement to tokenize</param>
        /// <returns>A list of tokens that corresponds to the switch statement</returns>
        public IntermediaryTokenList TokenizeSwitchStatement(ZScriptParser.SwitchStatementContext statement)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeSwitchStatement(tokens, statement);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given SWITCH statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeSwitchStatement(IList<Token> targetList, ZScriptParser.SwitchStatementContext statement)
        {
            var tokenizer = new SwitchStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given BREAK statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeBreakStatement(IList<Token> targetList, ZScriptParser.BreakStatementContext statement)
        {
            var tokenizer = new BreakStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given CONTINUE statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement to tokenize</param>
        public void TokenizeContinueStatement(IList<Token> targetList, ZScriptParser.ContinueStatementContext statement)
        {
            var tokenizer = new ContinueStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given return statement on this statement tokenizer context
        /// </summary>
        /// <param name="returnStatement">The statement to tokenize</param>
        /// <returns>A tokenized version of the given statement</returns>
        public IntermediaryTokenList TokenizeReturnStatement(ZScriptParser.ReturnStatementContext returnStatement)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeReturnStatement(tokens, returnStatement);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given return statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="returnStatement">The statement to tokenize</param>
        public void TokenizeReturnStatement(IList<Token> targetList, ZScriptParser.ReturnStatementContext returnStatement)
        {
            var tokenizer = new ReturnStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, returnStatement);
        }

        #endregion

        /// <summary>
        /// Tokenizes a given expression statement on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="statement">The statement containing the expression to tokenize</param>
        public void TokenizeExpressionStatement(IList<Token> targetList, ZScriptParser.StatementContext statement)
        {
            var postfixTokenizer = new PostfixExpressionTokenizer(this);
            postfixTokenizer.TokenizeStatement(targetList, statement);
        }

        /// <summary>
        /// Tokenizes a given expression on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="expression">The expression to tokenize</param>
        public void TokenizeExpression(IList<Token> targetList, ZScriptParser.ExpressionContext expression)
        {
            var postfixTokenizer = new PostfixExpressionTokenizer(this);
            postfixTokenizer.TokenizeExpression(targetList, expression);
        }

        /// <summary>
        /// Tokenizes a given expression on this statement tokenizer context
        /// </summary>
        /// <param name="expression">The expression to tokenize</param>
        /// <returns>A list of tokens corresponding to the expression that was tokenized</returns>
        public IntermediaryTokenList TokenizeExpression(ZScriptParser.ExpressionContext expression)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeExpression(tokens, expression);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given assignment expression on this statement tokenizer context
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="expression">The assignment expression to tokenize</param>
        public void TokenizeAssignmentExpression(IList<Token> targetList, ZScriptParser.AssignmentExpressionContext expression)
        {
            var postfixTokenizer = new PostfixExpressionTokenizer(this);
            postfixTokenizer.TokenizeAssignmentExpression(targetList, expression);
        }

        /// <summary>
        /// Tokenizes a given variable declaration
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeValueDeclareStatement(ZScriptParser.ValueDeclareStatementContext context)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeValueDeclaration(tokens, context.valueHolderDecl());
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given variable declaration
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="context">The context to tokenize</param>
        public void TokenizeValueDeclareStatement(IList<Token> targetList, ZScriptParser.ValueDeclareStatementContext context)
        {
            TokenizeValueDeclaration(targetList, context.valueHolderDecl());
        }

        /// <summary>
        /// Tokenizes a given variable declaration
        /// </summary>
        /// <param name="targetList">A target list to tokenize to</param>
        /// <param name="context">The context to tokenize</param>
        public void TokenizeValueDeclaration(IList<Token> targetList, ZScriptParser.ValueHolderDeclContext context)
        {
            var tokenizer = new VariableDeclarationStatementTokenizer(this);
            tokenizer.TokenizeStatement(targetList, context);
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

        private class InnerTemporaryCreator : ITemporaryDefinitionCreator
        {
            private readonly List<TemporaryDefinition> _usedDefinitions = new List<TemporaryDefinition>(); 

            public TemporaryDefinition GetDefinition()
            {
                var temp = new TemporaryDefinition("$TEMP" + _usedDefinitions.Count);

                _usedDefinitions.Add(temp);

                return temp;
            }

            public void ReleaseDefinition(TemporaryDefinition definition)
            {
                _usedDefinitions.Remove(definition);
            }
        }
    }

    /// <summary>
    /// Interface to be implemented by objects capable of creating temporary hidden definitions that can be used in code scopes
    /// </summary>
    public interface ITemporaryDefinitionCreator
    {
        /// <summary>
        /// Generates a new temporary definition on this ITemporaryDefinitionCreator
        /// </summary>
        /// <returns>The temporary definition that was created</returns>
        TemporaryDefinition GetDefinition();

        /// <summary>
        /// Marks a given temporary definition as released.
        /// After releasing, a definition is not considered valid anymore, and a new call to GenerateDefinition has to be made for a new definition
        /// </summary>
        /// <param name="definition">The temporary definition to release</param>
        void ReleaseDefinition(TemporaryDefinition definition);
    }

    /// <summary>
    /// Defines a temporary definition in code
    /// </summary>
    public class TemporaryDefinition
    {
        /// <summary>
        /// Gets the name for this temporary definition
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TemporaryDefinition class
        /// </summary>
        /// <param name="name">The name of the definition to create</param>
        public TemporaryDefinition(string name)
        {
            Name = name;
        }
    }
}