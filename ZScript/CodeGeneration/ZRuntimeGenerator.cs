﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Parsing;
using ZScript.Runtime;

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
        /// The input string to parse
        /// </summary>
        private string _input;

        /// <summary>
        /// The error listener for the parsing of the script
        /// </summary>
        private ZScriptErrorListener _errorListener;

        /// <summary>
        /// The parse tree that was generated by the parser
        /// </summary>
        private ZScriptParser.ProgramContext _tree;

        /// <summary>
        /// The parser for the currently parsed script
        /// </summary>
        private ZScriptParser _parser;

        /// <summary>
        /// The list of code errors raised during the analysis of the program
        /// </summary>
        private List<CodeError> _codeErrors;

        /// <summary>
        /// The list of warnings raised during the analysis of the program
        /// </summary>
        private List<Warning> _warnings; 

        /// <summary>
        /// Gets or sets the script string input
        /// </summary>
        public string Input
        {
            get { return _input; }
            set { _input = value; }
        }

        /// <summary>
        /// Returns the array of all the syntax errors that were found during the parsing of the script
        /// </summary>
        public SyntaxError[] SyntaxErrors
        {
            get { return _errorListener.SyntaxErrors.ToArray(); }
        }

        /// <summary>
        /// Gets a value specifying whether there were any syntax errors on the script generation process
        /// </summary>
        public bool HasSyntaxErrors
        {
            get { return _errorListener != null && _errorListener.SyntaxErrors.Count > 0; }
        }

        /// <summary>
        /// Gets a value specifying whether there were any code errors on the runtime generation process
        /// </summary>
        public bool HasCodeErrors
        {
            get { return _codeErrors.Count > 0; }
        }

        /// <summary>
        /// Gets a value specifying whether there are any syntax or code error pending on this runtime generator
        /// </summary>
        public bool HasErrors
        {
            get { return HasSyntaxErrors || HasCodeErrors; }
        }

        /// <summary>
        /// Creates a new instance of the ZScriptGenerator class using a specified string as input
        /// </summary>
        /// <param name="input">The input string containing the code to parse</param>
        public ZRuntimeGenerator(string input)
        {
            _codeErrors = new List<CodeError>();
            _input = input;
        }

        /// <summary>
        /// Parses the input string
        /// </summary>
        public void ParseInputString()
        {
            _errorListener = new ZScriptErrorListener();

            AntlrInputStream stream = new AntlrInputStream(_input);
            ITokenSource lexer = new ZScriptLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            
            _parser = new ZScriptParser(tokens)
            {
                BuildParseTree = true,
                //Interpreter = {PredictionMode = PredictionMode.Sll}
            };

            // Reset error listener
            _errorListener.SyntaxErrors.Clear();
            _parser.AddErrorListener(_errorListener);

            try
            {
                _tree = _parser.program();
            }
            // Deal with parse exceptions
            catch (ParseCanceledException)
            {
                _parser.Interpreter.PredictionMode = PredictionMode.Ll;
                
                // Reset error listener
                _errorListener.SyntaxErrors.Clear();

                _tree = _parser.program();
            }
        }

        /// <summary>
        /// Analyzes the program that was parsed, collecting definitions and raises warnings and errors for the code
        /// </summary>
        public DefinitionsCollector CollectDefinitions()
        {
            if (_tree == null)
            {
                throw new Exception("The ParseInputString method must be called before any generation can be performed");
            }

            _codeErrors = new List<CodeError>();
            _warnings = new List<Warning>();

            // Analyze the scopes
            var scopeAnalyzer = new ScopeAnalyzer();
            scopeAnalyzer.AnalyzeProgram(_tree);

            _codeErrors.AddRange(scopeAnalyzer.Errors);
            _warnings.AddRange(scopeAnalyzer.Warnings);

            foreach (var error in _codeErrors)
            {
                Console.WriteLine("Error at " + error.ContextName + " at line " + error.Line + " position " + error.Position + ": " + error.Message);
            }

            foreach (var warning in _warnings)
            {
                Console.WriteLine("Warning at " + warning.ContextName + " at line " + warning.Line + " position " + warning.Position + ": " + warning.Message);
            }

            return scopeAnalyzer.DefinitionsCollector;
        }

        /// <summary>
        /// Generates a new runtime definition based on the script that was parsed
        /// </summary>
        /// <returns>A newly created ZRuntimeDefinition object that can be used to execute the parsed code</returns>
        public ZRuntimeDefinition GenerateRuntimeDefinition()
        {
            if (_tree == null)
            {
                throw new Exception("The ParseInputString method must be called before any generation can be performed.");
            }

            // Analyze the program
            var collector = CollectDefinitions();

            if (HasErrors)
            {
                throw new Exception("A runtime definition cannot be created: Errors detected during code parsing and analysis.");
            }

            var runtimeDefinition = new ZRuntimeDefinition();

            // Collect the definitions now
            runtimeDefinition.AddFunctionDefs(GenerateFunctions(collector));
            runtimeDefinition.AddGlobalVariables(GenerateGlobalVariables(collector));
            runtimeDefinition.AddExportFunctionDefs(GenerateExportFunctions(collector));
            runtimeDefinition.AddClosurenDefs(GenerateClosures(collector));

            return runtimeDefinition;
        }

        /// <summary>
        /// Generates a new runtime based on the script that was parsed, setting the provided runtime owner as the owner of the runtime object to create
        /// </summary>
        /// <param name="owner">A runtime owner object that will own the runtime to be created</param>
        /// <returns>A newly created ZRuntime object that can be used to execute the parsed code</returns>
        public ZRuntime GenerateRuntime(IRuntimeOwner owner)
        {
            return new ZRuntime(GenerateRuntimeDefinition(), owner);
        }

        /// <summary>
        /// Returns a list of ZFunctions generated from a given definitions collector
        /// </summary>
        /// <param name="definitions">A valid definitions populated with function definitions</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private ZFunction[] GenerateFunctions(DefinitionsCollector definitions)
        {
            var tokenizer = new FunctionBodyTokenizer(definitions) { DebugTokens = Debug };
            var funcDefs = definitions.CollectedBaseScope.Definitions.OfType<FunctionDefinition>().Where(t => !(t is ExportFunctionDefinition) && !(t is ClosureDefinition) );

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
        /// <param name="definitions">A valid definitions populated with closure definitions</param>
        /// <returns>An array containing ZClosureFunction available at the top-level scope</returns>
        private ZClosureFunction[] GenerateClosures(DefinitionsCollector definitions)
        {
            var tokenizer = new FunctionBodyTokenizer(definitions) { DebugTokens = Debug };
            var funcDefs = definitions.CollectedBaseScope.Definitions.OfType<ClosureDefinition>();

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
        /// <param name="definitions">A valid definitions populated with function definitions</param>
        /// <returns>An array containing ZFunctions available at the top-level scope</returns>
        private ZExportFunction[] GenerateExportFunctions(DefinitionsCollector definitions)
        {
            var tokenizer = new FunctionBodyTokenizer(definitions) { DebugTokens = Debug };
            var funcDefs = definitions.CollectedBaseScope.Definitions.OfType<ExportFunctionDefinition>();

            return funcDefs.Select(def => new ZExportFunction(def.Name, GenerateFunctionArguments(def.Arguments))).ToArray();
        }

        /// <summary>
        /// Returns a list of GlobalVariables generated from a given definitions collector
        /// </summary>
        /// <param name="definitions">A valid definitions populated with function definitions</param>
        /// <returns>An array containing GlobalVariables available at the top-level scope</returns>
        private GlobalVariable[] GenerateGlobalVariables(DefinitionsCollector definitions)
        {
            var tokenizer = new StatementTokenizerContext(definitions);
            var varDefs = definitions.CollectedBaseScope.Definitions.OfType<GlobalVariableDefinition>();

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
        /// Error listener for the parsing
        /// </summary>
        private class ZScriptErrorListener : BaseErrorListener
        {
            /// <summary>
            /// A list of all the syntax errors reported
            /// </summary>
            public readonly List<SyntaxError> SyntaxErrors = new List<SyntaxError>();

            // 
            // SintaxError listener
            // 
            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                SyntaxErrors.Add(new SyntaxError(line, charPositionInLine, msg));
            }
        }
    }
}