using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class that analyzes expressions and 
    /// </summary>
    public class VariableUsageAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// A list of all the errors raised during the current analysis
        /// </summary>
        private readonly List<CodeError> _errorList = new List<CodeError>();

        /// <summary>
        /// The current scope for the definitions
        /// </summary>
        private DefinitionScope _currentScope;

        /// <summary>
        /// Gets a list of all the errors raised during the current analysis
        /// </summary>
        public List<CodeError> CollectedErrors
        {
            get { return _errorList; }
        }

        /// <summary>
        /// Creates a new instance of the VariableUsageAnalyzer class
        /// </summary>
        /// <param name="scope">The scope to analyzer</param>
        public VariableUsageAnalyzer(DefinitionScope scope)
        {
            _currentScope = scope;
        }

        #region Scope walking

        // Function definitions have their own scope, containing the parameters
        public override void EnterFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            ExitContextScope();
        }

        public override void EnterBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            ExitContextScope();
        }

        public override void EnterObjectDefinition(ZScriptParser.ObjectDefinitionContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitObjectDefinition(ZScriptParser.ObjectDefinitionContext context)
        {
            ExitContextScope();
        }

        public override void EnterSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            ExitContextScope();
        }

        public override void EnterSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            ExitContextScope();
        }

        public override void EnterObjectFunction(ZScriptParser.ObjectFunctionContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitObjectFunction(ZScriptParser.ObjectFunctionContext context)
        {
            ExitContextScope();
        }

        #endregion

        #region Definition checking

        public override void EnterExpression(ZScriptParser.ExpressionContext context)
        {
            // First entrance
            if (context.memberName() != null)
            {
                var name = context.memberName().IDENT().GetText();

                if (!DefinitionNameExists(name))
                {
                    RegisterMemberNotFound(context.memberName());
                }
            }
            if (context.leftValue() != null && context.leftValue().memberName() != null)
            {
                var name = context.leftValue().memberName().IDENT().GetText();

                if (!DefinitionNameExists(name))
                {
                    RegisterMemberNotFound(context.leftValue().memberName());
                }
            }
        }

        public override void EnterAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            // First entrance
            if (context.leftValue().memberName() != null)
            {
                var name = context.leftValue().memberName().IDENT().GetText();

                if (!DefinitionNameExists(name))
                {
                    RegisterMemberNotFound(context.leftValue().memberName());
                }
            }
        }

        #endregion

        /// <summary>
        /// Enters a scope with the given context binded. If the context is not found, an exception is raised
        /// </summary>
        /// <param name="context">The context to enter</param>
        void EnterContextScope(ParserRuleContext context)
        {
            // Get the scope on the current scope tree that matches the context
            foreach (var s in _currentScope.ChildrenScopes)
            {
                if (s.Context == context)
                {
                    _currentScope = s;
                    return;
                }
            }

            throw new Exception("Trying to move to analysis context '" + context + "' that was not defined in any children scope");
        }

        /// <summary>
        /// Exists the context scope, jumping to the parent context
        /// </summary>
        void ExitContextScope()
        {
            _currentScope = _currentScope.ParentScope;
        }

        /// <summary>
        /// Gets the current top-most scope
        /// </summary>
        /// <returns>The current top-most scope</returns>
        DefinitionScope CurrentScope()
        {
            return _currentScope;
        }

        /// <summary>
        /// Returns whether a given definition name is defined in any of the current scopes
        /// </summary>
        /// <param name="definitionName">The definition name to verify</param>
        /// <returns>Whether a definition with the given name exists</returns>
        bool DefinitionNameExists(string definitionName)
        {
            var scope = _currentScope;

            while (scope != null)
            {
                if (scope.Definitions.Any(d => d.Name == definitionName))
                    return true;

                scope = scope.ParentScope;
            }

            return false;
        }

        /// <summary>
        /// Registers a new variable not found error
        /// </summary>
        /// <param name="member">The member containing the name of the variable that was not found</param>
        void RegisterMemberNotFound(ZScriptParser.MemberNameContext member)
        {
            CodeError error = new CodeError(member.IDENT().Symbol.Line, member.IDENT().Symbol.Column, CodeError.CodeErrorType.AccessUndeclaredDefinition);
            error.Message += " '" + member.IDENT().GetText() + "'";
            error.Context = CurrentScope().Context;

            _errorList.Add(error);
        }
    }
}