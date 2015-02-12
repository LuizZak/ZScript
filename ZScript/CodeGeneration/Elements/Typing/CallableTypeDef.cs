using System;
using System.Linq;
using System.Text;

namespace ZScript.CodeGeneration.Elements.Typing
{
    /// <summary>
    /// Specifies a callable type definition
    /// </summary>
    public class CallableTypeDef : TypeDef, IEquatable<CallableTypeDef>
    {
        /// <summary>
        /// The types for the parameter of this callable type deifinition
        /// </summary>
        private readonly TypeDef[] _parameterTypes;

        /// <summary>
        /// The return type for this callable
        /// </summary>
        private readonly TypeDef _returnType;

        /// <summary>
        /// Gets the types for the parameters of this callable type definition
        /// </summary>
        public TypeDef[] ParameterTypes
        {
            get { return _parameterTypes; }
        }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        public TypeDef ReturnType
        {
            get { return _returnType; }
        }

        /// <summary>
        /// Initializes a new instance of the CallableTypeDef
        /// </summary>
        /// <param name="parameterTypes">The type for the parameters of this callable type definition</param>
        /// <param name="returnType">The return type for this callable type definition</param>
        public CallableTypeDef(TypeDef[] parameterTypes, TypeDef returnType)
            : base("callable")
        {
            _parameterTypes = parameterTypes;
            _returnType = returnType;
        }

        /// <summary>
        /// Gets a string representation of this CallableTypeDEf
        /// </summary>
        /// <returns>A string representation of this CallableTypeDef</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(");
            
            builder.Append(string.Join(",", (object[])_parameterTypes));
            
            builder.Append("->");

            builder.Append(_returnType);

            builder.Append(")");

            return builder.ToString();
        }

        #region Equality members

        public bool Equals(CallableTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && _parameterTypes.SequenceEqual(other._parameterTypes) && Equals(_returnType, other._returnType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CallableTypeDef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_parameterTypes != null ? _parameterTypes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_returnType != null ? _returnType.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator==(CallableTypeDef left, CallableTypeDef right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(CallableTypeDef left, CallableTypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}