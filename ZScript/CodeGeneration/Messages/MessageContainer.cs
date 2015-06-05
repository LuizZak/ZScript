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
        /// Gets a list containing all of the code messages stored in this message container
        /// </summary>
        public CodeMessage[] AllMessages => _warningList.Concat(_errorList.Concat<CodeMessage>(_syntaxErrors)).ToArray();

        /// <summary>
        /// Returns the array of all the syntax errors that were found during the parsing of the script
        /// </summary>
        public SyntaxError[] SyntaxErrors => _syntaxErrors.ToArray();

        /// <summary>
        /// Gets an array of all the errors that were found
        /// </summary>
        public CodeError[] CodeErrors => _errorList.ToArray();

        /// <summary>
        /// Gets an array of all the warnings that were raised
        /// </summary>
        public Warning[] Warnings => _warningList.ToArray();

        /// <summary>
        /// Gets a value specifying whether there are any syntax errors registered on this message container
        /// </summary>
        public bool HasSyntaxErrors => _syntaxErrors.Count > 0;

        /// <summary>
        /// Gets a value specifying whether there are any code errors registered on this message container
        /// </summary>
        public bool HasCodeErrors => _errorList.Count > 0;

        /// <summary>
        /// Gets a value specifying whether there are any syntax or code error registered on this message container
        /// </summary>
        public bool HasErrors => HasSyntaxErrors || HasCodeErrors;

        /// <summary>
        /// Gets a value specifying whether there are any warnings registered on this message container
        /// </summary>
        public bool HasWarnings => _warningList.Count > 0;

        /// <summary>
        /// Gets a value specifying whether there are any type of messages registered on this message container
        /// </summary>
        public bool HasMessages => HasErrors || HasWarnings;

        /// <summary>
        /// Prints all the errors and warning messages currently stored in this MessageContainer
        /// </summary>
        public void PrintMessages()
        {
            foreach (var error in SyntaxErrors)
            {
                Console.WriteLine(error);
            }

            foreach (var error in CodeErrors)
            {
                Console.WriteLine(error);
            }

            foreach (var warning in Warnings)
            {
                Console.WriteLine(warning);
            }
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
            RegisterError(new CodeError(line, position, errorCode) { Message = errorMessage, Context = context });
        }

        /// <summary>
        /// Registers an error at a context
        /// </summary>
        public void RegisterError(ParserRuleContext context, string errorMessage, ErrorCode errorCode = ErrorCode.Undefined)
        {
            if(context == null)
            {
                RegisterError(new CodeError(0, 0, errorMessage) { ErrorCode = errorCode });
                return;
            }

            RegisterError(new CodeError(context.Start.Line, context.Start.Column, errorMessage) { ErrorCode = errorCode, Context = context });
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