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
using System;

using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Class capable of expanding TypedTokens in a token stream
    /// </summary>
    public class TypedTokenExpander
    {
        /// <summary>
        /// The runtime context associated with this typed token expander
        /// </summary>
        private readonly RuntimeGenerationContext _context;

        /// <summary>
        /// Initializes a new instance of the TypedTokenExpander class with a context associated
        /// </summary>
        /// <param name="context">The context to associate with this typed token expander</param>
        public TypedTokenExpander(RuntimeGenerationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Expands the tokens in the given token list.
        /// Throws an Exception, if any token has a type that cannot be converted
        /// </summary>
        /// <param name="tokenList">The list of tokens to expand</param>
        /// <exception cref="Exception">A type context could not be converted to an appropriate type</exception>
        public void ExpandInList(TokenList tokenList)
        {
            var tokens = tokenList.Tokens;

            for (int i = 0; i < tokens.Length; i++)
            {
                var typedToken = tokens[i] as TypedToken;
                if (typedToken == null)
                    continue;
                if(typedToken.RawType != null)
                {
                    tokens[i] = new Token(typedToken.Type, typedToken.RawType, typedToken.Instruction);
                    continue;
                }

                var typeDef = typedToken.TypeDef;

                if(typedToken.TypeContext != null)
                {
                    // Substitute the token
                    typeDef = _context.ContextTypeProvider.TypeForContext(typedToken.TypeContext);
                }

                Type type = _context.TypeProvider.NativeTypeForTypeDef(typeDef, true);
                if (type == null)
                    throw new Exception("Could not expand type token " + typedToken);

                // Replace the token
                tokens[i] = new Token(typedToken.Type, type, typedToken.Instruction);
            }
        }
    }
}