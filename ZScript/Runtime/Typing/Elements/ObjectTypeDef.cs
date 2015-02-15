namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies the type for a ZObject
    /// </summary>
    public class ObjectTypeDef : TypeDef, IListTypeDef
    {
        /// <summary>
        /// Gets the type of objects subscript on this ObjectTypeDef
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return AnyType; }
        }

        /// <summary>
        /// Gets the type accepted by the subscript of the ObjectTypeDef
        /// </summary>
        public TypeDef SubscriptType
        {
            get { return StringType; }
        }

        /// <summary>
        /// Initializes a new instance of the ObjectTypeDef class
        /// </summary>
        public ObjectTypeDef() : base("object")
        {

        }
    }
}