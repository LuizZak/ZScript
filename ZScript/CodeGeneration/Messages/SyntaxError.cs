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
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public SyntaxError(int line, int column, string message)
        {
            Column = column;
            Line = line;
            Message = message;
        }
    }
}