namespace ZScript.CodeGeneration.Elements.Typing
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
        /// Initializes a new instance of the ObjectTypeDef class
        /// </summary>
        public ObjectTypeDef() : base("object")
        {
        }
    }
}