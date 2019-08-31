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

using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// Interface for objects that collect diagnostic messages
    /// </summary>
    public interface IDiagnosticsCollector
    {
        /// <summary>
        /// Creates a new diagnostic message
        /// </summary>
        DiagnosticMessage Diagnose(DiagnosticLevel level, string message, SourceLocation location);

        /// <summary>
        /// Creates a new error diagnostic message
        /// </summary>
        DiagnosticMessage Error(string message, SourceLocation location);

        /// <summary>
        /// Creates a new warning diagnostic message
        /// </summary>
        DiagnosticMessage Warning(string message, SourceLocation location);

        /// <summary>
        /// Creates a new note diagnostic message
        /// </summary>
        DiagnosticMessage Note(string message, SourceLocation location);
    }
    
    /// <summary>
    /// Specifies a level for a diagnostic message
    /// </summary>
    public enum DiagnosticLevel
    {
        /// <summary>
        /// A note, usually accompanies a warning or error message.
        /// </summary>
        Note,
        
        /// <summary>
        /// A warning raises attention to a potential programmer error that is not
        /// a compilation error.
        /// </summary>
        Warning,

        /// <summary>
        /// Errors that stop the program from being compiled correctly.
        /// </summary>
        Error
    }
}