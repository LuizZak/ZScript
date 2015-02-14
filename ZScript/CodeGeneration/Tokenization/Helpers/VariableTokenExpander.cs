﻿using System.Collections.Generic;
using System.Linq;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Elements;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Helper token expansion class that expands variables into faster, direct memory access
    /// </summary>
    public class VariableTokenExpander
    {
        /// <summary>
        /// The array of functions to use when expanding the addresses in the tokens 
        /// </summary>
        private readonly ZFunction[] _functions;
        
        /// <summary>
        /// The global scope for the definitions to find
        /// </summary>
        private CodeScope _globalScope;

        /// <summary>
        /// Initializes a new instance of the VariableTokenExpander class
        /// </summary>
        /// <param name="functions">The array of functions to use when expanding the addresses in the tokens</param>
        /// <param name="globalScope">The global scope for the definitions to find</param>
        public VariableTokenExpander(ZFunction[] functions, CodeScope globalScope)
        {
            _functions = functions;
            _globalScope = globalScope;
        }

        /// <summary>
        /// Expands the jump tokens associated with the given token list
        /// </summary>
        /// <param name="tokens">The list of tokens to expand the jumps on</param>
        public void ExpandInList(TokenList tokens)
        {
            // TODO: Re-visit this portion of the code so we can manipulate pre-processed token lists, instead of post-processed TokenList objects

            for (int i = 0; i < tokens.Tokens.Length; i++)
            {
                VariableToken varToken = tokens.Tokens[i] as VariableToken;
                if (varToken == null)
                    continue;

                // Find the definition the token is pointing at
                var def = _globalScope.GetDefinitionByName(varToken.VariableName);

                if (def is FunctionDefinition)
                {
                    int index = 0;
                    for (int j = 0; j < _functions.Length; j++)
                    {
                        if (_functions[j].Name == varToken.VariableName)
                        {
                            tokens.Tokens[i] = new Token(TokenType.GlobalFunction, j);
                            break;
                        }
                    }
                }
            }

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