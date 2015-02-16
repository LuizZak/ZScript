﻿#region License information
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
using Antlr4.Runtime.Tree;

using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Analyzes and expands the types of definitions
    /// </summary>
    public class StaticTypeAnalyzer : IDefinitionTypeProvider
    {
        /// <summary>
        /// The type provider for resolving types
        /// </summary>
        private readonly TypeProvider _typeProvider;

        /// <summary>
        /// The code scope containing the definitions to expand
        /// </summary>
        private readonly CodeScope _baseScope;

        /// <summary>
        /// The message container to report error messages to
        /// </summary>
        private readonly MessageContainer _container;

        /// <summary>
        /// The type resolver used to expand the types
        /// </summary>
        private readonly ExpressionTypeResolver _typeResolver;

        /// <summary>
        /// Initializes a new instance of the DefinitionTypeExpander class
        /// </summary>
        /// <param name="typeProvider">The type provider for resolving types</param>
        /// <param name="scope">The definition collector containing the definitions to expand</param>
        /// <param name="container">The message container to report error messages to</param>
        public StaticTypeAnalyzer(TypeProvider typeProvider, CodeScope scope, MessageContainer container)
        {
            _typeProvider = typeProvider;
            _baseScope = scope;
            _container = container;

            _typeResolver = new ExpressionTypeResolver(typeProvider, container, this);
        }

        /// <summary>
        /// Performs the expansion of the definitions using the current settings defined in the constructor of this definition type expander
        /// </summary>
        public void Expand()
        {
            // Get all definitons
            var definitions = _baseScope.GetAllDefinitionsRecursive().ToArray();

            // Expand functions first
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => !(d is ClosureDefinition)))
            {
                ExpandFunctionDefinition(definition);
            }

            // Expand function arguments, ignoring function arguments defined within closures for now
            foreach (
                var argumentDefinition in
                    definitions.OfType<FunctionArgumentDefinition>()
                        .Where(d => !(d.Context.IsContainedInRuleType<ZScriptParser.ClosureExpressionContext>())))
            {
                ExpandFunctionArgument(argumentDefinition);
            }

            // Reload the callable type definitions now
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => !(d is ClosureDefinition)))
            {
                definition.RecreateCallableDefinition();
            }

            // Iterate over value holder definitions, ignoring definitions of function arguments (which where processed earlier)
            // or contained within closures (which will be processed later)
            foreach (
                var valueHolderDefinition in
                    definitions.OfType<ValueHolderDefinition>()
                        .Where(
                            d =>
                                !(d is FunctionArgumentDefinition) &&
                                // Ignore value holders inside closures, for now
                                !(d.Context.IsContainedInRuleType<ZScriptParser.ClosureExpressionContext>())))
            {
                ExpandValueHolderDefinition(valueHolderDefinition);
            }

            // Expand closures now
            foreach (var definition in definitions.OfType<ClosureDefinition>())
            {
                ExpandClosureDefinition(definition);
            }

            // Verify return types now
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => d.BodyContext != null))
            {
                AnalyzeReturns(definition);
            }

            ProcessExpressions(_baseScope);
        }

        /// <summary>
        /// Performs deeper analysis of types by exploring expression nodes and deriving their types, as well as pre-evaluating any constants
        /// </summary>
        /// <param name="scope">The code scope to process expressions on</param>
        private void ProcessExpressions(CodeScope scope)
        {
            var resolver = new ExpressionConstantResolver(new BinaryExpressionTypeProvider(_typeProvider), new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_typeResolver, resolver);
            var definitions = scope.Definitions;

            foreach (var definition in definitions)
            {
                if(definition.Context != null)
                    traverser.Traverse(definition.Context);
            }
        }

        /// <summary>
        /// Expands the type of a closure definition, inferring types of arguments and returns when possible
        /// </summary>
        /// <param name="definition">The closure definition to expand</param>
        private void ExpandClosureDefinition(ClosureDefinition definition)
        {
            // Find the context the closure was defined in
            var definedContext = (ZScriptParser.ClosureExpressionContext)definition.Context;
            var contextType = FindExpectedTypeForClosure(definedContext);

            // Get the parameter types
            foreach (var argumentDefinition in definition.Arguments)
            {
                ExpandFunctionArgument(argumentDefinition);
            }

            // Use the type to define the type of the closure
            var newType = _typeProvider.FindCommonType(definition.CallableTypeDef, contextType) as CallableTypeDef;

            if(newType != null)
            {
                // Iterate over the arguments and modify the return type
                for (int i = 0; i < newType.ParameterInfos.Length; i++)
                {
                    definition.Arguments[i].IsVariadic = newType.ParameterInfos[i].IsVariadic;
                    definition.Arguments[i].Type = newType.ParameterInfos[i].ParameterType;
                }

                // Don't update the return type if the closure has a return type and new type is void: this may cause errors during return type analysis
                if (!definition.HasReturnType || newType.ReturnType != _typeProvider.VoidType())
                {
                    definition.ReturnType = newType.ReturnType;
                    definition.HasReturnType = true;
                }
            }

            // Now expand the closure like a normal function
            ExpandFunctionDefinition(definition);
            Array.ForEach(definition.Arguments, ExpandFunctionArgument);
            
            // Get all of the local variables and expand them now
            var scope = ScopeForContext(definition.Context);

            foreach (var valueDef in scope.GetAllDefinitionsRecursive().OfType<ValueHolderDefinition>().Where(d => !(d is FunctionArgumentDefinition)))
            {
                ExpandValueHolderDefinition(valueDef);
            }

            // Re-create the callable definition for the closure
            definition.RecreateCallableDefinition();
        }

        /// <summary>
        /// Returns the type definition that specifies the expected type context for a closure definition.
        /// May return an Any type, in case no expected types where found
        /// </summary>
        /// <param name="closureContext">The context for the closure</param>
        /// <returns>A type definition that states the expected type for the closure in its defining context</returns>
        private TypeDef FindExpectedTypeForClosure(ZScriptParser.ClosureExpressionContext closureContext)
        {
            // Search closures registered on expected type closures list
            return closureContext.InferredType ?? _typeProvider.AnyType();
        }

        /// <summary>
        /// Expands the type of a given function definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionDefinition(FunctionDefinition definition)
        {
            if (!definition.HasReturnType || definition.ReturnTypeContext == null)
                return;

            definition.ReturnType = _typeResolver.ResolveType(definition.ReturnTypeContext.type());
        }

        /// <summary>
        /// Analyzes the return statements of a given function definition, raising errors about mismatched types
        /// </summary>
        /// <param name="definition">The function definition to analyze the return statements from</param>
        private void AnalyzeReturns(FunctionDefinition definition)
        {
            if (!definition.HasReturnType)
                return;

            // Verify return statements
            foreach (var statement in definition.ReturnStatements)
            {
                if (statement.expression() != null)
                {
                    // Resolve a second time, inferring types to closures
                    _typeResolver.ExpectedType = definition.ReturnType;

                    var type = _typeResolver.ResolveExpression(statement.expression());

                    if (!_typeProvider.CanImplicitCast(type, definition.ReturnType))
                    {
                        var message = "Cannot implicitly convert return value type " + type +
                                      ", function expects return type of " + definition.ReturnType;
                        _container.RegisterError(definition.Context.Start.Line, definition.Context.Start.Column, message,
                            ErrorCode.InvalidCast, definition.Context);
                    }

                    _typeResolver.ExpectedType = null;
                }
            }
        }

        /// <summary>
        /// Expands the type of a given variable definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandValueHolderDefinition(ValueHolderDefinition definition)
        {
            // Evaluate definition type
            if (!definition.HasType)
            {
                definition.Type = _typeProvider.AnyType();
            }
            else
            {
                definition.Type = _typeResolver.ResolveType(definition.TypeContext);
            }

            if (!definition.HasValue)
            {
                return;
            }

            // Compare applicability
            // TODO: Deal with this nasy type check horror
            var argumentDefinition = definition as FunctionArgumentDefinition;
            var valueType = (argumentDefinition != null
                ? _typeResolver.ResolveCompileConstant(argumentDefinition.DefaultValue)
                : _typeResolver.ResolveExpression(definition.ValueExpression.ExpressionContext));

            if (!definition.HasType)
            {
                definition.Type = valueType;
            }

            var varType = definition.Type;

            if (!_typeProvider.CanImplicitCast(valueType, varType))
            {
                var message = "Cannot assign value of type " + valueType + " to variable of type " + varType;
                _container.RegisterError(definition.Context.Start.Line, definition.Context.Start.Column, message, ErrorCode.InvalidCast, definition.Context);
            }
        }

        /// <summary>
        /// Expands the type of a given function argument definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionArgument(FunctionArgumentDefinition definition)
        {
            ExpandValueHolderDefinition(definition);

            if (definition.IsVariadic)
            {
                definition.Type = _typeProvider.ListForType(definition.Type);
            }
        }

        //
        // IDefinitionTypeProvider.TypeForDefinition implementation
        // 
        public TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName)
        {
            // Search for the inner-most scope that contains the context, and search the definition from there
            var scopeForDefinition = ScopeForContext(context);

            if (scopeForDefinition != null)
            {
                var def = scopeForDefinition.GetDefinitionByName(definitionName);

                // Bind the definition to the member name
                context.Definition = def;

                var holderDefinition = def as ValueHolderDefinition;
                if (holderDefinition != null)
                {
                    context.IsConstant = holderDefinition.IsConstant;

                    return holderDefinition.Type ?? TypeDef.AnyType;
                }

                var funcDef = def as FunctionDefinition;
                if (funcDef != null)
                {
                    // Functions cannot be reassigned
                    context.IsConstant = true;

                    return funcDef.CallableTypeDef;
                }

                var objDef = def as ObjectDefinition;
                if (objDef != null)
                {
                    // Object definitions cannot be reassigned
                    context.IsConstant = true;

                    return TypeDef.AnyType;
                }
            }

            _container.RegisterError(context, "Cannot resolve definition name " + definitionName + " on type expanding phase.", ErrorCode.UndeclaredDefinition);

            return _typeProvider.AnyType();
        }

        /// <summary>
        /// Returns the inner-most scope that contains a given context on the current definitions collector's scopes, or null, if none was found
        /// </summary>
        /// <param name="context">The context to search in the scopes</param>
        /// <returns>The inner-most scope that contains the given context, or null, if none was found</returns>
        private CodeScope ScopeForContext(RuleContext context)
        {
            var scopes = _baseScope.GetAllScopesRecursive().ToArray();

            bool found = false;
            CodeScope scopeForDefinition = null;

            RuleContext p = context;
            while (p != null)
            {
                // If the context is a program context, return the base global scope
                if (p is ZScriptParser.ProgramContext)
                    return _baseScope;

                foreach (var scope in scopes)
                {
                    if (p == scope.Context)
                    {
                        scopeForDefinition = scope;
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;

                p = p.Parent;
            }

            return scopeForDefinition;
        }

        /// <summary>
        /// Traverses expressions, performing type checks on them
        /// </summary>
        private class ExpressionStatementsTraverser : ZScriptBaseListener
        {
            /// <summary>
            /// The type resolver to use when resolving the expressions
            /// </summary>
            private readonly ExpressionTypeResolver _typeResolver;

            /// <summary>
            /// The constant resolver to use when pre-evaluating constants
            /// </summary>
            private readonly ExpressionConstantResolver _constantResolver;

            /// <summary>
            /// Initializes a new instance of the ExpressionStatementsTraverser class
            /// </summary>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public ExpressionStatementsTraverser(ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver)
            {
                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
            }

            /// <summary>
            /// Traverses a given context, analyzing the expressions contained within them
            /// </summary>
            /// <param name="context">The context to start traversing at</param>
            public void Traverse(RuleContext context)
            {
                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(this, context);
            }

            // 
            // EnterStatement override
            // 
            public override void EnterStatement(ZScriptParser.StatementContext context)
            {
                if (context.expression() != null)
                {
                    AnalyzeExpression(context.expression());
                }
                else if (context.assignmentExpression() != null)
                {
                    AnalyzeAssignmentExpression(context.assignmentExpression());
                }
            }

            /// <summary>
            /// Analyzes a given expression, making sure the expression is correct and valid
            /// </summary>
            /// <param name="context">The context containig the expression to analyze</param>
            private void AnalyzeExpression(ZScriptParser.ExpressionContext context)
            {
                _typeResolver.ResolveExpression(context);

                _constantResolver.ExpandConstants(context);

                // Check if a left value is trying to be modified
                if (context.leftValue() != null && context.leftValue().IsConstant)
                {
                    var message = "Cannot modify value of constant value " + context.leftValue().GetText();
                    _typeResolver.MessageContainer.RegisterError(context, message, ErrorCode.ModifyingConstant);
                }
            }

            /// <summary>
            /// Analyzes a given assignment expression, making sure the assignment is correct and valid
            /// </summary>
            /// <param name="context">The context containig the assignment expression to analyze</param>
            private void AnalyzeAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
            {
                _typeResolver.ResolveAssignmentExpression(context);

                _constantResolver.ExpandConstants(context);

                // Check if the left value is not a constant
                if (context.leftValue().IsConstant)
                {
                    var message = "Cannot reassign constant value " + context.leftValue().GetText();
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.ModifyingConstant);
                }
            }

            // 
            // EnterIfStatement override
            // 
            public override void EnterIfStatement(ZScriptParser.IfStatementContext context)
            {
                var analyzer = new IfStatementAnalyzer(context, _typeResolver, _constantResolver);
                analyzer.Process();
            }

            // 
            // EnterWhileStatement override
            // 
            public override void EnterWhileStatement(ZScriptParser.WhileStatementContext context)
            {
                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                if (!provider.CanImplicitCast(_typeResolver.ResolveExpression(context.expression()), provider.BooleanType()))
                {
                    const string message = "Expression on while condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.InvalidCast);
                }

                _constantResolver.ExpandConstants(context.expression());
            }

            // 
            // EnterSwitchStatement override
            // 
            public override void EnterSwitchStatement(ZScriptParser.SwitchStatementContext context)
            {
                var analyzer = new SwitchCaseAnalyzer(context, _typeResolver, _constantResolver);
                analyzer.Process();
            }

            // 
            // EnterForInit override
            // 
            public override void EnterForInit(ZScriptParser.ForInitContext context)
            {
                // For loops can ommit the init expression
                if (context.expression() == null)
                    return;

                _typeResolver.ResolveExpression(context.expression());
                _constantResolver.ExpandConstants(context.expression());
            }

            // 
            // EnterForCondition override
            // 
            public override void EnterForCondition(ZScriptParser.ForConditionContext context)
            {
                // For loops can ommit the condition expression
                if (context.expression() == null)
                    return;

                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                var conditionType = _typeResolver.ResolveExpression(context.expression());

                if (!provider.CanImplicitCast(conditionType, provider.BooleanType()))
                {
                    const string message = "Expression on for condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.InvalidCast);
                }

                _constantResolver.ExpandConstants(context.expression());
            }

            // 
            // EnterForIncrement override
            // 
            public override void EnterForIncrement(ZScriptParser.ForIncrementContext context)
            {
                // For loops can ommit the increment expression
                if (context.expression() == null)
                    return;

                _typeResolver.ResolveExpression(context.expression());
                _constantResolver.ExpandConstants(context.expression());
            }
        }

        /// <summary>
        /// Helper class that helps with type and integrity analysis of switch statements
        /// </summary>
        class SwitchCaseAnalyzer : ZScriptBaseListener
        {
            /// <summary>
            /// The type resolver to use when resolving the expressions
            /// </summary>
            private readonly ExpressionTypeResolver _typeResolver;

            /// <summary>
            /// The constant resolver to use when pre-evaluating constants
            /// </summary>
            private readonly ExpressionConstantResolver _constantResolver;

            /// <summary>
            /// The context for the switch statement to analyze
            /// </summary>
            private readonly ZScriptParser.SwitchStatementContext _switchContext;

            /// <summary>
            /// Type definition for the current switch's condition, used to check conditions on the switch's case blocks
            /// </summary>
            private TypeDef _switchType;

            /// <summary>
            /// List of case blocks that where processed
            /// </summary>
            private readonly List<ZScriptParser.CaseBlockContext> _processedCases;

            /// <summary>
            /// Initializes a new instance of the SwitchCaseExplorer class
            /// </summary>
            /// <param name="switchContext">The context containing the statement to process</param>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public SwitchCaseAnalyzer(ZScriptParser.SwitchStatementContext switchContext, ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver)
            {
                _switchContext = switchContext;
                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
                _processedCases = new List<ZScriptParser.CaseBlockContext>();
            }

            /// <summary>
            /// Processes the statement
            /// </summary>
            public void Process()
            {
                // Expand switch statement's expression
                var type = _typeResolver.ResolveExpression(_switchContext.expression());
                _constantResolver.ExpandConstants(_switchContext.expression());

                // Push the type of the switch statement into the switch type stack so the EnterCaseBlock can utilize the value
                _switchType = type;

                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(this, _switchContext);
                
                AnalyzeConstantSwitch();
            }

            /// <summary>
            /// Verifes the cases processed, and check for constants in both case and switch expression evaluation, raising warnings when
            /// either the switch is constant and matches a constant case label, or a constant switch matches none of the constant case labels
            /// </summary>
            private void AnalyzeConstantSwitch()
            {
                if (!_switchContext.expression().IsConstant)
                    return;

                // Whether the switch matches another constant case
                bool matched = false;
                // Whether all of the cases are constant values
                bool allConstant = true;

                foreach (var caseContext in _processedCases)
                {
                    if (!caseContext.expression().IsConstant)
                    {
                        allConstant = false;
                        continue;
                    }

                    if (caseContext.expression().ConstantValue.Equals(_switchContext.expression().ConstantValue))
                    {
                        const string message = "Constant case label that always matches constant switch expression";
                        _typeResolver.MessageContainer.RegisterWarning(caseContext.expression(), message, WarningCode.ConstantSwitchExpression);

                        matched = true;
                    }
                    else
                    {
                        const string message = "Constant case label that never matches constant switch expression";
                        _typeResolver.MessageContainer.RegisterWarning(caseContext.expression(), message, WarningCode.ConstantSwitchExpression);
                    }
                }

                if (!matched && allConstant)
                {
                    const string message = "Constant switch statement that matches no case never executes";
                    _typeResolver.MessageContainer.RegisterWarning(_switchContext.expression(), message, WarningCode.ConstantSwitchExpression);
                }
            }

            // 
            // EnterCaseBlock override
            // 
            public override void EnterCaseBlock(ZScriptParser.CaseBlockContext context)
            {
                // Expand case's check expression
                var caseType = _typeResolver.ResolveExpression(context.expression());
                _constantResolver.ExpandConstants(context.expression());

                // Verify case type against the current switch's expression type
                if (!_typeResolver.TypeProvider.CanImplicitCast(caseType, _switchType))
                {
                    const string message = "Expression on case label does not matches the expression provided on the switch block";
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.InvalidCast);

                    _processedCases.Add(context);

                    return;
                }

                // Verify case against existing cases
                foreach (var prevCase in _processedCases)
                {
                    // Check the constants in the cases and raise an error about duplicated case statements
                    if (prevCase.expression().IsConstant && context.expression().IsConstant &&
                        prevCase.expression().ConstantValue.Equals(context.expression().ConstantValue))
                    {
                        const string message = "Repeated entry of constant-valued case on switch block";
                        _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.RepeatedCaseLabelValue);
                    }
                }

                _processedCases.Add(context);
            }
        }

        /// <summary>
        /// Helper class that helps with type ant integrity analysis of if statements
        /// </summary>
        class IfStatementAnalyzer : ZScriptBaseListener
        {
            /// <summary>
            /// The type resolver to use when resolving the expressions
            /// </summary>
            private readonly ExpressionTypeResolver _typeResolver;

            /// <summary>
            /// The constant resolver to use when pre-evaluating constants
            /// </summary>
            private readonly ExpressionConstantResolver _constantResolver;

            /// <summary>
            /// The context for the statement to analyze
            /// </summary>
            private readonly ZScriptParser.IfStatementContext _ifContext;

            /// <summary>
            /// Initializes a new instance of the IfStatementAnalyzer class
            /// </summary>
            /// <param name="ifContext">The context containing the statement to process</param>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public IfStatementAnalyzer(ZScriptParser.IfStatementContext ifContext, ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver)
            {
                _ifContext = ifContext;
                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
            }

            /// <summary>
            /// Processes the statement
            /// </summary>
            public void Process()
            {
                var provider = _typeResolver.TypeProvider;

                var expression = _ifContext.expression();

                _typeResolver.ResolveExpression(expression);
                _constantResolver.ExpandConstants(expression);

                // Check if expression has a boolean type
                if (!provider.CanImplicitCast(expression.EvaluatedType, provider.BooleanType()))
                {
                    const string message = "Expression on if condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
                    string message;

                    if (expression.ConstantValue.Equals(true))
                    {
                        message = "True constant in if expression always executes the contained statements";
                    }
                    else
                    {
                        message = "False constant in if expression never executes the contained statements";
                    }

                    _typeResolver.MessageContainer.RegisterWarning(expression, message, WarningCode.ConstantIfCondition);
                }
            }
        }
    }

    /// <summary>
    /// Static class containing helper methods for dealing with rule contexts
    /// </summary>
    public static class RuleContextHelpers
    {
        /// <summary>
        /// Returns a value specifying whether this rule context is contained within a context type.
        /// If any of the parents of this context are of the given type, true is returned
        /// </summary>
        /// <typeparam name="T">The type of the parent context to try to search</typeparam>
        /// <param name="context">The context to analyze</param>
        /// <returns>true if any of the parents in the chain of parenting of this rule context are of type T, false otherwise</returns>
        public static bool IsContainedInRuleType<T>(this RuleContext context) where T : RuleContext
        {
            while (context != null)
            {
                context = context.Parent;

                if (context is T)
                    return true;
            }

            return false;
        }
    }
}