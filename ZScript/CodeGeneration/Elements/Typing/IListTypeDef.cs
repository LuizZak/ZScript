namespace ZScript.CodeGeneration.Elements.Typing
{
    /// <summary>
    /// Interface to be implemented by types that enclose other values and can be subscripted like arrays
    /// </summary>
    public interface IListTypeDef
    {
        /// <summary>
        /// Gets the type of items enclosed in this list type
        /// </summary>
        TypeDef EnclosingType { get; }

        /// <summary>
        /// Gets the type of object that is accepted by the subscripting syntax of the list
        /// </summary>
        TypeDef SubscriptType { get; }
    }
}