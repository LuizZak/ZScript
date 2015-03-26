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
    /// Represents a warning that was generated during the generation of the code
    /// </summary>
    public class Warning : CodeMessage
    {
        /// <summary>
        /// Gets or sets the warning code for this warning
        /// </summary>
        public WarningCode WarningCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the Warning class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        /// <param name="warningCode">The code for the warning</param>
        public Warning(int line, int column, string message, WarningCode warningCode)
        {
            Column = column;
            Line = line;
            Message = message;
            WarningCode = warningCode;
        }

        /// <summary>
        /// Transforms this code message into a string
        /// </summary>
        /// <returns>The string representation of this code message</returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(ContextName))
            {
                return "Warning at line " + Line + " position " + Column + ": " + Message;
            }

            return "Warning at " + ContextName + " at line " + Line + " position " + Column + ": " + Message;
        }
    }

    /// <summary>
    /// Specifies the code for a warning
    /// </summary>
    public enum WarningCode
    {
        /// <summary>An undefined warning</summary>
        Undefined,
        /// <summary>A definition is created, but never user</summary>
        UnusedDefinition,
        /// <summary>A definition is created, but its value is only set and never get</summary>
        DefinitionOnlySet,

        /// <summary>Specifies a warning issued when a condition for an if statement resolves to a constant</summary>
        ConstantIfCondition,

        /// <summary>Specifies a warning issued when an expression for a switch statement resolves to a constant value that matches a constant defined in a case label</summary>
        ConstantSwitchExpression,

        /// <summary>Wraning raised whenever a redundant base() call is placed inside an inherited constructor</summary>
        RedundantBaseCall,

        /// <summary>Warning raised when the expression on the left side of a null-coalesce is a non-optional value</summary>
        NonOptionalNullCoalesceLeftSize,
    }
}