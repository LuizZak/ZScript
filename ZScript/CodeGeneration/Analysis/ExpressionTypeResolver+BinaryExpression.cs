using System;
using ZScript.CodeGeneration.Elements.Typing;
using ZScript.CodeGeneration.Messages;
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Extension to the ExpressionTypeResolver that resolves binary operators
    /// </summary>
    public partial class ExpressionTypeResolver
    {
        /*
        expression:  '(' expression ')' valueAccess?
                   | '(' assignmentExpression ')'
                   |  prefixOperator leftValue
                   |  leftValue postfixOperator
                   |  closureExpression valueAccess?
                   |  memberName valueAccess?
                   |  objectLiteral objectAccess?
                   |  arrayLiteral valueAccess?
                   |  newExpression valueAccess?
                   |  '(' type ')' expression
                   // Unary expressions
                   |  '-' expression
                   |  '!' expression
                   // Binary expressions
                   |  expression multOp expression
                   |  expression additionOp expression
                   |  expression bitwiseAndXOrOp expression
                   |  expression bitwiseOrOp expression
                   |  expression comparisionOp expression
                   |  expression logicalOp expression
                   |  constantAtom objectAccess?
                   ;
        */

        /// <summary>
        /// Returns a binary expression as defined by the expression contained within a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveBinaryExpression(ZScriptParser.ExpressionContext context)
        {
            var expressions = context.expression();

            var type1 = ResolveExpression(expressions[0]);
            var type2 = ResolveExpression(expressions[1]);
            
            // Register an error when trying to perform an operation with a void value
            if (type1.IsVoid || type2.IsVoid)
            {
                _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Cannot perform binary operations with values of type void", ErrorCode.VoidOnBinaryExpression, context);

                return _typeProvider.AnyType();
            }

            var str = OperatorOnExpression(context);
            if (str == "")
            {
                throw new Exception("Failed to analyze binary expression correctly");
            }

            var instruction = TokenFactory.InstructionForOperator(str);

            // Arithmetic instructions with any operands propagate anys
            if ((type1.IsAny || type2.IsAny) && IsArithmetic(instruction))
            {
                return _typeProvider.AnyType();
            }

            if (!_typeProvider.BinaryExpressionProvider.CanPerformOperation(instruction, type1, type2))
            {
                var message = "Cannot perform " + instruction + " operation on values of type " + type1 + " and " + type2;
                _messageContainer.RegisterError(context.Start.Line, context.Start.Column, message, ErrorCode.InvalidTypesOnOperation, context);

                return _typeProvider.AnyType();
            }

            return _typeProvider.BinaryExpressionProvider.TypeForOperation(instruction, type1, type2);
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

        /// <summary>
        /// Returns whether a given instruction represents an arithmetic instruction
        /// </summary>
        /// <param name="instruction">The instruction to check</param>
        /// <returns>Whether the instruction is an arithmetic instruction</returns>
        private bool IsArithmetic(VmInstruction instruction)
        {
            return instruction == VmInstruction.Multiply || instruction == VmInstruction.Divide ||
                   instruction == VmInstruction.Modulo || instruction == VmInstruction.Add ||
                   instruction == VmInstruction.Subtract;
        }
    }
}