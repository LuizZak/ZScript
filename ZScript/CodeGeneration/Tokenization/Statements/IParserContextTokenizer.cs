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
using Antlr4.Runtime;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Interface to be implemented by classes capalbe of tokenizing parser contexts
    /// </summary>
    public interface IParserContextTokenizer<in T> where T : ParserRuleContext
    {
        /// <summary>
        /// Tokenizes a given context into a given list of tokens
        /// </summary>
        /// <param name="targetList">The target token list to add the generated tokens to</param>
        /// <param name="context">The context containinng</param>
        void TokenizeStatement(IList<Token> targetList, T context);

        /// <summary>
        /// Tokenizes a given context into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        IntermediaryTokenList TokenizeStatement(T context);
    }
}