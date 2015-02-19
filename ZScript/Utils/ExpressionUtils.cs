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

using ZScript.Elements;

namespace ZScript.Utils
{
    public class ExpressionUtils
    {
        /// <summary>
        /// Returns the unary, arithmetic or logical operator on a given expression context.
        /// Returns an empty string if no operator is found
        /// </summary>
        /// <param name="context">The context containing the operator</param>
        /// <returns>The string that represents the operator</returns>
        public static string OperatorOnExpression(ZScriptParser.ExpressionContext context)
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
            else if (context.bitwiseAndOp() != null)
            {
                str = context.bitwiseAndOp().GetText();
            }
            else if (context.bitwiseXOrOp() != null)
            {
                str = context.bitwiseXOrOp().GetText();
            }
            else if (context.bitwiseOrOp() != null)
            {
                str = context.bitwiseOrOp().GetText();
            }
            else if (context.bitwiseShift() != null)
            {
                str = context.bitwiseShift().GetText();
            }
            else if (context.T_IS() != null)
            {
                str = context.T_IS().GetText();
            }
            else if (context.relationalOp() != null)
            {
                str = context.relationalOp().GetText();
            }
            else if (context.equalityOp() != null)
            {
                str = context.equalityOp().GetText();
            }
            else if (context.logicalAnd() != null)
            {
                str = context.logicalAnd().GetText();
            }
            else if (context.logicalOr() != null)
            {
                str = context.logicalOr().GetText();
            }
            else if (context.unaryOperator() != null)
            {
                str = context.unaryOperator().GetText();
            }

            return str;
        }

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

            string str = context.GetText();

            // Return the first character of the operator
            return TokenFactory.CreateOperatorToken(str.Substring(0, str.Length - 1));
        }
    }
}