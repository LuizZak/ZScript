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

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization
{
    /// <summary>
    /// Class capable of tokenizing a function body from an AST tree into a TokenList
    /// </summary>
    public class FunctionBodyTokenizer
    {
        /// <summary>
        /// Whether to print the tokens in the console
        /// </summary>
        public bool DebugTokens;

        /// <summary>
        /// The generation context for this statement tokenizer
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// Initializes a new instance of the FunctionBodyTokenizer class
        /// </summary>
        /// <param name="context">The context for the runtime generation</param>
        public FunctionBodyTokenizer(RuntimeGenerationContext context)
        {
            _generationContext = context;
        }

        /// <summary>
        /// Tokenizes the contents of the given function body context, coming from a parse tree
        /// </summary>
        /// <param name="context">The function body to tokenize</param>
        /// <returns>A token list for the givne function body</returns>
        public TokenList TokenizeBody(ZScriptParser.FunctionBodyContext context)
        {
            var state = context.blockStatement();

            var stc = new StatementTokenizerContext(_generationContext);
            var tokens = new IntermediaryTokenList();
            stc.TokenizeBlockStatement(tokens, state);

            if (DebugTokens)
            {
                Console.WriteLine("Final token list, before expanding variables and jumps:");
                TokenUtils.PrintTokens(tokens);
            }

            JumpTokenOptimizer.OptimizeJumps(tokens, VmInstruction.Interrupt);

            var finalList = tokens.ToTokenList();

            var typeExpander = new TypedTokenExpander(_generationContext);
            typeExpander.ExpandInList(finalList);

            if (DebugTokens)
            {
                Console.WriteLine("Final token list:");
                TokenUtils.PrintTokens(finalList);
            }

            return finalList;
        }
    }
}