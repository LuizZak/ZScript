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
using System.Collections.Generic;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a type that defines a dictionary, that is, a set of values mapped to keys
    /// </summary>
    public class DictionaryTypeDef : NativeTypeDef, IListTypeDef, IEquatable<DictionaryTypeDef>
    {
        /// <summary>
        /// The types of values in the dictionary
        /// </summary>
        private readonly TypeDef _keyType;

        /// <summary>
        /// The types of keys in the dictionary
        /// </summary>
        private readonly TypeDef _valueType;

        /// <summary>
        /// Gets the type of the values enclosed in this dictionary
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// Gets the type of the keys that map in this dictionary
        /// </summary>
        public TypeDef SubscriptType
        {
            get { return _keyType; }
        }

        /// <summary>
        /// Initializes a new instance of the DictionaryTypeDef class
        /// </summary>
        /// <param name="keyType">The types of values in the dictionary</param>
        /// <param name="valueType">The types of keys in the dictionary</param>
        public DictionaryTypeDef(TypeDef keyType, TypeDef valueType)
            : base(typeof(Dictionary<,>))
        {
            _keyType = keyType;
            _valueType = valueType;
        }

        /// <summary>
        /// Gets a string representation of this DictionaryTypeDef
        /// </summary>
        /// <returns>A string representation of this DictionaryTypeDef</returns>
        public override string ToString()
        {
            return "[" + _keyType + ":" + _valueType + "]";
        }

        #region Equality members

        public bool Equals(DictionaryTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(_valueType, other._valueType) && Equals(_keyType, other._keyType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DictionaryTypeDef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_valueType != null ? _valueType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_keyType != null ? _keyType.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator==(DictionaryTypeDef left, DictionaryTypeDef right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(DictionaryTypeDef left, DictionaryTypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}