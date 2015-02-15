using System.Collections.Generic;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Helper class used to aid in the tokenization process of an IF statement
    /// </summary>
    public class IfStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents the last block before the end of the current if blocks
        /// </summary>
        private JumpTargetToken _ifBlockEndTarget;

        /// <summary>
        /// A stack of jump token targets that aim at inside the IF bodies that were processed
        /// </summary>
        private readonly Stack<Token> _elseTargets = new Stack<Token>();

        /// <summary>
        /// Initializes a new instance of the IfStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public IfStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containinng</param>
        public List<Token> TokenizeStatement(ZScriptParser.IfStatementContext context)
        {
            // Create the end if block target
            _ifBlockEndTarget = new JumpTargetToken();

            var tokens = new List<Token>();

            // Read first if block of the chain
            tokens.AddRange(TokenizeIfStatement(context));

            // Stick the if block end target at the end of the list
            tokens.Add(_ifBlockEndTarget);

            return tokens;
        }

        /// <summary>
        /// Tokenizes a given IF statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the IF statement to tokenize</param>
        private List<Token> TokenizeIfStatement(ZScriptParser.IfStatementContext context)
        {
            // Read expression
            List<Token> retTokens = _context.TokenizeExpression(context.expression());

            // Add the conditional jump token that fires when the expression turns out false
            var falseJump = new JumpToken(_ifBlockEndTarget, true, false);
            retTokens.Add(falseJump);

            // Tokenize the statement body
            retTokens.AddRange(_context.TokenizeStatement(context.statement()));

            // Add jump-to-end token
            var jumpToEnd = new JumpToken(_ifBlockEndTarget);
            retTokens.Add(jumpToEnd);

            // Make a false condition jump to the next target of the IF chain, if it exists, or the end of the chain
            if (context.elseStatement() != null)
            {
                var elseTokens = TokenizeElseStatement(context.elseStatement());

                if (_elseTargets.Count > 0)
                    falseJump.TargetToken = _elseTargets.Pop();

                retTokens.AddRange(elseTokens);
            }
            else
            {
                retTokens.RemoveReference(jumpToEnd);
            }

            return retTokens;
        }

        /// <summary>
        /// Tokenizes a given ELSE statement into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the ELSE statement to tokenize</param>
        private List<Token> TokenizeElseStatement(ZScriptParser.ElseStatementContext context)
        {
            List<Token> retTokens = new List<Token>();

            var target = new JumpTargetToken();
            retTokens.Add(target);
            _elseTargets.Push(target);

            if (context.statement().ifStatement() != null)
            {
                retTokens.AddRange(TokenizeIfStatement(context.statement().ifStatement()));
                return retTokens;
            }

            retTokens.AddRange(_context.TokenizeStatement(context.statement()));

            return retTokens;
        }
    }
}