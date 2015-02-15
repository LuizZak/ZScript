using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Elements;
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

                    _parser = new ZScriptParser(tokens)
                    {
                        BuildParseTree = true,
                        Interpreter = { PredictionMode = PredictionMode.Sll },
                        ErrorHandler = new BailErrorStrategy()
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

            // Walk the source trees, now that the definitions were collected
            ParseTreeWalker walker = new ParseTreeWalker();
            foreach (var source in _sourceProvider.Sources)
            {
                DefinitionAnalyzer varAnalyzer = new DefinitionAnalyzer(completeScope, _messageContainer);
                walker.Walk(varAnalyzer, source.Tree);
            }

            // Analyze the return of the scopes
            var returnAnalyzer = new ReturnStatementAnalyzer();
            returnAnalyzer.AnalyzeScope(completeScope, _messageContainer);

            // Expand the definitions contained within the collector
            var typeExpander = new StaticTypeAnalyzer(_typeProvider, completeScope, _messageContainer);
            typeExpander.Expand();

            AnalyzeCollisions(completeScope);

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

            foreach (var error in _messageContainer.CodeErrors)
            {
                Console.WriteLine("Error at " + error.ContextName + " at line " + error.Line + " position " + error.Column + ": " + error.Message);
            }

            foreach (var warning in _messageContainer.Warnings)
            {
                Console.WriteLine("Warning at " + warning.ContextName + " at line " + warning.Line + " position " + warning.Column + ": " + warning.Message);
            }

            if (HasErrors)
            {
                throw new Exception("A runtime definition cannot be created: Errors detected during code parsing and analysis.");
            }

            var runtimeDefinition = new ZRuntimeDefinition();

            // Collect the definitions now
            runtimeDefinition.AddFunctionDefs(GenerateFunctions(scope));
            runtimeDefinition.AddGlobalVariables(GenerateGlobalVariables(scope));
            runtimeDefinition.AddExportFunctionDefs(GenerateExportFunctions(scope));
            runtimeDefinition.AddClosurenDefs(GenerateClosures(scope));

            // Expand the definitions in all of the list now
            var expander = new VariableTokenExpander(runtimeDefinition.GetFunctions(), scope);
            foreach (var function in runtimeDefinition.GetFunctions())
            {
                if (function.Tokens != null)
                    expander.ExpandInList(function.Tokens);
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
        /// Returns a list of ZFunctions generated from a given definitions collector
        /// </summary>
        /// <param name="scope">The code scope containing the functions to generate</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private ZFunction[] GenerateFunctions(CodeScope scope)
        {
            var tokenizer = new FunctionBodyTokenizer(scope, _messageContainer) { DebugTokens = Debug };
            var funcDefs = scope.Definitions.OfType<FunctionDefinition>().Where(t => !(t is ExportFunctionDefinition) && !(t is ClosureDefinition) );

            return
                funcDefs.Select(
                    def =>
                        new ZFunction(def.Name, tokenizer.TokenizeBody(def.BodyContext),
                            GenerateFunctionArguments(def.Arguments))
                        ).ToArray();
        }

        /// <summary>
        /// Returns a list of ZClosureFunction generated from a given definitions collector
        /// </summary>
        /// <param name="scope">The code scope containing the closures to generate</param>
        /// <returns>An array containing ZClosureFunction available at the top-level scope</returns>
        private ZClosureFunction[] GenerateClosures(CodeScope scope)
        {
            var tokenizer = new FunctionBodyTokenizer(scope, _messageContainer) { DebugTokens = Debug };
            var funcDefs = scope.Definitions.OfType<ClosureDefinition>();

            return
                funcDefs.Select(
                    def =>
                        new ZClosureFunction(def.Name, tokenizer.TokenizeBody(def.BodyContext),
                            GenerateFunctionArguments(def.Arguments))
                        ).ToArray();
        }

        /// <summary>
        /// Returns a list of ZFunctions generated from a given definitions collector
        /// </summary>
        /// <param name="scope">The code scope populated with function definitions</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private ZExportFunction[] GenerateExportFunctions(CodeScope scope)
        {
            var funcDefs = scope.Definitions.OfType<ExportFunctionDefinition>();

            return funcDefs.Select(def => new ZExportFunction(def.Name, GenerateFunctionArguments(def.Arguments))).ToArray();
        }

        /// <summary>
        /// Returns a list of GlobalVariables generated from a given definitions collector
        /// </summary>
        /// <param name="scope">The code scope populated with the global variable definitions</param>
        /// <returns>An array containing GlobalVariables available at the top-level scope</returns>
        private GlobalVariable[] GenerateGlobalVariables(CodeScope scope)
        {
            var tokenizer = new StatementTokenizerContext(scope, _messageContainer);
            var varDefs = scope.Definitions.OfType<GlobalVariableDefinition>();

            return
                varDefs.Select(
                    def =>
                        new GlobalVariable
                        {
                            Name = def.Name,
                            HasValue = def.HasValue,
                            DefaultValue = null,
                            Type = def.Type,
                            ExpressionTokens = def.HasValue ? new TokenList(tokenizer.TokenizeExpression(def.ValueExpression.ExpressionContext)) : new TokenList()
                        })
                    .ToArray();
        }

        /// <summary>
        /// Generates an array of function arguments from a specified enumerable of function argument definitions
        /// </summary>
        /// <param name="arguments">An enumerable of function arguments read from the script</param>
        /// <returns>An array of function arguments generated from the given array of function argument definitions</returns>
        private FunctionArgument[] GenerateFunctionArguments(IEnumerable<FunctionArgumentDefinition> arguments)
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

                    if (definition.Context == null)
                    {
                        _messageContainer.RegisterError(0, 0,
                            "Duplicated definition of " + definition.Name + " collides with definition " + d,
                            ErrorCode.DuplicatedDefinition, definition.Context);
                    }
                    else
                    {
                        _messageContainer.RegisterError(definition.Context,
                            "Duplicated definition of " + definition.Name + " collides with definition " + d,
                            ErrorCode.DuplicatedDefinition);
                    }
                }
            }
        }

        /// <summary>
        /// Merges the definitions of a code scope into another code scope, recursively re-creating the scope tree on the target scope
        /// </summary>
        /// <param name="source">The source scope to copy the definitions from</param>
        /// <param name="target">The second scope to copy the definitions to</param>
        private void MergeScopesRecursive(CodeScope source, CodeScope target)
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
        }
    }
}