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
using JetBrains.Annotations;
using ZScript.Runtime.Execution.Wrappers;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a callable type definition
    /// </summary>
    public class CallableTypeDef : NativeTypeDef, IEquatable<CallableTypeDef>, ICallableTypeDef
    {
        /// <summary>
        /// Gets the types for the parameters of this callable type definition
        /// </summary>
        public TypeDef[] ParameterTypes { get; }

        /// <summary>
        /// Gets a tuple that represents the function parameters for this callable type definition
        /// </summary>
        public TupleTypeDef ParameterTuple { get; }

        /// <summary>
        /// Gets the information for the parameters of this callable type definition
        /// </summary>
        public CallableParameterInfo[] ParameterInfos { get; }

        /// <summary>
        /// Gets the information for the variadic parameter of this callable type definition.
        /// The value returned is null, if no variadic parameter exists on this callable type def
        /// </summary>
        public CallableParameterInfo VariadicParameter
        {
            get
            {
                if (!HasVariadic)
                    return null;

                return ParameterInfos.First(p => p.IsVariadic);
            }
        }

        /// <summary>
        /// Gets the count of arguments required by this callable type definition
        /// </summary>
        public int RequiredArgumentsCount { get; }

        /// <summary>
        /// Gets the total count of arguments accepted by this callable type definition.
        /// If there is at least one variadic argument, the value returned is int.MaxValue
        /// </summary>
        public int MaximumArgumentsCount => HasVariadic ? int.MaxValue : ParameterInfos.Length;

        /// <summary>
        /// Gets a value specifying whether a return type has been provided
        /// </summary>
        public bool HasReturnType { get; }

        /// <summary>
        /// Gets the return type for this callable
        /// </summary>
        public TypeDef ReturnType { get; }

        /// <summary>
        /// Gets a value specifying whether any of the arguments for this callable type definition is variadic
        /// </summary>
        public bool HasVariadic { get; }

        /// <summary>
        /// Initializes a new instance of the CallableTypeDef with parameter information and return type provided
        /// </summary>
        /// <param name="parameterInfos">The type for the callable's parameter</param>
        /// <param name="returnType">The return type for this callable type definition</param>
        /// <param name="hasReturnType">Whether a return type was provided for this callable</param>
        public CallableTypeDef([NotNull] CallableParameterInfo[] parameterInfos, TypeDef returnType, bool hasReturnType)
            : base(typeof(ICallableWrapper), "callable")
        {
            ParameterInfos = parameterInfos;
            ParameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();

            HasVariadic = ParameterInfos.Any(i => i.IsVariadic);

            ReturnType = returnType;

            ParameterTuple = new TupleTypeDef(ParameterTypes) { IsLastVariadic = HasVariadic };
            HasReturnType = hasReturnType;

            // Count the numer of parameters required
            foreach (var pInfo in ParameterInfos)
            {
                if (!pInfo.HasDefault && !pInfo.IsVariadic)
                    RequiredArgumentsCount++;
            }
        }

        /// <summary>
        /// Gets a string representation of this CallableTypeDef
        /// </summary>
        /// <returns>A string representation of this CallableTypeDef</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            
            builder.Append("(");

            bool first = true;
            foreach (var info in ParameterInfos)
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

            builder.Append(ReturnType);

            builder.Append(")");

            return builder.ToString();
        }

        #region Equality members

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        public bool Equals(CallableTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ParameterInfos.SequenceEqual(other.ParameterInfos) && Equals(ReturnType, other.ReturnType);
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
                hashCode = (hashCode * 397) ^ (ParameterInfos?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ReturnType?.GetHashCode() ?? 0);
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

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        #endregion

        /// <summary>
        /// Encapsulates information about a callable parameter's typing
        /// </summary>
        public class CallableParameterInfo : IEquatable<CallableParameterInfo>
        {
            /// <summary>
            /// Gets the type for this parameter
            /// </summary>
            public TypeDef ParameterType { get; }

            /// <summary>
            /// Gets the raw parameter type, ignoring the variadic modifier
            /// </summary>
            public TypeDef RawParameterType { get; }

            /// <summary>
            /// Gets a value specifying whether the parameter is variadic in nature, allowing acceptance of any number of arguments
            /// </summary>
            public bool IsVariadic { get; }

            /// <summary>
            /// Gets a value specifying whether the type for the parameter was provided
            /// </summary>
            public bool HasType { get; }

            /// <summary>
            /// Gets a value specifying whether the parameter has a default value associated with it
            /// </summary>
            public bool HasDefault { get; }

            /// <summary>
            /// Gets the default value for the parameter
            /// </summary>
            public object DefaultValue { get; }

            /// <summary>
            /// Initializes a new instance of the CallableParameterInfo class
            /// </summary>
            /// <param name="parameterType">The type of the parameter</param>
            /// <param name="hasType">Whether this parameter has a type associated with it by the script source</param>
            /// <param name="hasDefault">Whether this parameter has a default value</param>
            /// <param name="isVariadic">Whether this parameter is variadic in nature</param>
            /// <param name="defaultValue">The default value for the parameter</param>
            public CallableParameterInfo(TypeDef parameterType, bool hasType, bool hasDefault, bool isVariadic, object defaultValue = null)
            {
                ParameterType = parameterType;
                HasType = hasType;
                HasDefault = hasDefault;
                IsVariadic = isVariadic;
                RawParameterType = ParameterType == null ? null : IsVariadic && ParameterType is ListTypeDef ? ((ListTypeDef)ParameterType).EnclosingType : ParameterType;
                DefaultValue = defaultValue;
            }

            #region Equality members

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

            public bool Equals(CallableParameterInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(ParameterType, other.ParameterType) && IsVariadic.Equals(other.IsVariadic) && HasType.Equals(other.HasType) && HasDefault.Equals(other.HasDefault) && Equals(DefaultValue, other.DefaultValue);
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
                    var hashCode = ParameterType?.GetHashCode() ?? 0;
                    hashCode = (hashCode * 397) ^ IsVariadic.GetHashCode();
                    hashCode = (hashCode * 397) ^ HasType.GetHashCode();
                    hashCode = (hashCode * 397) ^ HasDefault.GetHashCode();
                    hashCode = (hashCode * 397) ^ (DefaultValue?.GetHashCode() ?? 0);
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

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

            #endregion
        }
    }
}