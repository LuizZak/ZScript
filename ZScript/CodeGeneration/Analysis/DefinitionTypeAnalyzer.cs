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
    public class DefinitionTypeAnalyzer : IDefinitionTypeProvider, IClosureExpectedTypeNotifier
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
        /// List of structs that bundle closures with expected types for later analysis
        /// </summary>
        private readonly List<ClosureExpectedType> _closureExpectedTypes;

        /// <summary>
        /// Initializes a new instance of the DefinitionTypeExpander class
        /// </summary>
        /// <param name="typeProvider">The type provider for resolving types</param>
        /// <param name="scope">The definition collector containing the definitions to expand</param>
        /// <param name="container">The message container to report error messages to</param>
        public DefinitionTypeAnalyzer(TypeProvider typeProvider, CodeScope scope, MessageContainer container)
        {
            _typeProvider = typeProvider;
            _baseScope = scope;
            _container = container;

            _typeResolver = new ExpressionTypeResolver(typeProvider, container, this);
            _typeResolver.ClosureExpectedTypeNotifier = this;
            _closureExpectedTypes = new List<ClosureExpectedType>();
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

            AnalyzeExpressions();
        }

        /// <summary>
        /// Performs deeper analysis of types by exploring expression nodes
        /// </summary>
        private void AnalyzeExpressions()
        {
            var traverser = new ExpressionStatementsTraverser(_typeResolver);
            var definitions = _baseScope.Definitions;

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

                definition.HasReturnType = true;
                // Don't update the return type if the new type is void: this may cause errors during return type analysis
                if(newType.ReturnType != _typeProvider.VoidType())
                    definition.ReturnType = newType.ReturnType;
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
            // TODO: Finish closure type searching

            // Search closures registered on expected type closures list
            foreach (var cet in _closureExpectedTypes)
            {
                if (cet.ClosureContext == closureContext)
                    return cet.ExpectedType;
            }
            
            return _typeProvider.AnyType();
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

                var holderDefinition = def as ValueHolderDefinition;
                if (holderDefinition != null)
                {
                    return holderDefinition.Type ?? TypeDef.AnyType;
                }

                var funcDef = def as FunctionDefinition;
                if (funcDef != null)
                {
                    return funcDef.CallableTypeDef;
                }

                var objDef = def as ObjectDefinition;
                if (objDef != null)
                {
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

        // 
        // IClosureExpectedTypeNotifier.ClosureTypeMatched implementation
        // 
        public void ClosureTypeMatched(ZScriptParser.ClosureExpressionContext context, TypeDef expectedType)
        {
            _closureExpectedTypes.Add(new ClosureExpectedType(context, expectedType));
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
            /// Initializes a new instance of the ExpressionStatementsTraverser class
            /// </summary>
            /// <param name="typeResolver">The type resolver to use when resolving the expressions</param>
            public ExpressionStatementsTraverser(ExpressionTypeResolver typeResolver)
            {
                _typeResolver = typeResolver;
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
                    _typeResolver.ResolveExpression(context.expression());

                    Console.WriteLine("Type for expression: " + context.expression().EvaluatedType);
                }
                else if (context.assignmentExpression() != null)
                {
                    _typeResolver.ResolveAssignmentExpression(context.assignmentExpression());

                    Console.WriteLine("Type for assignment expression: " + context.assignmentExpression().EvaluatedType);
                }
            }

            // 
            // EnterIfStatement override
            // 
            public override void EnterIfStatement(ZScriptParser.IfStatementContext context)
            {
                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                if (!provider.CanImplicitCast(_typeResolver.ResolveExpression(context.expression()), provider.BooleanType()))
                {
                    const string message = "Expression on if condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.InvalidCast);
                }
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
            }

            // 
            // EnterForStatement override
            // 
            public override void EnterForCondition(ZScriptParser.ForConditionContext context)
            {
                // For loops can ommit the condition
                if (context.expression() == null)
                    return;

                var provider = _typeResolver.TypeProvider;

                // Check if expression has a boolean type
                if (!provider.CanImplicitCast(_typeResolver.ResolveExpression(context.expression()), provider.BooleanType()))
                {
                    const string message = "Expression on for condition must be boolean";
                    _typeResolver.MessageContainer.RegisterError(context.expression(), message, ErrorCode.InvalidCast);
                }
            }
        }

        /// <summary>
        /// Struct that bundles a closure and an expected type together
        /// </summary>
        private struct ClosureExpectedType
        {
            /// <summary>
            /// The context containing the closure
            /// </summary>
            public readonly ZScriptParser.ClosureExpressionContext ClosureContext;

            /// <summary>
            /// The expected type that was imposed to the closure expression
            /// </summary>
            public readonly TypeDef ExpectedType;

            /// <summary>
            /// Initializes a new ClosureExpectedType struct
            /// </summary>
            /// <param name="closureContext">The context containing the closure</param>
            /// <param name="expectedType">The expected type that was imposed to the closure expression</param>
            public ClosureExpectedType(ZScriptParser.ClosureExpressionContext closureContext, TypeDef expectedType)
            {
                ClosureContext = closureContext;
                ExpectedType = expectedType;
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