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
using System.Text;
using JetBrains.Annotations;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Represents a tuple which encloses multiple values within
    /// </summary>
    public class TupleTypeDef : TypeDef, IEquatable<TupleTypeDef>
    {
        /// <summary>
        /// The signature for this tuple type def
        /// </summary>
        private ICallableTypeDef _initializerSignature;

        /// <summary>
        /// Gets the inner type labels for this tuple type definition.
        /// An entry being null means there is no label defined in the corresponding InnerType entry
        /// </summary>
        public string[] InnerTypeNames { get; }

        /// <summary>
        /// Gets the inner types for the tuple
        /// </summary>
        public TypeDef[] InnerTypes { get; }

        /// <summary>
        /// Gets or sets the variadic state of the last entry of the tuple.
        /// </summary>
        public bool IsLastVariadic { get; set; }

        /// <summary>
        /// Initializes a new instance of the TupleTypeDef class
        /// </summary>
        /// <param name="innerTypes">The inner types for the tuple</param>
        public TupleTypeDef([NotNull] params TypeDef[] innerTypes)
            : base("(" + string.Join(",", (IEnumerable<object>)innerTypes) + ")")
        {
            InnerTypes = innerTypes;
            InnerTypeNames = new string[innerTypes.Length];

            for (int i = 0; i < innerTypes.Length; i++)
            {
                InnerTypeNames[i] = i.ToString();
                AddField(new TypeFieldDef(InnerTypeNames[i], innerTypes[i], false));
            }
        }

        /// <summary>
        /// Initializes a new instance of the TupleTypeDef class
        /// </summary>
        /// <param name="innerTypeNames">An array of names for the inner types</param>
        /// <param name="innerTypes">The inner types for the tuple</param>
        public TupleTypeDef([NotNull] string[] innerTypeNames, [NotNull] TypeDef[] innerTypes)
            : base(CreateTupleName(innerTypeNames, innerTypes))
        {
            InnerTypes = innerTypes;
            InnerTypeNames = new string[innerTypes.Length];

            for (int i = 0; i < innerTypeNames.Length; i++)
            {
                string innerName = innerTypeNames[i] ?? i.ToString();
                InnerTypeNames[i] = innerName;
                AddField(new TypeFieldDef(innerName, innerTypes[i], false));
            }
        }

        /// <summary>
        /// Returns the positive index of a given label name on this tuple type def.
        /// Returns -1 when the index is not found
        /// </summary>
        /// <param name="labelName">The name of the label to search</param>
        /// <returns>The index of the label on the InnerTypeNames array, returns -1 if the label does not exists on this tuple</returns>
        public int IndexOfLabel(string labelName)
        {
            for (int i = 0; i < InnerTypeNames.Length; i++)
            {
                var typeName = InnerTypeNames[i];
                if (typeName == labelName)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets an assembly friendly display name for this type definition
        /// </summary>
        /// <returns>A string that can be used as an assembly-friendly name for this type definition</returns>
        public override string AssemblyFriendlyName()
        {
            return "tuple_" + string.Join("_", InnerTypes.Select(i => i.AssemblyFriendlyName()));
        }

        /// <summary>
        /// Gets the initializer signature for this TupleTypeDef, which describes the signature for the initializer call of literal tuples of this type
        /// </summary>
        /// <returns>The signature for this tuple type definition</returns>
        public ICallableTypeDef GetInitializerSignature()
        {
            if (_initializerSignature == null)
            {
                var returnType = this;
                var parameters = InnerTypes.Select(t => new CallableTypeDef.CallableParameterInfo(t, true, false, false)).ToArray();

                _initializerSignature = new CallableTypeDef(parameters, returnType, true);
            }

            return _initializerSignature;
        }

        #region Equality members

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        public override bool Equals(TypeDef other)
        {
            var tuple = other as TupleTypeDef;
            if (tuple == null)
                return false;

            return Equals(tuple);
        }

        public bool Equals(TupleTypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return InnerTypes.SequenceEqual(other.InnerTypes);
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
                int hash = 0;

                if (InnerTypes == null)
                    return hash;

                foreach (var type in InnerTypes)
                {
                    hash ^= (397 * type.GetHashCode());
                }

                return hash;
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

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        #endregion

        /// <summary>
        /// Creates and returns a string that represents a tuple with the given set of inner type names and inner types
        /// </summary>
        /// <param name="innerTypeNames">An array of names for the inner types</param>
        /// <param name="innerTypes">The inner types for the tuple</param>
        /// <returns>A representation of the tuple provided</returns>
        private static string CreateTupleName([NotNull] string[] innerTypeNames, TypeDef[] innerTypes)
        {
            var builder = new StringBuilder();
            
            builder.Append("(");

            for (int i = 0; i < innerTypeNames.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                if (innerTypeNames[i] != null)
                    builder.Append(innerTypeNames[i] + ": ");

                builder.Append(innerTypes[i]);
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}