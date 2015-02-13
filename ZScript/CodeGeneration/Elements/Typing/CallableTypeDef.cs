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
        /// The information for the parameters of this callable type definition
        /// </summary>
        private readonly CallableArgumentInfo[] _parameterInfos;

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
        /// Gets the information for the parameters of this callable type definition
        /// </summary>
        public CallableArgumentInfo[] ParameterInfos
        {
            get { return _parameterInfos; }
        }

        /// <summary>
        /// Gets the count of arguments required by this callable type definition
        /// </summary>
        public int RequiredCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < _parameterInfos.Length; i++)
                {
                    if (!_parameterInfos[i].HasDefault)
                        c++;
                }

                return c;
            }
        }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        public TypeDef ReturnType
        {
            get { return _returnType; }
        }

        /// <summary>
        /// Initializes a new instance of the CallableTypeDef with parameter information and return type provided
        /// </summary>
        /// <param name="parameterInfos">The type for the callable's parameter</param>
        /// <param name="returnType">The return type for this callable type definition</param>
        public CallableTypeDef(CallableArgumentInfo[] parameterInfos, TypeDef returnType)
            : base("callable")
        {
            _parameterInfos = parameterInfos;
            _parameterTypes = parameterInfos.Select(i => i.ArgumentType).ToArray();

            _returnType = returnType;
        }

        /*
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
        */

        /// <summary>
        /// Gets a string representation of this CallableTypeDEf
        /// </summary>
        /// <returns>A string representation of this CallableTypeDef</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(");

            bool first = true;
            foreach (var info in _parameterInfos)
            {
                if (!first)
                {
                    builder.Append(",");
                }
                first = false;

                builder.Append(info.ArgumentType);

                if (info.HasDefault)
                    builder.Append("*");

                if (info.IsVariadic)
                    builder.Append("...");
            }
            
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
            return base.Equals(other) && _parameterInfos.SequenceEqual(other._parameterInfos) && Equals(_returnType, other._returnType);
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
                hashCode = (hashCode * 397) ^ (_parameterInfos != null ? _parameterInfos.GetHashCode() : 0);
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

        /// <summary>
        /// Encapsulates information about a callable argument's typing
        /// </summary>
        public class CallableArgumentInfo : IEquatable<CallableArgumentInfo>
        {
            /// <summary>
            /// The type for this argument
            /// </summary>
            private readonly TypeDef _argumentType;

            /// <summary>
            /// Whether this argument is variadic in nature, allowing acceptance of any number of arguments
            /// </summary>
            private readonly bool _isVariadic;

            /// <summary>
            /// Whether the type for this argument was provided
            /// </summary>
            private readonly bool _hasType;

            /// <summary>
            /// Whether the argument has a default value associated with it
            /// </summary>
            private readonly bool _hasDefault;

            /// <summary>
            /// Gets the type for this argument
            /// </summary>
            public TypeDef ArgumentType
            {
                get { return _argumentType; }
            }

            /// <summary>
            /// Whether the argument is variadic in nature, allowing acceptance of any number of arguments
            /// </summary>
            public bool IsVariadic
            {
                get { return _isVariadic; }
            }

            /// <summary>
            /// Whether the type for the argument was provided
            /// </summary>
            public bool HasType
            {
                get { return _hasType; }
            }

            /// <summary>
            /// Gets a value specifying whether the argument has a default value associated with it
            /// </summary>
            public bool HasDefault
            {
                get { return _hasDefault; }
            }

            /// <summary>
            /// Initializes a new instance of the CallableArgumentInfo class
            /// </summary>
            /// <param name="argumentType">The type of the argument</param>
            /// <param name="hasType">Whether this argument has a type associated with it by the script source</param>
            /// <param name="hasDefault">Whether this argument has a default value</param>
            /// <param name="isVariadic">Whether this argument is variadic in nature</param>
            public CallableArgumentInfo(TypeDef argumentType, bool hasType, bool hasDefault, bool isVariadic)
            {
                _argumentType = argumentType;
                _hasType = hasType;
                _hasDefault = hasDefault;
                _isVariadic = isVariadic;
            }

            #region Equality members

            public bool Equals(CallableArgumentInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_argumentType, other._argumentType) && _isVariadic.Equals(other._isVariadic) && _hasType.Equals(other._hasType) && _hasDefault.Equals(other._hasDefault);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((CallableArgumentInfo)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (_argumentType != null ? _argumentType.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ _isVariadic.GetHashCode();
                    hashCode = (hashCode * 397) ^ _hasType.GetHashCode();
                    hashCode = (hashCode * 397) ^ _hasDefault.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator==(CallableArgumentInfo left, CallableArgumentInfo right)
            {
                return Equals(left, right);
            }

            public static bool operator!=(CallableArgumentInfo left, CallableArgumentInfo right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    }
}