using System;
using System.Collections.Generic;

using ZScript.CodeGeneration.Tokenizers.Helpers;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenizers.Statements
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
        /// Tokenizes a given For loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public List<Token> TokenizeStatement(ZScriptParser.BreakStatementContext context)
        {
            if (_context.CurrentBreakTarget == null)
            {
                throw new Exception("Break statement has no target");
            }

            return new List<Token> { new JumpToken(_context.CurrentBreakTarget) };
        }
    }
}