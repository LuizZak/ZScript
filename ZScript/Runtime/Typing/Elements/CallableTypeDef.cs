#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion
using System;
using System.Linq;
using System.Text;
using ZScript.Runtime.Execution.Wrappers;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a callable type definition
    /// </summary>
    public class CallableTypeDef : NativeTypeDef, IEquatable<CallableTypeDef>, ICallableTypeDef
    {
        /// <summary>
        /// The types for the parameter of this callable type definition
        /// </summary>
        private readonly TypeDef[] _parameterTypes;

        /// <summary>
        /// The information for the parameters of this callable type definition
        /// </summary>
        private readonly CallableParameterInfo[] _parameterInfos;

        /// <summary>
        /// The return type for this callable
        /// </summary>
        private readonly TypeDef _returnType;

        /// <summary>
        /// Whether any of the arguments for this callable type definition is variadic
        /// </summary>
        private readonly bool _hasVariadic;

        /// <summary>
        /// The count of parameters that are required to perform the call
        /// </summary>
        private readonly int _requiredCount;

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
        public CallableParameterInfo[] ParameterInfos
        {
            get { return _parameterInfos; }
        }

        /// <summary>
        /// Gets the information for the variadic parameter of this callable type definition.
        /// The value returned is null, if no variadic parameter exists on this callable type def
        /// </summary>
        public CallableParameterInfo VariadicParameter
        {
            get
            {
                if (!_hasVariadic)
                    return null;

                return ParameterInfos.First(p => p.IsVariadic);
            }
        }

        /// <summary>
        /// Gets the count of arguments required by this callable type definition
        /// </summary>
        public int RequiredArgumentsCount
        {
            get { return _requiredCount; }
        }

        /// <summary>
        /// Gets the total count of arguments accepted by this callable type definition.
        /// If there is at least one variadic argument, the value returned is int.MaxValue
        /// </summary>
        public int MaximumArgumentsCount
        {
            get { return HasVariadic ? int.MaxValue : _parameterInfos.Length; }
        }

        /// <summary>
        /// Gets a value specifying whether a return type has been provided
        /// </summary>
        public bool HasReturnType { get; private set; }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        public TypeDef ReturnType
        {
            get { return _returnType; }
        }

        /// <summary>
        /// Gets a value specifying whether any of the arguments for this callable type definition is variadic
        /// </summary>
        public bool HasVariadic
        {
            get { return _hasVariadic; }
        }

        /// <summary>
        /// Initializes a new instance of the CallableTypeDef with parameter information and return type provided
        /// </summary>
        /// <param name="parameterInfos">The type for the callable's parameter</param>
        /// <param name="returnType">The return type for this callable type definition</param>
        /// <param name="hasReturnType">Whether a return type was provided for this callable</param>
        public CallableTypeDef(CallableParameterInfo[] parameterInfos, TypeDef returnType, bool hasReturnType)
            : base(typeof(ICallableWrapper), "callable")
        {
            _parameterInfos = parameterInfos;
            _parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();

            _hasVariadic = _parameterInfos.Any(i => i.IsVariadic);

            _returnType = returnType;
            HasReturnType = hasReturnType;

            // Count the numer of parameters required
            foreach (var pInfo in _parameterInfos)
            {
                if (!pInfo.HasDefault && !pInfo.IsVariadic)
                    _requiredCount++;
            }
        }

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

                builder.Append(info.ParameterType);

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
        /// Encapsulates information about a callable parameter's typing
        /// </summary>
        public class CallableParameterInfo : IEquatable<CallableParameterInfo>
        {
            /// <summary>
            /// The type for this parameter
            /// </summary>
            private readonly TypeDef _parameterType;

            /// <summary>
            /// The raw parameter type, ignoring the variadic modifier
            /// </summary>
            private readonly TypeDef _rawParameterType;

            /// <summary>
            /// Whether this parameter is variadic in nature, allowing acceptance of any number of arguments
            /// </summary>
            private readonly bool _isVariadic;

            /// <summary>
            /// Whether the type for this parameter was provided
            /// </summary>
            private readonly bool _hasType;

            /// <summary>
            /// Whether the parameter has a default value associated with it
            /// </summary>
            private readonly bool _hasDefault;

            /// <summary>
            /// Gets the type for this parameter
            /// </summary>
            public TypeDef ParameterType
            {
                get { return _parameterType; }
            }

            /// <summary>
            /// Gets the raw parameter type, ignoring the variadic modifier
            /// </summary>
            public TypeDef RawParameterType
            {
                get { return _rawParameterType; }
            }

            /// <summary>
            /// Gets a value specifying whether the parameter is variadic in nature, allowing acceptance of any number of arguments
            /// </summary>
            public bool IsVariadic
            {
                get { return _isVariadic; }
            }

            /// <summary>
            /// Gets a value specifying whether the type for the parameter was provided
            /// </summary>
            public bool HasType
            {
                get { return _hasType; }
            }

            /// <summary>
            /// Gets a value specifying whether the parameter has a default value associated with it
            /// </summary>
            public bool HasDefault
            {
                get { return _hasDefault; }
            }

            /// <summary>
            /// Initializes a new instance of the CallableParameterInfo class
            /// </summary>
            /// <param name="parameterType">The type of the parameter</param>
            /// <param name="hasType">Whether this parameter has a type associated with it by the script source</param>
            /// <param name="hasDefault">Whether this parameter has a default value</param>
            /// <param name="isVariadic">Whether this parameter is variadic in nature</param>
            public CallableParameterInfo(TypeDef parameterType, bool hasType, bool hasDefault, bool isVariadic)
            {
                _parameterType = parameterType;
                _hasType = hasType;
                _hasDefault = hasDefault;
                _isVariadic = isVariadic;
                _rawParameterType = _parameterType == null ? null : _isVariadic && _parameterType is ListTypeDef ? ((ListTypeDef)_parameterType).EnclosingType : _parameterType;
            }

            #region Equality members

            public bool Equals(CallableParameterInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_parameterType, other._parameterType) && _isVariadic.Equals(other._isVariadic) && _hasType.Equals(other._hasType) && _hasDefault.Equals(other._hasDefault);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((CallableParameterInfo)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (_parameterType != null ? _parameterType.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ _isVariadic.GetHashCode();
                    hashCode = (hashCode * 397) ^ _hasType.GetHashCode();
                    hashCode = (hashCode * 397) ^ _hasDefault.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator==(CallableParameterInfo left, CallableParameterInfo right)
            {
                return Equals(left, right);
            }

            public static bool operator!=(CallableParameterInfo left, CallableParameterInfo right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    }
}