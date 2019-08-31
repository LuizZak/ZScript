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
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using JetBrains.Annotations;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Used by a generic type resolver to resolve and validate generic types
    /// </summary>
    public class ConstraintSolver
    {
        private readonly TypeProvider _typeProvider;
        private List<ConstraintType> _types = new List<ConstraintType>();
        private readonly List<TypeConstraint> _constraints = new List<TypeConstraint>();

        /// <summary>
        /// After a successful type resolving step, this dictionary maps the types that
        /// where pushed in with Add[Open/Closed]Type and their resulting concrete types,
        /// otherwise this value is null.
        /// </summary>
        [CanBeNull]
        public Dictionary<string, TypeDef> ResolvedTypes { get; set; }

        /// <summary>
        /// Whether the types within this constraint solver where successfully resolved
        /// </summary>
        public bool IsResolved { get; private set; }

        /// <summary>
        /// Gets a local message container on which messages are generated every time Solve() is called.
        /// </summary>
        public MessageContainer LocalContainer { get; } = new MessageContainer();

        /// <summary>
        /// Initializes a new constraint solver that will use the given type provider
        /// to solve the type constraints
        /// </summary>
        public ConstraintSolver(TypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        /// <summary>
        /// Adds an open type argument to resolve.
        /// </summary>
        public ConstraintType AddOpenType([NotNull] string name, TypeDirection direction, ParserRuleContext context = null)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            // Check type clash
            if (GetTypeNamed(name) != null)
            {
                throw new ArgumentException($"There's already a type named '{name}' registered within this constraint solver.", nameof(name));
            }

            var type = new ConstraintType(name, null, direction, context);

            _types.Add(type);

            return type;
        }

        /// <summary>
        /// Adds a Closed (concrete) type argument to constrain open types
        /// with.
        /// </summary>
        /// <param name="concreteType">The concrete type this closed type represents</param>
        /// <param name="direction">Direction of type (e.g. argument going in, or argument going out)</param>
        /// <param name="context">An optional context for this type, e.g. the location of the call site.s</param>
        public ConstraintType AddClosedType([NotNull] TypeDef concreteType, TypeDirection direction, ParserRuleContext context = null)
        {
            var name = concreteType.Name;

            Ensure.ArgumentNotNull(name, nameof(name));
            Ensure.ArgumentNotNull(concreteType, nameof(concreteType));

            // Check type clash
            if (GetTypeNamed(name) != null)
            {
                throw new ArgumentException($"There's already a type named '{name}' registered within this constraint solver.", nameof(name));
            }

            var type = new ConstraintType(name, concreteType, direction, context);

            _types.Add(type);

            return type;
        }

        /// <summary>
        /// Adds a binding constraint between two types to use during type resolving
        /// </summary>
        public void AddConstraint([NotNull] string openTypeName, Constraint constraint, [NotNull] string typeName, ParserRuleContext context = null)
        {
            Ensure.ArgumentNotNull(openTypeName, nameof(openTypeName));
            Ensure.ArgumentNotNull(typeName, nameof(typeName));

            if(GetTypeNamed(openTypeName) == null)
                throw new ArgumentException($"No type named {openTypeName} registered within this constraint solver.", nameof(openTypeName));
            if (GetTypeNamed(typeName) == null)
                throw new ArgumentException($"No type named {typeName} registered within this constraint solver.", nameof(typeName));

            var c = new TypeConstraint(openTypeName, constraint, typeName, context);
            _constraints.Add(c);
        }

        /// <summary>
        /// Adds a binding constraint to a concrete type definition and binds it to a generic type name
        /// </summary>
        public void AddConstraint([NotNull] string openTypeName, Constraint constraint, [NotNull] TypeDef typeDef, ParserRuleContext context = null)
        {
            
        }

        /// <summary>
        /// Resolves the types within this constraint solver
        /// </summary>
        public void Solve()
        {
            IsResolved = false;

            // Clear container before doing minimizing rounds
            LocalContainer.Clear();

            // Minimize until we can't anymore
            while (PerformMinimizeRound(LocalContainer)) { }

            // Check if all types are now closed
            IsResolved = !LocalContainer.HasCodeErrors && _types.All(type => type.IsClosed);

            // Expose solved types
            ResolvedTypes = new Dictionary<string, TypeDef>();

            foreach (var type in _types)
            {
                if (!type.IsClosed)
                {
                    LocalContainer.RegisterError(type.Context, $"Failed to deduce type {type.Name}");
                    continue;
                }

                ResolvedTypes[type.Name] = type.ConcreteTypeDef;
            }
        }

        private bool PerformMinimizeRound(MessageContainer messageContainer)
        {
            bool performedWork = false;
            bool failed = false;

            // Output of this round of solving
            var newTypes = _types.ToList();

            // Flatten down same-type constraints until we can't anymore
            foreach (var constraint in _constraints)
            {
                int leftTypeIndex = GetIndexOfTypeNamedIn(constraint.FirstType, newTypes);
                int rightTypeIndex = GetIndexOfTypeNamedIn(constraint.SecondType, newTypes);

                var leftType = newTypes[leftTypeIndex];
                var rightType = newTypes[rightTypeIndex];

                if (leftType.IsClosed && rightType.IsClosed)
                {
                    switch (constraint.Constraint)
                    {
                        case Constraint.SameType:
                            if (leftType.ConcreteTypeDef != rightType.ConcreteTypeDef)
                            {
                                // TODO: Diagnose message here, etc...
                                messageContainer.RegisterError(constraint.Context, $"Cannot satisfy constraint {DescriptionForConstraint(constraint, newTypes)}");

                                failed = true;
                            }
                            break;
                        case Constraint.Subtype:
                            if (!_typeProvider.AreTypesCompatible(leftType.ConcreteTypeDef, rightType.ConcreteTypeDef))
                            {
                                // TODO: Diagnose message here, etc...
                                messageContainer.RegisterError(constraint.Context, $"Cannot satisfy constraint {DescriptionForConstraint(constraint, newTypes)}");

                                failed = true;
                            }
                            break;
                    }
                }
                // Concretize!
                else if (!leftType.IsClosed && rightType.IsClosed)
                {
                    switch (constraint.Constraint)
                    {
                        case Constraint.SameType:
                            Debug.Assert(rightType.ConcreteTypeDef != null, "leftType.ConcreteTypeDef != null");

                            leftType = leftType.SettingConcrete(rightType.ConcreteTypeDef);
                            performedWork = true;
                            break;
                        case Constraint.Subtype:
                            break;
                    }
                }
                // Concretize!
                else if (leftType.IsClosed && !rightType.IsClosed)
                {
                    switch (constraint.Constraint)
                    {
                        case Constraint.SameType:
                            Debug.Assert(leftType.ConcreteTypeDef != null, "leftType.ConcreteTypeDef != null");

                            rightType = rightType.SettingConcrete(leftType.ConcreteTypeDef);
                            performedWork = true;
                            break;
                        case Constraint.Subtype:
                            break;
                    }
                }

                // Record changes
                newTypes[leftTypeIndex] = leftType;
                newTypes[rightTypeIndex] = rightType;
            }

            _types = newTypes;
            return performedWork && !failed;
        }

        private ConstraintType? GetTypeNamed(string name)
        {
            if (_types.Exists(ot => ot.Name == name))
            {
                return _types.First(ot => ot.Name == name);
            }

            return null;
        }
        
        private static int GetIndexOfTypeNamedIn(string name, [NotNull] IEnumerable<ConstraintType> types)
        {
            return types.Select((type, index) => (type, index)).First(tuple => tuple.Item1.Name == name).Item2;
        }

        [NotNull]
        private static string DescriptionForConstraint(TypeConstraint constraint, [NotNull] IList<ConstraintType> types)
        {
            string ResolveNameWithAlias(string typeName)
            {
                var type = types.First(t => t.Name == typeName);

                return
                    type.ConcreteTypeDef != null
                        ? $"{typeName} (aka {type.ConcreteTypeDef.Name})"
                        : typeName;
            }

            return $"{ResolveNameWithAlias(constraint.FirstType)} {(constraint.Constraint == Constraint.SameType ? "==" : ":")} {ResolveNameWithAlias(constraint.SecondType)}";
        }

        /// <summary>
        /// An Open type is free type that is yet to be constrained down to a
        /// Closed type.
        /// </summary>
        public readonly struct ConstraintType : IEquatable<ConstraintType>
        {
            /// <summary>
            /// Initializes a new instance of the ConstraintType struct
            /// </summary>
            public ConstraintType([NotNull] string name, TypeDef concreteTypeDef, TypeDirection direction, [CanBeNull] ParserRuleContext context) : this()
            {
                Name = name;
                ConcreteTypeDef = concreteTypeDef;
                Direction = direction;
                Context = context;
            }

            /// <summary>
            /// Name for this Open type
            /// e.g.: 'T'
            /// </summary>
            [NotNull]
            public string Name { get; }

            /// <summary>
            /// If this open type is resolved, this references a concrete type
            /// for the result.
            /// </summary>
            [CanBeNull]
            public TypeDef ConcreteTypeDef { get; }

            /// <summary>
            /// Gets an optional AST context for this type constraint
            /// </summary>
            [CanBeNull]
            public ParserRuleContext Context { get; }

            /// <summary>
            /// Direction of this type
            /// </summary>
            public TypeDirection Direction { get; }

            /// <summary>
            /// Gets a value specifying whether this constraint type is closed (i.e. resolved)
            /// </summary>
            public bool IsClosed => ConcreteTypeDef != null;

            /// <summary>
            /// Returns a copy of this constraint type w/ the given concrete type def associated
            /// </summary>
            public ConstraintType SettingConcrete([NotNull] TypeDef concreteTypeDef)
            {
                return new ConstraintType(Name, concreteTypeDef, Direction, Context);
            }

            /// <summary>
            /// Returns whether this constraint type instance matches another constraint type instance exactly.
            /// </summary>
            public bool Equals(ConstraintType other)
            {
                return string.Equals(Name, other.Name) && Equals(ConcreteTypeDef, other.ConcreteTypeDef) && Direction == other.Direction;
            }

            /// <summary>
            /// Returns whether this constraint type instance matches another object.
            /// 
            /// Method only returns true if obj is another ConstraintType instance.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ConstraintType type && Equals(type);
            }

            /// <summary>
            /// Gets the hash code for this constraint type
            /// </summary>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Name.GetHashCode();
                    hashCode = (hashCode * 397) ^ (ConcreteTypeDef?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (int) Direction;
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Constrains an open type to another open type or closed type.
        /// </summary>
        private readonly struct TypeConstraint : IEquatable<TypeConstraint>
        {
            public TypeConstraint([NotNull] string firstType, Constraint constraint, [NotNull] string secondType, [CanBeNull] ParserRuleContext context) : this()
            {
                FirstType = firstType;
                Constraint = constraint;
                SecondType = secondType;
                Context = context;
            }

            [NotNull]
            public string FirstType { get; }
            public Constraint Constraint { get; }
            [NotNull]
            public string SecondType { get; }

            /// <summary>
            /// Gets an optional AST context for this type constraint
            /// </summary>
            [CanBeNull]
            public ParserRuleContext Context { get; }

            public override string ToString()
            {
                return $"{FirstType} {(Constraint == Constraint.SameType ? "==" : ":")} {SecondType}";
            }

            public bool Equals(TypeConstraint other)
            {
                return string.Equals(FirstType, other.FirstType) && Constraint == other.Constraint && string.Equals(SecondType, other.SecondType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is TypeConstraint other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = FirstType.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int) Constraint;
                    hashCode = (hashCode * 397) ^ SecondType.GetHashCode();
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Specifies the direction of this open type.
        /// Open types can either be In (e.g. a function argument)
        /// or Out (e.g. a function's return value)
        /// </summary>
        public enum TypeDirection
        {
            /// <summary>
            /// An In type (e.g. a function argument)
            /// </summary>
            In,
            
            /// <summary>
            /// An Out type (e.g. a function's return value)
            /// </summary>
            Out
        }

        /// <summary>
        /// Used during type constraining to specify the relationships
        /// between open and closed types
        /// </summary>
        public enum Constraint
        {
            /// <summary>
            /// Specifies a same type constraint, e.g. 'T == Int'.
            /// </summary>
            SameType,
            /// <summary>
            /// Specifies a sub-type constraint, e.g. 'T: IEquatable'.
            /// </summary>
            Subtype
        }
    }
}