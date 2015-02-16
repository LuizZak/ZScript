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

using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization.Helpers;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing break statements
    /// </summary>
    public class BreakStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the BreakStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public BreakStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given Break statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediateTokenList TokenizeStatement(ZScriptParser.BreakStatementContext context)
        {
            if (_context.CurrentBreakTarget == null)
            {
                _context.MessageContainer.RegisterError(context, "Break statement has no target", ErrorCode.NoTargetForBreakStatement);
                return new IntermediateTokenList();
            }

            return new IntermediateTokenList { new JumpToken(_context.CurrentBreakTarget) };
        }
    }
}