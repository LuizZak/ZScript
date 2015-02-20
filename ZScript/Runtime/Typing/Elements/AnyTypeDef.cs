namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents a type that can be used to hold any value
    /// </summary>
    public class AnyTypeDef : TypeDef
    {
        /// <summary>
        /// Initializes a new instance of the 'any' type
        /// </summary>
        public AnyTypeDef() : base("any", false)
        {

        }
    }
}