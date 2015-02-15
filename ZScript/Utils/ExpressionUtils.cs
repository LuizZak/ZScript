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
            else if (context.comparisionOp() != null)
            {
                str = context.comparisionOp().GetText();
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
    }
}