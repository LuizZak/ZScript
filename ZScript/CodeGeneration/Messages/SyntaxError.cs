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

using Antlr4.Runtime;

namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents an structure that encapsulates syntax errors that were reported during the parsing of the script
    /// </summary>
    public class SyntaxError : CodeMessage
    {
        /// <summary>
        /// Initializes a new instance of the SyntaxError class
        /// </summary>
        /// <param name="token">The token in which the syntax error occurred</param>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public SyntaxError(IToken token, int line, int column, string message)
        {
            Token = token;
            Column = column;
            Line = line;
            Message = message;
        }

        /// <summary>
        /// Transforms this code message into a string
        /// </summary>
        /// <returns>The string representation of this code message</returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(ContextName))
            {
                return "Sytax error at line " + Line + " position " + Column + ": " + Message;
            }

            return "Syntax error at " + ContextName + " at line " + Line + " position " + Column + ": " + Message;
        }
    }
}