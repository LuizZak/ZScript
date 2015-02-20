﻿using System;
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
                    return;

                // Substitute the token
                var type = _context.TypeProvider.NativeTypeForTypeDef(_context.ContextTypeProvider.TypeForContext(typedToken.TypeContext));

                if(type == null)
                    throw new Exception("Could not expand type context " + typedToken.TypeContext.GetText());

                // Replace the token
                tokens[i] = new Token(typedToken.Type, type, typedToken.Instruction);
            }
        }
    }
}