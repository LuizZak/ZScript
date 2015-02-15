using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing return statements
    /// </summary>
    public class ReturnStatementTokenizer
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
        /// Tokenizes a given For loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediateTokenList TokenizeStatement(ZScriptParser.ReturnStatementContext context)
        {
            IntermediateTokenList tokens = new IntermediateTokenList();

            if(context.expression() != null)
                tokens.AddRange(_context.TokenizeExpression(context.value));

            tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Ret));

            return tokens;
        }
    }
}