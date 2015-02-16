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
namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Represents a ZScript source that comes from a provided string
    /// </summary>
    public class ZScriptStringSource : ZScriptDefinitionsSource
    {
        /// <summary>
        /// The script string
        /// </summary>
        private string _script;

        /// <summary>
        /// Gets or sets the script string
        /// </summary>
        public string Script
        {
            get { return _script; }
            set
            {
                _script = value;
                ParseRequired = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ZScriptString class
        /// </summary>
        /// <param name="script">The string containing the script to create</param>
        public ZScriptStringSource(string script)
        {
            Script = script;
            ParseRequired = true;
        }

        /// <summary>
        /// Gets the script source for this ZScriptString
        /// </summary>
        /// <returns>The script source for this ZScriptString</returns>
        public override string GetScriptSourceString()
        {
            return Script;
        }
    }
}