using System;
using Antlr4.Runtime;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class that analyzes expressions and returns warnings and errors related to usage of definitions
    /// </summary>
    public class DefinitionAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// The current scope for the definitions
        /// </summary>
        private CodeScope _currentScope;

        /// <summary>
        /// The message container that errors will be reported to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// Creates a new instance of the VariableUsageAnalyzer class
        /// </summary>
        /// <param name="scope">The scope to analyzer</param>
        /// <param name="messageContainer">The message container that errors will be reported to</param>
        public DefinitionAnalyzer(CodeScope scope, MessageContainer messageContainer)
        {
            _currentScope = scope;
            _messageContainer = messageContainer;
        }

        #region Scope walking

        public override void ExitProgram(ZScriptParser.ProgramContext context)
        {
            ExitContextScope();
        }

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
            // Add an usage for the inherited object, if available
            if (context.objectInherit() != null)
            {
                var name = context.objectInherit().objectName().GetText();
                Definition def = _currentScope.GetDefinitionByName(name);

                if (!(def is ObjectDefinition))
                {
                    _messageContainer.RegisterError(context.Start.Line, context.Start.Column,
                        "Unkown or invalid definition '" + name + "' to inherit from",
                        ErrorCode.UndeclaredDefinition);
                }

                RegisterDefinitionUsage(def, context);
            }

            EnterContextScope(context);
        }

        public override void ExitObjectDefinition(ZScriptParser.ObjectDefinitionContext context)
        {
            ExitContextScope();
        }

        public override void EnterObjectBody(ZScriptParser.ObjectBodyContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitObjectBody(ZScriptParser.ObjectBodyContext context)
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

        public override void EnterSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitSequenceBody(ZScriptParser.SequenceBodyContext context)
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

        public override void EnterClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitClosureExpression(ZScriptParser.ClosureExpressionContext context)
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

                RegisterDefinitionUsage(name, context.memberName());
            }
            if (context.leftValue() != null && context.leftValue().memberName() != null)
            {
                var name = context.leftValue().memberName().IDENT().GetText();

                RegisterDefinitionUsage(name, context.memberName());
            }
        }

        public override void EnterAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            // First entrance
            if (context.leftValue().memberName() != null)
            {
                var name = context.leftValue().memberName().IDENT().GetText();

                RegisterDefinitionUsage(name, context.leftValue().memberName());
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
            // Analyze the scope for unused definitions
            UnusedDefinitionsAnalyzer.Analyze(_currentScope, _messageContainer);

            _currentScope = _currentScope.ParentScope;
        }

        /// <summary>
        /// Gets the current top-most scope
        /// </summary>
        /// <returns>The current top-most scope</returns>
        CodeScope CurrentScope()
        {
            return _currentScope;
        }

        /// <summary>
        /// Registers a new definition usage
        /// </summary>
        /// <param name="definitionName">The name of the definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        void RegisterDefinitionUsage(string definitionName, ZScriptParser.MemberNameContext context)
        {
            var definition = _currentScope.GetDefinitionByName(definitionName);

            if (definition == null)
            {
                RegisterMemberNotFound(context);
            }

            _currentScope.AddDefinitionUsage(new DefinitionUsage(definition, context));
        }

        /// <summary>
        /// Registers a new definition usage
        /// </summary>
        /// <param name="def">The definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        void RegisterDefinitionUsage(Definition def, ParserRuleContext context)
        {
            _currentScope.AddDefinitionUsage(new DefinitionUsage(def, context));
        }

        /// <summary>
        /// Registers a new variable not found error
        /// </summary>
        /// <param name="member">The member containing the name of the variable that was not found</param>
        void RegisterMemberNotFound(ZScriptParser.MemberNameContext member)
        {
            CodeError error;

            if (member == null)
            {
                error = new CodeError(0, 0, ErrorCode.UndeclaredDefinition);
                error.Message += " UNKNOWN";
                error.Context = CurrentScope().Context;

                _messageContainer.RegisterError(error);

                return;
            }
            
            error = new CodeError(member.IDENT().Symbol.Line, member.IDENT().Symbol.Column, ErrorCode.UndeclaredDefinition);
            error.Message += " '" + member.IDENT().GetText() + "'";
            error.Context = CurrentScope().Context;

            _messageContainer.RegisterError(error);
        }
    }
}