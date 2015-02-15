namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a string primitive type definition
    /// </summary>
    public class StringTypeDef : TypeDef, IListTypeDef
    {
        /// <summary>
        /// Returns the type enclosed within the string type definition
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return AnyType; }
        }

        /// <summary>
        /// Gets the type of objects accepted by the subscript of the string type
        /// </summary>
        public TypeDef SubscriptType
        {
            get { return IntegerType; }
        }

        /// <summary>
        /// Initilaizes a new instance of the StringTypeDef class
        /// </summary>
        public StringTypeDef() : base("string")
        {

        }
    }
}