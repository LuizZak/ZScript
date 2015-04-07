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
    /// Represents a list type definition
    /// </summary>
    public class ListTypeDef : NativeTypeDef, IListTypeDef, IEquatable<ListTypeDef>
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
        /// Gets or sets the type of object accepted by the subscript of the list
        /// </summary>
        public TypeDef SubscriptType { get; set; }

        /// <summary>
        /// Initializes a new isntance of the ListTypeDef class
        /// </summary>
        /// <param name="enclosingType">The type of items in this list type</param>
        public ListTypeDef(TypeDef enclosingType)
            : base(typeof(List<>), "list<" + enclosingType.Name + ">")
        {
            _enclosingType = enclosingType;
            // Default the subscript to integer
            SubscriptType = IntegerType;

            // Setup elements
            AddField(new TypeFieldDef("Count", new NativeTypeDef(typeof(int)), true));
        }

        /// <summary>
        /// Gets an assembly friendly display name for this type definition
        /// </summary>
        /// <returns>A string that can be used as an assembly-friendly name for this type definition</returns>
        public override string AssemblyFriendlyName()
        {
            return "list_" + EnclosingType.AssemblyFriendlyName();
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
            if (obj.GetType() != GetType()) return false;
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