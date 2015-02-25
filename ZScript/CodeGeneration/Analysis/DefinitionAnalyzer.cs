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
        /// Gest the message container that errors will be reported to
        /// </summary>
        private MessageContainer Container
        {
            get { return _context.MessageContainer; }
        }

        /// <summary>
        /// Creates a new instance of the VariableUsageAnalyzer class
        /// </summary>
        /// <param name="context">The runtime generation context for base the analysis on</param>
        public DefinitionAnalyzer(RuntimeGenerationContext context)
        {
            _currentScope = context.BaseScope;
            _context = context;
            _classStack = new Stack<ClassDefinition>();
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

        public override void EnterClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            // Push class definition
            _classStack.Push(_currentScope.GetDefinitionByName<ClassDefinition>(context.className().GetText()));

            if (context.classInherit() != null)
            {
                // Mark inherited classes as 'used'
                var baseClass = _currentScope.GetDefinitionByName<ClassDefinition>(context.classInherit().className().GetText());

                if(baseClass != null)
                    RegisterDefinitionUsage(baseClass, context);
            }

            EnterContextScope(context);
        }

        public override void ExitClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            // Pop class definition
            _classStack.Pop();

            ExitContextScope();
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
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
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
        /// Registers a new definition usage
        /// </summary>
        /// <param name="definitionName">The name of the definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        void RegisterDefinitionUsage(string definitionName, ZScriptParser.MemberNameContext context)
        {
            var definition = _currentScope.GetDefinitionByName(definitionName);

            if (definition == null)
            {
                bool found = false;

                if(_classStack.Count > 0)
                {
                    // If we are in a class definition, search inheritance chain
                    var classDef = _classStack.Peek();

                    while(classDef != null)
                    {
                        var field = classDef.Fields.FirstOrDefault(f => f.Name == definitionName);

                        if (field != null)
                        {
                            definition = field;
                            found = true;
                            break;
                        }

                        classDef = classDef.BaseClass;
                    }
                }

                if(!found)
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

                Container.RegisterError(error);

                return;
            }
            
            error = new CodeError(member.IDENT().Symbol.Line, member.IDENT().Symbol.Column, ErrorCode.UndeclaredDefinition);
            error.Message += " '" + member.IDENT().GetText() + "'";
            error.Context = CurrentScope().Context;

            Container.RegisterError(error);
        }
    }
}