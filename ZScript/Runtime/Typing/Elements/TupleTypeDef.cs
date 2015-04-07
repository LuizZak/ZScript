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
using System.Linq;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents a tuple which encloses multiple values within
    /// </summary>
    public class TupleTypeDef : TypeDef, IEquatable<TupleTypeDef>
    {
        /// <summary>
        /// Gets the inner types for the tuple
        /// </summary>
        public TypeDef[] InnerTypes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TupleTypeDef class
        /// </summary>
        /// <param name="innerTypes">The inner types for the tuple</param>
        public TupleTypeDef(params TypeDef[] innerTypes)
            : base("(" + string.Join(",", (IEnumerable<object>)innerTypes) + ")", false)
        {
            InnerTypes = innerTypes;

            for (int i = 0; i < innerTypes.Length; i++)
            {
                AddField(new TypeFieldDef(i.ToString(), innerTypes[i], false));
            }
        }

        /// <summary>
        /// Initializes a new instance of the TupleTypeDef class
        /// </summary>
        /// <param name="innerTypeNames">An array of names for the inner types</param>
        /// <param name="innerTypes">The inner types for the tuple</param>
        public TupleTypeDef(IReadOnlyList<string> innerTypeNames, TypeDef[] innerTypes)
            : base("(" + string.Join(",", (IEnumerable<object>)innerTypes) + ")", false)
        {
            InnerTypes = innerTypes;

            for (int i = 0; i < innerTypeNames.Count; i++)
            {
                string innerName = innerTypeNames[i] ?? i.ToString();
                AddField(new TypeFieldDef(innerName, innerTypes[i], false));
            }
        }

        #region Equality members

        public bool Equals(TupleTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && InnerTypes.SequenceEqual(other.InnerTypes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TupleTypeDef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (InnerTypes != null ? InnerTypes.GetHashCode() : 0);
            }
        }

        public static bool operator==(TupleTypeDef left, TupleTypeDef right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(TupleTypeDef left, TupleTypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}