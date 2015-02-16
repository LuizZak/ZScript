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
using ZScript.CodeGeneration.Elements;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Specifies a token that points to a local/global variable in the memory
    /// </summary>
    public class VariableToken : Token
    {
        /// <summary>
        /// The name of the variable this token is referring to
        /// </summary>
        public string VariableName;

        /// <summary>
        /// Whether this variable token is pointing at a global (variable or function) definition
        /// </summary>
        public bool GlobalDefinition;

        /// <summary>
        /// The definition this variable token is pointing at
        /// </summary>
        public Definition PointingDefinition;

        /// <summary>
        /// Whether the variable is currently being get
        /// </summary>
        public bool IsGet;

        /// <summary>
        /// Initializes a new instance of the VariableToken class
        /// </summary>
        /// <param name="variableName">The name of the variable this token is referring to</param>
        /// <param name="isGet">Whether the variable is currently being get</param>
        public VariableToken(string variableName, bool isGet = true) : base(TokenType.MemberName, variableName)
        {
            VariableName = variableName;
            IsGet = isGet;
        }
    }
}