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
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing Continue statements
    /// </summary>
    public class ContinueStatementTokenizer : IParserContextTokenizer<ZScriptParser.ContinueStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the BreakStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public ContinueStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given Continue statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.ContinueStatementContext context)
        {
            var tokenList = new IntermediaryTokenList();
            TokenizeStatement(tokenList, context);
            return tokenList;
        }

        /// <summary>
        /// Tokenizes a given Continue statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to add the tokens to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.ContinueStatementContext context)
        {
            if (_context.CurrentContinueTarget != null)
            {
                targetList.Add(new JumpToken(_context.CurrentContinueTarget));
            }
        }
    }
}