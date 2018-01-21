﻿#region License information
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
using JetBrains.Annotations;
using ZScript.Elements;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing VAR and LET statements
    /// </summary>
    public class VariableDeclarationStatementTokenizer : IParserContextTokenizer<ZScriptParser.ValueHolderDeclContext>
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
        public IntermediaryTokenList TokenizeStatement([NotNull] ZScriptParser.ValueHolderDeclContext context)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given value declaration into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to add the tokens to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens that were tokenized from the given context</returns>
        public void TokenizeStatement(IList<Token> targetList, [NotNull] ZScriptParser.ValueHolderDeclContext context)
        {
            var expression = context.expression();
            var name = context.valueHolderDefine().valueHolderName().memberName().IDENT().GetText();

            if (expression == null) return;

            if (context.Definition != null && context.Definition.IsConstant && expression.IsConstant && expression.IsConstantPrimitive)
            {
                return;
            }

            _context.TokenizeExpression(targetList, expression);

            targetList.Add(new VariableToken(name, false) {GlobalDefinition = false, PointingDefinition = context.Definition });
            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));
        }
    }
}