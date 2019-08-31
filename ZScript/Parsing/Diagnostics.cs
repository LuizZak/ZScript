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

using System.Collections.Generic;
using System.Linq;
using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// Contains diagnostics raised during lexing, parsing and compilation steps.
    /// </summary>
    public class Diagnostics : IDiagnosticsCollector
    {
        private readonly List<DiagnosticMessage> _diagnostics = new List<DiagnosticMessage>();

        /// <summary>
        /// Gets all note-level diagnostics
        /// </summary>
        public DiagnosticMessage[] Notes => DiagnosticsWithLevel(DiagnosticLevel.Note);

        /// <summary>
        /// Gets all warning-level diagnostics
        /// </summary>
        public DiagnosticMessage[] Wargnings => DiagnosticsWithLevel(DiagnosticLevel.Warning);

        /// <summary>
        /// Gets all error-level diagnostics
        /// </summary>
        public DiagnosticMessage[] Errors => DiagnosticsWithLevel(DiagnosticLevel.Error);

        /// <summary>
        /// Creates a new diagnostic message
        /// </summary>
        public DiagnosticMessage Diagnose(DiagnosticLevel level, string message, SourceLocation location)
        {
            var diagnostic = new DiagnosticMessage(message, location, level);
            _diagnostics.Add(diagnostic);

            return diagnostic;
        }

        /// <inheritdoc />
        public DiagnosticMessage Error(string message, SourceLocation location)
        {
            return Diagnose(DiagnosticLevel.Error, message, location);
        }

        /// <inheritdoc />
        public DiagnosticMessage Warning(string message, SourceLocation location)
        {
            return Diagnose(DiagnosticLevel.Warning, message, location);
        }

        /// <inheritdoc />
        public DiagnosticMessage Note(string message, SourceLocation location)
        {
            return Diagnose(DiagnosticLevel.Note, message, location);
        }
        
        /// <summary>
        /// Gets all errors of a given diagnostic level
        /// </summary>
        public DiagnosticMessage[] DiagnosticsWithLevel(DiagnosticLevel level)
        {
            return _diagnostics.Where(d => d.Level == level).ToArray();
        }
    }
}