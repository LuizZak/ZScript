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

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents an optional type in the typing system
    /// </summary>
    public class OptionalTypeDef : TypeDef, IEquatable<OptionalTypeDef>, IOptionalTypeDef
    {
        /// <summary>
        /// The type wrapped in this optional type def
        /// </summary>
        private readonly TypeDef _wrappedType;

        /// <summary>
        /// Gets the type wrapped in this optional type def
        /// </summary>
        public TypeDef WrappedType => _wrappedType;

        /// <summary>
        /// Gets the type wrapped in this optional type def, unwrapping any wrapped optional type recursively
        /// </summary>
        public TypeDef BaseWrappedType
        {
            get
            {
                var def = WrappedType as OptionalTypeDef;
                if (def != null)
                    return def.BaseWrappedType;

                return WrappedType;
            }
        }

        /// <summary>
        /// Gets a positive integer that specifies the depth of this optional type.
        /// The depth corresponds to the number of types until the first non-optional type in the wrapped type chain.
        /// If the WrappedType is not an optional type definition, 0 is returned
        /// </summary>
        public int OptionalDepth
        {
            get
            {
                var def = WrappedType as OptionalTypeDef;
                if (def != null)
                    return def.OptionalDepth + 1;
                
                return 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the OptionalTypeDef class
        /// </summary>
        /// <param name="wrappedType">The type wrapped in this optional type def</param>
        public OptionalTypeDef(TypeDef wrappedType) : base("optional<" + wrappedType.Name + ">")
        {
            _wrappedType = wrappedType;
        }

        /// <summary>
        /// Gets an assembly friendly display name for this type definition
        /// </summary>
        /// <returns>A string that can be used as an assembly-friendly name for this type definition</returns>
        public override string AssemblyFriendlyName()
        {
            return "optional_" + WrappedType.AssemblyFriendlyName();
        }

        /// <summary>
        /// Gets a string representation of this OptionalTypeDef
        /// </summary>
        /// <returns>A string representation of this OptionalTypeDef</returns>
        public override string ToString()
        {
            return _wrappedType + "?";
        }

        #region Equality members

        public bool Equals(OptionalTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(_wrappedType, other._wrappedType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OptionalTypeDef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_wrappedType?.GetHashCode() ?? 0);
            }
        }

        public static bool operator==(OptionalTypeDef left, OptionalTypeDef right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(OptionalTypeDef left, OptionalTypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    /// <summary>
    /// Interface to be implemented by optional-type type classes
    /// </summary>
    public interface IOptionalTypeDef : ITypeDef
    {
        /// <summary>
        /// Gets the type wrapped in this optional type def
        /// </summary>
        TypeDef WrappedType { get; }
    }
}