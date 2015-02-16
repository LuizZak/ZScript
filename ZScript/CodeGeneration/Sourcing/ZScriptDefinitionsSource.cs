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

namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Abstract class that can be used as a source of ZScript definitions
    /// </summary>
    public abstract class ZScriptDefinitionsSource
    {
        /// <summary>
        /// Gets or sets the definitions contained within this ZScript source
        /// </summary>
        public DefinitionsCollector Definitions { get; set; }

        /// <summary>
        /// Gets or sets a pre-parsed context for the contents of this file
        /// </summary>
        public ZScriptParser.ProgramContext Tree { get; set; }

        /// <summary>
        /// Gets or sets a value whether this script source has changed since the last definition collection and requires a re-parsing
        /// </summary>
        public virtual bool ParseRequired { get; set; }

        /// <summary>
        /// Gets the string containing the script source to parse
        /// </summary>
        /// <returns>The string containing the script source to parse</returns>
        public abstract string GetScriptSourceString();
    }
}