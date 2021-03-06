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

using System;
using System.Collections.Generic;
using System.Linq;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Parsing;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Tokenization
{
    /// <summary>
    /// Class capable of tokenizing expressions and assignment expressions into lists of individual tokens
    /// </summary>
    public class PostfixExpressionTokenizer : ZScriptBaseListener
    {
        /// <summary>
        /// The list of tokens that were generated in this statement run
        /// </summary>
        private IntermediaryTokenList _tokens = new IntermediaryTokenList();

        /// <summary>
        /// Stack of short circuit targets, used during processing of short-circuits in the VisitExpressionOperator method
        /// </summary>
        readonly Stack<JumpToken> _shortCircuitJumps = new Stack<JumpToken>();

        /// <summary>
        /// The context used to tokenize the statements, in case a different statement appears
        /// </summary>
        private readonly StatementTokenizerContext _context;

        /// <summary>
        /// Whether the next visit member call is a root call, meaning it will point to a variable
        /// </summary>
        private bool _isRootMember = true;

        /// <summary>
        /// Whether the next visit member call is a get access
        /// </summary>
        private bool _isGetAccess = true;

        /// <summary>
        /// Gets the type provider associated with this PostfixExpressionTokenzer
        /// </summary>
        private TypeProvider TypeProvider
        {
            get { return _context.GenerationContext.TypeProvider; }
        }

        /// <summary>
        /// Initializes a new instance of the IfStatementTokenizer class
        /// </summary>
        /// <param name="context">The context used during tokenization</param>
        public PostfixExpressionTokenizer(StatementTokenizerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tokenizes a given statament into a list of tokens
        /// </summary>
        /// <param name="context">The statement to tokenize</param>
        /// <returns>A TokenList object generated from the given statament</returns>
        public IntermediaryTokenList TokenizeStatement(ZScriptParser.StatementContext context)
        {
            _tokens = new IntermediaryTokenList();

            if (context.expression() != null)
            {
                TokenizeExpression(context.expression());
            }
            else if (context.assignmentExpression() != null)
            {
                TokenizeAssignmentExpression(context.assignmentExpression());
            }

            return _tokens;
        }

        /// <summary>
        /// Tokenizes a given expression context into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the expression to tokenize</param>
        /// <returns>The list of tokens containing the expression that was tokenized</returns>
        public IntermediaryTokenList TokenizeExpression(ZScriptParser.ExpressionContext context)
        {
            _tokens = new IntermediaryTokenList();

            VisitExpression(context);

            return _tokens;
        }

        /// <summary>
        /// Tokenizes a given assignment expression context into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the assignment expression to tokenize</param>
        /// <returns>The list of tokens containing the assignment expression that was tokenized</returns>
        public IntermediaryTokenList TokenizeAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            _tokens = new IntermediaryTokenList();

            VisitAssignmentExpression(context);

            return _tokens;
        }

        /// <summary>
        /// Visits an expression context that contains a constant value pre-evaluated within it.
        /// This method throws an exception if the context does not contains a pre-evaluated constant
        /// </summary>
        /// <param name="context">The context containing the expression with the constant expanded within it</param>
        /// <exception cref="Exception">The context does not contains a pre-evaluated constant value</exception>
        void VisitExpressionWithConstant(ZScriptParser.ExpressionContext context)
        {
            // Verify constant values
            if (!context.IsConstant)
            {
                throw new Exception("Trying to visit an expression that has no constant pre-evaluated within it");
            }

            var s = context.ConstantValue as string;
            if (s != null)
            {
                _tokens.Add(TokenFactory.CreateStringToken(s));
            }
            else
            {
                _tokens.Add(TokenFactory.CreateBoxedValueToken(context.ConstantValue));
            }
        }

        #region Assignment Expression

        void VisitAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            if (ExpressionUtils.IsCompoundAssignmentOperator(context.assignmentOperator()))
            {
                // Detect compound assignment operations and duplicate the value of the left value
                _isGetAccess = context.leftValue().leftValueAccess() != null;
                VisitLeftValue(context.leftValue());

                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));

                _isGetAccess = true;

                if (context.expression() != null)
                {
                    VisitExpression(context.expression());
                }
                else if (context.assignmentExpression() != null)
                {
                    _isRootMember = true;

                    VisitAssignmentExpression(context.assignmentExpression());
                }

                _isRootMember = true;

                _isGetAccess = true;
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Swap));

                VisitAssignmentOperator(context.assignmentOperator());
            }
            else
            {
                _isGetAccess = true;

                if (context.expression() != null)
                {
                    VisitExpression(context.expression());
                }
                else if (context.assignmentExpression() != null)
                {
                    _isRootMember = true;

                    VisitAssignmentExpression(context.assignmentExpression());
                }

                _isRootMember = true;

                _isGetAccess = true;
                VisitLeftValue(context.leftValue());

                VisitAssignmentOperator(context.assignmentOperator());
            }
        }

        void VisitAssignmentOperator(ZScriptParser.AssignmentOperatorContext context)
        {
            // When the token is not a common equality operator, it must be one of
            // the other tokens that require the value to have an operation performed
            // on itself and then set again. We duplicate the value on top of the stack
            // so we can get it down bellow for the operation to perform
            if (ExpressionUtils.IsCompoundAssignmentOperator(context))
            {
                _tokens.Add(ExpressionUtils.OperatorForCompound(context));
            }

            // When compound, swap the values on top of the stack so the assignment works correctly
            if (ExpressionUtils.IsCompoundAssignmentOperator(context))
            {
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Swap));
            }

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));
        }

        void VisitLeftValue(ZScriptParser.LeftValueContext context)
        {
            // leftValue : (memberName | 'this') leftValueAccess?;
            // leftValueAccess : (funcCallArguments leftValueAccess) | ('.' leftValue) | (arrayAccess leftValueAccess?);
            if(context.memberName() != null)
            {
                VisitMemberName(context.memberName());
            }
            // 'this' special constant access
            else
            {
                _tokens.Add(TokenFactory.CreateVariableToken("this", true));
            }

            _isRootMember = false;

            if (context.leftValueAccess() != null)
            {
                VisitLeftValueAccess(context.leftValueAccess());
            }
        }

        void VisitLeftValueAccess(ZScriptParser.LeftValueAccessContext context)
        {
            if (context.unwrap != null)
            {
                // Add null-check token
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CheckNull));
            }

            if (context.functionCall() != null)
            {
                VisitFunctionCall(context.functionCall());

                VisitLeftValueAccess(context.leftValueAccess());
            }
            else if (context.fieldAccess() != null)
            {
                _isGetAccess = context.leftValueAccess() != null;

                VisitFieldAccess(context.fieldAccess(), IsFunctionCallAccess(context));

                if (context.leftValueAccess() != null)
                {
                    VisitLeftValueAccess(context.leftValueAccess());
                }
            }
            else if (context.arrayAccess() != null)
            {
                _isGetAccess = context.leftValueAccess() != null;

                VisitArrayAccess(context.arrayAccess());

                if (context.leftValueAccess() != null)
                {
                    VisitLeftValueAccess(context.leftValueAccess());
                }
            }
        }

        #endregion

        #region Expression

        void VisitExpression(ZScriptParser.ExpressionContext context)
        {
            // Verify constant values
            if (context.IsConstant && context.IsConstantPrimitive)
            {
                VisitExpressionWithConstant(context);
                return;
            }

            // Print the other side of the tree first
            if (context.expression().Length == 1)
            {
                // Unwrapping
                if (context.unwrap != null)
                {
                    VisitUnwrapExpression(context);
                }
                else if(context.type() == null)
                {
                    VisitUnaryExpression(context);
                }
                else
                {
                    // 'is' comparision
                    if (context.T_IS() != null)
                    {
                        VisitTypeCheckExpression(context);
                    }
                    // Type cast
                    else
                    {
                        VisitTypeCastExpression(context);
                    }
                }
            }
            else if (context.expression().Length == 2)
            {
                VisitBinaryExpression(context);
            }
            else if (context.expression().Length == 3)
            {
                VisitTernaryExpression(context);
            }
            else if (context.assignmentExpression() != null)
            {
                VisitAssignmentExpression(context.assignmentExpression());
            }
            // Primary expressions
            else if (context.T_THIS() != null)
            {
                // TODO: Move this to a separate method
                _tokens.Add(TokenFactory.CreateVariableToken("this", true));

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.T_BASE() != null)
            {
                // TODO: Move this to a separate method
                _tokens.Add(TokenFactory.CreateVariableToken("base", true));

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
                }
            }
            // Prefix/postfix operations
            else if (context.prefixOperator() != null)
            {
                _isGetAccess = context.leftValue().leftValueAccess() != null;
                VisitLeftValue(context.leftValue());

                VisitPrefixOperator(context.prefixOperator());
            }
            else if (context.postfixOperator() != null)
            {
                _isGetAccess = context.leftValue().leftValueAccess() != null;
                VisitLeftValue(context.leftValue());

                VisitPostfixOperator(context.postfixOperator());
            }
            else if (context.memberName() != null)
            {
                _isRootMember = true;

                VisitMemberName(context.memberName());

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
                }
            }
            else if (context.constantAtom() != null)
            {
                VisitConstantAtom(context.constantAtom());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.closureExpression() != null)
            {
                VisitClosureExpression(context.closureExpression());

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
                }
            }
            else if (context.arrayLiteral() != null)
            {
                VisitArrayLiteral(context.arrayLiteral());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.arrayLiteralInit() != null)
            {
                VisitArrayLiteralInit(context.arrayLiteralInit());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.dictionaryLiteral() != null)
            {
                VisitDictionaryLiteral(context.dictionaryLiteral());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.dictionaryLiteralInit() != null)
            {
                VisitDictionaryLiteralInit(context.dictionaryLiteralInit());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.objectLiteral() != null)
            {
                VisitObjectLiteral(context.objectLiteral());

                if (context.objectAccess() != null)
                {
                    VisitObjectAccess(context.objectAccess());
                }
            }
            else if (context.newExpression() != null)
            {
                VisitNewExpression(context.newExpression());

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
                }
            }
            else
            {
                throw new Exception("Unkown expression type encoutered: " + context.GetText());
            }

            // Add implicit casting
            // We ignore callable type definitions because are non-castable, currently
            if (context.ImplicitCastType != null && context.EvaluatedType != context.ImplicitCastType &&
                !context.ImplicitCastType.IsAny && !(context.ExpectedType is CallableTypeDef) &&
                context.EvaluatedType != TypeProvider.NullType())
            {
                var opt = context.EvaluatedType as OptionalTypeDef;
                if (opt != null && opt.BaseWrappedType == context.ImplicitCastType)
                {
                    return;
                }

                if (context.EvaluatedType == TypeProvider.AnyType())
                {
                    _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CheckType,
                        TypeProvider.NativeTypeForTypeDef(context.ImplicitCastType)));
                }
                else
                {
                    _tokens.Add(TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Cast,
                        context.ImplicitCastType));
                }
            }
        }

        private void VisitMemberName(ZScriptParser.MemberNameContext context)
        {
            if (_isRootMember)
            {
                _tokens.Add(TokenFactory.CreateVariableToken(context.IDENT().GetText(), _isGetAccess));
            }
            else
            {
                _tokens.Add(TokenFactory.CreateMemberNameToken(context.IDENT().GetText()));
            }
        }

        private void VisitPrefixOperator(ZScriptParser.PrefixOperatorContext context)
        {
            switch (context.GetText())
            {
                case "++":
                    _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.IncrementPrefix));
                    break;
                case "--":
                    _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.DecrementPrefix));
                    break;
            }
        }

        private void VisitPostfixOperator(ZScriptParser.PostfixOperatorContext context)
        {
            switch (context.GetText())
            {
                case "++":
                    _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.IncrementPostfix));
                    break;
                case "--":
                    _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.DecrementPostfix));
                    break;
            }
        }

        private void VisitClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            // Get the name of the closure to substitute
            // TODO: Abstract this reference to an interface so we can decouple the StatementTokenizerContext class from the expression tokenizer
            var closureName = _context.Scope.Definitions.First(def => def.Context == context).Name;

            _tokens.Add(TokenFactory.CreateVariableToken(closureName, true));
        }

        private void VisitNewExpression(ZScriptParser.NewExpressionContext context)
        {
            // Consume the type name
            VisitTypeName(context.typeName());

            // Add the function call
            VisitFunctionCallArguments(context.funcCallArguments());

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.New));
        }

        private void VisitTypeName(ZScriptParser.TypeNameContext context)
        {
            _tokens.Add(TokenFactory.CreateStringToken(context.GetText()));
        }

        private void VisitUnwrapExpression(ZScriptParser.ExpressionContext context)
        {
            // Evaluate expression
            VisitExpression(context.expression(0));

            // Add null-check token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CheckNull));

            if (context.valueAccess() != null)
            {
                VisitValueAccess(context.valueAccess());
            }
        }

        #region Type casting/checking

        private void VisitTypeCastExpression(ZScriptParser.ExpressionContext context)
        {
            VisitExpression(context.expression(0));

            _tokens.Add(TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Cast, context.type()));
        }

        private void VisitTypeCheckExpression(ZScriptParser.ExpressionContext context)
        {
            VisitExpression(context.expression(0));

            _tokens.Add(TokenFactory.CreateTypeToken(TokenType.Operator, VmInstruction.Is, context.type()));
        }

        #endregion

        #region Ternary, binary, unary

        private void VisitUnaryExpression(ZScriptParser.ExpressionContext context)
        {
            // Maybe we matched an unary operator?
            if (context.unaryOperator() != null)
            {
                VisitExpression(context.expression(0));

                var txt = context.unaryOperator().GetText();

                switch (txt)
                {
                    case "-":
                        _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.ArithmeticNegate));
                        break;
                    case "!":
                        _tokens.Add(TokenFactory.CreateOperatorToken(VmInstruction.LogicalNegate));
                        break;
                }
            }
            else
            {
                VisitExpression(context.expression(0));

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
                }
            }
        }

        private void VisitBinaryExpression(ZScriptParser.ExpressionContext context)
        {
            // Null coalescing expression
            if (context.T_NULL_COALESCE() != null)
            {
                VisitNullCoalescingExpression(context);
                return;
            }

            VisitExpression(context.expression(0));

            ProcessLogicalOperator(context);

            VisitExpression(context.expression(1));

            VisitExpressionOperator(context);
        }

        private void VisitTernaryExpression(ZScriptParser.ExpressionContext context)
        {
            // Constant evaluation
            if (context.expression(0).IsConstant && context.expression(0).EvaluatedType == TypeProvider.BooleanType())
            {
                if (context.expression(0).ConstantValue.Equals(true))
                {
                    VisitExpression(context.expression(1));
                }
                else
                {
                    VisitExpression(context.expression(2));
                }

                return;
            }

            // Pre-create the jump target token
            var endT = new JumpTargetToken();
            var falseT = new JumpTargetToken();

            // 1 - Add the ternary condition
            VisitExpression(context.expression(0));
            // 2 - Add the jump token
            _tokens.Add(new JumpToken(falseT, true, false));

            // 3 - Add the left side expression
            VisitExpression(context.expression(1));
            // 4 - Add another jump token pointing to the end of the expression
            _tokens.Add(new JumpToken(endT));

            // 5 - Add the false jump target
            _tokens.Add(falseT);
            // 6 - Add the right side expression
            VisitExpression(context.expression(2));

            // 7 - Append the jump target token
            _tokens.Add(endT);
        }

        private void VisitExpressionOperator(ZScriptParser.ExpressionContext context)
        {
            var str = ExpressionUtils.OperatorOnExpression(context);
            var t = TokenFactory.CreateOperatorToken(str);

            _tokens.Add(t);

            if (t.Instruction == VmInstruction.LogicalAnd || t.Instruction == VmInstruction.LogicalOr)
            {
                // If the token is a logical And or Or, deal with special jumps by inserting a jump target
                var jump = _shortCircuitJumps.Pop();
                var target = new JumpTargetToken();

                jump.TargetToken = target;
                _tokens.Add(target);
            }
        }

        private void VisitNullCoalescingExpression(ZScriptParser.ExpressionContext context)
        {
            // Null coalescing expression: a ?? b

            // Constant propagation
            if (context.expression(0).IsConstant)
            {
                // a is never null
                if (context.expression(0).ConstantValue != null)
                {
                    VisitExpression(context.expression(0));
                }
                // a is always null
                else
                {
                    VisitExpression(context.expression(1));
                }
                
                return;
            }

            // Prepare jumps
            var target = new JumpTargetToken();
            var jumpT = new JumpToken(target, true, true, true, true);

            // 'a'
            VisitExpression(context.expression(0));

            // Duplicate
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));

            // Add null verify jump
            _tokens.Add(jumpT);

            // Pop 'a'
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Pop));

            // 'b'
            VisitExpression(context.expression(1));

            _tokens.Add(target);
        }

        /// <summary>
        /// Processes the logical operator at a given expression context, dealing with logical short-circuiting by
        /// pushing a jump to the short-circuit jump stack when the operator on the context provided is a logical operator
        /// </summary>
        /// <param name="context">The context conaining the jump to perform</param>
        private void ProcessLogicalOperator(ZScriptParser.ExpressionContext context)
        {
            var str = ExpressionUtils.OperatorOnExpression(context);

            if (string.IsNullOrEmpty(str))
                return;

            var inst = TokenFactory.InstructionForOperator(str);
            JumpToken jumpToken;

            // Create a conditional peek jump for logical operators
            switch (inst)
            {
                case VmInstruction.LogicalAnd:
                    jumpToken = new JumpToken(null, true, false, false);
                    break;
                case VmInstruction.LogicalOr:
                    jumpToken = new JumpToken(null, true, true, false);
                    break;
                default:
                    return;
            }

            _tokens.Add(jumpToken);
            _shortCircuitJumps.Push(jumpToken);
        }

        #endregion

        #region Member accessing

        private void VisitObjectAccess(ZScriptParser.ObjectAccessContext context)
        {
            _isRootMember = false;

            if (context.arrayAccess() != null)
            {
                VisitArrayAccess(context.arrayAccess());
            }
            else if (context.fieldAccess() != null)
            {
                VisitFieldAccess(context.fieldAccess(), IsFunctionCallAccess(context));
            }
            if (context.valueAccess() != null)
            {
                VisitValueAccess(context.valueAccess());
            }
        }

        private void VisitValueAccess(ZScriptParser.ValueAccessContext context)
        {
            _isRootMember = false;
            // Verify null conditionality
            JumpTargetToken endTarget = null;

            if (context.nullable != null)
            {
                endTarget = new JumpTargetToken();
                // Add the duplicate token
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Duplicate));
                // Add the null-check jump
                _tokens.Add(new JumpToken(endTarget, true, false, true, true));
            }

            if (context.functionCall() != null)
            {
                VisitFunctionCall(context.functionCall());
            }
            else if (context.arrayAccess() != null)
            {
                VisitArrayAccess(context.arrayAccess());
            }
            else if (context.fieldAccess() != null)
            {
                VisitFieldAccess(context.fieldAccess(), IsFunctionCallAccess(context));
            }
            if (context.valueAccess() != null)
            {
                VisitValueAccess(context.valueAccess());
            }

            if(endTarget != null)
                _tokens.Add(endTarget);
        }

        private void VisitFieldAccess(ZScriptParser.FieldAccessContext context, bool functionCall)
        {
            VisitMemberName(context.memberName());

            if (functionCall)
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.GetCallable));
            else
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.GetMember));

            // Expand the index subscripter in case it is a get access
            if (_isGetAccess && !functionCall)
            {
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Get));
            }
        }

        private void VisitArrayAccess(ZScriptParser.ArrayAccessContext context)
        {
            VisitExpression(context.expression());

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript));

            // Expand the index subscripter in case it is a get access
            if (_isGetAccess)
            {
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Get));
            }
        }

        private void VisitFunctionCall(ZScriptParser.FunctionCallContext context)
        {
            VisitFunctionCallArguments(context.funcCallArguments());

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Call));
        }

        private void VisitFunctionCallArguments(ZScriptParser.FuncCallArgumentsContext args)
        {
            VisitExpressionList(args.expressionList());
        }

        #endregion

        #region Literals

        private void VisitArrayLiteral(ZScriptParser.ArrayLiteralContext context)
        {
            // Collect the expressions
            VisitExpressionList(context.expressionList());

            if (context.EvaluatedValueType == null)
                throw new InvalidOperationException("Array literal context lacked required type for values of the array.");

            // Add the array creation token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, TypeProvider.NativeTypeForTypeDef(context.EvaluatedValueType, true)));
        }

        private void VisitArrayLiteralInit(ZScriptParser.ArrayLiteralInitContext context)
        {
            // Create a 0 token to notify no arguments
            _tokens.Add(TokenFactory.CreateBoxedValueToken(0));

            if (context.EvaluatedValueType == null)
                throw new InvalidOperationException("Array literal initializer context lacked required type for values of the array.");

            // Add the array creation token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, TypeProvider.NativeTypeForTypeDef(context.EvaluatedValueType, true)));
        }

        private void VisitDictionaryLiteral(ZScriptParser.DictionaryLiteralContext context)
        {
            // Get the type for the key and value
            var keyType = context.EvaluatedKeyType;
            var valueType = context.EvaluatedValueType;

            if (keyType == null)
                throw new InvalidOperationException("Dictionary literal context lacked required type for key of dictionary.");
            if (valueType == null)
                throw new InvalidOperationException("Dictionary literal context lacked required type for values of dictionary.");

            // Collect the entries
            VisitDictionaryEntryList(context.dictionaryEntryList());

            // Add the dictionary creation token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary,
                new[]
                {
                    TypeProvider.NativeTypeForTypeDef(keyType, true),
                    TypeProvider.NativeTypeForTypeDef(valueType, true)
                }));
        }

        private void VisitDictionaryLiteralInit(ZScriptParser.DictionaryLiteralInitContext context)
        {
            // Create a 0 token to notify no arguments
            _tokens.Add(TokenFactory.CreateBoxedValueToken(0));

            // Get the type for the key and value
            var keyType = context.EvaluatedKeyType;
            var valueType = context.EvaluatedValueType;

            if (keyType == null)
                throw new InvalidOperationException("Dictionary literal initializer context lacked required type for key of dictionary.");
            if (valueType == null)
                throw new InvalidOperationException("Dictionary literal initializer context lacked required type for values of dictionary.");

            // Add the dictionary creation token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary,
                new[]
                {
                    TypeProvider.NativeTypeForTypeDef(keyType, true),
                    TypeProvider.NativeTypeForTypeDef(valueType, true)
                }));
        }

        private void VisitDictionaryEntryList(ZScriptParser.DictionaryEntryListContext context)
        {
            var entryDefs = context.dictionaryEntry();

            if (entryDefs.Length == 0)
            {
                _tokens.Add(TokenFactory.CreateBoxedValueToken(0));
                return;
            }

            int argCount = 0;
            foreach (var entry in entryDefs)
            {
                VisitExpression(entry.expression(0));
                VisitExpression(entry.expression(1));

                argCount++;
            }

            _tokens.Add(TokenFactory.CreateBoxedValueToken(argCount));
        }

        private void VisitObjectLiteral(ZScriptParser.ObjectLiteralContext context)
        {
            VisitObjectEntryList(context.objectEntryList());

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateObject));
        }

        private void VisitObjectEntryList(ZScriptParser.ObjectEntryListContext context)
        {
            if (context == null)
            {
                _tokens.Add(TokenFactory.CreateBoxedValueToken(0));
                return;
            }

            var entryDefs = context.objectEntryDefinition();

            int argCount = 0;
            foreach (var entry in entryDefs)
            {
                VisitExpression(entry.expression());
                VisitEntryName(entry.entryName());

                argCount++;
            }

            _tokens.Add(TokenFactory.CreateBoxedValueToken(argCount));
        }

        private void VisitEntryName(ZScriptParser.EntryNameContext context)
        {
            if (context.IDENT() != null)
            {
                _tokens.Add(TokenFactory.CreateStringToken(context.IDENT().GetText()));
            }
            else
            {
                _tokens.Add(TokenFactory.CreateStringToken(ConstantAtomParser.ParseStringAtom(context.stringLiteral())));
            }
        }

        #endregion

        private void VisitExpressionList(ZScriptParser.ExpressionListContext context)
        {
            if (context == null)
            {
                _tokens.Add(TokenFactory.CreateBoxedValueToken(0));
                return;
            }

            var argsExps = context.expression();

            int argCount = 0;
            foreach (var argExp in argsExps)
            {
                VisitExpression(argExp);
                argCount++;
            }

            _tokens.Add(TokenFactory.CreateBoxedValueToken(argCount));
        }

        private void VisitConstantAtom(ZScriptParser.ConstantAtomContext context)
        {
            if (context.stringLiteral() != null)
            {
                var strToken = context.stringLiteral().GetText();
                strToken = strToken.Substring(1, strToken.Length - 2);

                _tokens.Add(TokenFactory.CreateStringToken(strToken));
                return;
            }
            if (context.T_FALSE() != null)
            {
                _tokens.Add(TokenFactory.CreateFalseToken());
                return;
            }
            if (context.T_TRUE() != null)
            {
                _tokens.Add(TokenFactory.CreateTrueToken());
                return;
            }
            if (context.T_NULL() != null)
            {
                _tokens.Add(TokenFactory.CreateNullToken());
                return;
            }

            var str = context.GetText();

            object value;
            var succeed = ValueParser.TryParseValueBoxed(str, out value);
            if (succeed)
            {
                _tokens.Add(TokenFactory.CreateBoxedValueToken(value));
            }
            else
            {
                throw new Exception("Invalid constant atom token received: " + context);
            }
        }

        #endregion

        /// <summary>
        /// Returns whether a given object access node represents a function call
        /// </summary>
        /// <param name="context">The context containing the value access</param>
        /// <returns>Whether the node represents a function call</returns>
        private static bool IsFunctionCallAccess(ZScriptParser.ObjectAccessContext context)
        {
            return context.valueAccess() != null && context.valueAccess().functionCall() != null;
        }

        /// <summary>
        /// Returns whether a given value access node represents a function call
        /// </summary>
        /// <param name="context">The context containing the value access</param>
        /// <returns>Whether the node represents a function call</returns>
        private static bool IsFunctionCallAccess(ZScriptParser.ValueAccessContext context)
        {
            return context.valueAccess() != null && context.valueAccess().functionCall() != null;
        }

        /// <summary>
        /// Returns whether a given left value access node represents a function call
        /// </summary>
        /// <param name="context">The context containing the value access</param>
        /// <returns>Whether the node represents a function call</returns>
        private static bool IsFunctionCallAccess(ZScriptParser.LeftValueAccessContext context)
        {
            return context.leftValueAccess() != null && context.leftValueAccess().functionCall() != null;
        }
    }
}