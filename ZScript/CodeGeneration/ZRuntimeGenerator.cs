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
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using ZScript.Builders;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Sourcing;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;

using ZScript.Elements;
using ZScript.Elements.ValueHolding;

using ZScript.Parsing;

using ZScript.Runtime;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Helper entry point for the ZScript library
    /// </summary>
    public class ZRuntimeGenerator
    {
        /// <summary>
        /// Whether to debug the process in the console
        /// </summary>
        public bool Debug;

        /// <summary>
        /// The container for error and warning messages
        /// </summary>
        private MessageContainer _messageContainer;

        /// <summary>
        /// The error listener for the parsing of the script
        /// </summary>
        private readonly ZScriptSyntaxErrorListener _syntaxErrorListener;

        /// <summary>
        /// The parser for the currently parsed script
        /// </summary>
        private ZScriptParser _parser;

        /// <summary>
        /// The internal type provider used to generate typing for the definitions
        /// </summary>
        private readonly TypeProvider _typeProvider;

        /// <summary>
        /// The sources provider for this runtime generator
        /// </summary>
        private readonly SourceProvider _sourceProvider;

        /// <summary>
        /// The native type builder used during generation time
        /// </summary>
        private readonly ClassNativeTypeBuilder _nativeTypeBuilder;

        /// <summary>
        /// Returns the array of all the syntax errors that were found during the parsing of the script
        /// </summary>
        public SyntaxError[] SyntaxErrors
        {
            get { return _messageContainer.SyntaxErrors; }
        }

        /// <summary>
        /// Gets a value specifying whether there were any syntax errors on the script generation process
        /// </summary>
        public bool HasSyntaxErrors
        {
            get { return _messageContainer.HasSyntaxErrors; }
        }

        /// <summary>
        /// Gets a value specifying whether there were any code errors on the runtime generation process
        /// </summary>
        public bool HasCodeErrors
        {
            get { return _messageContainer.HasCodeErrors; }
        }

        /// <summary>
        /// Gets a value specifying whether there are any syntax or code error pending on this runtime generator
        /// </summary>
        public bool HasErrors
        {
            get { return _messageContainer.HasErrors; }
        }

        /// <summary>
        /// Gets or sets the message container for this scope analyzer
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
            set
            {
                _messageContainer = value;
                _syntaxErrorListener.MessageContainer = value;
            }
        }

        /// <summary>
        /// Gets the sources provider for this runtime generator
        /// </summary>
        public SourceProvider SourceProvider
        {
            get { return _sourceProvider; }
        }

        /// <summary>
        /// Gets the internal type provider used to generate typing for the definitions
        /// </summary>
        public TypeProvider TypeProvider
        {
            get { return _typeProvider; }
        }

        /// <summary>
        /// Creates a new instance of the ZScriptGenerator class using a specified string as input
        /// </summary>
        public ZRuntimeGenerator()
        {
            _typeProvider = new TypeProvider();
            _messageContainer = new MessageContainer();
            _syntaxErrorListener = new ZScriptSyntaxErrorListener(_messageContainer);
            _nativeTypeBuilder = new ClassNativeTypeBuilder("ZScript_Assembly");

            _sourceProvider = new SourceProvider();
        }

        /// <summary>
        /// Creates a new instance of the ZScriptGenerator class using a specified string as input
        /// </summary>
        /// <param name="input">The input string containing the code to parse</param>
        public ZRuntimeGenerator(string input)
            : this()
        {
            // Add a string source to the provider
            _sourceProvider.AddSource(new ZScriptStringSource(input));
        }

        /// <summary>
        /// Creates a new runtime generation context from the given scope
        /// </summary>
        /// <param name="scope">The scope to create the generation context around of</param>
        /// <returns>A new RuntimeGenerationContext created from the given scope</returns>
        private RuntimeGenerationContext CreateContext(CodeScope scope)
        {
            var context = new RuntimeGenerationContext(scope, _messageContainer, _typeProvider);
            context.ContextTypeProvider = new ExpressionTypeResolver(context);
            var definitionTypeProvider = new DefaultDefinitionTypeProvider(context);

            // Assign the context
            context.DefinitionTypeProvider = definitionTypeProvider;

            // Register the native type source for the calss native type source
            context.TypeProvider.RegisterCustomNativeTypeSource(_nativeTypeBuilder);

            return context;
        }

        /// <summary>
        /// Parses the input string
        /// </summary>
        public void ParseSources()
        {
            _messageContainer.ClearSyntaxErrors();

            // Iterate over the source providers, parsing them one by one
            foreach (var source in _sourceProvider.Sources)
            {
                if (source.ParseRequired)
                {
                    AntlrInputStream stream = new AntlrInputStream(source.GetScriptSourceString());
                    ITokenSource lexer = new ZScriptLexer(stream);
                    ITokenStream tokens = new CommonTokenStream(lexer);

                    // TODO: See how to implement the bail error strategy correctly
                    _parser = new ZScriptParser(tokens)
                    {
                        BuildParseTree = true,
                        //Interpreter = { PredictionMode = PredictionMode.Sll },
                        //ErrorHandler = new BailErrorStrategy()
                    };

                    _parser.AddErrorListener(_syntaxErrorListener);

                    try
                    {
                        source.Tree = _parser.program();
                    }
                    // Deal with parse exceptions
                    catch (ParseCanceledException)
                    {
                        _parser.ErrorHandler = new DefaultErrorStrategy();
                        _parser.Interpreter.PredictionMode = PredictionMode.Ll;

                        // Reset error listener
                        _messageContainer.ClearSyntaxErrors();

                        source.Tree = _parser.program();
                    }

                    source.ParseRequired = false;

                    // Clear the definitions for the source file
                    source.Definitions = null;
                }
            }
        }

        /// <summary>
        /// Analyzes the program that was parsed, collecting definitions and raises warnings and errors for the code
        /// </summary>
        /// <returns>A code scope containing all of the definitions for the runtime</returns>
        public CodeScope CollectDefinitions()
        {
            if (RequiresParsing())
            {
                ParseSources();
            }

            CodeScope completeScope = new CodeScope();

            // Clear code errors and warnings that happened in previous definition collections
            _messageContainer.ClearCodeErrors();
            _messageContainer.ClearWarnings();

            foreach (var source in _sourceProvider.Sources)
            {
                if (source.Definitions == null)
                {
                    // Analyze the code of the scope
                    var collector = new DefinitionsCollector(_messageContainer);
                    collector.Collect(source.Tree);

                    source.Definitions = collector;
                }

                // Merge scopes
                completeScope = MergeScopes(completeScope, source.Definitions.CollectedBaseScope);
            }

            var context = CreateContext(completeScope);

            // Expand class definitions
            ClassDefinitionExpander classExpander = new ClassDefinitionExpander(context);
            classExpander.Expand();

            _nativeTypeBuilder.CreateTypes(context);

            // Walk the source trees, now that the definitions were collected
            ParseTreeWalker walker = new ParseTreeWalker();
            foreach (var source in _sourceProvider.Sources)
            {
                DefinitionAnalyzer varAnalyzer = new DefinitionAnalyzer(context);
                walker.Walk(varAnalyzer, source.Tree);
            }

            // Analyze the return of the scopes
            var returnAnalyzer = new ReturnStatementAnalyzer();
            returnAnalyzer.CollectReturnsOnDefinitions(context);

            // Expand the definitions contained within the collector
            var staticTypeAnalyzer = new StaticTypeAnalyzer(context);
            staticTypeAnalyzer.Analyze();

            returnAnalyzer.Analyze(context);

            // Analyze function parameters, now that they are all expanded
            var parameterAnalyzer = new FunctionParametersAnalyzer(context);
            parameterAnalyzer.Analyze();

            AnalyzeCollisions(completeScope);

            // Give another pass to classes: now verifying the methods
            classExpander.PostVerification();

            return completeScope;
        }

        /// <summary>
        /// Generates a new runtime definition based on the script that was parsed.
        /// If the parsing has faild due to errors, an exception is raised
        /// </summary>
        /// <returns>A newly created ZRuntimeDefinition object that can be used to execute the parsed code</returns>
        /// <exception cref="Exception">Errors during the script generation did not allow a runtime definition to be generated</exception>
        public ZRuntimeDefinition GenerateRuntimeDefinition()
        {
            // Analyze the program
            var scope = CollectDefinitions();

            _messageContainer.PrintMessages();

            if (HasErrors)
            {
                throw new Exception("A runtime definition cannot be created: Errors detected during code parsing and analysis.");
            }

            // Create the class type converter
            var context = CreateContext(scope);
            
            var runtimeDefinition = new ZRuntimeDefinition();

            // Collect the definitions now
            runtimeDefinition.AddFunctionDefs(GenerateFunctions(context));
            runtimeDefinition.AddGlobalVariables(GenerateGlobalVariables(context));
            runtimeDefinition.AddExportFunctionDefs(GenerateExportFunctions(context));
            runtimeDefinition.AddClosurenDefs(GenerateClosures(context));
            runtimeDefinition.AddClassDefinitions(GenerateClasses(context));

            // Expand the definitions in all of the list now
            var variableExpander = new VariableTokenExpander(runtimeDefinition.GetFunctions(), scope);
            foreach (var function in runtimeDefinition.GetFunctions())
            {
                if (function.Tokens != null)
                {
                    variableExpander.ExpandInList(function.Tokens);
                }
            }
            
            return runtimeDefinition;
        }

        /// <summary>
        /// Generates a new runtime based on the script that was parsed, setting the provided runtime owner as the owner of the runtime object to create
        /// If the parsing has faild due to errors, an exception is raised
        /// </summary>
        /// <param name="owner">A runtime owner object that will own the runtime to be created</param>
        /// <returns>A newly created ZRuntime object that can be used to execute the parsed code</returns>
        /// <exception cref="Exception">Errors during the script generation did not allow a runtime definition to be generated</exception>
        public ZRuntime GenerateRuntime(IRuntimeOwner owner)
        {
            return new ZRuntime(GenerateRuntimeDefinition(), owner);
        }

        /// <summary>
        /// Returns a value speicfying whether the generator requires parsing of source scripts
        /// </summary>
        /// <returns>true if the generator requires parsing, false otherwise</returns>
        private bool RequiresParsing()
        {
            return _sourceProvider.Sources.Any(source => source.ParseRequired);
        }

        /// <summary>
        /// Returns a list of ZFunctions generated from a given code scope
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private IEnumerable<ZFunction> GenerateFunctions(RuntimeGenerationContext context)
        {
            var scope = context.BaseScope;

            var tokenizer = new FunctionBodyTokenizer(context) { DebugTokens = Debug };
            var funcDefs = scope.Definitions.OfType<FunctionDefinition>().Where(t => !(t is ExportFunctionDefinition) && !(t is ClosureDefinition) );
            var zFuncs = new List<ZFunction>();

            foreach (var funcDef in funcDefs)
            {
                var tokens = tokenizer.TokenizeBody(funcDef.BodyContext);

                funcDef.Tokens = tokens;

                var zFunction = new ZFunction(funcDef.Name, tokens, GenerateFunctionArguments(funcDef.Parameters))
                {
                    Signature = funcDef.CallableTypeDef
                };

                zFuncs.Add(zFunction);
            }

            return zFuncs;
        }

        /// <summary>
        /// Returns a list of ZClasses generated from a given code scope
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        /// <returns>An array containing ZClasses available at the top-level scope</returns>
        private IEnumerable<ZClass> GenerateClasses(RuntimeGenerationContext context)
        {
            var scope = context.BaseScope;

            var stmtTokenizer = new StatementTokenizerContext(context);
            var tokenizer = new FunctionBodyTokenizer(context) { DebugTokens = Debug };

            var classDefs = scope.Definitions.OfType<ClassDefinition>();
            List<ZClass> classes = new List<ZClass>();

            // Temporary dictionary used to help sort base methods
            var baseMethodQueue = new Dictionary<ZMethod, MethodDefinition>();
            // Dictionary of method definitions and their target transformed ZMethods. Used to setup the base calls
            var transformedMethods = new Dictionary<MethodDefinition, ZMethod>();

            foreach (var classDef in classDefs)
            {
                // Iterate over the methods
                List<ZMethod> methods = new List<ZMethod>();

                foreach (var methodDef in classDef.GetAllMethods())
                {
                    var tokens = new TokenList();
                    if (methodDef.BodyContext != null)
                    {
                        tokens = tokenizer.TokenizeBody(methodDef.BodyContext);
                        methodDef.Tokens = tokens;
                    }

                    methodDef.RecreateCallableDefinition();

                    ZMethod method = new ZMethod(methodDef.Name, tokens, GenerateFunctionArguments(methodDef.Parameters))
                    {
                        Signature = methodDef.CallableTypeDef
                    };

                    methods.Add(method);

                    transformedMethods[methodDef] = method;

                    if (methodDef.BaseMethod != null)
                        baseMethodQueue[method] = methodDef.BaseMethod;
                }

                List<ZClassField> fields = new List<ZClassField>();

                foreach (var fieldDef in classDef.GetAllFields())
                {
                    var tokens = new TokenList();
                    if (fieldDef.HasValue)
                    {
                        tokens = stmtTokenizer.TokenizeExpression(fieldDef.ValueExpression.ExpressionContext).ToTokenList();
                    }

                    var field = new ZClassField(fieldDef.Name, tokens)
                    {
                        Type = _typeProvider.NativeTypeForTypeDef(fieldDef.Type),
                        HasValue = fieldDef.HasValue
                    };

                    fields.Add(field);
                }

                var tokenList = new TokenList();

                if (classDef.PublicConstructor.BodyContext != null)
                    tokenList = tokenizer.TokenizeBody(classDef.PublicConstructor.BodyContext);

                classDef.PublicConstructor.Tokens = tokenList;

                ConstructorDefinition constructor = classDef.NonDefaultConstructor;

                tokenList = constructor.Tokens;

                // Tokenize constructor
                var zConstructor = new ZMethod(constructor.Name, tokenList, GenerateFunctionArguments(constructor.Parameters));
                transformedMethods[constructor] = zConstructor;

                if (constructor.BaseMethod != null)
                    baseMethodQueue[zConstructor] = constructor.BaseMethod;

                classes.Add(new ZClass(classDef.Name, methods.ToArray(), fields.ToArray(), zConstructor, _nativeTypeBuilder.TypeForClassType(classDef.ClassTypeDef)));
            }

            // Run over the base methods, setting the correct base methods now
            foreach (var zMethod in baseMethodQueue.Keys)
            {
                zMethod.BaseMethod = transformedMethods[baseMethodQueue[zMethod]];
            }

            return classes;
        }

        /// <summary>
        /// Returns a list of ZClosureFunction generated from a given code scope
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        /// <returns>An array containing ZClosureFunction available at the top-level scope</returns>
        private IEnumerable<ZClosureFunction> GenerateClosures(RuntimeGenerationContext context)
        {
            var scope = context.BaseScope;

            var tokenizer = new FunctionBodyTokenizer(context) { DebugTokens = Debug };
            var closureDefs = scope.Definitions.OfType<ClosureDefinition>();
            var zClosures = new List<ZClosureFunction>();

            foreach (var funcDef in closureDefs)
            {
                var tokens = tokenizer.TokenizeBody(funcDef.BodyContext);

                funcDef.Tokens = tokens;

                var zFunction = new ZClosureFunction(funcDef.Name, tokens, GenerateFunctionArguments(funcDef.Parameters))
                {
                    Signature = funcDef.CallableTypeDef
                };

                zClosures.Add(zFunction);
            }

            return zClosures;
        }

        /// <summary>
        /// Returns a list of ZFunctions generated from a given code scope
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private IEnumerable<ZExportFunction> GenerateExportFunctions(RuntimeGenerationContext context)
        {
            var scope = context.BaseScope;

            var funcDefs = scope.Definitions.OfType<ExportFunctionDefinition>();

            return funcDefs.Select(def => new ZExportFunction(def.Name, GenerateFunctionArguments(def.Parameters)));
        }

        /// <summary>
        /// Returns a list of GlobalVariables generated from a given code scope
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        /// <returns>An array containing GlobalVariables available at the top-level scope</returns>
        private IEnumerable<GlobalVariable> GenerateGlobalVariables(RuntimeGenerationContext context)
        {
            var scope = context.BaseScope;

            var tokenizer = new StatementTokenizerContext(context);
            var varDefs = scope.Definitions.OfType<GlobalVariableDefinition>();

            return
                varDefs.Select(
                    def =>
                        new GlobalVariable
                        {
                            Name = def.Name,
                            HasValue = def.HasValue,
                            DefaultValue = null,
                            Type = _typeProvider.NativeTypeForTypeDef(def.Type),
                            ExpressionTokens = def.HasValue ? new TokenList(tokenizer.TokenizeExpression(def.ValueExpression.ExpressionContext)) : new TokenList()
                        });
        }

        /// <summary>
        /// Generates an array of function arguments from a specified enumerable of function argument definitions
        /// </summary>
        /// <param name="arguments">An enumerable of function arguments read from the script</param>
        /// <returns>An array of function arguments generated from the given array of function argument definitions</returns>
        private static FunctionArgument[] GenerateFunctionArguments(IEnumerable<FunctionArgumentDefinition> arguments)
        {
            return
                arguments.Select(
                    arg => new FunctionArgument(arg.Name, arg.IsVariadic, arg.HasValue, arg.HasValue ? ConstantAtomParser.ParseCompileConstantAtom(arg.DefaultValue) : null))
                    .ToArray();
        }

        /// <summary>
        /// Merges two scopes into one containing all of the definitions of the two scopes.
        /// The method raises errors if one or more definitions collide
        /// </summary>
        /// <param name="source1">The first scope to merge</param>
        /// <param name="source2">The second scope to merge</param>
        /// <returns>A single code scope containing all of the definitions merged</returns>
        private CodeScope MergeScopes(CodeScope source1, CodeScope source2)
        {
            CodeScope newScope = new CodeScope();

            MergeScopesRecursive(source1, newScope);
            MergeScopesRecursive(source2, newScope);
            
            return newScope;
        }

        /// <summary>
        /// Analyzes collisions of definitions in a given code scope
        /// </summary>
        private void AnalyzeCollisions(CodeScope scope)
        {
            var allDefs = scope.Definitions;

            foreach (var definition in allDefs)
            {
                var defs = scope.GetDefinitionsByName(definition.Name);

                foreach (var d in defs)
                {
                    if (d == definition)
                        continue;

                    // Collisions between exported definitions are ignored
                    if (d is ExportFunctionDefinition && !(definition is ExportFunctionDefinition) ||
                        definition is ExportFunctionDefinition && !(d is ExportFunctionDefinition))
                        continue;

                    // Shadowing of global variables
                    if (d is GlobalVariableDefinition && !(definition is GlobalVariableDefinition) ||
                        !(d is GlobalVariableDefinition) && definition is GlobalVariableDefinition)
                        continue;

                    int defLine = definition.Context == null ? 0 : definition.Context.Start.Line;
                    int defColumn = definition.Context == null ? 0 : definition.Context.Start.Column;

                    int dLine = d.Context == null ? 0 : d.Context.Start.Line;
                    int dColumn = d.Context == null ? 0 : d.Context.Start.Column;

                    string message = "Duplicated definition of '" + definition.Name + "' at line " + defLine +
                                     " column " + defColumn + " collides with definition " + d + " at line " + dLine +
                                     " column " + dColumn;

                    _messageContainer.RegisterError(definition.Context, message, ErrorCode.DuplicatedDefinition);
                }
            }
        }

        /// <summary>
        /// Merges the definitions of a code scope into another code scope, recursively re-creating the scope tree on the target scope
        /// </summary>
        /// <param name="source">The source scope to copy the definitions from</param>
        /// <param name="target">The second scope to copy the definitions to</param>
        private static void MergeScopesRecursive(CodeScope source, CodeScope target)
        {
            // Re-create the scope tree on the target
            foreach (var subScope in source.ChildrenScopes)
            {
                var scope = new CodeScope { Context = subScope.Context };
                target.AddSubscope(scope);

                MergeScopesRecursive(subScope, scope);
            }

            foreach (var d in source.Definitions)
            {
                target.AddDefinition(d);
            }
            foreach (var usage in source.DefinitionUsages)
            {
                target.AddDefinitionUsage(usage);
            }
        }

        /// <summary>
        /// Default definition type provider for the runtime generator
        /// </summary>
        public class DefaultDefinitionTypeProvider : IDefinitionTypeProvider
        {
            /// <summary>
            /// A message container to report error messages and warnings to
            /// </summary>
            private readonly RuntimeGenerationContext _context;

            /// <summary>
            /// Stack of definitions, used to run through definitions in function bodies
            /// </summary>
            private readonly Stack<List<ValueHolderDefinition>> _localsStack;

            /// <summary>
            /// Initializes a new instance of the DefaultDefinitionTypeProvider class
            /// </summary>
            /// <param name="context">The runtime generation context for this default definition type provider</param>
            public DefaultDefinitionTypeProvider(RuntimeGenerationContext context)
            {
                _context = context;
                _localsStack = new Stack<List<ValueHolderDefinition>>();
            }

            /// <summary>
            /// Clears all the locals in the locals stack
            /// </summary>
            public void ClearLocalStack()
            {
                _localsStack.Clear();
            }

            /// <summary>
            /// Pushes a new definition scope
            /// </summary>
            public void PushLocalScope()
            {
                _localsStack.Push(new List<ValueHolderDefinition>());
            }

            /// <summary>
            /// Adds a given definition to the top of the definition stack
            /// </summary>
            /// <param name="definition">The definition to add to the top of the definition stack</param>
            public void AddLocal(ValueHolderDefinition definition)
            {
                _localsStack.Peek().Add(definition);
            }

            /// <summary>
            /// Pops a definition scope
            /// </summary>
            public void PopLocalScope()
            {
                _localsStack.Pop();
            }

            // 
            // IDefinitionTypeProvider.TypeForDefinition implementation
            // 
            public TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName)
            {
                foreach (var locals in _localsStack)
                {
                    var def = locals.FirstOrDefault(d => d.Name == definitionName);
                    if (def != null)
                    {
                        context.IsConstant = def.IsConstant;
                        context.Definition = def;
                        context.HasDefinition = true;

                        return def.Type;
                    }
                }

                // Search for the inner-most scope that contains the context, and search the definition from there
                var scopeForDefinition = _context.BaseScope.GetScopeContainingContext(context);

                if (scopeForDefinition != null)
                {
                    var definitions = scopeForDefinition.GetDefinitionsByName(definitionName).ToList();

                    foreach (var def in definitions)
                    {
                        // Bind the definition to the member name
                        context.Definition = def;
                        context.HasDefinition = (def != null);

                        // Local variables cannot be fetched through this mean
                        if (def is LocalVariableDefinition)
                        {
                            continue;
                        }

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

                        var objDef = def as ClassDefinition;
                        if (objDef != null)
                        {
                            // Class definitions cannot be reassigned
                            context.IsConstant = true;

                            return objDef.PublicConstructor.CallableTypeDef;
                        }
                    }
                }

                // Search in class inheritance chain
                var classDef = GetClassContainingContext(context);

                if (classDef != null)
                {
                    while (classDef != null)
                    {
                        var field = classDef.Fields.FirstOrDefault(f => f.Name == definitionName);

                        if (field != null)
                        {
                            return field.Type;
                        }

                        classDef = classDef.BaseClass;
                    }
                }

                _context.MessageContainer.RegisterError(context, "Cannot resolve definition name " + definitionName + " on type expanding phase.", ErrorCode.UndeclaredDefinition);

                return _context.TypeProvider.AnyType();
            }

            // 
            // IDefinitionTypeProvider.TypeForThis
            // 
            public TypeDef TypeForThis(ParserRuleContext context)
            {
                // Search for the inner-most scope that contains the context, and search the definition from there
                var scope = _context.BaseScope.GetScopeContainingContext(context);

                // Iterate back until we hit the scope for a class definition
                while (scope != null)
                {
                    var definitionContext = scope.Context as ZScriptParser.ClassDefinitionContext;
                    if (definitionContext != null)
                    {
                        return definitionContext.ClassDefinition.ClassTypeDef;
                    }

                    scope = scope.ParentScope;
                }

                return _context.TypeProvider.AnyType();
            }

            // 
            // IDefinitionTypeProvider.TypeForBase
            // 
            public TypeDef TypeForBase(ParserRuleContext context)
            {
                // Search for the inner-most scope that contains the context, and search the definition from there
                var scope = _context.BaseScope.GetScopeContainingContext(context);

                // Iterate back until we hit the scope for a class definition
                while (scope != null)
                {
                    var definitionContext = scope.Context as ZScriptParser.ClassMethodContext;
                    if (definitionContext != null)
                    {
                        if(definitionContext.MethodDefinition.BaseMethod != null)
                            return definitionContext.MethodDefinition.BaseMethod.CallableTypeDef;

                        break;
                    }
                    // Break on closures
                    if (scope.Context is ZScriptParser.ClosureExpressionContext)
                    {
                        break;
                    }

                    scope = scope.ParentScope;
                }

                return _context.TypeProvider.AnyType();
            }

            // 
            // IDefinitionTypeProvider.HasBaseTarget
            // 
            public bool HasBaseTarget(ParserRuleContext context)
            {
                // Search for the inner-most scope that contains the context, and search the definition from there
                var scope = _context.BaseScope.GetScopeContainingContext(context);

                // Iterate back until we hit the scope for a class definition
                while (scope != null)
                {
                    var definitionContext = scope.Context as ZScriptParser.ClassMethodContext;
                    if (definitionContext != null)
                    {
                        if (definitionContext.MethodDefinition.BaseMethod != null)
                            return true;

                        break;
                    }
                    // Quit on closures with falsehood
                    if (scope.Context is ZScriptParser.ClosureExpressionContext)
                    {
                        return false;
                    }

                    scope = scope.ParentScope;
                }

                return false;
            }

            /// <summary>
            /// Gets the inner-most class definition described by the given context.
            /// Returns null, when not in a class definition context
            /// </summary>
            /// <returns>The inner-most class definition for a given context</returns>
            private ClassDefinition GetClassContainingContext(RuleContext context)
            {
                // Traverse backwards
                while (context != null)
                {
                    var classContext = context as ZScriptParser.ClassDefinitionContext;
                    if (classContext != null)
                    {
                        // Get the class definition for the context
                        return
                            _context.BaseScope.GetScopeContainingContext(classContext)
                                .GetDefinitionByName<ClassDefinition>(classContext.className().IDENT().GetText());
                    }

                    context = context.Parent;
                }

                return null;
            }
        }

        /// <summary>
        /// Class used to generate native types from class definitions and feed them to type providers during compilation
        /// </summary>
        private class ClassNativeTypeBuilder : INativeTypeSource
        {
            /// <summary>
            /// The class type builder this class source will use to build the classes
            /// </summary>
            private readonly ClassTypeBuilder _classTypeBuilder;

            /// <summary>
            /// A dictionary mapping class type defs to their respective native types
            /// </summary>
            private readonly Dictionary<ClassTypeDef, Type> _mappedTypes;

            /// <summary>
            /// Initializes a new instance of the ClassNativeTypeSource class
            /// </summary>
            /// <param name="assemblyName">The name for the assembly to generate and create the class types on</param>
            public ClassNativeTypeBuilder(string assemblyName)
            {
                var typeBuildingContext = TypeBuildingContext.CreateBuilderContext(assemblyName);
                _classTypeBuilder = new ClassTypeBuilder(typeBuildingContext);

                _mappedTypes = new Dictionary<ClassTypeDef, Type>();
            }

            /// <summary>
            /// Creates the types on the runtime generation context provided on this class native type source
            /// </summary>
            public void CreateTypes(RuntimeGenerationContext generationContext)
            {
                ClearCache();

                var classDefinitions = generationContext.BaseScope.GetDefinitionsByType<ClassDefinition>();

                foreach (var classDefinition in classDefinitions)
                {
                    var nativeType = _classTypeBuilder.ConstructType(classDefinition);

                    _mappedTypes[classDefinition.ClassTypeDef] = nativeType;
                }
            }

            /// <summary>
            /// Clears all the class types registered on this class native type builder
            /// </summary>
            private void ClearCache()
            {
                _classTypeBuilder.ClearCache();
                _mappedTypes.Clear();
            }

            /// <summary>
            /// Returns a native type for the associated class type, or null, if none exists
            /// </summary>
            /// <param name="classType">The class type to get</param>
            /// <returns>A native type that was associated with the given calss type at creation time, or null, if none exists</returns>
            public Type TypeForClassType(ClassTypeDef classType)
            {
                Type native;
                return _mappedTypes.TryGetValue(classType, out native) ? native : null;
            }

            // 
            // INativeTypeSource.NativeTypeForTypeDef implementation
            // 
            public Type NativeTypeForTypeDef(TypeDef type, bool anyAsObject = false)
            {
                if (!(type is ClassTypeDef))
                    return null;

                return TypeForClassType((ClassTypeDef)type);
            }
        }
    }
}