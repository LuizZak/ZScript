﻿using System;
using System.Collections.Generic;
using ZScript.CodeGeneration.Tokenizers.Helpers;
using ZScript.CodeGeneration.Tokenizers.Statements;
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenizers
{
    /// <summary>
    /// Class capable of tokenizing a function body from an AST tree into a TokenList
    /// </summary>
    public class FunctionBodyTokenizer
    {
        /// <summary>
        /// Tokenizes the contents of the given function body context, coming from a parse tree
        /// </summary>
        /// <param name="context">The function body to tokenize</param>
        /// <returns>A token list for the givne function body</returns>
        public TokenList TokenizeBody(ZScriptParser.FunctionBodyContext context)
        {
            var state = context.blockStatement();

            var stc = new StatementTokenizerContext();
            var tokens = stc.TokenizeBlockStatement(state);

            Console.WriteLine("Final token list, before expanding jumps:");
            PrintTokens(tokens);

            Console.WriteLine("Final token list:");
            JumpTokenExpander.ExpandInList(tokens, VmInstruction.Interrupt);
            PrintTokens(tokens);

            return new TokenList { Tokens = tokens.ToArray() };
        }

        private static void PrintTokens(List<Token> tokenList)
        {
            int add = 0;

            int jt = 0;
            foreach (var token in tokenList)
            {
                Console.Write("{0:0000000}", add++);
                Console.Write(": ");

                if (token is JumpToken)
                {
                    Console.Write("[");
                    Console.Write(tokenList.IndexOf(((JumpToken)token).TargetToken) + (++jt));
                    Console.WriteLine(" JUMP]");
                    continue;
                }
                if (token is JumpTargetToken)
                {
                    Console.WriteLine("JUMP_TARGET ");
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.Operator:
                    case TokenType.Instruction:
                        Console.Write(token.Instruction);
                        break;
                    default:
                        Console.Write(token.TokenObject);
                        break;
                }

                Console.WriteLine("");
            }
            Console.WriteLine();
        }
    }

    class PostfixExpressionPrinter : ZScriptBaseListener
    {
        public void PrintStatement(ZScriptParser.StatementContext context)
        {
            if (context.expression() != null)
            {
                PrintExpression(context.expression());
            }
            else if (context.assignmentExpression() != null)
            {
                PrintAssignmentExpression(context.assignmentExpression());
            }

            Console.WriteLine(" CLEAR_STACK ");
        }

        #region Assignment Expression

        void PrintAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            // Detect compound assignment operations and duplicate the value of the left value
            if (IsCompoundAssignmentOperator(context.assignmentOperator()))
            {
                PrintLeftValue(context.leftValue());
                Console.Write(" ");
            }

            if (context.expression() != null)
            {
                PrintExpression(context.expression());
            }
            else if (context.assignmentExpression() != null)
            {
                PrintAssignmentExpression(context.assignmentExpression());
            }

            Console.Write(" ");

            PrintLeftValue(context.leftValue());

            Console.Write(" ");

            PrintAssignmentOperator(context.assignmentOperator());
        }

        void PrintAssignmentOperator(ZScriptParser.AssignmentOperatorContext context)
        {
            // When the token is not a common equality operator, it must be one of
            // the other tokens that require the value to have an operation performed
            // on itself and then set again. We duplicate the value on top of the stack
            // so we can get it down bellow for the operation to perform
            if (IsCompoundAssignmentOperator(context))
            {
                Console.Write("GET ");
            }

            Console.Write(OperatorForCompound(context));

            // When compound, swap the values on top of the stack so the assignment works correctly
            if (IsCompoundAssignmentOperator(context))
            {
                Console.Write(" SWAP ");
            }

            Console.Write("SET");

            // Assignment operators
            /*
            T_EQUALS : '=';
            T_PLUS_EQUALS : '+=';
            T_MINUS_EQUALS : '-=';
            T_TIMES_EQUALS : '*=';
            T_DIV_EQUALS : '/=';
            T_MOD_EQUALS : '%=';
            T_XOR_EQUALS : '^=';
            T_AND_EQUALS : '&=';
            T_TILDE_EQUALS : '~=';
            T_OR_EQUALS : '|=';
            */
        }

        void PrintLeftValue(ZScriptParser.LeftValueContext context)
        {
            // leftValue : memberName leftValueAccess?;
            // leftValueAccess : (funcCallArguments leftValueAccess) | ('.' leftValue) | (arrayAccess leftValueAccess?);
            Console.Write(context.memberName().GetText());

            if (context.leftValueAccess() != null)
            {
                PrintLeftValueAccess(context.leftValueAccess());
            }
        }

        private void PrintLeftValueAccess(ZScriptParser.LeftValueAccessContext context)
        {
            while (true)
            {
                Console.Write(" ");

                if (context.funcCallArguments() != null)
                {
                    PrintFunctionCallArguments(context.funcCallArguments());

                    context = context.leftValueAccess();
                    continue;
                }

                if (context.fieldAccess() != null)
                {
                    PrintFieldAccess(context.fieldAccess());

                    if (context.leftValueAccess() != null)
                    {
                        context = context.leftValueAccess();
                        continue;
                    }
                }
                else if (context.arrayAccess() != null)
                {
                    PrintArrayAccess(context.arrayAccess());

                    if (context.leftValueAccess() != null)
                    {
                        context = context.leftValueAccess();
                        continue;
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Returns whether a given assignment operatored stored within an AssignmentOperatorContext is a compound assignment operator.
        /// Compound assignments are used when a variable should have an arithmetic operation performed between its value and the expression value
        /// before the result can then be assigned back to the variable.
        /// </summary>
        /// <param name="context">The context that contains the assignment operator</param>
        /// <returns>Whether the given assignment operator is a compound assignment operator</returns>
        bool IsCompoundAssignmentOperator(ZScriptParser.AssignmentOperatorContext context)
        {
            return context.GetText() != "=";
        }

        /// <summary>
        /// Returns the underlying arithmetic operator from a provided compound assignment operator.
        /// If the operator is not an assignment operator, null is returned
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The underlying arithmetic operator from a provided compound assignment operator</returns>
        object OperatorForCompound(ZScriptParser.AssignmentOperatorContext context)
        {
            if (!IsCompoundAssignmentOperator(context))
            {
                return null;
            }
            // Assignment operators
            /*
            T_EQUALS : '=';
            T_PLUS_EQUALS : '+=';
            T_MINUS_EQUALS : '-=';
            T_TIMES_EQUALS : '*=';
            T_DIV_EQUALS : '/=';
            T_MOD_EQUALS : '%=';
            T_XOR_EQUALS : '^=';
            T_AND_EQUALS : '&=';
            T_TILDE_EQUALS : '~=';
            T_OR_EQUALS : '|=';
            */
            // Return the first character of the operator
            return context.GetText()[0].ToString();
        }

        #endregion

        #region Expression

        void PrintExpression(ZScriptParser.ExpressionContext context)
        {
            // Print the other side of the tree first
            if (context.expression().Length == 1)
            {
                // Maybe we matched an unary operator?
                if (context.ChildCount == 2)
                {
                    PrintExpression(context.expression(0));

                    var txt = context.GetChild(0).GetText();

                    switch (txt)
                    {
                        case "-":
                            Console.Write(" UNARY_NEGATE ");
                            break;
                        case "!":
                            Console.Write(" NEGATE ");
                            break;
                    }
                }
                else
                {
                    PrintExpression(context.expression(0));

                    if (context.valueAccess() != null)
                    {
                        PrintMemberAccess(context.valueAccess());
                    }
                }
            }
            else if (context.expression().Length >= 2)
            {
                PrintExpression(context.expression(0));

                Console.Write(" ");

                PrintExpression(context.expression(1));

                Console.Write(" ");

                PrintExpressionOperator(context);

                Console.Write(" ");
            }
            else if (context.assignmentExpression() != null)
            {
                PrintAssignmentExpression(context.assignmentExpression());
            }
            else if (context.prefixOperator() != null)
            {
                PrintPrefixOperation(context.prefixOperator());

                Console.Write(" ");

                PrintLeftValue(context.leftValue());
            }
            else if (context.postfixOperator() != null)
            {
                PrintLeftValue(context.leftValue());

                Console.Write(" ");

                PrintPostfixOperation(context.postfixOperator());
            }

            if (context.memberName() != null)
            {
                Console.Write(context.memberName().IDENT().GetText());

                if (context.valueAccess() != null)
                {
                    PrintMemberAccess(context.valueAccess());
                }
            }
            else if (context.constantAtom() != null)
            {
                var atom = context.constantAtom();

                Console.Write(atom.GetText());
            }
        }

        private static void PrintExpressionOperator(ZScriptParser.ExpressionContext context)
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

            Console.Write(str);
        }

        private void PrintPrefixOperation(ZScriptParser.PrefixOperatorContext context)
        {
            switch (context.GetText())
            {
                case "++":
                    Console.Write("PRE_INC");
                    break;
                case "--":
                    Console.Write("PRE_DEINC");
                    break;
            }
        }

        private void PrintPostfixOperation(ZScriptParser.PostfixOperatorContext context)
        {
            switch (context.GetText())
            {
                case "++":
                    Console.Write("INC");
                    break;
                case "--":
                    Console.Write("DEINC");
                    break;
            }
        }

        private void PrintMemberAccess(ZScriptParser.ValueAccessContext context)
        {
            while (true)
            {
                Console.Write(" ");
                if (context.functionCall() != null)
                {
                    PrintFunctionCall(context.functionCall());
                }
                else if (context.arrayAccess() != null)
                {
                    PrintArrayAccess(context.arrayAccess());
                }
                else if (context.fieldAccess() != null)
                {
                    PrintFieldAccess(context.fieldAccess());
                }
                if (context.valueAccess() != null)
                {
                    context = context.valueAccess();
                    continue;
                }
                break;
            }
        }

        void PrintFieldAccess(ZScriptParser.FieldAccessContext context)
        {
            Console.Write(context.memberName().GetText());

            Console.Write(" GET_MEMBER ");
        }

        void PrintArrayAccess(ZScriptParser.ArrayAccessContext context)
        {
            PrintExpression(context.expression());

            Console.Write(" GET_SUBSCRIPT");
        }

        void PrintFunctionCall(ZScriptParser.FunctionCallContext context)
        {
            var args = context.funcCallArguments();

            PrintFunctionCallArguments(args);

            Console.Write(" CALL");
        }

        private void PrintFunctionCallArguments(ZScriptParser.FuncCallArgumentsContext args)
        {
            if (args.expressionList() == null)
            {
                Console.Write(0);
                return;
            }

            var argsExps = args.expressionList().expression();

            foreach (var argExp in argsExps)
            {
                Console.Write(" ");

                PrintExpression(argExp);
            }

            Console.Write(" ");

            Console.Write(argsExps.Length);
        }

        #endregion
    }

    class InfixExpressionPrinter : ZScriptBaseListener
    {
        public void PrintStatement(ZScriptParser.StatementContext context)
        {
            if (context.expression() != null)
            {
                PrintExpression(context.expression());
            }
            else if (context.assignmentExpression() != null)
            {
                PrintAssignmentExpression(context.assignmentExpression());
            }

            Console.WriteLine(";");
        }

        void PrintLeftValue(ZScriptParser.LeftValueContext context)
        {
            // leftValue : memberName leftValueAccess?;
            // leftValueAccess : (funcCallArguments leftValueAccess) | ('.' leftValue) | (arrayAccess leftValueAccess?);
            Console.Write(context.memberName().GetText());

            if (context.leftValueAccess() != null)
            {
                PrintLeftValueAccess(context.leftValueAccess());
            }
        }

        void PrintLeftValueAccess(ZScriptParser.LeftValueAccessContext context)
        {
            if (context.funcCallArguments() != null)
            {
                PrintFunctionCallArguments(context.funcCallArguments());

                PrintLeftValueAccess(context.leftValueAccess());
            }
            else if (context.fieldAccess() != null)
            {
                PrintFieldAccess(context.fieldAccess());

                if (context.leftValueAccess() != null)
                {
                    PrintLeftValueAccess(context.leftValueAccess());
                }
            }
            else if (context.arrayAccess() != null)
            {
                PrintArrayAccess(context.arrayAccess());

                if (context.leftValueAccess() != null)
                {
                    PrintLeftValueAccess(context.leftValueAccess());
                }
            }
        }

        void PrintAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            PrintLeftValue(context.leftValue());

            Console.Write(context.assignmentOperator().GetText());

            if (context.expression() != null)
            {
                PrintExpression(context.expression());
            }
            else if (context.assignmentExpression() != null)
            {
                PrintAssignmentExpression(context.assignmentExpression());
            }
        }

        void PrintExpression(ZScriptParser.ExpressionContext context)
        {
            // Print the other side of the tree first
            if (context.expression().Length == 1)
            {
                // Maybe we matched an unary operator?
                if (context.ChildCount == 2)
                {
                    var txt = context.GetChild(0).GetText();
                    if (txt == "-" || txt == "+" || txt == "!")
                    {
                        Console.Write(txt);
                    }

                    PrintExpression(context.expression(0));
                }
                else
                {
                    Console.Write("(");

                    PrintExpression(context.expression(0));

                    Console.Write(")");

                    if (context.valueAccess() != null)
                    {
                        PrintMemberAccess(context.valueAccess());
                    }
                }
            }
            else if (context.expression().Length >= 2)
            {
                PrintExpression(context.expression(0));

                Console.Write(" ");

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

                Console.Write(str);

                Console.Write(" ");

                PrintExpression(context.expression(1));
            }
            else if (context.assignmentExpression() != null)
            {
                Console.Write("(");
                PrintAssignmentExpression(context.assignmentExpression());
                Console.Write(")");
            }
            else if (context.prefixOperator() != null)
            {
                PrintPrefixOperation(context.prefixOperator());

                PrintLeftValue(context.leftValue());
            }
            else if (context.postfixOperator() != null)
            {
                PrintLeftValue(context.leftValue());

                PrintPostfixOperation(context.postfixOperator());
            }

            if (context.memberName() != null)
            {
                Console.Write(context.memberName().IDENT().GetText());

                if (context.valueAccess() != null)
                {
                    PrintMemberAccess(context.valueAccess());
                }
            }
            else if (context.constantAtom() != null)
            {
                var atom = context.constantAtom();

                Console.Write(atom.GetText());
            }
        }

        private void PrintPrefixOperation(ZScriptParser.PrefixOperatorContext context)
        {
            Console.Write(context.GetText());
        }

        private void PrintPostfixOperation(ZScriptParser.PostfixOperatorContext context)
        {
            Console.Write(context.GetText());
        }

        private void PrintMemberAccess(ZScriptParser.ValueAccessContext context)
        {
            while (true)
            {
                if (context.functionCall() != null)
                {
                    PrintFunctionCall(context.functionCall());
                }
                else if (context.arrayAccess() != null)
                {
                    PrintArrayAccess(context.arrayAccess());
                }
                else if (context.fieldAccess() != null)
                {
                    PrintFieldAccess(context.fieldAccess());
                }

                if (context.valueAccess() != null)
                {
                    context = context.valueAccess();
                    continue;
                }
                break;
            }
        }

        void PrintFieldAccess(ZScriptParser.FieldAccessContext context)
        {
            Console.Write(".");
            Console.Write(context.memberName().GetText());
        }

        void PrintArrayAccess(ZScriptParser.ArrayAccessContext context)
        {
            Console.Write("[");

            PrintExpression(context.expression());

            Console.Write("]");
        }

        void PrintFunctionCall(ZScriptParser.FunctionCallContext context)
        {
            Console.Write("(");

            var args = context.funcCallArguments();

            PrintFunctionCallArguments(args);

            Console.Write(")");
        }

        private void PrintFunctionCallArguments(ZScriptParser.FuncCallArgumentsContext args)
        {
            if (args.expressionList() == null)
                return;

            var argsExps = args.expressionList().expression();

            bool first = true;
            foreach (var argExp in argsExps)
            {
                if (!first)
                    Console.Write(",");
                first = false;

                PrintExpression(argExp);
            }
        }
    }
}