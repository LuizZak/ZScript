using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a value holder definition
    /// </summary>
    public class ValueHolderDefinition : Definition
    {
        /// <summary>
        /// Gets or sets a value that represents the expression containing the value for this variable definition
        /// </summary>
        public Expression ValueExpression { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this variable has a expression specifying its value
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this value holder definition has a type associated with it.
        /// If the value is false, the value holder has a type infered from the value expression
        /// </summary>
        public bool HasType { get; set; }

        /// <summary>
        /// Gets or sets the type associated with this value holder definition
        /// </summary>
        public TypeDef Type { get; set; }

        /// <summary>
        /// Gets or sets the context for the type of this value holder definition
        /// </summary>
        public ZScriptParser.TypeContext TypeContext { get; set; }

        /// <summary>
        /// Whether this value holder is constant
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Whether this value holder is an instance value
        /// </summary>
        public bool IsInstanceValue;
    }
}