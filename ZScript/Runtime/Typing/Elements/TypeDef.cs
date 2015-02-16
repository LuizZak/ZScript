﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a type definition
    /// </summary>
    public class TypeDef : IEquatable<TypeDef>
    {
        /// <summary>
        /// The name for this type
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Whether this type definition represents the 'any' type
        /// </summary>
        private readonly bool _isAny;

        /// <summary>
        /// Whether this type definition represents the 'void' type
        /// </summary>
        private readonly bool _isVoid;

        /// <summary>
        /// Whether this type definition represents a native type
        /// </summary>
        private readonly bool _isNative;

        /// <summary>
        /// The base type this type inherited from.
        /// May be null, in case this type represents the basic object type
        /// </summary>
        private readonly TypeDef _baseType;

        /// <summary>
        /// Array that contains the available methods of this TypeDef
        /// </summary>
        private TypeMethodDef[] _methods;

        /// <summary>
        /// Array that contains the available fields of this TypeDef
        /// </summary>
        private TypeFieldDef[] _fields;

        /// <summary>
        /// Gets the name for this type
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'any' type
        /// </summary>
        public bool IsAny
        {
            get { return _isAny; }
        }

        /// <summary>
        /// Gets a value specifying whether this type definition represents the 'void' type
        /// </summary>
        public bool IsVoid
        {
            get { return _isVoid; }
        }

        /// <summary>
        /// Gets a value specifying whether this type definition represents a native type
        /// </summary>
        public bool IsNative
        {
            get { return _isNative; }
        }

        /// <summary>
        /// Initializes a new instance of the TypeDef class
        /// </summary>
        /// <param name="name">The name for the type</param>
        /// <param name="native">Whether this type represents a native type</param>
        public TypeDef(string name, bool native = false)
        {
            _name = name;
            _isAny = name == "any";
            _isVoid = name == "void";
            _isNative = native;
        }

        /// <summary>
        /// Initializes a new instance of the TypeDef class, providing a base type to set as this type's base type.
        /// </summary>
        /// <param name="name">The name for the type</param>
        /// <param name="baseType">The type this type inherits from</param>
        /// <param name="fields">The fields for this type definition</param>
        /// <param name="methods">The methods for this type definition</param>
        /// <param name="native">Whether this type represents a native type</param>
        private TypeDef(string name, TypeDef baseType, TypeFieldDef[] fields, TypeMethodDef[] methods, bool native = false)
            : this(name, native)
        {
            _baseType = baseType;
            _fields = fields;
            _methods = methods;
        }

        /// <summary>
        /// Gets a string representation of this TypeDef
        /// </summary>
        /// <returns>A string representation of this TypeDef</returns>
        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Returns an array pointing to all the public methods of this type definition
        /// </summary>
        /// <param name="inherited">Whether to search the parent chain when searching for elements</param>
        /// <returns>An array containing all the public methods of this type definition</returns>
        public TypeMethodDef[] GetMethods(bool inherited = true)
        {
            if (!inherited || _baseType == null)
                return _methods.ToArray();

            var methods = new List<TypeMethodDef>();

            TypeDef d = this;
            while (d != null)
            {
                methods.AddRange(_methods);

                d = d._baseType;
            }

            return methods.ToArray();
        }

        /// <summary>
        /// Returns an array containing all of the public fields of this type definition
        /// </summary>
        /// <param name="inherited">Whether to search the parent chain when searching for elements</param>
        /// <returns>An array containing all of the public fields of this type definition</returns>
        public TypeFieldDef[] GetFields(bool inherited = true)
        {
            if (!inherited || _baseType == null)
                return _fields.ToArray();

            var fields = new List<TypeFieldDef>();

            TypeDef d = this;
            while (d != null)
            {
                fields.AddRange(_fields);

                d = d._baseType;
            }

            return fields.ToArray();
        }

        #region Equality members

        /// <summary>
        /// Returns a value specifying whether this TypeDef instance equals a given TypeDef instance
        /// </summary>
        /// <param name="other">The TypeDef to test against</param>
        /// <returns>true if this TypeDef equals the other TypeDef, false otherwise</returns>
        public bool Equals(TypeDef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _isVoid.Equals(other._isVoid) && _isAny.Equals(other._isAny) && string.Equals(_name, other._name) && _isNative.Equals(other._isNative);
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
                var hashCode = _isVoid.GetHashCode();
                hashCode = (hashCode * 397) ^ _isAny.GetHashCode();
                hashCode = (hashCode * 397) ^ _isNative.GetHashCode();
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
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
        public static readonly TypeDef AnyType = new TypeDef("any");

        /// <summary>
        /// The type definition that represents the 'void' type
        /// </summary>
        public static readonly TypeDef VoidType = new TypeDef("void");

        /// <summary>
        /// The type definition for an integer type in the runtime.
        /// The integer type is defined as an Int64 integer in the C# runtime
        /// </summary>
        public static readonly TypeDef IntegerType = new TypeDef("int");

        /// <summary>
        /// The type definition for a floating-point type in the runtime.
        /// The integer type is defined as a double floating point in the C# runtime
        /// </summary>
        public static readonly TypeDef FloatType = new TypeDef("float");

        /// <summary>
        /// The type definition for a null type in the runtime.
        /// The integer type is defined as a null in the C# runtime
        /// </summary>
        public static readonly TypeDef NullType = new TypeDef("null");

        /// <summary>
        /// The type definition for a boolean type in the runtime.
        /// The integer type is defined as a bool in the C# runtime
        /// </summary>
        public static readonly TypeDef BooleanType = new TypeDef("bool");

        /// <summary>
        /// The type definition for a string type in the runtime.
        /// The integer type is defined as a string in the C# runtime
        /// </summary>
        public static readonly StringTypeDef StringType = new StringTypeDef();

        /// <summary>
        /// Creates type definitions for three core objects that have linked shared dependencies
        /// </summary>
        /// <param name="objectType">The resulting object type for this operation</param>
        /// <param name="stringType">The resulting string type for this operation</param>
        /// <param name="boolType">The resulting bool type for this operation</param>
        public static void GenerateBaseTypes(out TypeDef objectType, out TypeDef stringType, out TypeDef boolType)
        {
            objectType = new TypeDef("object");
            stringType = new TypeDef("string", objectType, null, null);
            boolType = new TypeDef("bool", objectType, null, null);
            
            var toString = new TypeMethodDef("ToString", new ParameterInfo[0], stringType);
            var equals = new TypeMethodDef("Equals", new [] { new ParameterInfo("obj", objectType) }, boolType);

            // Add the methods to the object type
            var methods = new[] { toString, equals };

            objectType._methods = methods;
        }
    }

    /// <summary>
    /// Represents information about a type's member
    /// </summary>
    public abstract class TypeMemberDef
    {
        /// <summary>
        /// The name for this member
        /// </summary>
        protected readonly string _name;

        /// <summary>
        /// Initializes a new instance of the TypeMemberDef class
        /// </summary>
        /// <param name="memberName">The name of the member to create</param>
        protected TypeMemberDef(string memberName)
        {
            _name = memberName;
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
        private readonly TypeDef _fieldType;

        /// <summary>
        /// The type for the field
        /// </summary>
        public TypeDef FieldType
        {
            get { return _fieldType; }
        }

        /// <summary>
        /// Initializes a new instance of the TypeFieldDef class
        /// </summary>
        /// <param name="fieldName">The name for the field</param>
        /// <param name="fieldType">The type that can be stored on the field</param>
        public TypeFieldDef(string fieldName, TypeDef fieldType)
            : base(fieldName)
        {
            _fieldType = fieldType;
        }
    }

    /// <summary>
    /// Represents a method for a type definition
    /// </summary>
    public class TypeMethodDef : TypeMemberDef
    {
        /// <summary>
        /// Initializes a new instance of the TypeMethodDef class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="parameters">The parameters for the method</param>
        /// <param name="returnType">The return type for the method</param>
        public TypeMethodDef(string name, ParameterInfo[] parameters, TypeDef returnType)
            : base(name)
        {
            
        }
    }

    /// <summary>
    /// Specifies information about a method's parameter
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// The type for the parameter
        /// </summary>
        private readonly TypeDef _type;

        /// <summary>
        /// The name for this parameter
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Gets the type for the parameter
        /// </summary>
        public TypeDef ParameterType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the name for this parameter
        /// </summary>
        public string ParameterName
        {
            get { return _name; }
        }

        /// <summary>
        /// Initializes a new instance of the ParameterInfo class
        /// </summary>
        /// <param name="name">The name for this parameter</param>
        /// <param name="type">The type signature for this parameter</param>
        public ParameterInfo(string name, TypeDef type)
        {
            _name = name;
            _type = type;
        }
    }
}