﻿#region License information
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

using System.Reflection;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Class that provides facilities to deal with type interoperability
    /// </summary>
    public class TypeProvider
    {
        /// <summary>
        /// Type provider used to resolve binary expressions
        /// </summary>
        private readonly BinaryExpressionTypeProvider _binaryExpressionProvider;

        /// <summary>
        /// Gets type provider used to resolve binary expressions
        /// </summary>
        public BinaryExpressionTypeProvider BinaryExpressionProvider
        {
            get { return _binaryExpressionProvider; }
        }

        /// <summary>
        /// Initializes a new instance of the TypeProvider class
        /// </summary>
        public TypeProvider()
        {
            _binaryExpressionProvider = new BinaryExpressionTypeProvider(this);
        }

        /// <summary>
        /// Performs a cast on an object from its type to a type provided
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="newType">The new type to convert to</param>
        /// <returns></returns>
        public object CastObject(object value, Type newType)
        {
            // No casting necessary
            if (value.GetType() == newType)
                return value;

            if (value is IConvertible)
            {
                return Convert.ChangeType(value, newType);
            }

            MethodInfo castMethod = GetType().GetMethod("Cast").MakeGenericMethod(newType);
            return castMethod.Invoke(null, new[] {value});
        }

        /// <summary>
        /// Returns a TypeDef that matches the given type name
        /// </summary>
        /// <param name="typeName">The name of the type to get</param>
        /// <returns>The type that references the provided type name</returns>
        public TypeDef TypeNamed(string typeName)
        {
            switch (typeName)
            {
                case "void":
                    return VoidType();
                case "any":
                    return AnyType();
                case "int":
                    return IntegerType();
                case "float":
                    return FloatType();
                case "bool":
                    return BooleanType();
                case "string":
                    return StringType();
                case "object":
                    return ObjectType();
            }

            return new TypeDef(typeName, true);
        }

        /// <summary>
        /// Returns a native equivalent for a given TypeDef.
        /// If no equivalent native type is found, null is returned
        /// </summary>
        /// <param name="typeDef">The type to get the native equivalent of</param>
        /// <returns>A Type that represents a native equivalent for the given type</returns>
        public Type NativeTypeForTypeDef(TypeDef typeDef)
        {
            var listTypeDef = typeDef as ListTypeDef;
            if (listTypeDef != null)
            {
                // Get the native type definition if the inner type
                var innerType = NativeTypeForTypeDef(listTypeDef.EnclosingType);
                var listType = typeof(List<>);

                return listType.MakeGenericType(innerType ?? typeof(object));
            }

            var nativeTypeDef = typeDef as NativeTypeDef;
            if (nativeTypeDef != null)
            {
                return nativeTypeDef.NativeType;
            }

            if (typeDef == NullType())
            {
                return typeof(object);
            }

            // No equivalents
            return null;
        }

        /// <summary>
        /// Returns a type that represents a list of items of a given type
        /// </summary>
        /// <param name="type">A valid type definition</param>
        /// <returns>A type that represents a type of list for the given object type</returns>
        public ListTypeDef ListForType(TypeDef type)
        {
            return new ListTypeDef(type) { SubscriptType = IntegerType() };
        }

        /// <summary>
        /// Tries to come up with the most common type that fits the type of two provided types
        /// </summary>
        /// <param name="type1">The first type to infer</param>
        /// <param name="type2">The second type to infer</param>
        /// <returns>The most common type that fits the two provided types</returns>
        public TypeDef FindCommonType(TypeDef type1, TypeDef type2)
        {
            if (type1 == null) throw new ArgumentNullException("type1");
            if (type2 == null) throw new ArgumentNullException("type2");

            // Equal types: return the type itself
            if (type1 == type2)
                return type1;

            // Void causes both types to be void
            if (type1.IsVoid || type2.IsVoid)
                return VoidType();

            // Any causes both types to be any as well
            if(type1.IsAny || type2.IsAny)
                return AnyType();

            if (type1 == NullType())
                return type2;
            if (type2 == NullType())
                return type1;
            if(type1 == NullType() && type2 == NullType())
                return NullType();

            // Integer -> Float conversion
            var intType = IntegerType();
            var floatType = FloatType();

            if ((type1 == intType || type2 == intType) &&
               (type1 == floatType || type2 == floatType))
                return floatType;

            // Callables with same argument count: Infer the arguments
            var ct1 = type1 as CallableTypeDef;
            var ct2 = type2 as CallableTypeDef;
            if (ct1 != null && ct2 != null && ct1.ParameterTypes.Length == ct2.ParameterTypes.Length)
            {
                // Mismatched variadic typing should
                for (int i = 0; i < ct1.ParameterInfos.Length; i++)
                {
                    if(ct1.ParameterInfos[i].IsVariadic != ct2.ParameterInfos[i].IsVariadic)
                        return AnyType();
                }

                // TODO: Clear this nasty select statement
                var newParams =
                    ct1.ParameterInfos.Select(
                        (t, i) =>
                        {
                            var pt1 = t.ParameterType ?? ct2.ParameterTypes[i] ?? TypeDef.AnyType;
                            var pt2 = ct2.ParameterTypes[i] ?? t.ParameterType ?? TypeDef.AnyType;

                            return new CallableTypeDef.CallableParameterInfo(
                                FindCommonType(pt1, pt2), true,
                                t.HasDefault || ct2.ParameterInfos[i].HasDefault,
                                t.IsVariadic && ct2.ParameterInfos[i].IsVariadic);
                        });

                TypeDef newReturn = null;

                // Deal with return type providing
                if (ct1.HasReturnType && ct2.HasReturnType)
                {
                    var rt1 = ct1.ReturnType ?? ct2.ReturnType ?? TypeDef.AnyType;
                    var rt2 = ct2.ReturnType ?? ct1.ReturnType ?? TypeDef.AnyType;

                    newReturn = FindCommonType(rt1, rt2);
                }
                else if (ct1.HasReturnType)
                {
                    newReturn = ct1.ReturnType ?? TypeDef.AnyType;
                }
                else if (ct2.HasReturnType)
                {
                    newReturn = ct2.ReturnType ?? TypeDef.AnyType;
                }

                return new CallableTypeDef(newParams.ToArray(), newReturn ?? AnyType(), newReturn != null);
            }

            // Inferring lists
            var list1 = type1 as IListTypeDef;
            var list2 = type2 as IListTypeDef;
            if (list1 != null && list2 != null)
            {
                return new ListTypeDef(FindCommonType(list1.EnclosingType, list2.EnclosingType));
            }

            // Last case: Any type to fit them all
            return AnyType();
        }

        /// <summary>
        /// Returns whether an origin type can be cast to a target type
        /// </summary>
        /// <param name="origin">The origin type to cast</param>
        /// <param name="target">The target type to cast</param>
        /// <returns>Whether the origin type can be explicitly casted to the target type</returns>
        public bool CanExplicitCast(TypeDef origin, TypeDef target)
        {
            // Cannot convert voids
            if (origin.IsVoid || target.IsVoid)
                return false;

            // Casts to and from any is possible
            if (origin.IsAny || target.IsAny)
                return true;

            // Cannot convert to null types
            if (target == NullType())
                return false;

            // Same casts are always valid
            if (origin == target)
                return true;

            // numeric -> numeric
            if (_binaryExpressionProvider.IsNumeric(origin) && _binaryExpressionProvider.IsNumeric(target))
                return true;

            // numeric, logical -> string
            if ((_binaryExpressionProvider.IsNumeric(origin) || _binaryExpressionProvider.IsLogicType(origin)) && target == StringType())
                return true;

            // Callables
            if (origin is CallableTypeDef && target is CallableTypeDef)
                return CheckCallableCompatibility((CallableTypeDef)origin, (CallableTypeDef)target);

            // Booleans can only be compared to booleans
            if ((origin == BooleanType()) != (target == BooleanType()))
                return false;

            // TODO: Improve native type checking to be able to handle primitive value types
            NativeTypeDef nativeOrigin = origin as NativeTypeDef;
            NativeTypeDef nativeTarget = target as NativeTypeDef;

            if (origin.IsNative && target.IsNative && nativeOrigin != null && nativeTarget != null)
            {
                return nativeTarget.NativeType.IsAssignableFrom(nativeOrigin.NativeType);
            }

            return false;
        }

        /// <summary>
        /// Returns whether an origin type can be implicitly cast to a target type
        /// </summary>
        /// <param name="origin">The origin type to cast</param>
        /// <param name="target">The target type to cast</param>
        /// <returns>Whether the origin type can be implicitly casted to the target type</returns>
        public bool CanImplicitCast(TypeDef origin, TypeDef target)
        {
            // Cannot convert voids
            if (origin.IsVoid || target.IsVoid)
                return false;

            // Same casts are always valid
            if (origin == target)
                return true;

            // Assigning from null is allowed
            if (origin == NullType())
                return true;

            // int -> float
            if (origin == IntegerType() && target == FloatType())
                return true;

            // Casts to and from any is possible
            if (origin.IsAny || target.IsAny)
                return true;

            // Callables
            if (origin is CallableTypeDef && target is CallableTypeDef)
                return CheckCallableCompatibility((CallableTypeDef)origin, (CallableTypeDef)target);

            // Booleans can only be compared to booleans
            if ((origin == BooleanType()) != (target == BooleanType()))
                return false;

            // TODO: Improve native type checking to be able to handle primitive value types
            NativeTypeDef nativeOrigin = origin as NativeTypeDef;
            NativeTypeDef nativeTarget = target as NativeTypeDef;

            if (origin.IsNative && target.IsNative && nativeOrigin != null && nativeTarget != null)
            {
                return nativeTarget.NativeType.IsAssignableFrom(nativeOrigin.NativeType);
            }

            return false;
        }

        /// <summary>
        /// Tests whether two given callable type definitions are compatible
        /// </summary>
        /// <param name="origin">The origin type to check</param>
        /// <param name="target">The target type to check</param>
        /// <returns>Whether the origin callable type is compatible with the given target callable type</returns>
        private bool CheckCallableCompatibility(CallableTypeDef origin, CallableTypeDef target)
        {
            // Check required argument count
            if (origin.RequiredArgumentsCount != target.RequiredArgumentsCount)
                return false;

            // Check implicit return type, ignoring void return types on the target
            // (since the origin return value will never be used, if the target's return value is void)
            if (!target.ReturnType.IsVoid && !CanImplicitCast(origin.ReturnType, target.ReturnType))
                return false;

            // Check argument implicit casts
            int c = Math.Min(origin.RequiredArgumentsCount, target.RequiredArgumentsCount);
            for (int i = 0; i < c; i++)
            {
                if (!CanImplicitCast(origin.ParameterTypes[i], target.ParameterTypes[i]))
                    return false;
            }

            // Callable types are compatible
            return true;
        }
        
        /// <summary>
        /// Returns the type to associate with 'any' values in the runtime
        /// </summary>
        /// <returns>The type to associate with 'any' values in the runtime</returns>
        public TypeDef AnyType()
        {
            return TypeDef.AnyType;
        }

        /// <summary>
        /// Returns the type to associate with void values in the runtime
        /// </summary>
        /// <returns>The type to associate with void values in the runtime</returns>
        public TypeDef VoidType()
        {
            return TypeDef.VoidType;
        }

        /// <summary>
        /// Returns the type to associate with null values in the runtime
        /// </summary>
        /// <returns>The type to associate with null values in the runtime</returns>
        public TypeDef NullType()
        {
            return TypeDef.NullType;
        }

        /// <summary>
        /// Returns the type to associate with integers in the runtime
        /// </summary>
        /// <returns>The type to associate with integers in the runtime</returns>
        public TypeDef IntegerType()
        {
            return TypeDef.IntegerType;
        }

        /// <summary>
        /// Returns the type to associate with floats in the runtime
        /// </summary>
        /// <returns>The type to associate with floats in the runtime</returns>
        public TypeDef FloatType()
        {
            return TypeDef.FloatType;
        }

        /// <summary>
        /// Returns the type associated with booleans in the runtime
        /// </summary>
        /// <returns>The type associated with booleans in the runtime</returns>
        public TypeDef BooleanType()
        {
            return TypeDef.BooleanType;
        }

        /// <summary>
        /// Returns the type associated with strings in the runtime
        /// </summary>
        /// <returns>The type associated with strings in the runtime</returns>
        public TypeDef StringType()
        {
            return TypeDef.StringType;
        }

        /// <summary>
        /// Returns the type associated with objects in the runtime
        /// </summary>
        /// <returns>The type associated with objects in the runtime</returns>
        public ObjectTypeDef ObjectType()
        {
            return new ObjectTypeDef();
        }

        /// <summary>
        /// Generic static type casting helper method
        /// </summary>
        /// <typeparam name="T">The type to cast the object to</typeparam>
        /// <param name="o">The object to cast</param>
        /// <returns>A casted version of the given object</returns>
        public static T Cast<T>(object o)
        {
            return (T)o;
        }
    }
}