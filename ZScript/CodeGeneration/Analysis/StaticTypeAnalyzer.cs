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
using ZScript.CodeGeneration.Analysis.Definitions;
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
        /// The generic type analyzer used to expand generic types
        /// </summary>
        private readonly GenericSignatureAnalyzer _genericAnalyzer;

        /// <summary>
        /// Type source for classes
        /// </summary>
        private readonly ClassTypeSource _classTypeSource;

        /// <summary>
        /// Type source for generics
        /// </summary>
        private readonly GenericTypeSource _genericTypeSource;

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
            _genericTypeSource = new GenericTypeSource();
            _genericAnalyzer = new GenericSignatureAnalyzer(generationContext);
        }

        /// <summary>
        /// Performs the analysis of the definitions using the current settings defined in the constructor of this static type analyzer
        /// </summary>
        public void Analyze()
        {
            // Assign the type source for the type provider
            TypeProvider.RegisterCustomTypeSource(_classTypeSource);
            TypeProvider.RegisterCustomTypeSource(_genericTypeSource);

            // Clear generic context
            _genericTypeSource.Clear();

            // Get all definitons
            var definitions = _baseScope.GetAllDefinitionsRecursive().ToArray();

            // Register the classes
            _classTypeSource.Classes.AddRange(definitions.OfType<ClassDefinition>());

            // Analyze class fields
            foreach (var classDef in definitions.OfType<ClassDefinition>())
            {
                ExpandClassDefinition(classDef);
            }

            // Analyze functions first
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => !(d is ClosureDefinition)))
            {
                ExpandFunctionDefinition(definition);
            }

            // Analyze global variables now
            foreach (var globalVar in definitions.OfType<GlobalVariableDefinition>())
            {
                ExpandGlobalVarable(globalVar);
            }

            // Analyze function arguments, ignoring function arguments defined within closures for now
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
            // local variables (which will be processed in the ProcessStatments method), and global variables (which where processed, earlier as well)
            foreach (
                var valueHolderDefinition in
                    definitions.OfType<ValueHolderDefinition>()
                        .Where(
                            d =>
                                !(d is LocalVariableDefinition) &&
                                !(d is FunctionArgumentDefinition) &&
                                !(d is GlobalVariableDefinition)))
            {
                ExpandValueHolderDefinition(valueHolderDefinition);
            }
            
            ProcessStatements(_baseScope);

            // Verify return types now
            foreach (var definition in definitions.OfType<FunctionDefinition>().Where(d => d.BodyContext != null))
            {
                AnalyzeReturns(definition);
            }
        }

        /// <summary>
        /// Performs deeper analysis of types by exploring statement nodes and deriving their types, as well as pre-evaluating any constants
        /// </summary>
        /// <param name="scope">The code scope to process expressions on</param>
        private void ProcessStatements(CodeScope scope)
        {
            var resolver = new ExpressionConstantResolver(_generationContext, new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_generationContext, this, resolver);
            var definitions = scope.Definitions;

            foreach (var definition in definitions.Where(d => !(d is GlobalVariableDefinition)))
            {
                // Generic signature context
                var funcDef = definition as FunctionDefinition;
                if (funcDef != null)
                {
                    _genericTypeSource.PushGenericContext(funcDef.GenericSignature);
                }

                if(!(definition is ClosureDefinition) && definition.Context != null)
                    traverser.Traverse(definition.Context);

                if (funcDef != null)
                {
                    _genericTypeSource.PopContext();
                }
            }
        }

        /// <summary>
        /// Expands the type of a given function definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionDefinition(FunctionDefinition definition)
        {
            // Push the context for the function definition's generic context
            _genericTypeSource.PushGenericContext(definition.GenericSignature);

            _genericAnalyzer.AnalyzeSignature(definition.GenericSignature);

            if (definition.HasReturnType && definition.ReturnTypeContext != null)
            {
                definition.ReturnType = _typeResolver.ResolveType(definition.ReturnTypeContext.type(), true);
            }

            _genericTypeSource.PopContext();
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
                if (statement.expression() == null) continue;

                // Push the context for the function definition's generic context
                _genericTypeSource.PushGenericContext(statement.TargetFunction.GenericSignature);

                // Resolve a second time, inferring types to closures
                statement.expression().ExpectedType = definition.ReturnType;

                var type = _typeResolver.ResolveExpression(statement.expression());

                if (!TypeProvider.CanImplicitCast(type, definition.ReturnType))
                {
                    var target = statement.expression();

                    var message = "Cannot implicitly convert return value type " + type + ", function expects return type of " + definition.ReturnType;
                    Container.RegisterError(target.Start.Line, target.Start.Column, message, ErrorCode.InvalidCast, target);
                }

                // Pop the generic signature context
                _genericTypeSource.PopContext();
            }
        }

        /// <summary>
        /// Expands the type of a given global variable
        /// </summary>
        /// <param name="definition">The global variable to expand</param>
        private void ExpandGlobalVarable(GlobalVariableDefinition definition)
        {
            var resolver = new ExpressionConstantResolver(_generationContext, new TypeOperationProvider());
            var traverser = new ExpressionStatementsTraverser(_generationContext, this, resolver);

            traverser.Traverse(definition.Context);
        }

        /// <summary>
        /// Expands the type of a given class definition
        /// </summary>
        /// <param name="definition">The class definition to expand</param>
        private void ExpandClassDefinition(ClassDefinition definition)
        {
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
            definition.Type = TypeProvider.AnyType();

            // Evaluate definition type
            if (definition.HasType)
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
                if (definition.Context != null && !(definition.Type is OptionalTypeDef) && !(definition is FunctionArgumentDefinition))
                {
                    var message = "Non-optional variable " + definition.Name + " requires a starting value";
                    Container.RegisterError(definition.Context, message, ErrorCode.ValuelessNonOptionalDeclaration);
                }

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
                var expT = definition.HasType ? definition.Type : null;

                if (expT is OptionalTypeDef)
                {
                    expT = ((OptionalTypeDef)expT).BaseWrappedType;
                }

                definition.ValueExpression.ExpressionContext.ExpectedType = expT;
                definition.ValueExpression.ExpressionContext.HasTypeBeenEvaluated = false;
                valueType = _typeResolver.ResolveExpression(definition.ValueExpression.ExpressionContext);
            }

            if (!definition.HasType)
            {
                definition.Type = valueType;
                definition.HasInferredType = valueType != null;
            }

            var varType = definition.Type;

            if (varType == TypeProvider.NullType())
            {
                var message = "Cannot infer type of null-valued value holder " + definition.Name;
                Container.RegisterError(definition.Context, message, ErrorCode.IncompleteType);
            }

            if (varType != null && !TypeProvider.CanImplicitCast(valueType, varType))
            {
                var message = "Cannot assign value of type " + valueType + " to variable of type " + varType;
                Container.RegisterError(definition.Context, message, ErrorCode.InvalidCast);
            }
        }

        /// <summary>
        /// Expands the type of a given function argument definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionArgument(FunctionArgumentDefinition definition)
        {
            // Push the generic context
            _genericTypeSource.PushGenericContext(definition.Function.GenericSignature);

            ExpandValueHolderDefinition(definition);

            if (definition.IsVariadic)
            {
                definition.Type = TypeProvider.ListForType(definition.Type);
            }

            _genericTypeSource.PopContext();
        }

        /// <summary>
        /// Traverses expressions, performing type checks on them
        /// </summary>
        private class ExpressionStatementsTraverser : ZScriptBaseListener, IClosureExpectedTypeNotifier
        {
            /// <summary>
            /// The context for the runtime generation 
            /// </summary>
            private readonly RuntimeGenerationContext _context;

            /// <summary>
            /// The type analyzer that owns this ExpressionStatementsTraverser
            /// </summary>
            private readonly StaticTypeAnalyzer _typeAnalyzer;

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
            /// Gets the type provider for this expression statement traverser
            /// </summary>
            private TypeProvider TypeProvider
            {
                get { return _context.TypeProvider; }
            }

            /// <summary>
            /// Initializes a new instance of the ExpressionStatementsTraverser class
            /// </summary>
            /// <param name="context">The context for the runtime generation</param>
            /// <param name="typeAnalyzer">The type analyzer that owns this ExpressionStatementsTraverser</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public ExpressionStatementsTraverser(RuntimeGenerationContext context, StaticTypeAnalyzer typeAnalyzer, ExpressionConstantResolver constantResolver)
            {
                var target = constantResolver.Context.DefinitionTypeProvider as ZRuntimeGenerator.DefaultDefinitionTypeProvider;
                if (target != null)
                {
                    _definitionsTarget = target;
                }

                _context = context;
                _typeAnalyzer = typeAnalyzer;
                _typeResolver = typeAnalyzer._typeResolver;
                _constantResolver = constantResolver;

                _typeResolver.ClosureExpectedTypeNotifier = this;
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
                PushLocalScope();

                AnalyzeClosureDefinition(context.Definition);
            }

            // 
            // ExitClosureExpression override
            // 
            public override void ExitClosureExpression(ZScriptParser.ClosureExpressionContext context)
            {
                PopLocalScope();
            }

            // 
            // EnterReturnStatement override
            // 
            public override void EnterReturnStatement(ZScriptParser.ReturnStatementContext context)
            {
                if (context.expression() == null)
                    return;

                context.expression().ExpectedType = context.TargetFunction.ReturnType ?? TypeProvider.VoidType();

                AnalyzeExpression(context.expression());
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
            }

            /// <summary>
            /// Analyzes a given value holder declaration context
            /// </summary>
            /// <param name="valueHolderDecl">The context containing the value declaration to analyze</param>
            private void AnalyzeVariableDeclaration(ZScriptParser.ValueHolderDeclContext valueHolderDecl)
            {
                _typeAnalyzer.ExpandValueHolderDefinition(valueHolderDecl.Definition);

                var localDef = valueHolderDecl.Definition as LocalVariableDefinition;

                if (localDef != null)
                    AddLocal(localDef);
            }

            /// <summary>
            /// Expands the type of a given function argument definition
            /// </summary>
            /// <param name="definition">The definition to expand</param>
            private void AnalyzeFunctionArgument(FunctionArgumentDefinition definition)
            {
                _typeAnalyzer.ExpandValueHolderDefinition(definition);

                if (definition.IsVariadic)
                {
                    definition.Type = TypeProvider.ListForType(definition.Type);
                }
            }

            /// <summary>
            /// Expands the type of a closure definition, inferring types of arguments and returns when possible
            /// </summary>
            /// <param name="definition">The closure definition to expand</param>
            private void AnalyzeClosureDefinition(ClosureDefinition definition)
            {
                // Find the context the closure was defined in
                var definedContext = (ZScriptParser.ClosureExpressionContext)definition.Context;
                var contextType = FindExpectedTypeForClosure(definedContext);

                bool returnModified = false;

                // Get the parameter types
                foreach (var argumentDefinition in definition.Parameters)
                {
                    AnalyzeFunctionArgument(argumentDefinition);
                }

                // Use the type to define the type of the closure
                var newType = TypeProvider.FindCommonType(definition.CallableTypeDef, contextType) as CallableTypeDef;

                if (newType != null)
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
                if (definition.HasReturnType && definition.ReturnTypeContext != null)
                {
                    definition.ReturnType = _typeResolver.ResolveType(definition.ReturnTypeContext.type(), true);
                }

                if (!definition.HasReturnType)
                {
                    definition.ReturnType = TypeProvider.VoidType();
                    definition.HasReturnType = true;

                    returnModified = true;
                }

                // Re-create the callable definition for the closure
                definition.RecreateCallableDefinition();

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
                    var t = ((ZScriptParser.ExpressionContext)closureContext.Parent).ExpectedType ?? TypeProvider.AnyType();

                    var optT = t as OptionalTypeDef;
                    if (optT != null)
                    {
                        return optT.BaseWrappedType;
                    }
                    
                    return t;
                }

                return TypeProvider.AnyType();
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
                var expression = context.expression();
                var expType = _typeResolver.ResolveExpression(expression);

                _constantResolver.ExpandConstants(expression);

                if (!provider.CanImplicitCast(expType, provider.BooleanType()))
                {
                    const string message = "Expressions on while conditions must be of boolean type";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
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

                // Pre-analyze valued switches
                if (context.valueHolderDecl() != null)
                {
                    AnalyzeVariableDeclaration(context.valueHolderDecl());
                }

                var analyzer = new SwitchCaseAnalyzer(context, _typeResolver, _constantResolver);
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

                AnalyzeExpression(context.expression());

                // Check if expression has a boolean type
                var conditionType = context.expression().EvaluatedType;

                if (!provider.CanImplicitCast(conditionType, provider.BooleanType()))
                {
                    const string message = "Expression on for condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
                    context.IsConstant = true;
                    context.ConstantValue = expression.ConstantValue.Equals(true);
                }
            }

            // 
            // EnterForIncrement override
            // 
            public override void EnterForIncrement(ZScriptParser.ForIncrementContext context)
            {
                // For loops can ommit the increment expression
                if (context.expression() == null)
                    return;

                AnalyzeExpression(context.expression());
            }

            // 
            // EnterForEachHeader override
            // 
            public override void EnterForEachHeader(ZScriptParser.ForEachHeaderContext context)
            {
                // Verify type of the expression
                var valueType = _typeResolver.ResolveExpression(context.expression());

                // Verify compatibility
                if (!_context.TypeProvider.IsEnumerable(valueType))
                {
                    _context.MessageContainer.RegisterError(context.expression(), "Value on for each loop must be a enumerable list type", ErrorCode.InvalidCast);
                }

                // Type of variable is the inner type of the value
                var itemType = _context.TypeProvider.AnyType();
                var listTypeDef = valueType as IListTypeDef;
                if (listTypeDef != null)
                {
                    itemType = listTypeDef.EnclosingType;
                }

                var loopVar = context.LoopVariable;
                loopVar.ValueExpression = new Expression(context.expression());
                loopVar.HasValue = true;

                _typeAnalyzer.ExpandValueHolderType(loopVar);

                var varType = loopVar.Type;

                if (loopVar.HasType && varType != null && !TypeProvider.AreTypesCompatible(itemType, varType))
                {
                    var message = "Cannot assign value of type " + itemType + " to variable of type " + varType;
                    _context.MessageContainer.RegisterError(loopVar.Context, message, ErrorCode.InvalidCast);
                }
                
                loopVar.Type = itemType;

                AddLocal(loopVar);
            }

            // 
            // IClosureExpectedTypeNotifier.ClosureTypeMatched implementation
            // 
            public void ClosureTypeMatched(ZScriptParser.ClosureExpressionContext context, TypeDef expectedType)
            {
                AnalyzeClosureDefinition(context.Definition);

                context.IsInferred = true;
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
            /// Initializes a new instance of the SwitchCaseExplorer class
            /// </summary>
            /// <param name="switchContext">The context containing the statement to process</param>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public SwitchCaseAnalyzer(ZScriptParser.SwitchStatementContext switchContext,
                ExpressionTypeResolver typeResolver, ExpressionConstantResolver constantResolver)
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

                var varName = _switchContext.valueHolderDecl() == null ? null : _switchContext.valueHolderDecl().valueHolderDefine().valueHolderName().GetText();

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
            /// The context for the statement to analyze
            /// </summary>
            private readonly ZScriptParser.IfStatementContext _ifContext;

            /// <summary>
            /// Initializes a new instance of the IfStatementAnalyzer class
            /// </summary>
            /// <param name="ifContext">The context containing the statement to process</param>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            /// <param name="constantResolver">A constant resolver to use for pre-evaluating constants in expressions</param>
            public IfStatementAnalyzer(ZScriptParser.IfStatementContext ifContext, ExpressionTypeResolver typeResolver,
                ExpressionConstantResolver constantResolver)
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
                    const string message = "Expressions on if conditions must be of boolean type";
                    _typeResolver.MessageContainer.RegisterError(expression, message, ErrorCode.InvalidCast);
                }
                else if (expression.IsConstant && expression.EvaluatedType == _typeResolver.TypeProvider.BooleanType())
                {
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