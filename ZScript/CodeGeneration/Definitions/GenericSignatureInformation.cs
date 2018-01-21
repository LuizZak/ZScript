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
using JetBrains.Annotations;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Represents definitions for generic parametrization of a function signature
    /// </summary>
    public class GenericSignatureInformation
    {
        /// <summary>
        /// Gets an array of generic types defined within this generic parameters definition
        /// </summary>
        public GenericTypeDefinition[] GenericTypes { get; }

        /// <summary>
        /// The constraints for this generic signature information
        /// </summary>
        public GenericTypeConstraint[] Constraints { get; }

        /// <summary>
        /// Initializes a new instance of the GenericParametersDefinition class with an empty list of generic types
        /// </summary>
        public GenericSignatureInformation()
            : this(new GenericTypeDefinition[0])
        {

        }

        /// <summary>
        /// Initializes a new instance of the GenericParametersDefinition class with a specified set of generic types
        /// </summary>
        public GenericSignatureInformation(GenericTypeDefinition[] types)
            : this(types, new GenericTypeConstraint[0])
        {

        }

        /// <summary>
        /// Initializes a new instance of the GenericParametersDefinition class with a specified set of generic types and type constraints
        /// </summary>
        /// <param name="genericTypes">The generic types for this generic parameters definition</param>
        /// <param name="constraints">The constraints for this generic signature</param>
        public GenericSignatureInformation(GenericTypeDefinition[] genericTypes, GenericTypeConstraint[] constraints)
        {
            GenericTypes = genericTypes;
            Constraints = constraints;
        }
    }

    /// <summary>
    /// Represents a generic type definition for a function signature
    /// </summary>
    public class GenericTypeDefinition : TypeDef, IInheritableTypeDef
    {
        /// <summary>
        /// Gets or sets the base type for this ClassTypeDef.
        /// If the value provided is a base type of this class, an ArgumentException is raised
        /// </summary>
        /// <exception cref="ArgumentException">The provided type value causes a circular inheritance chain</exception>
        public TypeDef BaseType
        {
            get => baseType;
            set => baseType = value;
        }

        /// <summary>
        /// Gets or sets the context this generic type definition was defined in
        /// </summary>
        public ZScriptParser.GenericTypeContext Context { get; set; }

        /// <summary>
        /// The concrete type for this generic type definition.
        /// 
        /// A generic type is considered Open until this parameter is non-null and points to a complete type, at which point
        /// it becomes a Closed (complete) generic type.
        /// </summary>
        [CanBeNull]
        public TypeDef ConcreteType { get; set; }

        /// <summary>
        /// Returns whether this generic type is complete.
        /// </summary>
        public bool IsComplete => ConcreteType != null;

        /// <summary>
        /// Initializes a new instance of the GenericTypeDefinition class with a generic name specified
        /// </summary>
        /// <param name="name">The name for the generic type</param>
        public GenericTypeDefinition(string name)
            : base(name)
        {
            
        }
    }

    /// <summary>
    /// Represents a generic type constraint from a generic signature information
    /// </summary>
    public readonly struct GenericTypeConstraint : IEquatable<GenericTypeConstraint>
    {
        /// <summary>
        /// Gets the name of the type to constraint
        /// </summary>
        public readonly string TypeName;

        /// <summary>
        /// Gets the name of the type the type name is supposed to inherit from
        /// </summary>
        public readonly string BaseTypeName;

        /// <summary>
        /// Gets the context this generic type constraint was defined in
        /// </summary>
        public readonly ZScriptParser.GenericConstraintContext Context;

        /// <summary>
        /// Initializes a new GenericTypeConstraint structure
        /// </summary>
        public GenericTypeConstraint(string typeName, string baseTypeName, ZScriptParser.GenericConstraintContext context)
        {
            TypeName = typeName;
            BaseTypeName = baseTypeName;
            Context = context;
        }

        #region Equality members

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        public bool Equals(GenericTypeConstraint other)
        {
            return string.Equals(BaseTypeName, other.BaseTypeName) && string.Equals(TypeName, other.TypeName) && Context == other.Context;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GenericTypeConstraint constraint && Equals(constraint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BaseTypeName?.GetHashCode() ?? 0) * 397) ^
                       ((TypeName?.GetHashCode() ?? 0) * 397) ^
                       (Context?.GetHashCode() ?? 0);
            }
        }

        public static bool operator==(GenericTypeConstraint left, GenericTypeConstraint right)
        {
            return left.Equals(right);
        }

        public static bool operator!=(GenericTypeConstraint left, GenericTypeConstraint right)
        {
            return !left.Equals(right);
        }

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        #endregion
    }
}