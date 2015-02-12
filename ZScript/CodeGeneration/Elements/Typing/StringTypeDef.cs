using System;

namespace ZScript.CodeGeneration.Elements.Typing
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
        /// Initilaizes a new instance of the StringTypeDef class
        /// </summary>
        public StringTypeDef() : base("string")
        {

        }
    }
}