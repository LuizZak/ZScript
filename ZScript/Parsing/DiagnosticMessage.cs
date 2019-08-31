﻿#region License information
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
    /// A diagnostic message from a <see cref="Diagnostics"/> container.
    /// </summary>
    public class DiagnosticMessage
    {
        /// <summary>
        /// Message for the diagnostic
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The location for this diagnostic message
        /// </summary>
        public SourceLocation Location { get; }

        /// <summary>
        /// Level for this diagnostic message
        /// </summary>
        public DiagnosticLevel Level { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        public DiagnosticMessage(string message, SourceLocation location, DiagnosticLevel level)
        {
            Message = message;
            Location = location;
            Level = level;
        }
    }
}