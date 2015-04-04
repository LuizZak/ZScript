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

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Definitions;
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
        private readonly CodeScope _globalScope;

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
                var def = varToken.PointingDefinition;
                if (def is FunctionDefinition)
                {
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