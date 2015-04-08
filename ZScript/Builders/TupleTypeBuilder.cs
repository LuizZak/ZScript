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
using System.Reflection.Emit;

using ZScript.Runtime;
using ZScript.Runtime.Typing;
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
        private readonly Dictionary<TupleTypeDef, Type> _mappedTypes;

        /// <summary>
        /// Initializes a new instance of the TupleTypeBuilder class with a type building context
        /// </summary>
        /// <param name="typeBuildingContext">The context for the type building process</param>
        public TupleTypeBuilder(TypeBuildingContext typeBuildingContext)
        {
            _typeBuildingContext = typeBuildingContext;

            _mappedTypes = new Dictionary<TupleTypeDef, Type>();
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
        /// <param name="typeProvider">The type provider to get the type of the tuple fields to populate</param>
        /// <param name="tuple">The class definition to construct</param>
        public Type ConstructType(TypeProvider typeProvider, TupleTypeDef tuple)
        {
            if (_mappedTypes.ContainsKey(tuple))
            {
                return _mappedTypes[tuple];
            }

            const TypeAttributes attr = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.Serializable;

            var typeBuilder = _typeBuildingContext.ModuleBuilder.DefineType("$TUPLE" + (_count++) + TupleNameSuffix, attr);

            typeBuilder.AddInterfaceImplementation(typeof(ITuple));

            // Fill in the fields for the tuple
            var fields = new FieldBuilder[tuple.InnerTypes.Length];
            for (int i = 0; i < tuple.InnerTypes.Length; i++)
            {
                var innerType = tuple.InnerTypes[i];
                var fieldType = typeProvider.NativeTypeForTypeDef(innerType);

                fields[i] = typeBuilder.DefineField("Field" + i, fieldType, FieldAttributes.Public);
            }

            CreateConstructors(typeProvider, tuple, typeBuilder, fields);

            var tupleType = typeBuilder.CreateType();

            _mappedTypes[tuple] = tupleType;

            return tupleType;
        }

        /// <summary>
        /// Creates the tuple constructors on a given type builder
        /// </summary>
        /// <param name="typeProvider">The type provider to get the type of the tuple fields to populate</param>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        private void CreateConstructors(TypeProvider typeProvider, TupleTypeDef tuple, TypeBuilder builder, FieldBuilder[] fields)
        {
            CreateFieldConstructor(typeProvider, tuple, builder, fields);
            CreateCopyConstructor(tuple, builder, fields);
        }

        /// <summary>
        /// Creates the tuple copy constructor on a given type builder
        /// </summary>
        /// <param name="typeProvider">The type provider to get the type of the tuple fields to populate</param>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        private void CreateCopyConstructor(TupleTypeDef tuple, TypeBuilder builder, FieldBuilder[] fields)
        {
            var baseConst = typeof(object).GetConstructor(Type.EmptyTypes);

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
        /// <param name="typeProvider">The type provider to get the type of the tuple fields to populate</param>
        /// <param name="tuple">The tuple containing the fields to initialize</param>
        /// <param name="builder">The type builder to send the tuple fiends into</param>
        private void CreateFieldConstructor(TypeProvider typeProvider, TupleTypeDef tuple, TypeBuilder builder, FieldBuilder[] fields)
        {
            var arguments = tuple.InnerTypes.Select(t => typeProvider.NativeTypeForTypeDef(t)).ToArray();

            var baseConst = typeof(object).GetConstructor(Type.EmptyTypes);

            var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, arguments);
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
    }
}