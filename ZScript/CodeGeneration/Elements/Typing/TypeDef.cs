using System;

namespace ZScript.CodeGeneration.Elements.Typing
{
    /// <summary>
    /// Specifies a type definition
    /// </summary>
    public class TypeDef : IEquatable<TypeDef>
    {
        /// <summary>
        /// The name for this type
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Whether this type definition represents the 'any' type
        /// </summary>
        private readonly bool _isAny;

        /// <summary>
        /// Whether this type definition represents the 'void' type
        /// </summary>
        private readonly bool _isVoid;

        /// <summary>
        /// Gets the name for this type
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'any' type
        /// </summary>
        public bool IsAny
        {
            get { return _isAny; }
        }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'void' type
        /// </summary>
        public bool IsVoid
        {
            get { return _isVoid; }
        }

        /// <summary>
        /// Initializes a new instance of the TypeDef class
        /// </summary>
        /// <param name="name">The name for the type</param>
        public TypeDef(string name)
        {
            _name = name;
            _isAny = name == "any";
            _isVoid = name == "void";
        }

        /// <summary>
        /// Gets a string representation of this TypeDef
        /// </summary>
        /// <returns>A string representation of this TypeDef</returns>
        public override string ToString()
        {
            return _name;
        }

        #region Equality members

        /// <summary>
        /// Returns a value specifying whether this TypeDef instance equals a given TypeDef instance
        /// </summary>
        /// <param name="other">The TypeDef to test against</param>
        /// <returns>true if this TypeDef equals the other TypeDef, false otherwise</returns>
        public bool Equals(TypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _isVoid.Equals(other._isVoid) && _isAny.Equals(other._isAny) && string.Equals(_name, other._name);
        }

        /// <summary>
        /// Returns a value specifying whether this TypeDef instance equals a given object
        /// </summary>
        /// <param name="obj">The object to test against</param>
        /// <returns>true if this TypeDef equals the passed object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeDef)obj);
        }

        /// <summary>
        /// Gets the hashcode for this TypeDef instance
        /// </summary>
        /// <returns>The hashcode for this TypeDef instance</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _isVoid.GetHashCode();
                hashCode = (hashCode * 397) ^ _isAny.GetHashCode();
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Compares two TypeDef instances for equality
        /// </summary>
        public static bool operator==(TypeDef left, TypeDef right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two TypeDef instances for inequality
        /// </summary>
        public static bool operator!=(TypeDef left, TypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// The type definition that represents the 'any' type
        /// </summary>
        public static readonly TypeDef AnyType = new TypeDef("any");

        /// <summary>
        /// The type definition that represents the 'void' type
        /// </summary>
        public static readonly TypeDef VoidType = new TypeDef("void");

        /// <summary>
        /// The type definition for an integer type in the runtime.
        /// The integer type is defined as an Int64 integer in the C# runtime
        /// </summary>
        public static readonly TypeDef IntegerType = new TypeDef("int");

        /// <summary>
        /// The type definition for a floating-point type in the runtime.
        /// The integer type is defined as a double floating point in the C# runtime
        /// </summary>
        public static readonly TypeDef FloatType = new TypeDef("float");

        /// <summary>
        /// The type definition for a null type in the runtime.
        /// The integer type is defined as a null in the C# runtime
        /// </summary>
        public static readonly TypeDef NullType = new TypeDef("null");

        /// <summary>
        /// The type definition for a boolean type in the runtime.
        /// The integer type is defined as a bool in the C# runtime
        /// </summary>
        public static readonly TypeDef BooleanType = new TypeDef("bool");

        /// <summary>
        /// The type definition for a string type in the runtime.
        /// The integer type is defined as a string in the C# runtime
        /// </summary>
        public static readonly StringTypeDef StringType = new StringTypeDef();
    }
}