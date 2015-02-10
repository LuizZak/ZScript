namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a type definition
    /// </summary>
    public class TypeDef
    {
        /// <summary>
        /// Gets or sets the name for this type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'any' type
        /// </summary>
        public bool IsAny { get; private set; }

        /// <summary>
        /// Gets a value specifying whther this type definition represents the 'void' type
        /// </summary>
        public bool IsVoid { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TypeDef class
        /// </summary>
        /// <param name="name">The name for the type</param>
        public TypeDef(string name)
        {
            Name = name;
            IsAny = name == "any";
            IsVoid = name == "void";
        }

        /// <summary>
        /// The type definition that represents the 'any' type
        /// </summary>
        public static TypeDef AnyType = new TypeDef("any");

        /// <summary>
        /// The type definition that represents the 'void' type
        /// </summary>
        public static TypeDef VoidType = new TypeDef("void");
    }
}