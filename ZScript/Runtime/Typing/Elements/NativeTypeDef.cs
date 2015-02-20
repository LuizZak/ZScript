using System;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents a type definition that has a direct native equivalent
    /// </summary>
    public class NativeTypeDef : TypeDef
    {
        /// <summary>
        /// The type this native type is equivalent of
        /// </summary>
        protected Type nativeType;

        /// <summary>
        /// Gets the type this native type is equivalent of
        /// </summary>
        public Type NativeType
        {
            get { return nativeType; }
        }

        /// <summary>
        /// Initializes a new instance of the NativeTypeDef class
        /// </summary>
        /// <param name="type">The type to create this native type out of</param>
        public NativeTypeDef(Type type) : base(type.Name, true)
        {
            nativeType = type;
        }

        /// <summary>
        /// Initializes a new instance of the NativeTypeDef class
        /// </summary>
        /// <param name="type">The type to create this native type out of</param>
        /// <param name="name">The usage name for the type</param>
        public NativeTypeDef(Type type, string name)
            : base(name, true)
        {
            nativeType = type;
        }
    }
}