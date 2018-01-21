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
using JetBrains.Annotations;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing return statements
    /// </summary>
    public class ReturnStatementTokenizer : IParserContextTokenizer<ZScriptParser.ReturnStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the ReturnStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public ReturnStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given Return statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeStatement([NotNull] ZScriptParser.ReturnStatementContext context)
        {
            IntermediaryTokenList tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given Return statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The taret list to tokenize to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public void TokenizeStatement([NotNull] IList<Token> targetList, [NotNull] ZScriptParser.ReturnStatementContext context)
        {
            if (context.expression() != null)
                _context.TokenizeExpression(targetList, context.value);

            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.Ret));
        }
    }
}