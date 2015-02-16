using System.Linq;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Class capable of creating a tree of definition scopes, containing variables, functions and objects for contexts
    /// </summary>
    public class DefinitionsCollector : ZScriptBaseListener
    {
        /// <summary>
        /// The count of closures that were created
        /// </summary>
        private int _closuresCount;

        /// <summary>
        /// The message container that errors will be reported to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private CodeScope _baseScope;

        /// <summary>
        /// The current scope for the definitions
        /// </summary>
        private CodeScope _currentScope;

        /// <summary>
        /// Gets the collected base scope containing the scopes defined
        /// </summary>
        public CodeScope CollectedBaseScope
        {
            get { return _baseScope; }
        }

        /// <summary>
        /// Initializes a new instance of the ScopeCollector class
        /// </summary>
        /// <param name="messageContainer">The message container that errors will be reported to</param>
        public DefinitionsCollector(MessageContainer messageContainer)
        {
            _baseScope = new CodeScope();
            _currentScope = _baseScope;
            _messageContainer = messageContainer;
        }

        /// <summary>
        /// Analyzes a given subset of a parser rule context on this definitions collector
        /// </summary>
        /// <param name="context">The context to analyze</param>
        public void Collect(ParserRuleContext context)
        {
            var walker = new ParseTreeWalker();
            walker.Walk(this, context);
        }

        #region Scope collection

        // Function definitions have their own scope, containing the parameters
        public override void EnterProgram(ZScriptParser.ProgramContext context)
        {
            // Push the default global scope
            //PushScope(context);
            _currentScope = _baseScope = new CodeScope { Context = context };
        }

        public override void EnterExportDefinition(ZScriptParser.ExportDefinitionContext context)
        {
            // Define the function
            DefineExportFunction(context);

            PushScope(context);
        }

        public override void ExitExportDefinition(ZScriptParser.ExportDefinitionContext context)
        {
            PopScope();
        }

        public override void EnterFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            // Define the function
            DefineFunction(context);
            PushScope(context);
        }

        public override void ExitFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            PopScope();
        }

        public override void EnterBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PushScope(context);
        }

        public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PopScope();
        }

        public override void EnterObjectDefinition(ZScriptParser.ObjectDefinitionContext context)
        {
            DefineObject(context);

            PushScope(context);
        }

        public override void ExitObjectDefinition(ZScriptParser.ObjectDefinitionContext context)
        {
            PopScope();
        }

        public override void EnterSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            PushScope(context);
        }

        public override void ExitSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            PopScope();
        }

        public override void EnterSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            PushScope(context);
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            PopScope();
        }

        #endregion

        #region Definition collection

        public override void EnterGlobalVariable(ZScriptParser.GlobalVariableContext context)
        {
            DefineGlobalVariable(context);
        }

        public override void EnterValueHolderDecl(ZScriptParser.ValueHolderDeclContext context)
        {
            if (!IsInGlobalScope())
                DefineValueHolder(context);
        }

        public override void EnterFunctionArg(ZScriptParser.FunctionArgContext context)
        {
            DefineFunctionArgument(context);
        }

        public override void EnterObjectBody(ZScriptParser.ObjectBodyContext context)
        {
            DefineHiddenVariable("this");

            PushScope(context);
        }

        public override void ExitObjectBody(ZScriptParser.ObjectBodyContext context)
        {
            PopScope();
        }

        public override void EnterSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            DefineHiddenVariable("this");
            DefineHiddenVariable("T");
            DefineHiddenVariable("async");

            PushScope(context);
        }

        public override void ExitSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            PopScope();
        }

        public override void EnterClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            DefineClosure(context);
            PushScope(context);
        }

        public override void ExitClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            PopScope();
        }

        public override void EnterTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            DefineTypeAlias(context);
            PushScope(context);
        }

        public override void ExitTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            PopScope();
        }

        public override void EnterObjectFunction(ZScriptParser.ObjectFunctionContext context)
        {
            PushScope(context);

            // Add a definition pointing to the base overriden function.
            // The analysis of whether this is a valid call is done in a separate analysis phase
            DefineFunction(new FunctionDefinition("base", null, new FunctionArgumentDefinition[0]));
        }

        public override void ExitObjectFunction(ZScriptParser.ObjectFunctionContext context)
        {
            PopScope();
        }

        #endregion

        /// <summary>
        /// Pushes a new scope of variables
        /// </summary>
        /// <param name="context">A context binded to the scope</param>
        void PushScope(ParserRuleContext context)
        {
            var newScope = new CodeScope { Context = context };

            _currentScope.AddSubscope(newScope);
            _currentScope = newScope;
        }

        /// <summary>
        /// Pops the current scope, removing along all the variables from the current scope
        /// </summary>
        void PopScope()
        {
            _currentScope = _currentScope.ParentScope;
        }

        /// <summary>
        /// Returns whether the current scope is the global scope
        /// </summary>
        /// <returns>A boolean value specifying whether the current scope is the global scope</returns>
        bool IsInGlobalScope()
        {
            return _currentScope.Context is ZScriptParser.ProgramContext;
        }

        /// <summary>
        /// Returns whether the current scope is an instance (object or sequence) scope
        /// </summary>
        /// <returns>A boolean value specifying whether the current scope is instance (object or sequence) scope</returns>
        bool IsInInstanceScope()
        {
            return _currentScope.Context is ZScriptParser.ObjectBodyContext ||
                   _currentScope.Context is ZScriptParser.SequenceBodyContext;
        }

        /// <summary>
        /// Defines a new variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The context containing the variable to define</param>
        void DefineValueHolder(ZScriptParser.ValueHolderDeclContext variable)
        {
            var def = DefinitionGenerator.GenerateValueHolderDef(variable);

            def.IsInstanceValue = IsInInstanceScope();

            CheckCollisions(def, variable);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new global variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The global variable to define</param>
        void DefineGlobalVariable(ZScriptParser.GlobalVariableContext variable)
        {
            var def = DefinitionGenerator.GenerateGlobalVariable(variable);

            if (!def.HasValue && def.IsConstant)
            {
                _messageContainer.RegisterError(variable, "Constants require a value to be assigned on declaration", ErrorCode.ValuelessConstantDeclaration);
            }

            CheckCollisions(def, variable);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a function argument on the top-most scope
        /// </summary>
        /// <param name="argument">The argument to define</param>
        void DefineFunctionArgument(ZScriptParser.FunctionArgContext argument)
        {
            var def = DefinitionGenerator.GenerateFunctionArgumentDef(argument);

            CheckCollisions(def, argument.argumentName());

            // Try to find the definition in the current function definition for the argument, and store that instead
            var funcDef = (FunctionDefinition)_baseScope.GetDefinitionByContextRecursive(_currentScope.Context);

            _currentScope.AddDefinition(funcDef.Arguments.First(a => a.Context == argument));
        }

        /// <summary>
        /// Defines a variable that is hidden, that is, it can be accessed, but does not comes from the script source
        /// </summary>
        /// <param name="variableName">The name of the variable to define</param>
        void DefineHiddenVariable(string variableName)
        {
            var def = new ValueHolderDefinition { Name = variableName };

            CheckCollisions(def, null);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new export function on the current top-most scope
        /// </summary>
        /// <param name="exportFunction">The export function to define</param>
        void DefineExportFunction(ZScriptParser.ExportDefinitionContext exportFunction)
        {
            var def = DefinitionGenerator.GenerateExportFunctionDef(exportFunction);

            CheckCollisions(def, exportFunction.functionName());

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineFunction(ZScriptParser.FunctionDefinitionContext function)
        {
            var def = DefinitionGenerator.GenerateFunctionDef(function);

            CheckCollisions(def, function.functionName());

            DefineFunction(def);
        }
        
        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineFunction(FunctionDefinition function)
        {
            _currentScope.AddDefinition(function);
        }

        /// <summary>
        /// Defines a new closure in the current top-most scope
        /// </summary>
        /// <param name="closure">The closure to define</param>
        void DefineClosure(ZScriptParser.ClosureExpressionContext closure)
        {
            var def = DefinitionGenerator.GenerateClosureDef(closure);

            def.Name = ClosureDefinition.ClosureNamePrefix + (_closuresCount++);

            // Closures are always defined at the base scope
            _baseScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new object definition in the current top-most scope
        /// </summary>
        /// <param name="objectDefinition">The object to define</param>
        void DefineObject(ZScriptParser.ObjectDefinitionContext objectDefinition)
        {
            var def = new ObjectDefinition
            {
                Name = objectDefinition.objectName().IDENT().GetText(),
                Context = objectDefinition
            };

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new type alias definition in the current top-most scope
        /// </summary>
        /// <param name="typeAlias">The type alias to define</param>
        void DefineTypeAlias(ZScriptParser.TypeAliasContext typeAlias)
        {
            var def = TypeAliasDefinitionGenerator.GenerateTypeAlias(typeAlias);

            _currentScope.AddTypeAliasDefinition(def);
        }

        /// <summary>
        /// Checks collisions with the specified definition against the definitions in the available scopes
        /// </summary>
        /// <param name="def">The definition to check</param>
        /// <param name="context">A context used during analysis to report where the error happened</param>
        void CheckCollisions(Definition def, ParserRuleContext context)
        {
            var defs = _currentScope.GetDefinitionsByName(def.Name);

            foreach (var d in defs)
            {
                // Collisions between exported definitions are ignored
                if (d is ExportFunctionDefinition && !(def is ExportFunctionDefinition))
                    continue;

                // Constructor definition
                if (d is ObjectDefinition && def is FunctionDefinition && IsContextChildOf(def.Context, d.Context))
                    continue;

                var message = "Duplicated definition of " + def.Name + " collides with definition " + d;

                if (context == null)
                {
                    _messageContainer.RegisterError(0, 0, message, ErrorCode.DuplicatedDefinition, def.Context);
                }
                else
                {
                    _messageContainer.RegisterError(context.Start.Line, context.Start.Column, message, ErrorCode.DuplicatedDefinition, def.Context);
                }
            }
        }

        /// <summary>
        /// Returns whether a rule context is child of another context.
        /// </summary>
        /// <param name="context">The context to check</param>
        /// <param name="parent">The context to check for parenting</param>
        /// <returns>Whether 'context' is child of 'parent'</returns>
        bool IsContextChildOf(RuleContext context, RuleContext parent)
        {
            while (context != null)
            {
                context = context.Parent;

                if (context == parent)
                    return true;
            }

            return false;
        }
    }
}