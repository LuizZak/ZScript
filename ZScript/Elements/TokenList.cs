#region License information
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
#endregion

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