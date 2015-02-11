using System.Collections.Generic;
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
        /// A list of all the errors raised during the definition collection
        /// </summary>
        private readonly List<CodeError> _errorList = new List<CodeError>();

        /// <summary>
        /// List of all the warnings raised during analysis
        /// </summary>
        private readonly List<Warning> _warningList = new List<Warning>();

        /// <summary>
        /// The count of closures that were created
        /// </summary>
        private int _closuresCount;

        /// <summary>
        /// Gets an array of all the warnings that were raised
        /// </summary>
        public Warning[] Warnings
        {
            get { return _warningList.ToArray(); }
        }

        /// <summary>
        /// Gets a list of all the errors raised during the current analysis
        /// </summary>
        public List<CodeError> CollectedErrors
        {
            get { return _errorList; }
        }

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private DefinitionScope _baseScope;

        /// <summary>
        /// The current scope for the definitions
        /// </summary>
        private DefinitionScope _currentScope;

        /// <summary>
        /// Gets the collected base scope containing the scopes defined
        /// </summary>
        public DefinitionScope CollectedBaseScope
        {
            get { return _baseScope; }
        }

        /// <summary>
        /// Initializes a new instance of the ScopeCollector class
        /// </summary>
        public DefinitionsCollector()
        {
            _baseScope = new DefinitionScope();
            _currentScope = _baseScope;
        }

        #region Scope collection

        // Function definitions have their own scope, containing the parameters
        public override void EnterProgram(ZScriptParser.ProgramContext context)
        {
            // Push the default global scope
            //PushScope(context);
            _currentScope = _baseScope = new DefinitionScope { Context = context };
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

        public override void EnterVarDecl(ZScriptParser.VarDeclContext context)
        {
            if (!IsInGlobalScope())
                DefineVariable(context);
        }

        public override void EnterLetDecl(ZScriptParser.LetDeclContext context)
        {
            if (!IsInGlobalScope())
                DefineConstant(context);
        }

        public override void EnterFunctionArg(ZScriptParser.FunctionArgContext context)
        {
            DefineFunctionArgument(context);
        }

        public override void EnterObjectBody(ZScriptParser.ObjectBodyContext context)
        {
            DefineHiddenVariable("this");
        }

        public override void EnterSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            DefineHiddenVariable("this");
            DefineHiddenVariable("T");
            DefineHiddenVariable("async");
        }

        public override void EnterClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            DefineClosure(context);
        }

        #endregion

        /// <summary>
        /// Pushes a new scope of variables
        /// </summary>
        /// <param name="context">A context binded to the scope</param>
        void PushScope(ParserRuleContext context)
        {
            var newScope = new DefinitionScope { Context = context };

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
        /// REturns an array of all definitions in the available scopes that match the specified name
        /// </summary>
        /// <param name="definitionName">The definition name to match</param>
        /// <returns>An array of all the definitions that were found matching the name</returns>
        Definition[] GetDefinitionsByName(string definitionName)
        {
            var scope = _currentScope;
            var defs = new List<Definition>();

            while (scope != null)
            {
                defs.AddRange(scope.Definitions.Where(d => d.Name == definitionName));

                scope = scope.ParentScope;
            }

            return defs.ToArray();
        }

        /// <summary>
        /// Defines a new variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The context containing the variable to define</param>
        void DefineVariable(ZScriptParser.VarDeclContext variable)
        {
            var valueHolderDecl = variable.variableDeclare().valueHolderDecl();

            var def = new ValueHolderDefinition
            {
                Name = valueHolderDecl.valueHolderName().IDENT().GetText(),
                Context = variable,
                ValueExpression = new Expression(variable.variableDeclare().expression())
            };

            CheckCollisions(def, valueHolderDecl.valueHolderName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a new constant in the current top-most scope
        /// </summary>
        /// <param name="constant">The context containing the constant to define</param>
        void DefineConstant(ZScriptParser.LetDeclContext constant)
        {
            var valueHolderDecl = constant.constantDeclare().valueHolderDecl();

            var def = new ValueHolderDefinition
            {
                Name = valueHolderDecl.valueHolderName().IDENT().GetText(),
                Context = constant,
                ValueExpression = new Expression(constant.constantDeclare().expression()),
                IsConstant = true
            };

            CheckCollisions(def, valueHolderDecl.valueHolderName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a new global variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The global variable to define</param>
        void DefineGlobalVariable(ZScriptParser.GlobalVariableContext variable)
        {
            var varDecl = variable.variableDeclare().valueHolderDecl();
            var def = new GlobalVariableDefinition
            {
                Name = varDecl.valueHolderName().IDENT().GetText(),
                Context = variable,
                HasValue = variable.variableDeclare().expression() != null,
            };

            if (def.HasValue)
            {
                def.ValueExpression = new Expression(variable.variableDeclare().expression());
            }

            CheckCollisions(def, varDecl.valueHolderName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a function argument on the top-most scope
        /// </summary>
        /// <param name="argument">The argument to define</param>
        void DefineFunctionArgument(ZScriptParser.FunctionArgContext argument)
        {
            var def = new FunctionArgumentDefinition { Name = argument.argumentName().IDENT().GetText(), Context = argument };

            CheckCollisions(def, argument.argumentName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a variable that is hidden, that is, it can be accessed, but does not comes from the script source
        /// </summary>
        /// <param name="variableName">The name of the variable to define</param>
        void DefineHiddenVariable(string variableName)
        {
            var def = new ValueHolderDefinition { Name = variableName };

            CheckCollisions(def, null);

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a new export function on the current top-most scope
        /// </summary>
        /// <param name="exportFunction">The export function to define</param>
        void DefineExportFunction(ZScriptParser.ExportDefinitionContext exportFunction)
        {
            var def = FunctionDefinitionGenerator.GenerateExportFunctionDef(exportFunction);

            CheckCollisions(def, exportFunction.functionName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineFunction(ZScriptParser.FunctionDefinitionContext function)
        {
            var def = FunctionDefinitionGenerator.GenerateFunctionDef(function);

            CheckCollisions(def, function.functionName().IDENT());

            _currentScope.Definitions.Add(def);
        }

        /// <summary>
        /// Defines a new closure in the current top-most scope
        /// </summary>
        /// <param name="closure">The closure to define</param>
        void DefineClosure(ZScriptParser.ClosureExpressionContext closure)
        {
            var def = FunctionDefinitionGenerator.GenerateClosureDef(closure);

            def.Name = ClosureDefinition.ClosureNamePrefix + (_closuresCount++);

            // Closures are always defined at the base scope
            _baseScope.Definitions.Add(def);
        }

        /// <summary>
        /// Checks collisions with the specified definition against the definitions in the available scopes
        /// </summary>
        /// <param name="def">The definition to check</param>
        /// <param name="node">A node used during analysis to report errors</param>
        void CheckCollisions(Definition def, ITerminalNode node)
        {
            var defs = GetDefinitionsByName(def.Name);

            foreach (var d in defs)
            {
                // Collisions between exported definitions are ignored
                if (d is ExportFunctionDefinition && !(def is ExportFunctionDefinition))
                    continue;

                if (node == null)
                {
                    RegisterError(0, 0, "Duplicated definition of " + def.Name + " collides with definition " + d);
                }
                else
                {
                    RegisterError(node, "Duplicated definition of " + def.Name + " collides with definition " + d);
                }
            }
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        void RegisterWarning(int line, int position, string warningMessage)
        {
            _warningList.Add(new Warning(line, position, warningMessage));
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        void RegisterWarning(ITerminalNode context, string warningMessage)
        {
            _warningList.Add(new Warning(context.Symbol.Line, context.Symbol.Column, warningMessage));
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        void RegisterError(int line, int position, string warningMessage)
        {
            _errorList.Add(new CodeError(line, position, warningMessage));
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        void RegisterError(ITerminalNode context, string warningMessage)
        {
            _errorList.Add(new CodeError(context.Symbol.Line, context.Symbol.Column, warningMessage));
        }

        /// <summary>
        /// Generator that creates function definitions
        /// </summary>
        private static class FunctionDefinitionGenerator
        {
            /// <summary>
            /// Generates a new function definition using the given context
            /// </summary>
            /// <param name="context">The context to generate the function definition from</param>
            /// <returns>A function definition generated from the given context</returns>
            public static FunctionDefinition GenerateFunctionDef(ZScriptParser.FunctionDefinitionContext context)
            {
                return new FunctionDefinition(context.functionName().IDENT().GetText(), context.functionBody(), CollectFunctionArguments(context.functionArguments())) { Context = context };
            }

            /// <summary>
            /// Generates a new closure definition using the given context
            /// </summary>
            /// <param name="context">The context to generate the closure definition from</param>
            /// <returns>A closure definition generated from the given context</returns>
            public static ClosureDefinition GenerateClosureDef(ZScriptParser.ClosureExpressionContext context)
            {
                return new ClosureDefinition("", context.functionBody(), CollectFunctionArguments(context.functionArguments())) { Context = context };
            }

            /// <summary>
            /// Generates a new export function definition using the given context
            /// </summary>
            /// <param name="context">The context to generate the export function definition from</param>
            /// <returns>An export function definition generated from the given context</returns>
            public static ExportFunctionDefinition GenerateExportFunctionDef(ZScriptParser.ExportDefinitionContext context)
            {
                return new ExportFunctionDefinition(context.functionName().IDENT().GetText(), CollectFunctionArguments(context.functionArguments())) { Context = context };
            }

            /// <summary>
            /// Returns an array of function arguments from a given function definition context
            /// </summary>
            /// <param name="context">The context to collect the function arguments from</param>
            /// <returns>An array of function arguments that were collected</returns>
            private static FunctionArgumentDefinition[] CollectFunctionArguments(ZScriptParser.FunctionArgumentsContext context)
            {
                if (context == null || context.argumentList() == null)
                    return new FunctionArgumentDefinition[0];

                var argList = context.argumentList().functionArg();
                var args = new FunctionArgumentDefinition[argList.Length];

                int i = 0;
                foreach (var arg in argList)
                {
                    var a = new FunctionArgumentDefinition
                    {
                        Name = arg.argumentName().IDENT().GetText(),
                        HasValue = arg.compileConstant() != null,
                        Context = arg
                    };

                    if (arg.variadic != null)
                    {
                        a.IsVariadic = true;
                    }
                    else if (arg.compileConstant() != null)
                    {
                        a.HasValue = true;
                        a.DefaultValue = arg.compileConstant();
                    }

                    args[i++] = a;
                }

                return args;
            }
        }
    }
}