using System.Collections.Generic;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
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
        public List<Token> TokenizeValueDeclaration(ZScriptParser.ValueDeclContext context)
        {
            if (context.varDecl() != null)
            {
                return TokenizeVariableDeclaration(context.varDecl());
            }
            
            return TokenizeLetDeclaration(context.letDecl());
        }

        /// <summary>
        /// Tokenizes a given variable declaration into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens that were tokenized from the given context</returns>
        public List<Token> TokenizeVariableDeclaration(ZScriptParser.VarDeclContext context)
        {
            var valueHolderDecl = context.variableDeclare().valueHolderDecl();

            var expression = context.variableDeclare().expression();
            var name = valueHolderDecl.valueHolderName().IDENT().GetText();

            if (expression != null)
            {
                List<Token> tokens = _context.TokenizeExpression(expression);
                tokens.Add(new VariableToken(name, false));
                tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));

                return tokens;
            }

            return new List<Token>();
        }

        /// <summary>
        /// Tokenizes a given constant declaration into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens that were tokenized from the given context</returns>
        public List<Token> TokenizeLetDeclaration(ZScriptParser.LetDeclContext context)
        {
            var valueHolderDecl = context.constantDeclare().valueHolderDecl();

            var expression = context.constantDeclare().expression();
            var name = valueHolderDecl.valueHolderName().IDENT().GetText();

            List<Token> tokens = _context.TokenizeExpression(expression);
            tokens.Add(new VariableToken(name, false));
            tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));

            return tokens;
        }
    }
}