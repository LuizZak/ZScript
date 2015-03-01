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

using Antlr4.Runtime.Tree;
using ZScript.CodeGeneration.Definitions;
using ZScript.Elements;
using ZScript.Parsing;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Specifies a class capable of propagating constants in an expression tree
    /// </summary>
    public class ExpressionConstantResolver : ZScriptBaseListener
    {
        /// <summary>
        /// The runtime generation context to get the type provider from
        /// </summary>
        private readonly RuntimeGenerationContext _context;

        /// <summary>
        /// Gets the type provider which will be usd to check expression solvability
        /// </summary>
        private TypeProvider TypeProvider
        {
            get { return _context.TypeProvider; }
        }

        /// <summary>
        /// The type provider which will be used to perform the operations on the constants
        /// </summary>
        private readonly TypeOperationProvider _typeOperationProvider;

        /// <summary>
        /// Initializes a new instance of the ExpressionConstantResolver class
        /// </summary>
        /// <param name="context">The runtime generation context to get the type provider from</param>
        /// <param name="typeOperationProvider">A type provider which will be used to perform the operations on the constants</param>
        public ExpressionConstantResolver(RuntimeGenerationContext context, TypeOperationProvider typeOperationProvider)
        {
            _context = context;
            _typeOperationProvider = typeOperationProvider;
        }

        /// <summary>
        /// Expands the constants on a given expression context
        /// </summary>
        /// <param name="context">The context containing the constants to expand</param>
        public void ExpandConstants(ZScriptParser.ExpressionContext context)
        {
            // Traverse the expression tree
            var walker = new ParseTreeWalker();

            // Do a pass to expand the constants
            walker.Walk(this, context);

            // Now resolve the tree root
            ResolveExpression(context);
        }

        /// <summary>
        /// Expands the constants on a given assignment expression context
        /// </summary>
        /// <param name="context">The context containing the constants to expand</param>
        public void ExpandConstants(ZScriptParser.AssignmentExpressionContext context)
        {
            // Traverse the expression tree
            var walker = new ParseTreeWalker();

            // Do a pass to expand the constants
            walker.Walk(this, context);

            // Now resolve the tree too
            ResolveAssignmentExpression(context);
        }

        // 
        // ZScriptBaseListener.EnterExpression implementation
        // 
        public override void EnterExpression(ZScriptParser.ExpressionContext context)
        {
            if (context.constantAtom() != null && context.objectAccess() == null)
            {
                // Get the value of the constant atom
                var value = ConstantAtomParser.ParseConstantAtom(context.constantAtom());

                // Verify if any implicit casts are in place
                if (context.ImplicitCastType != null && !context.ImplicitCastType.IsAny && TypeProvider.CanImplicitCast(context.ImplicitCastType, context.EvaluatedType))
                {
                    // TODO: Deal with native types that are not present
                    var nativeType = TypeProvider.NativeTypeForTypeDef(context.ImplicitCastType);

                    if(nativeType != null)
                        value = TypeProvider.CastObject(value, nativeType);
                }

                context.IsConstant = true;
                context.IsConstantPrimitive = IsValuePrimitive(value);
                context.ConstantValue = value;
            }
        }

        /// <summary>
        /// Resolves the expression contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the expression to resolve</param>
        void ResolveExpression(ZScriptParser.ExpressionContext context)
        {
            // No need to analyze an expression that was already marked as constant
            if (!context.IsConstant)
            {
                if (context.expression().Length == 2)
                {
                    ResolveBinaryExpression(context);
                }
                if (context.unaryOperator() != null)
                {
                    ResolveUnaryExpression(context);
                }
                if (context.arrayLiteral() != null)
                {
                    ResolveArrayLiteral(context.arrayLiteral());
                }
                if (context.memberName() != null)
                {
                    ResolveMemberNameExpression(context);
                }
            }

            // Verify if any implicit casts are in place
            if (context.IsConstant && context.ImplicitCastType != null && !context.ImplicitCastType.IsAny)
            {
                var nativeType = TypeProvider.NativeTypeForTypeDef(context.ImplicitCastType);

                if (TypeProvider.CanImplicitCast(context.EvaluatedType, context.ImplicitCastType) && nativeType != null)
                    context.ConstantValue = TypeProvider.CastObject(context.ConstantValue, TypeProvider.NativeTypeForTypeDef(context.ImplicitCastType));
            }
        }

        /// <summary>
        /// Resolves the assignment expression contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the expression to resolve</param>
        void ResolveAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            if(context.expression() != null)
                ResolveExpression(context.expression());
            else
                ResolveAssignmentExpression(context.assignmentExpression());
        }

        /// <summary>
        /// Resolves the unary expression contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the expression to resolve</param>
        void ResolveUnaryExpression(ZScriptParser.ExpressionContext context)
        {
            // Find out which unary operator it is
            var inst = TokenFactory.InstructionForUnaryOperator(ExpressionUtils.OperatorOnExpression(context));

            var exp1 = context.expression(0);

            // Resosolve the operand
            ResolveExpression(exp1);

            if (exp1.IsConstant && exp1.EvaluatedType != null && TypeProvider.BinaryExpressionProvider.CanUnary(exp1.EvaluatedType, inst))
            {
                var value = PerformUnaryExpression(exp1.ConstantValue, inst, _typeOperationProvider);

                context.ConstantValue = value;
                context.IsConstant = true;
                context.IsConstantPrimitive = IsValuePrimitive(value);
            }
        }

        /// <summary>
        /// Resolves the binary expression contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the expression to resolve</param>
        void ResolveBinaryExpression(ZScriptParser.ExpressionContext context)
        {
            var exp1 = context.expression(0);
            var exp2 = context.expression(1);

            // Resolve the two expressions
            ResolveExpression(exp1);
            ResolveExpression(exp2);

            // Propagate the constant value now
            if (exp1.IsConstant && exp2.IsConstant && exp1.EvaluatedType != null && exp2.EvaluatedType != null)
            {
                object expV1 = exp1.ConstantValue;
                object expV2 = exp2.ConstantValue;

                var inst = TokenFactory.InstructionForOperator(ExpressionUtils.OperatorOnExpression(context));

                // Check the possibility of performing an operation on the two types with the type operator
                if (TypeProvider.BinaryExpressionProvider.CanPerformOperation(inst, exp1.EvaluatedType, exp2.EvaluatedType))
                {
                    object value = PerformBinaryExpression(expV1, expV2, inst, _typeOperationProvider);

                    context.ConstantValue = value;

                    context.IsConstantPrimitive = IsValuePrimitive(value);
                    context.IsConstant = true;
                }
            }
        }

        /// <summary>
        /// Resolves the array literal contained within a given expression context
        /// </summary>
        /// <param name="context">The context to analyze</param>
        void ResolveArrayLiteral(ZScriptParser.ArrayLiteralContext context)
        {
            if (context.expressionList() == null)
                return;

            // Setup the contents of the expression list
            foreach (var expression in context.expressionList().expression())
            {
                ResolveExpression(expression);
            }
        }

        /// <summary>
        /// Resolves a context that contains a member name assigned to it
        /// </summary>
        /// <param name="context">The member name context to analyze</param>
        /// <returns>true if the member name resolved to a valid constant; false otherwise</returns>
        void ResolveMemberNameExpression(ZScriptParser.ExpressionContext context)
        {
            object value;

            if (context.memberName() == null || context.valueAccess() != null || !ResolveMemberName(context.memberName(), out value))
                return;

            context.IsConstant = true;
            context.IsConstantPrimitive = IsValuePrimitive(value);
            context.ConstantValue = value;
        }

        /// <summary>
        /// Resolves a member name context, searching for the definition that is specified by the context,
        /// and trying to resolve a constant value, in case it is a constant variable definition.
        /// The method returns a boolean specifying whether the constant value was evaluated successfully,
        /// as well as returning the value in the out parameter 'value'. It gets set to null, if no constant
        /// was resolved from the member name context, though
        /// </summary>
        /// <param name="context">The member name context to analyze</param>
        /// <param name="value">The value the member name was resolved to</param>
        /// <returns>true if the member name resolved to a valid constant; false otherwise</returns>
        bool ResolveMemberName(ZScriptParser.MemberNameContext context, out object value)
        {
            var valueHolderDef = context.Definition as ValueHolderDefinition;
            if (context.HasDefinition && valueHolderDef != null && valueHolderDef.IsConstant && valueHolderDef.HasValue)
            {
                ExpandConstants(valueHolderDef.ValueExpression.ExpressionContext);

                if (valueHolderDef.ValueExpression.ExpressionContext.IsConstant)
                {
                    value = valueHolderDef.ValueExpression.ExpressionContext.ConstantValue;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Performs a binary operation specified by a given VM instruction between two operands on a given type operation provider.
        /// If the instruction does not describes an operation, an exception is raised
        /// </summary>
        /// <param name="operand1">The first operand to operate on</param>
        /// <param name="operand2">The second operand to operate on</param>
        /// <param name="instruction">The instruction containing the operation to perform</param>
        /// <param name="typeOperationProvider">A type provider to perform the operations on</param>
        /// <returns>The result of the given binary operation on the two operands</returns>
        /// <exception cref="ArgumentException">The provided instruction does not describes a valid binary operation</exception>
        private object PerformBinaryExpression(object operand1, object operand2, VmInstruction instruction, TypeOperationProvider typeOperationProvider)
        {
            object ret;

            // Evaluate the operation
            switch (instruction)
            {
                // Sum and subtraction
                case VmInstruction.Add:
                    ret = typeOperationProvider.Sum(operand1, operand2);
                    break;
                case VmInstruction.Subtract:
                    ret = typeOperationProvider.Subtract(operand1, operand2);
                    break;
                // Multiplication and division
                case VmInstruction.Multiply:
                    ret = typeOperationProvider.Multiply(operand1, operand2);
                    break;
                case VmInstruction.Divide:
                    ret = typeOperationProvider.Divide(operand1, operand2);
                    break;
                // Modulo operator
                case VmInstruction.Modulo:
                    ret = typeOperationProvider.Modulo(operand1, operand2);
                    break;
                // Bitwise operators
                case VmInstruction.BitwiseAnd:
                    ret = typeOperationProvider.BitwiseAnd(operand1, operand2);
                    break;
                case VmInstruction.BitwiseOr:
                    ret = typeOperationProvider.BitwiseOr(operand1, operand2);
                    break;
                case VmInstruction.BitwiseXOr:
                    ret = typeOperationProvider.BitwiseXOr(operand1, operand2);
                    break;
                // Equality/Inequality checks
                case VmInstruction.Equals:
                    ret = typeOperationProvider.Equals(operand1, operand2);
                    break;
                case VmInstruction.Unequals:
                    ret = !typeOperationProvider.Equals(operand1, operand2);
                    break;
                case VmInstruction.Less:
                    ret = typeOperationProvider.Less(operand1, operand2);
                    break;
                case VmInstruction.LessOrEquals:
                    ret = typeOperationProvider.LessOrEquals(operand1, operand2);
                    break;
                case VmInstruction.Greater:
                    ret = typeOperationProvider.Greater(operand1, operand2);
                    break;
                case VmInstruction.GreaterOrEquals:
                    ret = typeOperationProvider.GreaterOrEquals(operand1, operand2);
                    break;
                case VmInstruction.LogicalAnd:
                    ret = ((bool)operand1 && (bool)operand2);
                    break;
                case VmInstruction.LogicalOr:
                    ret = ((bool)operand1 || (bool)operand2);
                    break;
                default:
                    throw new ArgumentException("The VM instruction " + instruction + " cannot be used as a valid binary operator");
            }

            return ret;
        }

        /// <summary>
        /// Performs an unary operation specified by a given VM instruction on an operand on a given type operation provider.
        /// If the instruction does not describes an operation, an exception is raised
        /// </summary>
        /// <param name="operand">The operand to perform the unary operation on</param>
        /// <param name="instruction">The instruction containing the operation to perform</param>
        /// <param name="typeOperationProvider">A type provider to perform the operations on</param>
        /// <returns>The result of the given unary operation on the operand</returns>
        /// <exception cref="ArgumentException">The provided instruction does not describes a valid unary operation</exception>
        private object PerformUnaryExpression(object operand, VmInstruction instruction, TypeOperationProvider typeOperationProvider)
        {
            object ret;

            // Evaluate the operation
            switch (instruction)
            {
                // Sum and subtraction
                case VmInstruction.ArithmeticNegate:
                    ret = typeOperationProvider.ArithmeticNegate(operand);
                    break;
                case VmInstruction.LogicalNegate:
                    ret = typeOperationProvider.LogicalNegate(operand);
                    break;
                default:
                    throw new ArgumentException("The VM instruction " + instruction + " cannot be used as a valid unary operator");
            }

            return ret;
        }

        /// <summary>
        /// Returns a boolean value stating whether a given value is a primitive type value
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>Whether the type is a primitive type value</returns>
        private bool IsValuePrimitive(object value)
        {
            if (value == null)
                return false;

            if (value is string || value is bool || value is int || value is long || value is float || value is double)
                return true;

            return false;
        }
    }
}