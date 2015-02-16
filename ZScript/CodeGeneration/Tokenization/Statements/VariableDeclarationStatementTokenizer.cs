using ZScript.Elements;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing VAR and LET statements
    /// </summary>
    public class VariableDeclarationStatementTokenizer
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Initializes a new instance of the VariableDeclarationStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public VariableDeclarationStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given value declaration into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens that were tokenized from the given context</returns>
        public IntermediateTokenList TokenizeValueHolderDeclaration(ZScriptParser.ValueHolderDeclContext context)
        {
            var expression = context.expression();
            var name = context.valueHolderName().memberName().IDENT().GetText();

            if (expression != null)
            {
                IntermediateTokenList tokens = _context.TokenizeExpression(expression);
                tokens.Add(new VariableToken(name, false) { GlobalDefinition = false });
                tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));

                return tokens;
            }

            return new IntermediateTokenList();
        }
    }
}