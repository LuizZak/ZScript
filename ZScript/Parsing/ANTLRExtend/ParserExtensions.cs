using ZScript.Runtime.Typing.Elements;

public partial class ZScriptParser
{
    /// <summary>
    /// Provides extensions to the ExpressionContext for tree weighting and storaging of
    /// context-sensitive information like marking tree portion as constant valued
    /// </summary>
    public partial class ExpressionContext
    {
        /// <summary>
        /// Gets or sets a value specifying whether this expression context has been weighted already
        /// </summary>
        public bool Weighted { get; set; }

        /// <summary>
        /// Gets or sets the weigth of this expression context.
        /// The weigth of the context depends on the weigth of child contexts, evaluating from bottom-top (leaf to root)
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the contents of this expression tree are constant in nature (e.g. they can be replaced by one single value
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the constant stored in this expression context is a primitive type.
        /// Primitive types include ints, floats, strings and bools
        /// </summary>
        public bool IsConstantPrimitive { get; set; }

        /// <summary>
        /// Gets or sets the value for the evaluated constant for this expression context
        /// </summary>
        public object ConstantValue { get; set; }

        /// <summary>
        /// Gets or sets the evaluated type associated with this expression context
        /// </summary>
        public TypeDef EvaluatedType { get; set; }
    }

    /// <summary>
    /// Provides extensions to the AssignmentExpressionContext for type definition storaging
    /// </summary>
    partial class AssignmentExpressionContext
    {
        /// <summary>
        /// Gets or sets the evaluated type associated with this assignment expression context
        /// </summary>
        public TypeDef EvaluatedType { get; set; }
    }

    /// <summary>
    /// Provides extensions to the ClosureExpressionContext for type inferring helping
    /// </summary>
    partial class ClosureExpressionContext
    {
        /// <summary>
        /// The type that was inferred to this ClosureExpressionContext while being processed by an ExpressionTypeResolver
        /// </summary>
        public TypeDef InferredType { get; set; }
    }
}