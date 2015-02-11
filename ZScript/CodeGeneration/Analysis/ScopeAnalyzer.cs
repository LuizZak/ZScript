using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Performs analysis in scopes to guarantee that variables are scoped correctly
    /// </summary>
    public class ScopeAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// A list of all the errors raised during the current analysis
        /// </summary>
        private List<CodeError> _errorList = new List<CodeError>();

        /// <summary>
        /// List of all the warnings raised during analysis
        /// </summary>
        private List<Warning> _warningList = new List<Warning>(); 

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private DefinitionsCollector _definitionsCollector;

        /// <summary>
        /// Gets the base scope for the definitions that were collected by the scope analyzer
        /// </summary>
        public DefinitionsCollector DefinitionsCollector
        {
            get { return _definitionsCollector; }
        }

        /// <summary>
        /// Gets an array of all the errors that were found
        /// </summary>
        public CodeError[] Errors
        {
            get { return _errorList.ToArray(); }
        }

        /// <summary>
        /// Gets an array of all the warnings that were raised
        /// </summary>
        public Warning[] Warnings
        {
            get { return _warningList.ToArray(); }
        }

        /// <summary>
        /// Analyzes a given program context for undeclared variable errors
        /// </summary>
        /// <param name="context">The context of the program to analyze</param>
        public void AnalyzeProgram(ZScriptParser.ProgramContext context)
        {
            _errorList = new List<CodeError>();
            _warningList = new List<Warning>();

            _definitionsCollector = new DefinitionsCollector();
            ParseTreeWalker walker = new ParseTreeWalker();

            // Walk twice - the first pass collects definitions, the second pass analyzes bodies
            //_analysisMode = AnalysisMode.DefinitionCollection;
            DefinitionsCollector collector = new DefinitionsCollector();
            walker.Walk(collector, context);

            _errorList.AddRange(collector.CollectedErrors);
            _warningList.AddRange(collector.Warnings);

            _definitionsCollector = collector;

            VariableUsageAnalyzer varAnalyzer = new VariableUsageAnalyzer(_definitionsCollector.CollectedBaseScope);
            walker.Walk(varAnalyzer, context);

            _errorList.AddRange(varAnalyzer.CollectedErrors);
        }
    }
}