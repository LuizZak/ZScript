﻿using System;
using System.Collections.Generic;
using System.Linq;

using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.CodeGeneration.Tokenization.Statements;
using ZScript.Elements;
using ZScript.Parsing;
using ZScript.Runtime.Execution;

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
        private List<Token> _tokens = new List<Token>();

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
        public List<Token> TokenizeStatement(ZScriptParser.StatementContext context)
        {
            _tokens = new List<Token>();

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
        public List<Token> TokenizeExpression(ZScriptParser.ExpressionContext context)
        {
            _tokens = new List<Token>();

            VisitExpression(context);

            return _tokens;
        }

        /// <summary>
        /// Tokenizes a given assignment expression context into a list of tokens
        /// </summary>
        /// <param name="context">The context containing the assignment expression to tokenize</param>
        /// <returns>The list of tokens containing the assignment expression that was tokenized</returns>
        public List<Token> TokenizeAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            _tokens = new List<Token>();

            VisitAssignmentExpression(context);

            return _tokens;
        }

        #region Assignment Expression

        void VisitAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            if (IsCompoundAssignmentOperator(context.assignmentOperator()))
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
            if (IsCompoundAssignmentOperator(context))
            {
                _tokens.Add(OperatorForCompound(context));
            }

            // When compound, swap the values on top of the stack so the assignment works correctly
            if (IsCompoundAssignmentOperator(context))
            {
                _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Swap));
            }

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.Set));
        }

        void VisitLeftValue(ZScriptParser.LeftValueContext context)
        {
            // leftValue : memberName leftValueAccess?;
            // leftValueAccess : (funcCallArguments leftValueAccess) | ('.' leftValue) | (arrayAccess leftValueAccess?);
            VisitMemberName(context.memberName());

            _isRootMember = false;

            if (context.leftValueAccess() != null)
            {
                VisitLeftValueAccess(context.leftValueAccess());
            }
        }

        void VisitLeftValueAccess(ZScriptParser.LeftValueAccessContext context)
        {
            if (context.functionCall() != null)
            {
                VisitFunctionCall(context.functionCall());

                VisitLeftValueAccess(context.leftValueAccess());
            }
            else if (context.fieldAccess() != null)
            {
                _isGetAccess = context.leftValueAccess() != null;

                VisitFieldAccess(context.fieldAccess());

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
            // Print the other side of the tree first
            if (context.expression().Length == 1)
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
            else if (context.expression().Length >= 2)
            {
                VisitExpression(context.expression(0));

                ProcessLogicalOperator(context);

                VisitExpression(context.expression(1));

                VisitExpressionOperator(context);
            }
            else if (context.assignmentExpression() != null)
            {
                VisitAssignmentExpression(context.assignmentExpression());
            }
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

                if (context.valueAccess() != null)
                {
                    VisitValueAccess(context.valueAccess());
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
                throw new Exception("Unkown expression type encoutered: " + context);
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

        private void VisitExpressionOperator(ZScriptParser.ExpressionContext context)
        {
            var str = OperatorOnExpression(context);
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

        /// <summary>
        /// Processes the logical operator at a given expression context, dealing with logical shortcircuiting
        /// </summary>
        /// <param name="context">The context conaining the jump to perform</param>
        private void ProcessLogicalOperator(ZScriptParser.ExpressionContext context)
        {
            var str = OperatorOnExpression(context);

            if (string.IsNullOrEmpty(str))
                return;

            var inst = TokenFactory.InstructionForOperator(str);
            JumpToken jumpToken;

            // Create a conditional peek jump for logical operators
            switch (inst)
            {
                case VmInstruction.LogicalAnd:
                    jumpToken = new JumpToken(null, true) { ConditionToJump = false, ConsumesStack = false };
                    break;
                case VmInstruction.LogicalOr:
                    jumpToken = new JumpToken(null, true) { ConditionToJump = true, ConsumesStack = false };
                    break;
                default:
                    return;
            }

            _tokens.Add(jumpToken);
            _shortCircuitJumps.Push(jumpToken);
        }

        /// <summary>
        /// Returns the arithmetic or logical operator on a given expression context.
        /// Returns an empty string if no operator is found
        /// </summary>
        /// <param name="context">The context containing the operator</param>
        /// <returns>The string that represents the operator</returns>
        private static string OperatorOnExpression(ZScriptParser.ExpressionContext context)
        {
            var str = "";

            if (context.multOp() != null)
            {
                str = context.multOp().GetText();
            }
            else if (context.additionOp() != null)
            {
                str = context.additionOp().GetText();
            }
            else if (context.bitwiseAndXOrOp() != null)
            {
                str = context.bitwiseAndXOrOp().GetText();
            }
            else if (context.bitwiseOrOp() != null)
            {
                str = context.bitwiseOrOp().GetText();
            }
            else if (context.comparisionOp() != null)
            {
                str = context.comparisionOp().GetText();
            }
            else if (context.logicalOp() != null)
            {
                str = context.logicalOp().GetText();
            }

            return str;
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
                VisitFieldAccess(context.fieldAccess());
            }
            if (context.valueAccess() != null)
            {
                VisitValueAccess(context.valueAccess());
            }
        }

        private void VisitValueAccess(ZScriptParser.ValueAccessContext context)
        {
            _isRootMember = false;

            while (true)
            {
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
                    VisitFieldAccess(context.fieldAccess());
                }
                if (context.valueAccess() != null)
                {
                    context = context.valueAccess();
                    continue;
                }
                break;
            }
        }

        private void VisitFieldAccess(ZScriptParser.FieldAccessContext context)
        {
            VisitMemberName(context.memberName());

            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.GetMember));

            // Expand the index subscripter in case it is a get access
            if (_isGetAccess)
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

            // Add the array creation token
            _tokens.Add(TokenFactory.CreateInstructionToken(VmInstruction.CreateArray));
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
        /// Returns whether a given assignment operatored stored within an AssignmentOperatorContext is a compound assignment operator.
        /// Compound assignments are used when a variable should have an arithmetic operation performed between its value and the expression value
        /// before the result can then be assigned back to the variable.
        /// </summary>
        /// <param name="context">The context that contains the assignment operator</param>
        /// <returns>Whether the given assignment operator is a compound assignment operator</returns>
        public static bool IsCompoundAssignmentOperator(ZScriptParser.AssignmentOperatorContext context)
        {
            return context.GetText() != "=";
        }

        /// <summary>
        /// Returns the underlying arithmetic operator from a provided compound assignment operator.
        /// If the operator is not an assignment operator, null is returned
        /// </summary>
        /// <param name="context">The context that contains the assignment operator to get the token for</param>
        /// <returns>The underlying arithmetic operator from a provided compound assignment operator</returns>
        public static Token OperatorForCompound(ZScriptParser.AssignmentOperatorContext context)
        {
            if (!IsCompoundAssignmentOperator(context))
            {
                return null;
            }

            // Return the first character of the operator
            return TokenFactory.CreateOperatorToken(context.GetText()[0].ToString());
        }
    }
}