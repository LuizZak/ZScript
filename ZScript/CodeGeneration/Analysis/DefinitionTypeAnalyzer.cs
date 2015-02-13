using System.Linq;

using Antlr4.Runtime;

using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Elements.Typing;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Analyzes and expands the types of definitions
    /// </summary>
    public class DefinitionTypeAnalyzer : IDefinitionTypeProvider
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
        public DefinitionTypeAnalyzer(TypeProvider typeProvider, CodeScope scope, MessageContainer container)
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
            foreach (var definition in definitions.OfType<FunctionDefinition>())
            {
                ExpandFunctionDefinition(definition);
            }

            foreach (var definition in definitions.OfType<FunctionArgumentDefinition>())
            {
                ExpandFunctionArgument(definition);
            }

            foreach (var definition in definitions.OfType<ValueHolderDefinition>().Where(d => !(d is FunctionArgumentDefinition)))
            {
                ExpandValueHolderDefinition(definition);
            }
        }

        /// <summary>
        /// Expands the type of a given function definition
        /// </summary>
        /// <param name="definition">The definition to expand</param>
        private void ExpandFunctionDefinition(FunctionDefinition definition)
        {
            if (!definition.HasReturnType)
                return;

            definition.ReturnType = _typeResolver.ResolveType(definition.ReturnTypeContext.type());

            // Verify return statements
            foreach (var statement in definition.ReturnStatements)
            {
                if (statement.expression() != null)
                {
                    var type = _typeResolver.ResolveExpression(statement.expression());

                    if (!_typeProvider.CanImplicitCast(type, definition.ReturnType))
                    {
                        var message = "Cannot implicitly convert return value type " + type + ", function expects return type of " + definition.ReturnType;
                        _container.RegisterError(definition.Context.Start.Line, definition.Context.Start.Column, message, ErrorCode.InvalidCast, definition.Context);
                    }
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
        private CodeScope ScopeForContext(ZScriptParser.MemberNameContext context)
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
    }
}