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
using Antlr4.Runtime.Tree;

using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Analyzes and expands the types of definitions
    /// </summary>
    public class StaticTypeAnalyzer
    {
        /// <summary>
        /// The generation context to use when analyzing types
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// The code scope containing the definitions to expand
        /// </summary>
        private readonly CodeScope _baseScope;

        /// <summary>
        /// The type resolver used to expand the types
        /// </summary>
        private readonly ExpressionTypeResolver _typeResolver;

        /// <summary>
        /// Type source for classes
        /// </summary>
        private readonly ClassTypeSource _classTypeSource;

        /// <summary>
        /// Gets the type provider for this static type analyzer
        /// </summary>
        private TypeProvider TypeProvider
        {
            get { return _generationContext.TypeProvider; }
        }

        /// <summary>
        /// Gets the message container to report errors and warnings to
        /// </summary>
        private MessageContainer Container
        {
            get { return _generationContext.MessageContainer; }
        }

        /// <summary>
        /// Initializes a new instance of the DefinitionTypeExpander class
        /// </summary>
        /// <param name="generationContext">The generation context for this static type analyzer</param>
        public StaticTypeAnalyzer(RuntimeGenerationContext generationContext)
        {
            _baseScope = generationContext.BaseScope;

            _generationContext = generationContext;

            _typeResolver = new ExpressionTypeResolver(generationContext);
            _classTypeSource = new ClassTypeSource();
        }

        /// <summary>
        /// Performs the expansion of the definitions using the current settings defined in the constructor of this definition type expander
        /// </summary>
        public void Expand()
        {
            // Assign the type source for the type provider
            TypeProvider.RegisterCustomTypeSource(_classTypeSource);

            // Get all definitons
            var definitions = _baseScope.GetAllDefinitionsRecursive().ToArray();

            // Expand class fields
            foreach (var classDef in definitions.OfType<ClassDefinition>())
            {
                ExpandClassDefinition(classDef);
            }

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
                                !(d is LocalVariableDefinition) &&
                                !(d is FunctionArgumentDefinition) &&
                                // Ignore value holders inside closures, for now
                                !(d.Context.IsContainedInRuleType<ZScriptParser.ClosureExpressionContext>())))
            {
                ExpandValueHolderDefinition(valueHolderDefinition);
            }
            
            //ProcessStatements(_baseScope);
            ProcessExpressions(_baseScope, ExpressionAnalysisMode.Complete);

            // Verify return types now
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => d.BodyContext != null && !(d is ClosureDefinition)))
            {
                AnalyzeReturns(definition);
            }

            //ProcessExpressions(_baseScope, ExpressionAnalysisMode.Complete ^ ExpressionAnalysisMode.InnerClosureExpressionResolving);

            // Expand closures now
            foreach (var definition in definitions.OfType<ClosureDefinition>())
            {
                ExpandClosureDefinition(definition);
            }

            ProcessExpressions(_baseScope, ExpressionAnalysisMode.ExpressionResolving);
        }

        /// <summary>
        /// Performs analysis of constant resolving of statements in a given code scope
        /// </summary>
        /// <param name="scope">The code scope to process expressions on</param>
        private void ProcessStatements(CodeScope scope)
        {
            var resolver = new ExpressionConstantResolver(_generationContext, new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_typeResolver, resolver, ExpressionAnalysisMode.ConstantStatementAnalysis);
            var definitions = scope.Definitions;

            foreach (var definition in definitions)
            {
                if (definition.Context != null)
                    traverser.Traverse(definition.Context);
            }
        }

        /// <summary>
        /// Performs deeper analysis of types by exploring expression nodes and deriving their types, as well as pre-evaluating any constants
        /// </summary>
        /// <param name="scope">The code scope to process expressions on</param>
        /// <param name="analysisMode">The mode of the analysis pass to perform</param>
        private void ProcessExpressions(CodeScope scope, ExpressionAnalysisMode analysisMode = ExpressionAnalysisMode.Complete)
        {
            var resolver = new ExpressionConstantResolver(_generationContext, new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_typeResolver, resolver, analysisMode);
            var definitions = scope.Definitions;

            foreach (var definition in definitions)
            {
                if(definition.Context != null)
                    traverser.Traverse(definition.Context);
            }
        }

        /// <summary>
        /// Performs deeper analysis of types by exploring expression nodes and deriving their types, as well as pre-evaluating any constants
        /// </summary>
        /// <param name="context">The context to process expressions on</param>
        /// <param name="analysisMode">The mode of the analysis pass to perform</param>
        private void ProcessExpressions(ParserRuleContext context, ExpressionAnalysisMode analysisMode = ExpressionAnalysisMode.Complete)
        {
            var resolver = new ExpressionConstantResolver(_generationContext, new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_typeResolver, resolver, analysisMode);

            traverser.Traverse(context);
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
            bool returnModified = false;

            // Get the parameter types
            foreach (var argumentDefinition in definition.Parameters)
            {
                ExpandFunctionArgument(argumentDefinition);
            }

            // Use the type to define the type of the closure
            var newType = TypeProvider.FindCommonType(definition.CallableTypeDef, contextType) as CallableTypeDef;

            if(newType != null)
            {
                // Iterate over the arguments and modify the return type
                for (int i = 0; i < newType.ParameterInfos.Length; i++)
                {
                    if (definition.Parameters[i].HasType)
                        continue;

                    definition.Parameters[i].IsVariadic = newType.ParameterInfos[i].IsVariadic;
                    definition.Parameters[i].Type = newType.ParameterInfos[i].ParameterType;
                }

                // Don't update the return type if the closure has a return type and new type is void: this may cause errors during return type analysis
                if (newType.HasReturnType && (!definition.HasReturnType || newType.ReturnType != TypeProvider.VoidType()))
                {
                    definition.ReturnType = newType.ReturnType;
                    definition.HasReturnType = true;

                    returnModified = true;
                }
            }

            // Now expand the closure like a normal function
            ExpandFunctionDefinition(definition);

            if (!definition.HasReturnType)
            {
                definition.ReturnType = _generationContext.TypeProvider.VoidType();
                definition.HasReturnType = true;

                returnModified = true;
            }

            // Get all of the local variables and expand them now
            var scope = _baseScope.GetScopeContainingContext(definition.Context);

            foreach (var valueDef in scope.GetAllDefinitionsRecursive().OfType<ValueHolderDefinition>().Where(d => !(d is FunctionArgumentDefinition)))
            {
                ExpandValueHolderDefinition(valueDef);
            }

            // Re-create the callable definition for the closure
            definition.RecreateCallableDefinition();

            // Analyze the return type of the closure now
            AnalyzeReturns(definition);
            
            // If the closure's return type was inferred and it is contained within a function call
            // expression, mark the type of the statement it is contained within to be resolved again
            var context = definition.Context.Parent;
            var expContext = ((ZScriptParser.ExpressionContext)context);

            if (returnModified && expContext.valueAccess() != null && expContext.valueAccess().functionCall() != null)
            {
                while (context != null)
                {
                    var expressionContext = context as ZScriptParser.ExpressionContext;
                    if (expressionContext != null)
                        expressionContext.HasTypeBeenEvaluated = false;

                    if (context is ZScriptParser.StatementContext)
                    {
                        break;
                    }

                    context = context.Parent;
                }
            }

            ProcessExpressions(((ZScriptParser.ClosureExpressionContext)definition.Context).functionBody(), ExpressionAnalysisMode.ExpressionResolving);
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
            if (closureContext.Parent != null)
            {
                return ((ZScriptParser.ExpressionContext)closureContext.Parent).ExpectedType ?? TypeProvider.AnyType();
            }

            return TypeProvider.AnyType();
        }

        /// <summary>
        /// Expands the type of a given function definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionDefinition(FunctionDefinition definition)
        {
            if (!definition.HasReturnType || definition.ReturnTypeContext == null)
                return;

            definition.ReturnType = _typeResolver.ResolveType(definition.ReturnTypeContext.type(), true);
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
                    statement.expression().ExpectedType = definition.ReturnType;

                    var type = _typeResolver.ResolveExpression(statement.expression());

                    if (!TypeProvider.CanImplicitCast(type, definition.ReturnType))
                    {
                        var target = statement.expression();

                        var message = "Cannot implicitly convert return value type " + type + ", function expects return type of " + definition.ReturnType;
                        Container.RegisterError(target.Start.Line, target.Start.Column, message, ErrorCode.InvalidCast, target);
                    }
                }
            }
        }

        /// <summary>
        /// Expands the type of a given class definition
        /// </summary>
        /// <param name="definition">The class definition to expand</param>
        private void ExpandClassDefinition(ClassDefinition definition)
        {
            // Register the class
            _classTypeSource.Classes.Add(definition);

            // Expand the fields
            foreach (var classField in definition.GetAllFields())
            {
                ExpandValueHolderType(classField);
            }

            // Update the class type def, since we need an updated signature to continue expanding the values
            definition.UpdateClassTypeDef();

            foreach (var classField in definition.GetAllFields())
            {
                ExpandValueHolderTypeFromValue(classField);
            }

            // Expand the functions
            foreach (var classMethod in definition.GetAllMethods())
            {
                ExpandFunctionDefinition(classMethod);
            }

            // Update the class type def
            definition.UpdateClassTypeDef();
        }

        /// <summary>
        /// Expands the type of a given variable definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandValueHolderDefinition(ValueHolderDefinition definition)
        {
            // Evaluate definition type
            ExpandValueHolderType(definition);
            ExpandValueHolderTypeFromValue(definition);
        }

        /// <summary>
        /// Expands the type declaration of a given variable definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandValueHolderType(ValueHolderDefinition definition)
        {
            // Evaluate definition type
            if (!definition.HasType)
            {
                definition.Type = TypeProvider.AnyType();
            }
            else
            {
                definition.Type = _typeResolver.ResolveType(definition.TypeContext, false);
            }
        }

        /// <summary>
        /// Expands the type of a given value holder definition from its initialization expression, if any
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandValueHolderTypeFromValue(ValueHolderDefinition definition)
        {
            if (!definition.HasValue)
            {
                return;
            }

            // Compare applicability
            TypeDef valueType;

            var argumentDefinition = definition as FunctionArgumentDefinition;
            if (argumentDefinition != null)
            {
                valueType = _typeResolver.ResolveCompileConstant(argumentDefinition.DefaultValue);
            }
            else
            {
                definition.ValueExpression.ExpressionContext.ExpectedType = definition.HasType ? definition.Type : null;
                definition.ValueExpression.ExpressionContext.HasTypeBeenEvaluated = false;
                valueType = _typeResolver.ResolveExpression(definition.ValueExpression.ExpressionContext);
            }

            if (!definition.HasType)
            {
                definition.Type = valueType;
                definition.HasInferredType = valueType != null;
            }

            var varType = definition.Type;

            if (varType != null && !TypeProvider.CanImplicitCast(valueType, varType))
            {
                var message = "Cannot assign value of type " + valueType + " to variable of type " + varType;
                Container.RegisterError(definition.Context.Start.Line, definition.Context.Start.Column, message,
                    ErrorCode.InvalidCast, definition.Context);
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
                definition.Type = TypeProvider.ListForType(definition.Type);
            }
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
            /// Target for definition collection
            /// </summary>
            private readonly ZRuntimeGenerator.DefaultDefinitionTypeProvider _definitionsTarget;

            /// <summary>
            /// The type of analysis to perform when using this expression statements traverser
            /// </summary>
            private readonly ExpressionAnalysisMode _analysisMode;

            /// <summary>
            /// The current closure depth
            /// </summary>
            private int _closureDepth;

            /// <summary>
            /// Initializes a new instance of the ExpressionStatementsTraverser class
            /// </summary>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            /// <param name="analysisMode">The type of analysis to perform when using this expression statements traverser</param>
            public ExpressionStatementsTraverser(ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver, ExpressionAnalysisMode analysisMode)
            {
                var target = constantResolver.Context.DefinitionTypeProvider as ZRuntimeGenerator.DefaultDefinitionTypeProvider;
                if (target != null)
                {
                    _definitionsTarget = target;
                }

                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
                _analysisMode = analysisMode;
            }

            /// <summary>
            /// Traverses a given context, analyzing the expressions contained within them
            /// </summary>
            /// <param name="context">The context to start traversing at</param>
            public void Traverse(RuleContext context)
            {
                ClearLocalStack();

                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(this, context);
            }

            // 
            // EnterStatement override
            // 
            public override void EnterStatement(ZScriptParser.StatementContext context)
            {
                if (_closureDepth > 0 && !_analysisMode.HasFlag(ExpressionAnalysisMode.InnerClosureExpressionResolving))
                    return;

                if (context.expression() != null)
                {
                    AnalyzeExpression(context.expression());
                }
                else if (context.assignmentExpression() != null)
                {
                    AnalyzeAssignmentExpression(context.assignmentExpression());
                }
            }

            // 
            // EnterClosureExpression override
            // 
            public override void EnterClosureExpression(ZScriptParser.ClosureExpressionContext context)
            {
                _closureDepth++;

                PushLocalScope();
            }

            // 
            // ExitClosureExpression override
            // 
            public override void ExitClosureExpression(ZScriptParser.ClosureExpressionContext context)
            {
                _closureDepth--;

                PopLocalScope();
            }

            #region Scope walking

            public override void EnterProgram(ZScriptParser.ProgramContext context)
            {
                PushLocalScope();
            }

            public override void ExitProgram(ZScriptParser.ProgramContext context)
            {
                PopLocalScope();
            }

            public override void EnterBlockStatement(ZScriptParser.BlockStatementContext context)
            {
                PushLocalScope();
            }

            public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
            {
                PopLocalScope();
            }

            public override void EnterSequenceFrame(ZScriptParser.SequenceFrameContext context)
            {
                PushLocalScope();
            }

            public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
            {
                PopLocalScope();
            }

            public override void EnterForStatement(ZScriptParser.ForStatementContext context)
            {
                PushLocalScope();
            }

            public override void ExitForStatement(ZScriptParser.ForStatementContext context)
            {
                PopLocalScope();
            }

            #endregion

            /// <summary>
            /// Clears all the locals in the locals stack
            /// </summary>
            private void ClearLocalStack()
            {
                if (_definitionsTarget != null)
                    _definitionsTarget.ClearLocalStack();
            }

            /// <summary>
            /// Pushes a new definition scope
            /// </summary>
            private void PushLocalScope()
            {
                if (_definitionsTarget != null)
                    _definitionsTarget.PushLocalScope();
            }

            /// <summary>
            /// Adds a given definition to the top of the definition stack
            /// </summary>
            /// <param name="definition">The definition to add to the top of the definition stack</param>
            private void AddLocal(LocalVariableDefinition definition)
            {
                if (_definitionsTarget != null)
                    _definitionsTarget.AddLocal(definition);
            }

            /// <summary>
            /// Pops a definition scope
            /// </summary>
            private void PopLocalScope()
            {
                if (_definitionsTarget != null)
                    _definitionsTarget.PopLocalScope();
            }

            /// <summary>
            /// Analyzes a given expression, making sure the expression is correct and valid
            /// </summary>
            /// <param name="context">The context containig the expression to analyze</param>
            private void AnalyzeExpression(ZScriptParser.ExpressionContext context)
            {
                if (context.HasTypeBeenEvaluated)
                    return;

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
                if (context.HasTypeBeenEvaluated)
                    return;

                _typeResolver.ResolveAssignmentExpression(context);

                _constantResolver.ExpandConstants(context);

                // Check if the left value is not a constant
                if (context.leftValue().IsConstant)
                {
                    var message = "Cannot reassign constant value " + context.leftValue().GetText();
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.ModifyingConstant);
                }
            }

            /// <summary>
            /// Analyzes a given value holder declaration context
            /// </summary>
            /// <param name="valueHolderDecl">The context containing the value declaration to analyze</param>
            private void AnalyzeVariableDeclaration(ZScriptParser.ValueHolderDeclContext valueHolderDecl)
            {
                var localDef = valueHolderDecl.Definition as LocalVariableDefinition;

                var definition = valueHolderDecl.Definition;
                var varType = definition.Type ?? _constantResolver.Context.TypeProvider.AnyType();

                if (definition.TypeContext != null)
                {
                    varType = _typeResolver.ResolveType(definition.TypeContext, false);
                }

                definition.Type = varType;

                var expression = valueHolderDecl.expression();
                if (expression == null)
                {
                    if (localDef != null)
                        AddLocal(localDef);

                    return;
                }

                if (expression.HasTypeBeenEvaluated)
                {
                    _constantResolver.ExpandConstants(expression);
                    if (localDef != null)
                        AddLocal(localDef);

                    return;
                }

                if (definition.TypeContext != null)
                {
                    expression.ExpectedType = varType;
                }

                var valueType = _typeResolver.ResolveExpression(expression);
                _constantResolver.ExpandConstants(expression);

                // Inferring
                if (definition.TypeContext == null)
                {
                    definition.Type = valueType;
                }

                if (varType != null && valueType != null && !_typeResolver.TypeProvider.CanImplicitCast(valueType, varType))
                {
                    var message = "Cannot assign value of type " + valueType + " to variable of type " + varType;
                    _typeResolver.MessageContainer.RegisterError(definition.Context, message, ErrorCode.InvalidCast);
                }

                if (localDef != null)
                    AddLocal(localDef);
            }

            // 
            // EnterValueDeclareStatement override
            // 
            public override void EnterValueDeclareStatement(ZScriptParser.ValueDeclareStatementContext context)
            {
                var valueHolderDecl = context.valueHolderDecl();

                AnalyzeVariableDeclaration(valueHolderDecl);
            }

            // 
            // EnterIfStatement override
            // 
            public override void EnterIfStatement(ZScriptParser.IfStatementContext context)
            {
                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                    return;

                var analyzer = new IfStatementAnalyzer(context, _typeResolver, _constantResolver, _analysisMode);
                analyzer.Process();
            }

            // 
            // EnterWhileStatement override
            // 
            public override void EnterWhileStatement(ZScriptParser.WhileStatementContext context)
            {
                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                var expression = context.expression();
                var expType = _typeResolver.ResolveExpression(expression);

                _constantResolver.ExpandConstants(expression);

                if (!provider.CanImplicitCast(expType, provider.BooleanType()))
                {
                    if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                        return;

                    const string message = "Expressions on while conditions must be of boolean type";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
                    if (!_analysisMode.HasFlag(ExpressionAnalysisMode.ConstantStatementAnalysis))
                        return;

                    context.IsConstant = true;
                    context.ConstantValue = expression.ConstantValue.Equals(true);
                }
            }

            // 
            // EnterSwitchStatement override
            // 
            public override void EnterSwitchStatement(ZScriptParser.SwitchStatementContext context)
            {
                PushLocalScope();

                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                    return;

                // Pre-analyze valued switches
                if (context.valueHolderDecl() != null)
                {
                    AnalyzeVariableDeclaration(context.valueHolderDecl());
                }

                var analyzer = new SwitchCaseAnalyzer(context, _typeResolver, _constantResolver, _analysisMode);
                analyzer.Process();
            }

            // 
            // ExitSwitchStatement override
            // 
            public override void ExitSwitchStatement(ZScriptParser.SwitchStatementContext context)
            {
                PopLocalScope();
            }

            // 
            // EnterForInit override
            // 
            public override void EnterForInit(ZScriptParser.ForInitContext context)
            {
                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                    return;
                
                // Value declaration
                if (context.valueHolderDecl() != null)
                {
                    AnalyzeVariableDeclaration(context.valueHolderDecl());
                }

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
                var expression = context.expression();
                if (expression == null)
                    return;

                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                var conditionType = _typeResolver.ResolveExpression(expression);
                _constantResolver.ExpandConstants(expression);

                if (!provider.CanImplicitCast(conditionType, provider.BooleanType()))
                {
                    if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                        return;

                    const string message = "Expression on for condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
                    if (!_analysisMode.HasFlag(ExpressionAnalysisMode.ConstantStatementAnalysis))
                        return;

                    context.IsConstant = true;
                    context.ConstantValue = expression.ConstantValue.Equals(true);
                }
            }

            // 
            // EnterForIncrement override
            // 
            public override void EnterForIncrement(ZScriptParser.ForIncrementContext context)
            {
                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.StatementAnalysis))
                    return;

                // For loops can ommit the increment expression
                if (context.expression() == null)
                    return;

                _typeResolver.ResolveExpression(context.expression());
                _constantResolver.ExpandConstants(context.expression());
            }
        }

        /// <summary>
        /// Speifies the type of analysis to perform on a run of the expression traverser
        /// </summary>
        [Flags]
        public enum ExpressionAnalysisMode
        {
            /// <summary>Specifies basic expression resolving</summary>
            ExpressionResolving = 0x1 << 0,
            /// <summary>Specifies basic expression resolving inside closures</summary>
            InnerClosureExpressionResolving = 0x1 << 1,
            /// <summary>Specifies statement expression analysis</summary>
            StatementAnalysis = 0x1 << 2,
            /// <summary>Specifies an analysis which tests whether statements have compile-time constant resolving</summary>
            ConstantStatementAnalysis = 0x1 << 3,
            /// <summary>Specifies a complete analysis</summary>
            Complete = 0xFFFF
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
            /// The analysis mode to perform on this statement analyzer
            /// </summary>
            private readonly ExpressionAnalysisMode _analysisMode;

            /// <summary>
            /// Initializes a new instance of the SwitchCaseExplorer class
            /// </summary>
            /// <param name="switchContext">The context containing the statement to process</param>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            /// <param name="analysisMode">The analysis mode to perform on this statement analyzer</param>
            public SwitchCaseAnalyzer(ZScriptParser.SwitchStatementContext switchContext,
                ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver,
                ExpressionAnalysisMode analysisMode)
            {
                _switchContext = switchContext;
                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
                _analysisMode = analysisMode;
                _processedCases = new List<ZScriptParser.CaseBlockContext>();
            }

            /// <summary>
            /// Processes the statement
            /// </summary>
            public void Process()
            {
                TypeDef type = _typeResolver.TypeProvider.AnyType();

                if (_switchContext.expression() != null)
                {
                    // Expand switch statement's expression
                    type = _typeResolver.ResolveExpression(_switchContext.expression());
                    _constantResolver.ExpandConstants(_switchContext.expression());
                }
                else if (_switchContext.valueHolderDecl().expression() != null)
                {
                    // Expand switch statement's expression
                    type = _typeResolver.ResolveExpression(_switchContext.valueHolderDecl().expression());
                    _constantResolver.ExpandConstants(_switchContext.valueHolderDecl().expression());
                }

                // Push the type of the switch statement into the switch type stack so the EnterCaseBlock can utilize the value
                _switchType = type;

                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(this, _switchContext);
                
                AnalyzeConstantSwitch();
                AnalyzeSwitchVariable();
            }

            /// <summary>
            /// Analyzes the switch variable against case labels, and issue warnings when a case label uses the variable as an expression
            /// </summary>
            private void AnalyzeSwitchVariable()
            {
                if (_switchContext.expression() != null)
                    return;

                if (_switchContext.valueHolderDecl().expression() == null)
                {
                    const string message = "Value holder declarations in switch statements require a starting value";
                    _typeResolver.MessageContainer.RegisterError(_switchContext.valueHolderDecl(), message, ErrorCode.MissingValueOnSwitchValueDefinition);
                    return;
                }

                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.ConstantStatementAnalysis))
                    return;

                var varName = _switchContext.valueHolderDecl() == null ? null : _switchContext.valueHolderDecl().valueHolderName().GetText();

                foreach (var caseContext in _processedCases)
                {
                    if (caseContext.expression().GetText() == varName)
                    {
                        const string message = "Using the switch variable as a case expression always evaluates to true";
                        _typeResolver.MessageContainer.RegisterWarning(caseContext.expression(), message, WarningCode.ConstantSwitchExpression);
                        break;
                    }
                }
            }

            /// <summary>
            /// Verifes the cases processed, and check for constants in both case and switch expression evaluation, raising warnings when
            /// either the switch is constant and matches a constant case label, or a constant switch matches none of the constant case labels
            /// </summary>
            private void AnalyzeConstantSwitch()
            {
                if (!_analysisMode.HasFlag(ExpressionAnalysisMode.ConstantStatementAnalysis))
                    return;

                var expression = _switchContext.expression() ?? _switchContext.valueHolderDecl().expression();

                if (expression == null || !expression.IsConstant)
                {
                    return;
                }

                // Whether the switch matches another constant case
                bool matched = false;
                // Whether all of the cases are constant values
                bool allConstant = true;

                var switchValue = (_switchContext.expression() ?? _switchContext.valueHolderDecl().expression()).ConstantValue;

                for (int i = 0; i < _processedCases.Count; i++)
                {
                    var caseContext = _processedCases[i];
                    if (!caseContext.expression().IsConstant)
                    {
                        allConstant = false;
                        continue;
                    }

                    if (caseContext.expression().ConstantValue.Equals(switchValue))
                    {
                        const string message = "Constant case label that always matches constant switch expression";
                        _typeResolver.MessageContainer.RegisterWarning(caseContext.expression(), message,
                            WarningCode.ConstantSwitchExpression);

                        // If this is the first constant case and it always evaluates to true, set it as the default case
                        if (allConstant && !matched)
                        {
                            _switchContext.IsConstant = true;
                            _switchContext.ConstantCaseIndex = i;
                            _switchContext.ConstantCase = caseContext;
                        }

                        matched = true;
                    }
                    else
                    {
                        const string message = "Constant case label that never matches constant switch expression";
                        _typeResolver.MessageContainer.RegisterWarning(caseContext.expression(), message,
                            WarningCode.ConstantSwitchExpression);
                    }
                }

                if (!matched && allConstant)
                {
                    _switchContext.IsConstant = true;
                    _switchContext.ConstantCaseIndex = -1;
                    _switchContext.ConstantCase = null;

                    var target = _switchContext.expression() ?? (ParserRuleContext)_switchContext.valueHolderDecl();

                    string message = "Constant switch statement with no default case that matches no case never executes";

                    if (_switchContext.switchBlock().defaultBlock() != null)
                    {
                        message = "Constant switch statement wtih a default case that matches no case always executes the default case";
                    }
                    
                    _typeResolver.MessageContainer.RegisterWarning(target, message, WarningCode.ConstantSwitchExpression);
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
            /// The analysis mode to perform on this statement analyzer
            /// </summary>
            private readonly ExpressionAnalysisMode _analysisMode;

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
            /// <param name="analysisMode">The analysis mode to perform on this statement analyzer</param>
            public IfStatementAnalyzer(ZScriptParser.IfStatementContext ifContext, ExpressionTypeResolver typeResolver,
                ExpressionConstantResolver constantResolver, ExpressionAnalysisMode analysisMode)
            {
                _ifContext = ifContext;
                _typeResolver = typeResolver;
                _constantResolver = constantResolver;
                _analysisMode = analysisMode;
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
                    const string message = "Expressions on if conditions must be of boolean type";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
                    if (!_analysisMode.HasFlag(ExpressionAnalysisMode.ConstantStatementAnalysis))
                        return;

                    _ifContext.IsConstant = true;
                    _ifContext.ConstantValue = expression.ConstantValue.Equals(true);

                    var message = _ifContext.ConstantValue
                                    ? "True constant in if expression always executes the contained statements"
                                    : "False constant in if expression never executes the contained statements";

                    _typeResolver.MessageContainer.RegisterWarning(expression, message, WarningCode.ConstantIfCondition);
                }
            }
        }

        /// <summary>
        /// Custom type source that can feed types based on class names
        /// </summary>
        class ClassTypeSource : ICustomTypeSource
        {
            /// <summary>
            /// List of classes registered on this ClassTypeSource
            /// </summary>
            public readonly List<ClassDefinition> Classes;

            /// <summary>
            /// Initializes a new instance of the ClassTypeSource class
            /// </summary>
            public ClassTypeSource()
            {
                Classes = new List<ClassDefinition>();
            }

            // 
            // ICustomTypeSource.HasType implementation
            // 
            public bool HasType(string typeName)
            {
                return Classes.Any(c => c.Name == typeName);
            }

            // 
            // ICustomTypeSource.TypeNamed implementation
            // 
            public TypeDef TypeNamed(string typeName)
            {
                foreach (var definition in Classes)
                {
                    if (definition.Name == typeName)
                        return definition.ClassTypeDef;
                }

                return null;
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