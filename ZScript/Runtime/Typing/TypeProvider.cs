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

using System.Reflection;
using ZScript.Elements;
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
        /// List of custom type sources for the program
        /// </summary>
        private readonly List<ICustomTypeSource> _customTypeSources;

        /// <summary>
        /// List of custom native type sources for the program
        /// </summary>
        private readonly List<INativeTypeSource> _customNativeTypeSources;

        /// <summary>
        /// Gets type provider used to resolve binary expressions
        /// </summary>
        public BinaryExpressionTypeProvider BinaryExpressionProvider
        {
            get { return _binaryExpressionProvider; }
        }

        /// <summary>
        /// Whether to provide default values for value types when trying to convert null to value types with <see cref="CastObject"/>
        /// </summary>
        public bool DefaultValueForNullValueType = true;

        /// <summary>
        /// Initializes a new instance of the TypeProvider class
        /// </summary>
        public TypeProvider()
        {
            _binaryExpressionProvider = new BinaryExpressionTypeProvider(this);
            _customTypeSources = new List<ICustomTypeSource>();
            _customNativeTypeSources = new List<INativeTypeSource>();

            RegisterCustomNativeTypeSource(new InternalNativeTypeConverter(this));
        }

        /// <summary>
        /// Registers a new custom type source in this type provider
        /// </summary>
        /// <param name="source">The type source to register on this provider</param>
        public void RegisterCustomTypeSource(ICustomTypeSource source)
        {
            _customTypeSources.Add(source);
        }

        /// <summary>
        /// Unregisters a new custom type source in this type provider, returning a value that
        /// specifies whether the source was registered on this type provider before being removed
        /// </summary>
        /// <param name="source">The type source to register on this provider</param>
        /// <returns>true if the source was registered before the call; false otherwise</returns>
        public bool UnregisterCustomNativeTypeSource(INativeTypeSource source)
        {
            return _customNativeTypeSources.Remove(source);
        }

        /// <summary>
        /// Registers a new custom native type source in this type provider
        /// </summary>
        /// <param name="source">The native type source to register on this provider</param>
        public void RegisterCustomNativeTypeSource(INativeTypeSource source)
        {
            _customNativeTypeSources.Add(source);
        }

        /// <summary>
        /// Unregisters a new custom native type source in this type provider, returning a value that
        /// specifies whether the source was registered on this type provider before being removed
        /// </summary>
        /// <param name="source">The native type source to register on this provider</param>
        /// <returns>true if the source was registered before the call; false otherwise</returns>
        public bool UnregisterCustomTypeSource(ICustomTypeSource source)
        {
            return _customTypeSources.Remove(source);
        }

        /// <summary>
        /// Performs a cast on an object from its type to a type provided
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="newType">The new type to convert to</param>
        /// <returns></returns>
        public object CastObject(object value, Type newType)
        {
            if (newType == null) throw new ArgumentNullException("newType");
            // Resolve nulls
            if (value == null)
            {
                if (newType.IsValueType)
                {
                    if (DefaultValueForNullValueType)
                        return Activator.CreateInstance(newType);

                    throw new Exception("Cannot convert 'null' to value type " + newType);
                }

                return null;
            }

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
        /// Returns a TypeDef that matches the given type name.
        /// The method returns null, if the type name provided does not exists
        /// </summary>
        /// <param name="typeName">The name of the type to get</param>
        /// <returns>The type that references the provided type name, or null, if no type was found</returns>
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
            
            // Search the custom type sources
            foreach (var customSource in _customTypeSources)
            {
                if (customSource.HasType(typeName))
                    return customSource.TypeNamed(typeName);
            }

            return null;
            //return new TypeDef(typeName, true);
        }

        /// <summary>
        /// Returns a native equivalent for a given TypeDef.
        /// If no equivalent native type is found, null is returned
        /// </summary>
        /// <param name="typeDef">The type to get the native equivalent of</param>
        /// <param name="anyAsObject">Whether to resolve 'any' types as 'object'. If left false, 'any' types return null</param>
        /// <returns>A Type that represents a native equivalent for the given type</returns>
        public Type NativeTypeForTypeDef(TypeDef typeDef, bool anyAsObject = false)
        {
            // Search in the native type sources
            foreach (var source in _customNativeTypeSources)
            {
                var ret = source.NativeTypeForTypeDef(typeDef, anyAsObject);

                if (ret != null)
                    return ret;
            }

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
        /// Returns a type that represents a dictionary of keys mapped into values of given types
        /// </summary>
        /// <param name="keyType">The type of the keys that will map the dictionary</param>
        /// <param name="valueType">The type of values that will be mapped in the dictionary</param>
        /// <returns>A new DictionaryTypeDef that represents the created dictionary type</returns>
        public DictionaryTypeDef DictionaryForTypes(TypeDef keyType, TypeDef valueType)
        {
            return new DictionaryTypeDef(keyType, valueType);
        }

        /// <summary>
        /// Returns a TupleTypeDef which contains the given set of inner types in it
        /// </summary>
        /// <param name="innerTypes">The inner types for the tuple</param>
        /// <returns>A new TupleTypeDef with the specified inner types containg within</returns>
        public TupleTypeDef TupleForTypes(params TypeDef[] innerTypes)
        {
            return new TupleTypeDef(innerTypes);
        }

        /// <summary>
        /// Returns a TupleTypeDef which contains the given set of inner types in it
        /// </summary>
        /// <param name="typeNames">An optional array of matching names for the tuple entries</param>
        /// <param name="innerTypes">The inner types for the tuple</param>
        /// <returns>A new TupleTypeDef with the specified inner types containg within</returns>
        public TupleTypeDef TupleForTypes(string[] typeNames, TypeDef[] innerTypes)
        {
            return new TupleTypeDef(typeNames, innerTypes);
        }

        /// <summary>
        /// Returns a optional type for the given type def
        /// </summary>
        /// <param name="type">The type to wrap in an optional</param>
        /// <returns>The optional type for the given type definition</returns>
        public OptionalTypeDef OptionalTypeForType(TypeDef type)
        {
            return InternalOptionalTypeForType(type, 0);
        }

        /// <summary>
        /// Returns a optional type for the given type def
        /// </summary>
        /// <param name="type">The type to wrap in an optional</param>
        /// <param name="depth">The depth of the optional to create</param>
        /// <returns>The optional type for the given type definition</returns>
        private OptionalTypeDef InternalOptionalTypeForType(TypeDef type, int depth)
        {
            if (depth <= 0)
                return new OptionalTypeDef(type);

            return new OptionalTypeDef(InternalOptionalTypeForType(type, depth - 1));
        }

        /// <summary>
        /// Tries to come up with the most common type that fits the type of two provided types
        /// </summary>
        /// <param name="type1">The first type to infer</param>
        /// <param name="type2">The second type to infer</param>
        /// <param name="withCast">
        /// Whether to take casts in consideration when finding the common type. Turning off cast considerations disables casting of compatible types e.g. int -> float etc.
        /// </param>
        /// <returns>The most common type that fits the two provided types</returns>
        public TypeDef FindCommonType(TypeDef type1, TypeDef type2, bool withCast = true)
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

            // Null interaction: Return the non-null type, and if both are null, return null as well
            if (type1 == NullType() && type2 == NullType())
                return NullType();
            if (type1 == NullType())
                return type2 is OptionalTypeDef ? type2 : OptionalTypeForType(type2);
            if (type2 == NullType())
                return type1 is OptionalTypeDef ? type1 : OptionalTypeForType(type1);

            // Integer -> Float conversion
            var intType = IntegerType();
            var floatType = FloatType();

            if (withCast && (type1 == intType || type2 == intType) && (type1 == floatType || type2 == floatType))
                return floatType;

            // Optional and non-optional type
            var opt1 = type1 as OptionalTypeDef;
            var opt2 = type2 as OptionalTypeDef;
            // Type 1 is non-optional / Type 2 is optional
            if (opt1 != null && opt2 == null)
            {
                if (opt1.BaseWrappedType == type2)
                {
                    return InternalOptionalTypeForType(type2, opt1.OptionalDepth);
                }
            }
            // Type 1 is optional / Type 2 is non-optional
            else if(opt1 == null && opt2 != null)
            {
                if (opt2.BaseWrappedType == type1)
                {
                    return InternalOptionalTypeForType(type1, opt2.OptionalDepth);
                }
            }
            // Type 1 and type2 are optional
            else if (opt1 != null)
            {
                if (opt1.BaseWrappedType == opt2.BaseWrappedType)
                {
                    return InternalOptionalTypeForType(opt1.BaseWrappedType, Math.Max(opt1.OptionalDepth, opt2.OptionalDepth));
                }

                //return InternalOptionalTypeForType(AnyType(), Math.Max(opt1.OptionalDepth, opt2.OptionalDepth));
                return AnyType();
            }

            // Inheritable types
            var classT1 = type1 as IInheritableTypeDef;
            var classT2 = type2 as IInheritableTypeDef;
            if (classT1 != null && classT2 != null)
            {
                // Check inheritance
                if (classT1.IsSubtypeOf(classT2))
                {
                    return type2;
                }
                if (classT2.IsSubtypeOf(classT1))
                {
                    return type1;
                }

                // Search in the inheritance chain of one of the classes
                var b = classT1.BaseType;

                while (b != null)
                {
                    if (classT1.IsSubtypeOf(b) && classT2.IsSubtypeOf(b))
                        return b;

                    if (b is IInheritableTypeDef)
                    {
                        b = ((IInheritableTypeDef)b).BaseType;
                    }
                    else
                    {
                        break;
                    }
                }
            }

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
                                FindCommonType(pt1, pt2, false), true,
                                t.HasDefault || ct2.ParameterInfos[i].HasDefault,
                                t.IsVariadic && ct2.ParameterInfos[i].IsVariadic);
                        });

                TypeDef newReturn = null;

                // Deal with return type providing
                if (ct1.HasReturnType && ct2.HasReturnType)
                {
                    var rt1 = ct1.ReturnType ?? ct2.ReturnType ?? TypeDef.AnyType;
                    var rt2 = ct2.ReturnType ?? ct1.ReturnType ?? TypeDef.AnyType;

                    newReturn = FindCommonType(rt1, rt2, false);
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
            var list1 = type1 as ListTypeDef;
            var list2 = type2 as ListTypeDef;
            if (list1 != null && list2 != null)
            {
                if (list1 == list2)
                    return list1;

                // Callable type resolving
                if (list1.EnclosingType is CallableTypeDef && list2.EnclosingType is CallableTypeDef)
                {
                    var common = FindCommonType(list1.EnclosingType, list2.EnclosingType, false);

                    if(!common.IsAny)
                        return ListForType(common);
                }
            }

            // Last case: Any type to fit them all
            return AnyType();
        }

        /// <summary>
        /// Returns a value specifying whether the given type is an enumerable type
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <returns>True if type is enumerable, false otherwise</returns>
        public bool IsEnumerable(TypeDef type)
        {
            return type is ListTypeDef;
        }

        /// <summary>
        /// Returns whether an origin type can be cast to a target type
        /// </summary>
        /// <param name="origin">The origin type to cast</param>
        /// <param name="target">The target type to cast</param>
        /// <returns>Whether the origin type can be explicitly casted to the target type</returns>
        public bool CanExplicitCast(TypeDef origin, TypeDef target)
        {
            // Explicit casts can always be performed if implicit casts can be performed
            if (CanImplicitCast(origin, target))
                return true;

            // Cannot convert voids
            if (origin.IsVoid || target.IsVoid)
                return false;

            // Casts to and from any are always possible
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

            // Inheritable typing
            if (origin is IInheritableTypeDef && target is IInheritableTypeDef)
                return CanExplicitCastInheritable((IInheritableTypeDef)origin, (IInheritableTypeDef)target);

            // TODO: Improve native type checking to be able to handle primitive value types
            NativeTypeDef nativeOrigin = origin as NativeTypeDef;
            NativeTypeDef nativeTarget = target as NativeTypeDef;

            if (origin.IsNative && target.IsNative && nativeOrigin != null && nativeTarget != null)
            {
                return NativeTypeForTypeDef(nativeTarget).IsAssignableFrom(NativeTypeForTypeDef(nativeOrigin)) || CanExplicitCastPrimitive(NativeTypeForTypeDef(nativeOrigin), NativeTypeForTypeDef(nativeTarget));
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

            // int -> float
            if (origin == IntegerType() && target == FloatType())
                return true;

            // Casts to and from any are always possible
            if (origin.IsAny || target.IsAny)
                return true;

            // Callables
            if (origin is CallableTypeDef && target is CallableTypeDef)
                return CheckCallableCompatibility((CallableTypeDef)origin, (CallableTypeDef)target);

            // Booleans can only be compared to booleans
            if ((origin == BooleanType()) != (target == BooleanType()))
                return false;

            // Class typing
            if (origin is IInheritableTypeDef && target is IInheritableTypeDef)
                return CanImplicitCastInheritable((IInheritableTypeDef)origin, (IInheritableTypeDef)target);

            // Optional typing
            var optionalTarget = target as OptionalTypeDef;
            if (optionalTarget != null)
            {
                if (CanImplicitCast(origin, optionalTarget.WrappedType))
                    return true;
            }

            if (AreTypesCompatible(origin, target))
                return true;

            // TODO: Improve native type checking to be able to handle primitive value types
            NativeTypeDef nativeOrigin = origin as NativeTypeDef;
            NativeTypeDef nativeTarget = target as NativeTypeDef;

            if (origin.IsNative && target.IsNative && nativeOrigin != null && nativeTarget != null)
            {
                Type typeOrigin = NativeTypeForTypeDef(nativeOrigin);
                Type typeTarget = NativeTypeForTypeDef(nativeTarget);

                return typeTarget.IsAssignableFrom(typeOrigin) || CanImplicitCastPrimitive(typeOrigin, typeTarget);
            }

            return false;
        }

        /// <summary>
        /// Returns whether the two inheritable types can be implicitly casted from one to another
        /// </summary>
        /// <param name="origin">The origin type to cast from</param>
        /// <param name="target">The type target to cast to</param>
        /// <returns>Whether the types can be implicitly casted</returns>
        private bool CanImplicitCastInheritable(IInheritableTypeDef origin, IInheritableTypeDef target)
        {
            // Check if origin is in the inheritance chain of target
            var curC = origin;

            while (curC != null)
            {
                if (curC == target)
                    return true;
                
                curC = curC.BaseType as IInheritableTypeDef;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the two inheritable types can be explicitly casted from one to another
        /// </summary>
        /// <param name="origin">The origin type to cast from</param>
        /// <param name="target">The type target to cast to</param>
        /// <returns>Whether the types can be explicitly casted</returns>
        private bool CanExplicitCastInheritable(IInheritableTypeDef origin, IInheritableTypeDef target)
        {
            // Implicit casts enable explicit casts by default
            if (CanImplicitCastInheritable(origin, target))
                return true;

            // Check if target is derived from origin by checking if it's in the inheritance chain of origin
            var curC = target;

            while (curC != null)
            {
                if (curC == origin)
                    return true;

                curC = curC.BaseType as IInheritableTypeDef;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the two primitive types can be implicitly casted from one to another
        /// </summary>
        /// <param name="origin">The origin type to cast from</param>
        /// <param name="target">The type target to cast to</param>
        /// <returns>Whether the types can be implicitly casted</returns>
        private bool CanImplicitCastPrimitive(Type origin, Type target)
        {
            // int -> long
            if (origin == typeof(int) && target == typeof(long))
                return true;
            // int -> double
            if (origin == typeof(int) && target == typeof(double))
                return true;
            // long -> double
            if (origin == typeof(long) && target == typeof(double))
                return true;
            // string -> char
            if (origin == typeof(string) && target == typeof(char))
                return true;

            return false;
        }

        /// <summary>
        /// Returns whether the two primitive types can be explicitly casted from one to another
        /// </summary>
        /// <param name="origin">The origin type to cast from</param>
        /// <param name="target">The type target to cast to</param>
        /// <returns>Whether the types can be explicitly casted</returns>
        private bool CanExplicitCastPrimitive(Type origin, Type target)
        {
            // If they can be implicitly casted, they can also be explicitly casted
            if (CanImplicitCastPrimitive(origin, target))
                return true;

            // long -> int
            if (origin == typeof(long) && target == typeof(int))
                return true;
            // double -> int
            if (origin == typeof(double) && target == typeof(int))
                return true;
            // string -> char
            if (origin == typeof(string) && target == typeof(char))
                return true;

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
            if (!target.ReturnType.IsVoid)
            {
                // Optional configuration
                if (!AreTypesCompatible(target.ReturnType, origin.ReturnType))
                    return false;
            }

            // Check argument implicit casts
            int c = Math.Min(origin.RequiredArgumentsCount, target.RequiredArgumentsCount);
            for (int i = 0; i < c; i++)
            {
                var typeO = origin.ParameterTypes[i];
                var typeT = target.ParameterTypes[i];

                if (!AreTypesCompatible(typeO, typeT))
                    return false;
            }

            // Check variadic type compatibility
            if (target.HasVariadic)
            {
                if (!origin.HasVariadic)
                    return false;

                var varT = target.VariadicParameter.RawParameterType;
                var varO = origin.VariadicParameter.RawParameterType;

                if (!AreTypesCompatible(varO, varT))
                    return false;
            }

            // Callable types are compatible
            return true;
        }

        /// <summary>
        /// Method used to help deal with value compatibility of types that are passed around
        /// </summary>
        /// <param name="origin">The origin type to verify</param>
        /// <param name="target">The target type to verify</param>
        /// <returns>Whether the types are compatible or not</returns>
        public bool AreTypesCompatible(TypeDef origin, TypeDef target)
        {
            // Types match
            if (origin == target)
                return true;
            
            // Assigning from null is allowed only on optionals
            if (origin == NullType() && target is OptionalTypeDef)
                return true;

            // Tuples
            var tupO = origin as TupleTypeDef;
            var tupT = target as TupleTypeDef;
            if (tupO != null && tupT != null)
            {
                if (tupT.InnerTypes.Length != tupO.InnerTypes.Length)
                    return false;

                for (int i = 0; i < tupT.InnerTypes.Length; i++)
                {
                    if (!AreTypesCompatible(tupO.InnerTypes[i], tupT.InnerTypes[i]))
                        return false;
                }

                return true;
            }

            // Optionallity
            var optO = origin as OptionalTypeDef;
            var optT = target as OptionalTypeDef;

            if (optO != null && optT != null)
                return AreTypesCompatible(optO.WrappedType, optT.WrappedType);

            // Optional parameter compatibility rules:
            // The test passes if any of the conditions are true:
            // 
            // 1. Target and origin have the same optionality configuration (same underlying type, same depth)
            // 2. Origin has an optional type which wraps the target type
            if (optO != null && optO.WrappedType == target)
                return true;
            if (optT != null && optT.WrappedType == origin)
                return true;

            // Inheritance
            if (origin is IInheritableTypeDef && ((IInheritableTypeDef)origin).IsSubtypeOf(target))
            {
                return true;
            }

            if (origin != target && !origin.IsAny)
                return false;

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

        /// <summary>
        /// The internal native type converter
        /// </summary>
        class InternalNativeTypeConverter : INativeTypeSource
        {
            /// <summary>
            /// The parent type provider for this native type converter
            /// </summary>
            private readonly TypeProvider _typeProvider;

            /// <summary>
            /// Initializes a new instance of the InternalNativeTypeConverter class
            /// </summary>
            /// <param name="typeProvider">The owning InternalNativeTypeConverter for this class</param>
            public InternalNativeTypeConverter(TypeProvider typeProvider)
            {
                _typeProvider = typeProvider;
            }

            // 
            // INativeTypeSource.NativeTypeForTypeDef implementation
            // 
            public Type NativeTypeForTypeDef(TypeDef typeDef, bool anyAsObject)
            {
                // Lists
                var listTypeDef = typeDef as ListTypeDef;
                if (listTypeDef != null)
                {
                    // Get the native type definition if the inner type
                    var innerType = _typeProvider.NativeTypeForTypeDef(listTypeDef.EnclosingType, anyAsObject);
                    var listType = typeof(List<>);

                    return listType.MakeGenericType(innerType ?? typeof(object));
                }

                // Dictionaries
                var dictTypeDef = typeDef as DictionaryTypeDef;
                if (dictTypeDef != null)
                {
                    var keyType = _typeProvider.NativeTypeForTypeDef(dictTypeDef.SubscriptType, anyAsObject);
                    var valueType = _typeProvider.NativeTypeForTypeDef(dictTypeDef.EnclosingType, anyAsObject);
                    var dictType = typeof(Dictionary<,>);

                    return dictType.MakeGenericType(keyType ?? typeof(object), valueType ?? typeof(object));
                }

                // Native types
                var nativeTypeDef = typeDef as NativeTypeDef;
                if (nativeTypeDef != null)
                {
                    return nativeTypeDef.NativeType;
                }

                // Null
                if (typeDef == _typeProvider.NullType())
                {
                    return typeof(object);
                }

                // Any
                if (anyAsObject && typeDef == _typeProvider.AnyType())
                {
                    return typeof(object);
                }

                // Optional
                var optDef = typeDef as OptionalTypeDef;
                if (optDef != null)
                {
                    var nativeWrapped = _typeProvider.NativeTypeForTypeDef(optDef.BaseWrappedType, true);

                    // Create the option instance
                    return typeof(Optional<>).MakeGenericType(nativeWrapped);
                }

                // Tuple
                // TODO: Implement proper tuple reporting here
                if (typeDef is TupleTypeDef)
                {
                    return typeof(List<object>);
                }

                // No equivalents
                return null;
            }
        }
    }

    /// <summary>
    /// Class that contains helper type methods
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Returns a value specifying whether a IInheritableTypeDef is a subtype of a given type
        /// </summary>
        /// <param name="inheritedType">The inherited type to analyze</param>
        /// <param name="type">The type to search for</param>
        /// <returns>Whether the given type is in the inheritance chain of this class type def</returns>
        public static bool IsSubtypeOf(this IInheritableTypeDef inheritedType, ITypeDef type)
        {
            var b = (TypeDef)inheritedType;

            while (b != null)
            {
                if (b == (type as TypeDef))
                {
                    return true;
                }
                if (b is IInheritableTypeDef)
                {
                    b = ((IInheritableTypeDef)b).BaseType;
                }
                else
                {
                    break;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Interface to be implemented by an object capable of returning a native type when presented with a NativeTypeDef-type class
    /// </summary>
    public interface INativeTypeSource
    {
        /// <summary>
        /// Gets a native type definition for the given type definition
        /// </summary>
        /// <param name="type">The type defintiion to get the native type of</param>
        /// <param name="anyAsObject">Whether to resolve 'any' types as 'object'. If left false, 'any' types return null</param>
        /// <returns>A type that matches the given native type definition</returns>
        Type NativeTypeForTypeDef(TypeDef type, bool anyAsObject = false);
    }
}