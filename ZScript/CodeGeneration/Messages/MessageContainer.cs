using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Class used to contain code messages, such as warnings and errors
    /// </summary>
    public class MessageContainer
    {
        /// <summary>
        /// A list of all the errors raised during the current analysis
        /// </summary>
        private readonly List<CodeError> _errorList = new List<CodeError>();

        /// <summary>
        /// List of all the warnings raised during analysis
        /// </summary>
        private readonly List<Warning> _warningList = new List<Warning>();

        /// <summary>
        /// A list of all the syntax errors reported
        /// </summary>
        private readonly List<SyntaxError> _syntaxErrors = new List<SyntaxError>();

        /// <summary>
        /// Returns the array of all the syntax errors that were found during the parsing of the script
        /// </summary>
        public SyntaxError[] SyntaxErrors
        {
            get { return _syntaxErrors.ToArray(); }
        }

        /// <summary>
        /// Gets an array of all the errors that were found
        /// </summary>
        public CodeError[] CodeErrors
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
        /// Gets a value specifying whether there are any syntax errors registered on this message container
        /// </summary>
        public bool HasSyntaxErrors
        {
            get { return _syntaxErrors.Count > 0; }
        }

        /// <summary>
        /// Gets a value specifying whether there are any code errors registered on this message container
        /// </summary>
        public bool HasCodeErrors
        {
            get { return _errorList.Count > 0; }
        }

        /// <summary>
        /// Gets a value specifying whether there are any syntax or code error registered on this message container
        /// </summary>
        public bool HasErrors
        {
            get { return HasSyntaxErrors || HasCodeErrors; }
        }

        /// <summary>
        /// Registers a syntax error
        /// </summary>
        /// <param name="error">The syntax error to register</param>
        public void RegisterSyntaxError(SyntaxError error)
        {
            _syntaxErrors.Add(error);
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        public void RegisterWarning(int line, int position, string warningMessage)
        {
            _warningList.Add(new Warning(line, position, warningMessage));
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        public void RegisterWarning(int line, int position, string warningMessage, WarningCode code, ParserRuleContext context = null)
        {
            _warningList.Add(new Warning(line, position, warningMessage, code) { Context = context });
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        public void RegisterWarning(ParserRuleContext context, string warningMessage, WarningCode code)
        {
            _warningList.Add(new Warning(context.Start.Line, context.Start.Column, warningMessage, code) { Context = context });
        }

        /// <summary>
        /// Registers a warning at a context
        /// </summary>
        public void RegisterWarning(ITerminalNode context, string warningMessage)
        {
            _warningList.Add(new Warning(context.Symbol.Line, context.Symbol.Column, warningMessage));
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        public void RegisterError(int line, int position, string errorMessage)
        {
            _errorList.Add(new CodeError(line, position, errorMessage));
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        public void RegisterError(int line, int position, string errorMessage, ErrorCode errorCode, ParserRuleContext context = null)
        {
            _errorList.Add(new CodeError(line, position, errorCode) { Message = errorMessage, Context = context });
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        public void RegisterError(ITerminalNode context, string errorMessage, ErrorCode errorCode = ErrorCode.Undefined)
        {
            _errorList.Add(new CodeError(context.Symbol.Line, context.Symbol.Column, errorMessage));
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        public void RegisterError(ParserRuleContext context, string errorMessage, ErrorCode errorCode = ErrorCode.Undefined)
        {
            _errorList.Add(new CodeError(context.Start.Line, context.Start.Column, errorMessage) { ErrorCode = errorCode, Context = context });
        }

        /// <summary>
        /// Registers an error
        /// </summary>
        public void RegisterError(CodeError error)
        {
            _errorList.Add(error);
        }

        /// <summary>
        /// Clears all the warnings and errors contained in this message container
        /// </summary>
        public void Clear()
        {
            _errorList.Clear();
            _warningList.Clear();
            _syntaxErrors.Clear();
        }

        /// <summary>
        /// Clears all the syntax errors reported
        /// </summary>
        public void ClearSyntaxErrors()
        {
            _syntaxErrors.Clear();
        }

        /// <summary>
        /// Clears all the syntax errors reported
        /// </summary>
        public void ClearCodeErrors()
        {
            _errorList.Clear();
        }

        /// <summary>
        /// Clears all the warnings reported
        /// </summary>
        public void ClearWarnings()
        {
            _warningList.Clear();
        }
    }
}