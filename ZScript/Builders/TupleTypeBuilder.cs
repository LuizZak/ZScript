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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;
using ZScript.Runtime;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Builders
{
    /// <summary>
    /// Class responsible for generating tuple structures from a set of parameter types
    /// </summary>
    public class TupleTypeBuilder
    {
        /// <summary>
        /// A suffix to add to the name of tuple structures created
        /// </summary>
        public const string TupleNameSuffix = "_t";

        /// <summary>
        /// A counter used to generate the names of the tuples in the type system
        /// </summary>
        private int _count;

        /// <summary>
        /// The type building context for the tuple type builder
        /// </summary>
        private readonly TypeBuildingContext _typeBuildingContext;

        /// <summary>
        /// The set of mapped types for this tuple type builder
        /// </summary>
        private readonly Dictionary<int, Type> _mappedTypes;

        /// <summary>
        /// Initializes a new instance of the TupleTypeBuilder class with a type building context
        /// </summary>
        /// <param name="typeBuildingContext">The context for the type building process</param>
        public TupleTypeBuilder(TypeBuildingContext typeBuildingContext)
        {
            _typeBuildingContext = typeBuildingContext;

            _mappedTypes = new Dictionary<int, Type>();
        }

        /// <summary>
        /// Clears the cache of mapped types registered on this ClassTypeBuilder
        /// </summary>
        public void ClearCache()
        {
            _mappedTypes.Clear();
            _count = 0;
        }

        /// <summary>
        /// Constructs and returns a type for a given class definition
        /// </summary>
        /// <param name="tuple">The class definition to construct</param>
        public Type ConstructType([NotNull] TupleTypeDef tuple)
        {
            int key = tuple.InnerTypes.Length;
            if (_mappedTypes.TryGetValue(key, out var outTuple))
            {
                return outTuple;
            }

            const TypeAttributes attr = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.Serializable;

            var typeBuilder = _typeBuildingContext.ModuleBuilder.DefineType("$TUPLE" + (_count++) + TupleNameSuffix, attr);
            var genericTypes = typeBuilder.DefineGenericParameters(tuple.InnerTypes.Select((it, i) => "T" + (i + 1)).ToArray());

            typeBuilder.AddInterfaceImplementation(typeof(ITuple));

            // Fill in the fields for the tuple
            var fields = new FieldBuilder[tuple.InnerTypes.Length];
            for (int i = 0; i < tuple.InnerTypes.Length; i++)
            {
                fields[i] = typeBuilder.DefineField("Field" + i, genericTypes[i], FieldAttributes.Public);
            }

            CreateConstructors(tuple, typeBuilder, fields);
            CreateEqualityComparision(typeBuilder, fields);
            CreateGetHashCode(typeBuilder, fields);

            var tupleType = typeBuilder.CreateType();

            _mappedTypes[key] = tupleType;

            return tupleType;
        }

        /// <summary>
        /// Creates the tuple constructors on a given type builder
        /// </summary>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        /// <param name="fields">The fields in the tuple type</param>
        private static void CreateConstructors([NotNull] TupleTypeDef tuple, [NotNull] TypeBuilder builder, [NotNull] IReadOnlyList<FieldBuilder> fields)
        {
            CreateFieldConstructor(tuple, builder, fields);
            CreateCopyConstructor(tuple, builder, fields);
        }

        /// <summary>
        /// Creates the tuple copy constructor on a given type builder
        /// </summary>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        /// <param name="fields">The fields in the tuple type</param>
        private static void CreateCopyConstructor([NotNull] TupleTypeDef tuple, [NotNull] TypeBuilder builder, IReadOnlyList<FieldBuilder> fields)
        {
            var baseConst = typeof(object).GetConstructor(Type.EmptyTypes);
            Debug.Assert(baseConst != null, nameof(baseConst) + " != null");

            var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[]{ builder });
            var ilGenerator = constructor.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, baseConst);

            // Emit argument moving
            for (int i = 0; i < tuple.InnerTypes.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Ldfld, fields[i]);
                ilGenerator.Emit(OpCodes.Stfld, fields[i]);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates the tuple field constructor on a given type builder
        /// </summary>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        /// <param name="fields">The fields in the tuple type</param>
        private static void CreateFieldConstructor([NotNull] TupleTypeDef tuple, [NotNull] TypeBuilder builder, [NotNull] IReadOnlyList<FieldBuilder> fields)
        {
            var baseConst = typeof(object).GetConstructor(Type.EmptyTypes);
            Debug.Assert(baseConst != null, nameof(baseConst) + " != null");

            var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, fields.Select(t => t.FieldType).ToArray());
            var ilGenerator = constructor.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, baseConst);

            // Emit argument moving
            for (int i = 0; i < tuple.InnerTypes.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                switch (i + 1)
                {
                    case 1:
                        ilGenerator.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        ilGenerator.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        ilGenerator.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        ilGenerator.Emit(OpCodes.Ldarga_S, (short)(i + 1));
                        break;
                }
                ilGenerator.Emit(OpCodes.Stfld, fields[i]);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates the tuple equality comparision on a given type builder
        /// </summary>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        /// <param name="fields">The fields in the tuple type</param>
        private static void CreateEqualityComparision([NotNull] TypeBuilder builder, [NotNull] IReadOnlyList<FieldBuilder> fields)
        {
            // Fetch the default 'equals' method of the Object class, which will be used to override the 'Equals' of the type
            var equalsMethod = typeof(object).GetMethod("Equals", new [] { typeof(object) });
            Debug.Assert(equalsMethod != null, nameof(equalsMethod) + " != null");
            
            // Fetch the static 'Equals' method of the Object class, which will be used to compare the tuple fields
            var equalsStaticMethod = typeof (object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
            Debug.Assert(equalsStaticMethod != null, nameof(equalsStaticMethod) + " != null");

            var equality = builder.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(bool), new [] { typeof(object) });

            // Define the override of the 'Equals' method
            builder.DefineMethodOverride(equality, equalsMethod);

            // Start writing the code
            var ilGenerator = equality.GetILGenerator();

            // Declare the local to store the casted tuple object we are going to compare against
            ilGenerator.DeclareLocal(builder);

            // The label to jump to if the conversion succeeds
            var labelNotNull = ilGenerator.DefineLabel();

            //// The next lines are equivalent to:
            ////  var tuple = other as $TUPLE_1<T1, T2, T3..., TN>
            ////  if(tuple == null)
            ////      return false;
            
            // Loads the 'object other' parameter and test whether it is an instance of this class
            ilGenerator.Emit(OpCodes.Ldarg_1);
            
            // var tuple = other as $TUPLE_1<T1, T2, T3..., TN> 
            ilGenerator.Emit(OpCodes.Isinst, builder);
            
            ilGenerator.Emit(OpCodes.Stloc_0);

            // Verify it is not null, if it is not, skip the next return statement
            // if(tuple == null)
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Brtrue_S, labelNotNull);

            // Return false
            //     return false;
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Ret);

            // Jump taret when not false
            ilGenerator.MarkLabel(labelNotNull);

            //// The next lines are equivalent to:
            ////  return Object.Equals(field0, other.field0) && Object.Equals(field1, other.field1) && ... && Object.Equals(fieldN, other.fieldN);
            
            // Label to jump to if any of the comparisions fails
            var labelReturnFalse = ilGenerator.DefineLabel();

            // Go field by field comparing the types
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var type = builder.GenericTypeParameters[i];

                // Load the local field (equivalent to this.fieldN)
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, field);
                ilGenerator.Emit(OpCodes.Box, type);

                // Load the field from the other tuple (equivalent to tuple.fieldN)
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldfld, field);
                ilGenerator.Emit(OpCodes.Box, type);

                // Call Object.Equals() with the arguments
                ilGenerator.Emit(OpCodes.Call, equalsStaticMethod);

                // If this is the last comparision, return its result, otherwise, test jump to the false branch
                if (i == fields.Count - 1)
                {
                    ilGenerator.Emit(OpCodes.Ret);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Brfalse_S, labelReturnFalse);
                }
            }

            // Build the return false bit
            ilGenerator.MarkLabel(labelReturnFalse);

            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Implements the GetHashCode() method for the given tuple type builder
        /// </summary>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        /// <param name="fields">The fields in the tuple type</param>
        private static void CreateGetHashCode([NotNull] TypeBuilder builder, [NotNull] IReadOnlyList<FieldBuilder> fields)
        {
            // Fetch the default 'GetHashCode' method of the Object class, which will be used to equate the fields of the tuples bellow
            var baseHashCode = typeof(object).GetMethod("GetHashCode", Type.EmptyTypes);
            Debug.Assert(baseHashCode != null, nameof(baseHashCode) + " != null");

            var hashCodeMethod = builder.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(int), Type.EmptyTypes);
            Debug.Assert(hashCodeMethod != null, nameof(hashCodeMethod) + " != null");

            // The GetHashCode implementation bellow is an implementation of a FNV hash method as bellow:

            // unchecked
            // {
            //    int hash = (int)2166136261;
            //    
            //    hash = (hash * 16777619) ^ (field0 == null ? 0 : field0.GetHashCode());
            //    hash = (hash * 16777619) ^ (field1 == null ? 0 : field1.GetHashCode());
            //    ...
            //    hash = (hash * 16777619) ^ (fieldN == null ? 0 : fieldN.GetHashCode());
            //    
            //    return hash;
            // }

            // Define the override of the 'GetHashCode' method
            builder.DefineMethodOverride(hashCodeMethod, baseHashCode);

            // Start writing the code
            var ilGenerator = hashCodeMethod.GetILGenerator();

            // Define the 'hash' local
            ilGenerator.DeclareLocal(typeof (int));

            // Store the inital 'hash' varuiable's value
            unchecked { ilGenerator.Emit(OpCodes.Ldc_I4, (int)2166136261); }

            ilGenerator.Emit(OpCodes.Stloc_0);
            
            // Iterate over each field and return the hashcode for that field
            for (int i = 0; i < fields.Count; i++)
            {
                // Jump to peform in case the field is null
                var nullJump = ilGenerator.DefineLabel();
                // Jump to perform in case the field is not null, used to jump over the '0' constant that is loaded
                // in case the field is null. 
                var over0Jump = ilGenerator.DefineLabel();

                var field = fields[i];
                var fieldType = builder.GenericTypeParameters[i];

                // Load the 'heap' and multiply by 16777619
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldc_I4, 16777619);
                ilGenerator.Emit(OpCodes.Mul);

                // Check 'this.fieldN' is not null
                ilGenerator.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilGenerator.Emit(OpCodes.Ldfld, field); // Load 'fieldN'

                // Box the value to compare to null
                ilGenerator.Emit(OpCodes.Box, fieldType);

                // Jump to constant 0 if field is null
                ilGenerator.Emit(OpCodes.Brfalse_S, nullJump);

                // Call 'this.fieldN.GetHashCode()'
                ilGenerator.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilGenerator.Emit(OpCodes.Ldflda, field); // Load 'fieldN'
                ilGenerator.Emit(OpCodes.Constrained, fieldType); // Constrain it to 'Tn'
                ilGenerator.Emit(OpCodes.Callvirt, baseHashCode); // Call System.Object.GetHashCode(), with 'fieldN' as the target called

                // Jump over 0 constant in case field was calculated
                ilGenerator.Emit(OpCodes.Br_S, over0Jump);

                ilGenerator.MarkLabel(nullJump);
                ilGenerator.Emit(OpCodes.Ldc_I4_0);

                ilGenerator.MarkLabel(over0Jump);
                // Apply 'xor' on 'heap' and 'this.fieldN.GetHashCode()'
                ilGenerator.Emit(OpCodes.Xor);

                // Store back to 'heap'
                ilGenerator.Emit(OpCodes.Stloc_0);
            }

            // Load 'heap' and return
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}