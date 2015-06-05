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
using Antlr4.Runtime;

using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Definitions;
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
        private readonly RuntimeGenerationContext _context;

        /// <summary>
        /// Stack of class definitions currently being analyzed
        /// </summary>
        private readonly Stack<ClassDefinition> _classStack;

        /// <summary>
        /// Stack of definitions, used to run through definitions in function bodies
        /// </summary>
        private readonly Stack<List<Definition>> _localsStack;

        /// <summary>
        /// Gest the message container that errors will be reported to
        /// </summary>
        private MessageContainer Container => _context.MessageContainer;

        /// <summary>
        /// Creates a new instance of the VariableUsageAnalyzer class
        /// </summary>
        /// <param name="context">The runtime generation context for base the analysis on</param>
        public DefinitionAnalyzer(RuntimeGenerationContext context)
        {
            _currentScope = context.BaseScope;
            _context = context;
            _classStack = new Stack<ClassDefinition>();
            _localsStack = new Stack<List<Definition>>();
        }

        #region Scope walking

        public override void EnterProgram(ZScriptParser.ProgramContext context)
        {
            _localsStack.Clear();

            PushDefinitionScope();
        }

        public override void ExitProgram(ZScriptParser.ProgramContext context)
        {
            PopDefinitionScope();

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

            PushDefinitionScope();
        }

        public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PopDefinitionScope();

            ExitContextScope();
        }

        public override void EnterClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            // Push class definition
            _classStack.Push(_currentScope.GetDefinitionByName<ClassDefinition>(context.className().GetText()));

            if (context.classInherit() != null)
            {
                // Mark inherited classes as 'used'
                var baseClass = _currentScope.GetDefinitionByName<ClassDefinition>(context.classInherit().className().GetText());

                if(baseClass != null)
                    RegisterDefinitionUsage(baseClass, context.classInherit().className());
            }

            EnterContextScope(context);

            PushDefinitionScope();
        }

        public override void ExitClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            // Pop class definition
            _classStack.Pop();

            ExitContextScope();

            PopDefinitionScope();
        }

        public override void EnterClassBody(ZScriptParser.ClassBodyContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitClassBody(ZScriptParser.ClassBodyContext context)
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

            PushDefinitionScope();
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            PopDefinitionScope();

            ExitContextScope();
        }

        public override void EnterClassMethod(ZScriptParser.ClassMethodContext context)
        {
            EnterContextScope(context);
        }

        public override void ExitClassMethod(ZScriptParser.ClassMethodContext context)
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

        public override void EnterForStatement(ZScriptParser.ForStatementContext context)
        {
            EnterContextScope(context);

            PushDefinitionScope();
        }

        public override void ExitForStatement(ZScriptParser.ForStatementContext context)
        {
            PopDefinitionScope();

            ExitContextScope();
        }

        public override void EnterForEachStatement(ZScriptParser.ForEachStatementContext context)
        {
            EnterContextScope(context);

            PushDefinitionScope();
        }

        public override void ExitForEachHeader(ZScriptParser.ForEachHeaderContext context)
        {
            AddDefinition(context.LoopVariable);
        }

        public override void ExitForEachStatement(ZScriptParser.ForEachStatementContext context)
        {
            PopDefinitionScope();

            ExitContextScope();
        }

        public override void EnterSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            EnterContextScope(context);

            PushDefinitionScope();
        }

        public override void ExitSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            ExitContextScope();
        }

        #endregion

        #region Definition checking

        public override void ExitValueHolderDecl(ZScriptParser.ValueHolderDeclContext context)
        {
            AddDefinition(context.Definition);
        }

        public override void EnterExpression(ZScriptParser.ExpressionContext context)
        {
            // First entrance
            if (context.memberName() != null)
            {
                var memberName = context.memberName().IDENT().GetText();
                RegisterDefinitionUsage(memberName, context.memberName());
            }
            if (context.leftValue() != null && context.leftValue().memberName() != null)
            {
                var memberName = context.leftValue().memberName().IDENT().GetText();
                RegisterDefinitionUsage(memberName, context.leftValue().memberName());
            }
        }

        public override void EnterAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            // First entrance
            if (context.leftValue().memberName() != null)
            {
                var memberName = context.leftValue().memberName().IDENT().GetText();
                RegisterDefinitionUsage(memberName, context.leftValue().memberName());
            }
        }

        #endregion

        /// <summary>
        /// Enters a scope with the given context binded. If the context is not found, an exception is raised
        /// </summary>
        /// <param name="context">The context to enter</param>
        /// <exception cref="ArgumentException">The context is not nested in any child code scope of the current code scope</exception>
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

            throw new ArgumentException("Trying to move to analysis context '" + context + "' that was not defined in any children scope");
        }

        /// <summary>
        /// Exists the context scope, jumping to the parent context
        /// </summary>
        void ExitContextScope()
        {
            // Analyze the scope for unused definitions
            UnusedDefinitionsAnalyzer.Analyze(_currentScope, Container);

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
        /// Pushes a new definition scope
        /// </summary>
        void PushDefinitionScope()
        {
            _localsStack.Push(new List<Definition>());
        }

        /// <summary>
        /// Adds a given definition to the top of the definition stack
        /// </summary>
        /// <param name="definition">The definition to add to the top of the definition stack</param>
        void AddDefinition(Definition definition)
        {
            _localsStack.Peek().Add(definition);
        }

        /// <summary>
        /// Pops a definition scope
        /// </summary>
        void PopDefinitionScope()
        {
            _localsStack.Pop();
        }

        /// <summary>
        /// Gets a definition by name in the current definition scope and stack.
        /// Local variable definitions are searched through the locals stack, and all other definitions are searched globally
        /// </summary>
        /// <param name="definitionName">The name of the definition to search for</param>
        /// <returns>The definition found, or null, if none was found</returns>
        Definition GetDefinitionByName(string definitionName)
        {
            foreach (var locals in _localsStack)
            {
                foreach (var local in locals)
                {
                    if (local.Name == definitionName)
                        return local;
                }
            }

            // Search through the current definition scope
            var definitions = _currentScope.GetDefinition(d => d.Name == definitionName && !(d is LocalVariableDefinition));

            if (definitions != null)
                return definitions;

            if (_classStack.Count <= 0)
                return null;

            // If we are in a class definition, search inheritance chain
            var classDef = _classStack.Peek();

            while (classDef != null)
            {
                var field = classDef.Fields.FirstOrDefault(f => f.Name == definitionName);

                if (field != null)
                {
                    return field;
                }

                classDef = classDef.BaseClass;
            }

            return null;
        }

        /// <summary>
        /// Registers a new definition usage
        /// </summary>
        /// <param name="definitionName">The name of the definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        void RegisterDefinitionUsage(string definitionName, ZScriptParser.MemberNameContext context)
        {
            var definition = GetDefinitionByName(definitionName);

            context.HasDefinition = definition != null;
            context.Definition = definition;

            RegisterDefinitionUsage(definition, context);
        }

        /// <summary>
        /// Registers a new definition usage
        /// </summary>
        /// <param name="def">The definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        void RegisterDefinitionUsage(Definition def, ParserRuleContext context)
        {
            if (def == null)
                RegisterMemberNotFound(context);

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

                Container.RegisterError(error);

                return;
            }
            
            error = new CodeError(member.IDENT().Symbol.Line, member.IDENT().Symbol.Column, ErrorCode.UndeclaredDefinition);
            error.Message += " '" + member.IDENT().GetText() + "'";
            error.Context = member;

            Container.RegisterError(error);
        }

        /// <summary>
        /// Registers a new variable not found error
        /// </summary>
        /// <param name="context">The context containing the name of the member that was not found</param>
        void RegisterMemberNotFound(ParserRuleContext context)
        {
            var member = context as ZScriptParser.MemberNameContext;
            if (member != null)
            {
                RegisterMemberNotFound(member);
                return;
            }

            CodeError error;

            if (context == null)
            {
                error = new CodeError(0, 0, ErrorCode.UndeclaredDefinition);
                error.Message += " UNKNOWN";
                error.Context = CurrentScope().Context;

                Container.RegisterError(error);

                return;
            }

            error = new CodeError(context.Start.Line, context.Start.Column, ErrorCode.UndeclaredDefinition);
            error.Message += " '" + context.GetText() + "'";
            error.Context = context;

            Container.RegisterError(error);
        }
    }
}