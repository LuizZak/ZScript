using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Represents a custom type source for a type provider
    /// </summary>
    public interface ICustomTypeSource
    {
        /// <summary>
        /// Returns whether this custom type source contains a type with a specified name
        /// </summary>
        /// <param name="typeName">The name to search the custom type</param>
        /// <returns>true if this ICustomTypeSource contains the given type name; false otherwise</returns>
        bool HasType(string typeName);

        /// <summary>
        /// Returns a custom type on this custom type source with the given name; or null, if none exist
        /// </summary>
        /// <param name="typeName">The name to search the custom type</param>
        /// <returns>A custom type from this ICustomTypeSource that matches the given name; or null, if none exist</returns>
        TypeDef TypeNamed(string typeName);
    }
}