namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a definition for an object that points to a value
    /// </summary>
    public class ValueHolder
    {
        /// <summary>
        /// Gets or sets the name for this value holder
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the value contained within this value holder definition
        /// </summary>
        public object Type { get; set; }
    }
}