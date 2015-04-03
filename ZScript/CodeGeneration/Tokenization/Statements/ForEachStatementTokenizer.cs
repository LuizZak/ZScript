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
using System.Collections;
using System.Collections.Generic;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization.Statements
{
    /// <summary>
    /// Class capable of tokenizing FOR EACH statements
    /// </summary>
    public class ForEachStatementTokenizer : IParserContextTokenizer<ZScriptParser.ForEachStatementContext>
    {
        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Represents the last block before the end of the current for blocks
        /// </summary>
        private JumpTargetToken _forBlockEndTarget;

        /// <summary>
        /// Represents the condition portion of the loop
        /// </summary>
        private JumpTargetToken _conditionTarget;

        /// <summary>
        /// Represents the body portion of the loop
        /// </summary>
        private JumpTargetToken _bodyTarget;

        /// <summary>
        /// Initializes a new instance of the ForEachStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public ForEachStatementTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given For Each loop statement into a list of tokens
        /// </summary>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.ForEachStatementContext context)
        {
            var tokens = new IntermediaryTokenList();
            TokenizeStatement(tokens, context);
            return tokens;
        }

        /// <summary>
        /// Tokenizes a given For Each loop statement into a list of tokens
        /// </summary>
        /// <param name="targetList">The target list to add the tokens to</param>
        /// <param name="context">The context to tokenize</param>
        /// <returns>A list of tokens tokenized from the given context</returns>
        public void TokenizeStatement(IList<Token> targetList, ZScriptParser.ForEachStatementContext context)
        {
            // Get some cached member infos for the iterators
            var disposeMethod = typeof(IDisposable).GetMethod("Dispose");
            var getEnumMethod = typeof(IEnumerable).GetMethod("GetEnumerator");
            var moveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
            var currentProp = typeof(IEnumerator).GetProperty("Current");

            var jumpOverDispose = new JumpTargetToken();

            _forBlockEndTarget = new JumpTargetToken();
            _conditionTarget = new JumpTargetToken();
            _bodyTarget = new JumpTargetToken();

            _context.PushBreakTarget(_forBlockEndTarget);
            _context.PushContinueTarget(_conditionTarget);

            // Get the temporary variable for the loop
            var tempDef = _context.TemporaryDefinitionCreator.GetDefinition();

            /*
                // Loop head
                1: Evaluate <list>
                2: Store <list>.GetEnumerator() in temp loop variable $TEMP
                
                // Loop iterating
                3: [Jump 6]
                4: Assign <item> as $TEMP.Current
                
                5: { Loop body }
                
                // Loop verifying
                6: Call $TEMP.MoveNext()
                7: [JumpIfTrue 4]
                8: Call $TEMP.Dispose()
            */
            // 1: Evaluate <list>
            _context.TokenizeExpression(targetList, context.forEachHeader().expression());
            // 2: Store <list>.GetEnumerator() in temp loop variable $TEMP
            targetList.AddRange(TokenFactory.CreateFunctionCall(getEnumMethod));
            targetList.Add(TokenFactory.CreateVariableToken(tempDef.Name, false));
            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));

            // Loop iterating
            // 3: [Jump 6]
            targetList.Add(new JumpToken(_conditionTarget));

            // Body target
            targetList.Add(_bodyTarget);
            // 4: Assign <item> as $TEMP.Current
            var value = TokenFactory.CreateMemberAccess(tempDef.Name, currentProp, true);
            targetList.AddRange(TokenFactory.CreateVariableAssignment(context.forEachHeader().valueHolderDefine().valueHolderName().memberName().IDENT().GetText(), value));

            // 5: { Loop body }
            _context.TokenizeStatement(targetList, context.statement());

            // Condition jump target
            targetList.Add(_conditionTarget);
            // 6: Call $TEMP.MoveNext()
            targetList.AddRange(TokenFactory.CreateMethodCall(tempDef.Name, moveNextMethod));
            // 7: [JumpIfTrue 4]
            targetList.Add(new JumpToken(_bodyTarget, true));
            // For Block end
            targetList.Add(_forBlockEndTarget);

            // 8: Call $TEMP.Dispose()
            targetList.Add(TokenFactory.CreateVariableToken(tempDef.Name, true));
            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));
            targetList.Add(TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, typeof(IDisposable)));

            // Verify whether the object is an IDisposable instance
            targetList.Add(new JumpToken(jumpOverDispose, true, false));
            //tokens.AddRange(TokenFactory.CreateMemberAccess("Dispose", MemberAccessType.MethodAccess, true));
            targetList.AddRange(TokenFactory.CreateFunctionCall(disposeMethod));
            targetList.Add(jumpOverDispose);

            targetList.Add(TokenFactory.CreateInstructionToken(VmInstruction.ClearStack));

            // Store the temporary definition back into the temporary definition collection
            _context.TemporaryDefinitionCreator.ReleaseDefinition(tempDef);

            _context.PopContinueTarget();
            _context.PopBreakTarget();
        }
    }
}