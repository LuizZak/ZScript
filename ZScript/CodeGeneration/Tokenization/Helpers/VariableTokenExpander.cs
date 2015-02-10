using System.Collections.Generic;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Helper token expansion class that expands variables into faster, direct memory access
    /// </summary>
    public class VariableTokenExpander
    {
        /// <summary>
        /// Expands the jump tokens associated with the given token list
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        public static void ExpandInList(List<Token> tokens)
        {
            /*
            for (int i = 0; i < tokens.Count; i++)
            {
                VariableToken varToken = tokens[i] as VariableToken;
                if (varToken == null)
                    continue;

                // Conver the token into a hashed string
                var hash = varToken.VariableName.GetHashCode();
                Token addressToken = new Token(TokenType.Value, hash);
                Token getInstToken = TokenFactory.CreateInstructionToken(VmInstruction.GetAtAddress);

                tokens[i] = addressToken;

                if(varToken.IsGet)
                    tokens.Insert(i + 1, getInstToken);
            }
            */
        }
    }
}