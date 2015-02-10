using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies a set of tokens that defines a line of script
    /// </summary>
    public class TokenList
    {
        /// <summary>
        /// The list of tokens
        /// </summary>
        public Token[] Tokens;

        /// <summary>
        /// Gets a string representation of this TokenList object
        /// </summary>
        public string StringFormat
        {
            get
            {
                StringBuilder outS = new StringBuilder();

                foreach (Token token in Tokens)
                {
                    if (token.TokenObject == null)
                    {
                        outS.Append("null ");
                    }
                    else
                    {
                        if (token.Type == TokenType.String)
                        {
                            outS.Append("\"" + token.TokenObject + "\" ");
                        }
                        else
                        {
                            outS.Append(token.TokenObject + " ");
                        }
                    }
                }

                return outS.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the TokenList class
        /// </summary>
        public TokenList()
        {
            Tokens = new Token[0];
        }

        /// <summary>
        /// Initializes a new instance of the TokenList class
        /// </summary>
        /// <param name="tokens">An enumerable containing the tokens to create</param>
        public TokenList(IEnumerable<Token> tokens)
        {
            Tokens = tokens.ToArray();
        }
    }
}