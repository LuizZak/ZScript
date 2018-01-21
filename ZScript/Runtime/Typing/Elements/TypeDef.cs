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
using JetBrains.Annotations;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a type definition
    /// </summary>
    public class TypeDef : ITypeDef
    {
        /// <summary>
        /// The name for this type
        /// </summary>
        protected readonly string name;

        /// <summary>
        /// Whether this type definition represents the 'any' type
        /// </summary>
        protected readonly bool isAny;

        /// <summary>
        /// Whether this type definition represents the 'void' type
        /// </summary>
        protected readonly bool isVoid;

        /// <summary>
        /// Whether this type definition represents a native type
        /// </summary>
        protected readonly bool isNative;

        /// <summary>
        /// The base type this type inherited from.
        /// May be null, in case this type represents the basic object type
        /// </summary>
        protected TypeDef baseType;

        /// <summary>
        /// Array that contains the available methods of this TypeDef
        /// </summary>
        protected List<TypeMethodDef> methods;

        /// <summary>
        /// Array that contains the available fields of this TypeDef
        /// </summary>
        protected List<TypeFieldDef> fields;

        /// <summary>
        /// Gets the name for this type
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'any' type
        /// </summary>
        public bool IsAny => isAny;

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'void' type
        /// </summary>
        public bool IsVoid => isVoid;

        /// <summary>
        /// Gets a value specifying whether this type definition represents a native type
        /// </summary>
        public bool IsNative => isNative;

        /// <summary>
        /// Returns a value specifying whether this type represents a generic parameterizable
        /// type
        /// </summary>
        public bool IsGeneric => false;

        /// <summary>
        /// If this type is generic (<see cref="IsGeneric"/> is true), this array has .Length > 0
        /// and contains all parameterizable types.
        /// 
        /// If any of this parameters is null, the type is not complete and must be parameterized
        /// before being used.
        /// </summary>
        [NotNull]
        public TypeDef[] GenericParameters => new TypeDef[0];

        /// <summary>
        /// Static constructor for the TypeDef class which deals with basic type creation
        /// </summary>
        static TypeDef()
        {
            AnyType = new AnyTypeDef();
            VoidType = new NativeTypeDef(typeof(void), "void");
            IntegerType = new NativeTypeDef(typeof(long), "int");
            FloatType = new NativeTypeDef(typeof(double), "float");
            NullType = new TypeDef("null");
            
            GenerateBaseTypes(out BaseObjectType, out StringType, out BooleanType);

            AnyType.baseType = BaseObjectType;
            VoidType.baseType = BaseObjectType;
            IntegerType.baseType = BaseObjectType;
            FloatType.baseType = BaseObjectType;
        }

        /// <summary>
        /// Initializes a new instance of the TypeDef class
        /// </summary>
        /// <param name="name">The name for the type</param>
        /// <param name="native">Whether this type represents a native type</param>
        public TypeDef(string name, bool native = false)
        {
            this.name = name;
            isAny = name == "any";
            isVoid = name == "void";
            isNative = native;

            fields = new List<TypeFieldDef>();
            methods = new List<TypeMethodDef>();
        }

        /// <summary>
        /// Gets an assembly friendly display name for this type definition
        /// </summary>
        /// <returns>A string that can be used as an assembly-friendly name for this type definition</returns>
        public virtual string AssemblyFriendlyName()
        {
            return name;
        }

        /// <summary>
        /// Gets a string representation of this TypeDef
        /// </summary>
        /// <returns>A string representation of this TypeDef</returns>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        /// Returns an array pointing to all the public methods of this type definition
        /// </summary>
        /// <param name="inherited">Whether to search the parent chain when searching for elements</param>
        /// <returns>An array containing all the public methods of this type definition</returns>
        [NotNull]
        public TypeMethodDef[] GetMethods(bool inherited = true)
        {
            if (!inherited || baseType == null)
                return methods.ToArray();

            var m = new List<TypeMethodDef>();

            TypeDef d = this;
            while (d != null)
            {
                m.AddRange(methods);

                d = d.baseType;
            }

            return m.ToArray();
        }

        /// <summary>
        /// Returns an array containing all of the public fields of this type definition
        /// </summary>
        /// <param name="inherited">Whether to search the parent chain when searching for elements</param>
        /// <returns>An array containing all of the public fields of this type definition</returns>
        [NotNull]
        public TypeFieldDef[] GetFields(bool inherited = true)
        {
            if (!inherited || baseType == null)
                return fields.ToArray();

            var f = new List<TypeFieldDef>();

            TypeDef d = this;
            while (d != null)
            {
                f.AddRange(fields);

                d = d.baseType;
            }

            return f.ToArray();
        }

        /// <summary>
        /// Returns a method in this TypeDef that matches the given name
        /// </summary>
        /// <param name="methodName">The name of the method to search for</param>
        /// <returns>The method type definition that was fetched; or null, if none was found</returns>
        [CanBeNull]
        public TypeMethodDef GetMethod(string methodName)
        {
            foreach (var methodDef in methods)
            {
                if (methodDef.Name == methodName)
                    return methodDef;
            }

            return baseType?.GetMethod(methodName);
        }

        /// <summary>
        /// Returns a field in this TypeDef that matches the given name
        /// </summary>
        /// <param name="fieldName">The name of the field to search for</param>
        /// <returns>The field type definition that was fetched; or null, if none was found</returns>
        [CanBeNull]
        public TypeFieldDef GetField(string fieldName)
        {
            foreach (var fieldDef in fields)
            {
                if (fieldDef.Name == fieldName)
                    return fieldDef;
            }

            return baseType?.GetField(fieldName);
        }

        /// <summary>
        /// Returns a member definition in this TypeDef that matches the given name
        /// </summary>
        /// <param name="memberName">The name of the mebmer to search for</param>
        /// <returns>The member type definition that was fetched; or null, if none was found</returns>
        [CanBeNull]
        public TypeMemberDef GetMember(string memberName)
        {
            // Search fields first
            var field = GetField(memberName);
            if (field != null)
                return field;

            // Method fetching
            return GetMethod(memberName);
        }

        /// <summary>
        /// Clears all fields defined in this type definition.
        /// This does not clears fields defined in parent classes
        /// </summary>
        public void ClearFields()
        {
            fields.Clear();
        }

        /// <summary>
        /// Clears all methods defined in this type definition.
        /// This does not clears methods defined in parent classes
        /// </summary>
        public void ClearMethods()
        {
            methods.Clear();
        }

        /// <summary>
        /// Adds an arbitrary field definition to this type definition
        /// </summary>
        /// <param name="fieldDef">The field to add to this type definition</param>
        public void AddField(TypeFieldDef fieldDef)
        {
            fields.Add(fieldDef);
        }

        /// <summary>
        /// Adds an arbitrary method definition to this type definition
        /// </summary>
        /// <param name="methodDef">The method to add to this type definition</param>
        public void AddMethod(TypeMethodDef methodDef)
        {
            methods.Add(methodDef);
        }

        /// <summary>
        /// Returns a static TypeDef that most-fittingly describes a given Type object
        /// </summary>
        /// <param name="type">The type to get the most fitting type out of</param>
        /// <returns>A TypeDef that matches the given type's description</returns>
        protected static TypeDef MostFittingType([NotNull] Type type)
        {
            // Numeric
            if (type == typeof(long))
                return IntegerType ?? new NativeTypeDef(typeof(long));
            if (type == typeof(double))
                return FloatType ?? new NativeTypeDef(typeof(double));
            // Boolean
            if (type == typeof(bool))
                return BooleanType ?? new NativeTypeDef(typeof(bool));
            // String
            if (type == typeof(string))
                return StringType;
            // Void
            if (type == typeof(void))
                return VoidType ?? new NativeTypeDef(typeof(void));

            // List
            if (type == typeof(List<>))
            {
                return new ListTypeDef(MostFittingType(type.GetGenericArguments()[0]));
            }

            return new NativeTypeDef(type);
        }

        #region Equality members

        /// <summary>
        /// Returns a value specifying whether this TypeDef instance equals a given TypeDef instance
        /// </summary>
        /// <param name="other">The TypeDef to test against</param>
        /// <returns>true if this TypeDef equals the other TypeDef, false otherwise</returns>
        public virtual bool Equals(TypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(name, other.name) && isVoid.Equals(other.isVoid) && isAny.Equals(other.isAny) && isNative.Equals(other.isNative);
        }

        /// <summary>
        /// Returns a value specifying whether this TypeDef instance equals a given object
        /// </summary>
        /// <param name="obj">The object to test against</param>
        /// <returns>true if this TypeDef equals the passed object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeDef)obj);
        }

        /// <summary>
        /// Gets the hashcode for this TypeDef instance
        /// </summary>
        /// <returns>The hashcode for this TypeDef instance</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = isVoid.GetHashCode();
                hashCode = (hashCode * 397) ^ isAny.GetHashCode();
                hashCode = (hashCode * 397) ^ isNative.GetHashCode();
                hashCode = (hashCode * 397) ^ (name?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Compares two TypeDef instances for equality
        /// </summary>
        public static bool operator==(TypeDef left, TypeDef right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two TypeDef instances for inequality
        /// </summary>
        public static bool operator!=(TypeDef left, TypeDef right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// The type definition that represents the 'any' type
        /// </summary>
        public static readonly TypeDef AnyType;

        /// <summary>
        /// The type definition that represents the 'void' type
        /// </summary>
        public static readonly TypeDef VoidType;

        /// <summary>
        /// The type definition for an integer type in the runtime.
        /// The integer type is defined as an Int64 integer in the C# runtime
        /// </summary>
        public static readonly TypeDef IntegerType;

        /// <summary>
        /// The type definition for a floating-point type in the runtime.
        /// The integer type is defined as a double floating point in the C# runtime
        /// </summary>
        public static readonly TypeDef FloatType;

        /// <summary>
        /// The type definition for a null type in the runtime.
        /// The integer type is defined as a null in the C# runtime
        /// </summary>
        public static readonly TypeDef NullType;

        /// <summary>
        /// The type definition for a boolean type in the runtime.
        /// The integer type is defined as a bool in the C# runtime
        /// </summary>
        public static readonly TypeDef BooleanType;

        /// <summary>
        /// The base type definition all types are derived from.
        /// The base object type is defined as an object in the C# runtime
        /// </summary>
        public static readonly TypeDef BaseObjectType;

        /// <summary>
        /// The type definition for a string type in the runtime.
        /// The integer type is defined as a string in the C# runtime
        /// </summary>
        public static readonly StringTypeDef StringType;

        /// <summary>
        /// Creates type definitions for three core objects that have linked shared dependencies
        /// </summary>
        /// <param name="objectType">The resulting object type for this operation</param>
        /// <param name="stringType">The resulting string type for this operation</param>
        /// <param name="boolType">The resulting bool type for this operation</param>
        public static void GenerateBaseTypes([NotNull] out TypeDef objectType, [NotNull] out StringTypeDef stringType,
            [NotNull] out TypeDef boolType)
        {
            objectType = new NativeTypeDef(typeof(object), "object");
            stringType = new StringTypeDef();
            boolType = new NativeTypeDef(typeof(bool), "bool");
            
            var toString = new TypeMethodDef("ToString", new ParameterInfo[0], stringType);
            var equals = new TypeMethodDef("Equals", new [] { new ParameterInfo("obj", objectType, false, false) }, boolType);

            // Add the methods to the object type
            var methods = new[] { toString, equals };

            objectType.methods = new List<TypeMethodDef>(methods);

            // Sort child types
            stringType.baseType = objectType;
            boolType.baseType = objectType;
        }
    }

    /// <summary>
    /// Interface to be implemented by type definitions
    /// </summary>
    public interface ITypeDef : IEquatable<TypeDef>
    {
        /// <summary>
        /// Gets the name for this type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'any' type
        /// </summary>
        bool IsAny { get; }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'void' type
        /// </summary>
        bool IsVoid { get; }

        /// <summary>
        /// Gets a value specifying whether this type definition represents a native type
        /// </summary>
        bool IsNative { get; }
    }

    /// <summary>
    /// Interface to be implemented by types that can inherit from base types
    /// </summary>
    public interface IInheritableTypeDef : ITypeDef
    {
        /// <summary>
        /// Gets or sets the base type for this ClassTypeDef.
        /// If the value provided is a base type of this class, an ArgumentException is raised
        /// </summary>
        /// <exception cref="ArgumentException">The provided type value causes a circular inheritance chain</exception>
        TypeDef BaseType { get; set; }
    }

    /// <summary>
    /// Represents information about a type's member
    /// </summary>
    public abstract class TypeMemberDef
    {
        /// <summary>
        /// The name for this member
        /// </summary>
        protected readonly string name;

        /// <summary>
        /// The name for this member
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Initializes a new instance of the TypeMemberDef class
        /// </summary>
        /// <param name="memberName">The name of the member to create</param>
        protected TypeMemberDef(string memberName)
        {
            name = memberName;
        }
    }

    /// <summary>
    /// Represents a field for a type definition
    /// </summary>
    public class TypeFieldDef : TypeMemberDef
    {
        /// <summary>
        /// The type for the field
        /// </summary>
        public TypeDef FieldType { get; }

        /// <summary>
        /// Gets a value specifying whether the field is readonly or not
        /// </summary>
        public bool Readonly { get; }

        /// <summary>
        /// Initializes a new instance of the TypeFieldDef class
        /// </summary>
        /// <param name="fieldName">The name for the field</param>
        /// <param name="fieldType">The type that can be stored on the field</param>
        /// <param name="isReadonly">Whether the field is readonly or not</param>
        public TypeFieldDef(string fieldName, TypeDef fieldType, bool isReadonly)
            : base(fieldName)
        {
            FieldType = fieldType;
            Readonly = isReadonly;
        }
    }

    /// <summary>
    /// Represents a method for a type definition
    /// </summary>
    public class TypeMethodDef : TypeMemberDef
    {
        /// <summary>
        /// Gets the array of parameters for the method
        /// </summary>
        public ParameterInfo[] Parameters { get; }

        /// <summary>
        /// Gets the return type for the method
        /// </summary>
        public TypeDef ReturnType { get; }

        /// <summary>
        /// Initializes a new instance of the TypeMethodDef class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="parameters">The parameters for the method</param>
        /// <param name="returnType">The return type for the method</param>
        public TypeMethodDef(string name, ParameterInfo[] parameters, TypeDef returnType)
            : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        /// <summary>
        /// Gets the callable type definition that mirrors this type method definition
        /// </summary>
        /// <returns>A callable type definition that mirrors this type method definition</returns>
        [NotNull]
        public CallableTypeDef CallableTypeDef()
        {
            var parameters = new CallableTypeDef.CallableParameterInfo[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
            {
                parameters[i] = new CallableTypeDef.CallableParameterInfo(Parameters[i].ParameterType, true, Parameters[i].Optional, Parameters[i].IsVariadic, Parameters[i].DefaultValue);
            }

            return new CallableTypeDef(parameters, ReturnType, true);
        }
    }

    /// <summary>
    /// Specifies information about a method's parameter
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Gets the type for the parameter
        /// </summary>
        public TypeDef ParameterType { get; }

        /// <summary>
        /// Gets the name for this parameter
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets a value specifying whether the parameter is variadic in nature
        /// </summary>
        public bool IsVariadic { get; }

        /// <summary>
        /// Gets a value specifying whether the parameter is optional
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// Gets the default value, in case this is an optional parameter
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Initializes a new instance of the ParameterInfo class
        /// </summary>
        /// <param name="name">The name for this parameter</param>
        /// <param name="type">The type signature for this parameter</param>
        /// <param name="isVariadic">Whether the parameter is variadic in nature</param>
        /// <param name="optional">Whether the parameter is optional</param>
        /// <param name="defaultValue">The default value, in case this is an optional parameter</param>
        public ParameterInfo(string name, TypeDef type, bool isVariadic, bool optional, object defaultValue = null)
        {
            ParameterName = name;
            ParameterType = type;
            IsVariadic = isVariadic;
            Optional = optional;
            DefaultValue = defaultValue;
        }
    }
}