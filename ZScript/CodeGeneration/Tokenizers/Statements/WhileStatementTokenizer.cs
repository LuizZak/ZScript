using System.Collections.Generic;

using ZScript.CodeGeneration.Tokenizers.Helpers;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenizers.Statements
{
    /// <summary>
    /// Class capable of tokenizing While statements
    /// </summary>
    public class WhileStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents a jump target to outside the while loop
        /// </summary>
        private JumpTargetToken _blockEnd;

        /// <summary>
        /// Represents the condition portion of the loop
        /// </summary>
        private JumpTargetToken _conditionTarget;

        /// <summary>
        /// Initializes a new instance of the WhileStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public WhileStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given While loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public List<Token> TokenizeStatement(ZScriptParser.WhileStatementContext context)
        {
            // WHILE loop tokenization:
            // 1 - Condition expression
            // 2 - Conditional jump to End
            // 3 - Body loop
            // 4 - Unconditional jump to Condition
            // 5 - End

            // Create the jump targets
            _conditionTarget = new JumpTargetToken();
            _blockEnd = new JumpTargetToken();

            _context.PushContinueTarget(_conditionTarget);
            _context.PushBreakTarget(_blockEnd);

            List<Token> tokens = new List<Token>();

            // 1 - Condition expression
            tokens.Add(_conditionTarget);

            var cond = context.expression();
            tokens.AddRange(_context.TokenizeExpression(cond));

            // 2 - Conditional jump to End
            tokens.Add(new JumpToken(_blockEnd, true));

            // 3 - Body loop
            tokens.AddRange(_context.TokenizeStatement(context.statement()));

            // 4 - Unconditional jump to Condition
            tokens.Add(new JumpToken(_conditionTarget));

            // 5 - End
            tokens.Add(_blockEnd);

            // Pop the targets
            _context.PopContinueTarget();
            _context.PopBreakTarget();

            return tokens;
        }
    }
}