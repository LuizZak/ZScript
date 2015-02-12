using System;

namespace ZScript.CodeGeneration.Elements.Typing
{
    /// <summary>
    /// Represents a list type definition
    /// </summary>
    public class ListTypeDef : TypeDef, IListTypeDef, IEquatable<ListTypeDef>
    {
        /// <summary>
        /// The type of items enclosed in this list type
        /// </summary>
        private readonly TypeDef _enclosingType;

        /// <summary>
        /// Gets the type of items enclosed in this list type
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return _enclosingType; }
        }

        /// <summary>
        /// Initializes a new isntance of the ListTypeDef class
        /// </summary>
        /// <param name="enclosingType">The type of items in this list type</param>
        public ListTypeDef(TypeDef enclosingType)
            : base("list")
        {
            _enclosingType = enclosingType;
        }

        /// <summary>
        /// Gets a string representation of this ListTypeDef
        /// </summary>
        /// <returns>A string representation of this ListTypeDef</returns>
        public override string ToString()
        {
            return "[" + _enclosingType + "]";
        }

        #region Equality members

        public bool Equals(ListTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(_enclosingType, other._enclosingType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListTypeDef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_enclosingType != null ? _enclosingType.GetHashCode() : 0);
            }
        }

        public static bool operator==(ListTypeDef left, ListTypeDef right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(ListTypeDef left, ListTypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}