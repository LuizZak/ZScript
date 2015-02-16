using ZScript.CodeGeneration.Tokenization.Helpers;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of the tokenizing FOR statements
    /// </summary>
    public class ForStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents the last block before the end of the current for blocks
        /// </summary>
        private JumpTargetToken _forBlockEndTarget;

        /// <summary>
        /// Represents the condition portion of the loop
        /// </summary>
        private JumpTargetToken _conditionTarget;

        /// <summary>
        /// Represents the increment portion of the loop
        /// </summary>
        private JumpTargetToken _incrementTarget;

        /// <summary>
        /// Initializes a new instance of the ForStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public ForStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given For loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediateTokenList TokenizeStatement(ZScriptParser.ForStatementContext context)
        {
            // FOR loop tokenization:
            // 1 - Loop start expression
            // 2 - Unconditional jump to Condition
            // 3 - Increment expression
            // 4 - Condition expression
            // 5 - Conditional jump to End
            // 6 - Body loop
            // 7 - Unconditional jump to Increment
            // 8 - End

            // Create the jump targets
            _conditionTarget = new JumpTargetToken();
            _incrementTarget = new JumpTargetToken();
            _forBlockEndTarget = new JumpTargetToken();

            _context.PushContinueTarget(_incrementTarget);
            _context.PushBreakTarget(_forBlockEndTarget);

            IntermediateTokenList tokens = new IntermediateTokenList();

            // 1 - Loop start expression
            var init = context.forInit();
            if(init != null)
                TokenizeForLoopInit(init, tokens);

            // 2 - Unconditional jump to Condition
            tokens.Add(new JumpToken(_conditionTarget));

            // 3 - Increment expression
            tokens.Add(_incrementTarget);

            var incr = context.forIncrement();
            if(incr != null)
                tokens.AddRange(_context.TokenizeExpression(incr.expression()));

            // 4 - Condition expression
            tokens.Add(_conditionTarget);

            var cond = context.forCondition();
            if (cond != null)
            {
                tokens.AddRange(_context.TokenizeExpression(cond.expression()));

                // 5 - Conditional jump to End
                tokens.Add(new JumpToken(_forBlockEndTarget, true, false));
            }

            // 6 - Body loop
            tokens.AddRange(_context.TokenizeStatement(context.statement()));

            // 7 - Unconditional jump to Increment
            tokens.Add(new JumpToken(_incrementTarget));

            // 8 - End
            tokens.Add(_forBlockEndTarget);

            // Pop the targets
            _context.PopContinueTarget();
            _context.PopBreakTarget();

            return tokens;
        }

        /// <summary>
        /// Tokenizes a given for loop init into a given list of tokens
        /// </summary>
        /// <param name="init">The FOR loop init to tokenize</param>
        /// <param name="tokens">The list of tokens to tokenize to</param>
        private void TokenizeForLoopInit(ZScriptParser.ForInitContext init, IntermediateTokenList tokens)
        {
            if (init.valueHolderDecl() != null)
            {
                tokens.AddRange(_context.TokenizeValueDeclaration(init.valueHolderDecl()));
            }
            else if (init.expression() != null)
            {
                tokens.AddRange(_context.TokenizeExpression(init.expression()));
            }
            else
            {
                tokens.AddRange(_context.TokenizeAssignmentExpression(init.assignmentExpression()));
            }
        }
    }
}