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
        public void Analyze(ParserRuleContext context)
        {
            var walker = new ParseTreeWalker();
            walker.Walk(this, context);

            CodeScopeReachabilityAnalyzer.Analyze(_baseScope);
            var rsa = new ReturnStatementAnalyzer();
            rsa.AnalyzeScope(_baseScope, _messageContainer);
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
                ValueExpression = new Expression(variable.variableDeclare().expression()),
                IsInstanceValue = IsInInstanceScope()
            };

            CheckCollisions(def, valueHolderDecl.valueHolderName().IDENT());

            _currentScope.AddDefinition(def);
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
                IsConstant = true,
                IsInstanceValue = IsInInstanceScope()
            };

            CheckCollisions(def, valueHolderDecl.valueHolderName().IDENT());

            _currentScope.AddDefinition(def);
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

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a function argument on the top-most scope
        /// </summary>
        /// <param name="argument">The argument to define</param>
        void DefineFunctionArgument(ZScriptParser.FunctionArgContext argument)
        {
            var def = new FunctionArgumentDefinition { Name = argument.argumentName().IDENT().GetText(), Context = argument };

            CheckCollisions(def, argument.argumentName().IDENT());

            _currentScope.AddDefinition(def);
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
            var def = FunctionDefinitionGenerator.GenerateExportFunctionDef(exportFunction);

            CheckCollisions(def, exportFunction.functionName().IDENT());

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineFunction(ZScriptParser.FunctionDefinitionContext function)
        {
            var def = FunctionDefinitionGenerator.GenerateFunctionDef(function);

            CheckCollisions(def, function.functionName().IDENT());

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
            var def = FunctionDefinitionGenerator.GenerateClosureDef(closure);

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

                // Constructor definition
                if (d is ObjectDefinition && def is FunctionDefinition && IsContextChildOf(def.Context, d.Context))
                    continue;

                if (node == null)
                {
                    _messageContainer.RegisterError(0, 0, "Duplicated definition of " + def.Name + " collides with definition " + d);
                }
                else
                {
                    _messageContainer.RegisterError(node, "Duplicated definition of " + def.Name + " collides with definition " + d);
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
                var f = new FunctionDefinition(context.functionName().IDENT().GetText(), context.functionBody(),
                    CollectFunctionArguments(context.functionArguments()))
                {
                    Context = context,
                    HasReturnType = context.returnType() != null,
                    IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
                };

                return f;
            }

            /// <summary>
            /// Generates a new closure definition using the given context
            /// </summary>
            /// <param name="context">The context to generate the closure definition from</param>
            /// <returns>A closure definition generated from the given context</returns>
            public static ClosureDefinition GenerateClosureDef(ZScriptParser.ClosureExpressionContext context)
            {
                var c = new ClosureDefinition("", context.functionBody(),
                    CollectFunctionArguments(context.functionArguments()))
                {
                    Context = context,
                    HasReturnType = context.returnType() != null,
                    IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
                };

                return c;
            }

            /// <summary>
            /// Generates a new export function definition using the given context
            /// </summary>
            /// <param name="context">The context to generate the export function definition from</param>
            /// <returns>An export function definition generated from the given context</returns>
            public static ExportFunctionDefinition GenerateExportFunctionDef(ZScriptParser.ExportDefinitionContext context)
            {
                var e = new ExportFunctionDefinition(context.functionName().IDENT().GetText(),
                    CollectFunctionArguments(context.functionArguments()))
                {
                    Context = context,
                    HasReturnType = context.returnType() != null,
                    IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
                };

                return e;
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

        /// <summary>
        /// Analyzes the reachability of the code scopes
        /// </summary>
        private static class CodeScopeReachabilityAnalyzer
        {
            /// <summary>
            /// Analyzes the given scope recursively, updating the reachability of the child scopes
            /// </summary>
            /// <param name="scope">The scope to analyze</param>
            public static void Analyze(CodeScope scope)
            {
                if (scope.Context == null || scope.Context.Parent == null)
                    return;

                scope.IsConditional = IsContextConditioanl(scope.Context.Parent);
            }

            /// <summary>
            /// Returns a value specifying whether a given context is conditional
            /// </summary>
            /// <param name="context">The context to avaliate the conditionability</param>
            /// <returns>Whether the given context is conditional</returns>
            private static bool IsContextConditioanl(RuleContext context)
            {
                return context is ZScriptParser.IfStatementContext || context is ZScriptParser.ForStatementContext ||
                       context is ZScriptParser.CaseBlockContext || context is ZScriptParser.DefaultBlockContext;
            }
        }

        /// <summary>
        /// Analyzes the return statements in the body of a function
        /// </summary>
        private class ReturnStatementAnalyzer
        {
            /// <summary>
            /// List of all the return statements of the currently processed function
            /// </summary>
            private List<ZScriptParser.ReturnStatementContext> _returnStatements;

            /// <summary>
            /// The current message container for the analyzer
            /// </summary>
            private MessageContainer _messageContainer;

            /// <summary>
            /// The current function definition being analyzed
            /// </summary>
            private FunctionDefinition _currentDefinition;

            /// <summary>
            /// Analyzes a given scope for functions with mismatching return statements
            /// </summary>
            /// <param name="scope">The scope to analyze</param>
            /// <param name="messageContainer">The container to report the error messages to</param>
            public void AnalyzeScope(CodeScope scope, MessageContainer messageContainer)
            {
                _messageContainer = messageContainer;

                var funcs = scope.GetAllDefinitionsRecursive().OfType<FunctionDefinition>();

                foreach (var func in funcs)
                {
                    // Functions missing bodies are not analyzed
                    if (func.BodyContext == null)
                        continue;

                    AnalyzeFunction(func);
                }
            }

            /// <summary>
            /// Analyzes a given function definition
            /// </summary>
            /// <param name="func">The function definition to analyze</param>
            /// <returns>The return statement state for the function</returns>
            private void AnalyzeFunction(FunctionDefinition func)
            {
                var context = func.BodyContext;

                _currentDefinition = func;

                // Check for inconsistent return statement valuation
                _returnStatements = new List<ZScriptParser.ReturnStatementContext>();

                if (context.blockStatement().statement().Length == 0)
                {
                    if (func.HasReturnType && !func.IsVoid)
                    {
                        _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                            "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
                    }

                    return;
                }

                var block = context.blockStatement();
                var state = AnalyzeBlockStatement(block);

                if (state == ReturnStatementState.Partial)
                {
                    _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Not all code paths of function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
                }
                else if (func.HasReturnType && !func.IsVoid && state != ReturnStatementState.Complete)
                {
                    _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                        "Not all code paths of non-void function '" + func.Name + "' return a value", ErrorCode.IncompleteReturnPaths, context);
                }

                // Check inconsistencies on return types
                if (!func.HasReturnType && _returnStatements.Count > 0)
                {
                    ZScriptParser.ReturnStatementContext last = _returnStatements[0];
                    bool valuedReturn = false;
                    bool inconsistentReturn = false;
                    foreach (var rsc in _returnStatements)
                    {
                        valuedReturn = valuedReturn || (rsc.expression() != null);
                        if((last.expression() == null) != (rsc.expression() == null))
                        {
                            inconsistentReturn = true;
                            break;
                        }

                        last = rsc;
                    }

                    // Report inconsistent returns
                    if (inconsistentReturn)
                    {
                        _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                                "Function '" + func.Name + "' has inconsistent returns: Some returns are void, some are valued.", ErrorCode.InconsistentReturns, context);
                    }
                    // Check for early-returns on functions that have a missing return value
                    else if (!func.HasReturnType && valuedReturn && state != ReturnStatementState.Complete)
                    {
                        _messageContainer.RegisterError(func.Context.Start.Line, func.Context.Start.Column,
                                "Function '" + func.Name + "' has inconsistent returns: Not all paths have valued returns.", ErrorCode.IncompleteReturnPathsWithValuedReturn, context);
                    }
                }
            }

            /// <summary>
            /// Analyzes a given block statement
            /// </summary>
            private ReturnStatementState AnalyzeBlockStatement(ZScriptParser.BlockStatementContext block)
            {
                var statements = block.statement();

                if(statements.Length == 0)
                    return ReturnStatementState.NotPresent;

                var state = ReturnStatementState.DoesNotApply;

                foreach (var stmt in statements)
                {
                    var blockState = AnalyzeStatement(stmt);

                    if(blockState == ReturnStatementState.Complete)
                        return blockState;

                    state = MergeReturnStates(state, blockState);
                }

                return state;
            }

            /// <summary>
            /// Analyzes a given statement context for return statement state
            /// </summary>
            private ReturnStatementState AnalyzeStatement(ZScriptParser.StatementContext context)
            {
                if(context.returnStatement() != null)
                    return AnalyzeReturnStatement(context.returnStatement());

                if(context.ifStatement() != null)
                    return AnalyzeIfStatement(context.ifStatement());

                if(context.switchStatement() != null)
                    return AnalyzeSwitchStatement(context.switchStatement());

                if(context.whileStatement() != null)
                    return AnalyzeWhileStatement(context.whileStatement());

                if (context.forStatement() != null)
                    return AnalyzeForStatement(context.forStatement());

                if(context.blockStatement() != null)
                    return AnalyzeBlockStatement(context.blockStatement());

                return ReturnStatementState.DoesNotApply;
            }

            /// <summary>
            /// Analyzes a given return statement. Always returns ReturnStatementState.Present
            /// </summary>
            private ReturnStatementState AnalyzeReturnStatement(ZScriptParser.ReturnStatementContext context)
            {
                if (IsCurrentReturnValueVoid())
                {
                    if(context.expression() != null)
                    {
                        _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Trying to return a value on a void context", ErrorCode.ReturningValueOnVoidFunction, context);
                    }
                }
                else if (_currentDefinition.HasReturnType && context.expression() == null)
                {
                    _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Return value is missing in non-void context", ErrorCode.MissingReturnValueOnNonvoid, context);
                }

                _returnStatements.Add(context);
                return ReturnStatementState.Complete;
            }

            /// <summary>
            /// Analyzes a given IF statement context for return statement state
            /// </summary>
            private ReturnStatementState AnalyzeIfStatement(ZScriptParser.IfStatementContext context)
            {
                // If's are always partial
                var state = AnalyzeStatement(context.statement());

                var elseStatement = context.elseStatement();

                if (elseStatement == null)
                {
                    return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
                }

                state = MergeReturnStates(state, AnalyzeStatement(elseStatement.statement()));

                return state;
            }

            /// <summary>
            /// Analyzes a given WHILE statement context for return statement state
            /// </summary>
            private ReturnStatementState AnalyzeWhileStatement(ZScriptParser.WhileStatementContext context)
            {
                // If's are always partial
                var state = AnalyzeStatement(context.statement());

                // Loop statements are always partial
                return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
            }

            /// <summary>
            /// Analyzes a given FOR statement context for return statement state
            /// </summary>
            private ReturnStatementState AnalyzeForStatement(ZScriptParser.ForStatementContext context)
            {
                // If's are always partial
                var state = AnalyzeStatement(context.statement());

                // Loop statements are always partial
                return state == ReturnStatementState.Partial ? state : ReturnStatementState.NotPresent;
            }

            /// <summary>
            /// Analyzes a given IF statement context for return statement state
            /// </summary>
            private ReturnStatementState AnalyzeSwitchStatement(ZScriptParser.SwitchStatementContext context)
            {
                var block = context.switchBlock();
                var cases = block.caseBlock();
                var def = block.defaultBlock();

                var state = ReturnStatementState.DoesNotApply;
                foreach (var cbc in cases)
                {
                    foreach (var statementContext in cbc.statement())
                    {
                        state = MergeReturnStates(state, AnalyzeStatement(statementContext));
                    }
                }

                if (def != null)
                {
                    foreach (var statementContext in def.statement())
                    {
                        state = MergeReturnStates(state, AnalyzeStatement(statementContext));
                    }
                }
                else
                {
                    return MergeReturnStates(ReturnStatementState.NotPresent, state);
                }

                return state == ReturnStatementState.DoesNotApply ? ReturnStatementState.NotPresent : state;
            }

            /// <summary>
            /// Merges two return statement states
            /// </summary>
            private ReturnStatementState MergeReturnStates(ReturnStatementState state1, ReturnStatementState state2)
            {
                if (state1 == ReturnStatementState.DoesNotApply)
                    return state2;

                if (state2 == ReturnStatementState.DoesNotApply)
                    return state1;

                if(state1 == ReturnStatementState.Complete && state2 == ReturnStatementState.Complete)
                    return ReturnStatementState.Complete;
                
                if(state1 == ReturnStatementState.NotPresent && state2 == ReturnStatementState.NotPresent)
                    return ReturnStatementState.NotPresent;
                
                return ReturnStatementState.Partial;
            }

            /// <summary>
            /// Whether the current return value is void
            /// </summary>
            /// <returns>Whether the current return value is void</returns>
            private bool IsCurrentReturnValueVoid()
            {
                return _currentDefinition.IsVoid;
            }

            /// <summary>
            /// Defines the return statement state for a block statement
            /// </summary>
            private enum ReturnStatementState
            {
                /// <summary>The state does not apply to the statement</summary>
                DoesNotApply,
                /// <summary>A return statement is present in all sub statements</summary>
                Complete,
                /// <summary>A return statement is not present in any sub statement</summary>
                NotPresent,
                /// <summary>A return statement is partial (or conditional) in one or more of the substatements</summary>
                Partial
            }
        }
    }
}