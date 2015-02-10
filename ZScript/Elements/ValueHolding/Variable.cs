namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a variable definition
    /// </summary>
    public class Variable : ValueHolder
    {
        /// <summary>
        /// Gets or sets whether this variable definition has a default value binded to it
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets or sets the default value for this variable definition
        /// </summary>
        public object DefaultValue { get; set; }
    }
}