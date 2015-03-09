using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using ZScript.CodeGeneration.Definitions;
using ZScript.Elements;
using ZScript.Runtime;

namespace ZScript.Builders
{
    /// <summary>
    /// Adds support for constructing a type from a class definition
    /// </summary>
    public class ClassTypeBuilder
    {
        /// <summary>
        /// The class name suffix to append to the end of type names when generating types
        /// </summary>
        public const string ClassNameSuffix = "_class";

        /// <summary>
        /// Suffix indexer used to resolve class collisions during generation
        /// </summary>
        private static int _classSuffix;

        /// <summary>
        /// Dictionary that maps class definition objects into types
        /// </summary>
        private readonly Dictionary<ClassDefinition, Type> _mappedTypes;

        /// <summary>
        /// The type building context used during type buiding
        /// </summary>
        private readonly TypeBuildingContext _typeBuildingContext;

        /// <summary>
        /// Initializes a new instance of the ClassTypeBuilder class
        /// </summary>
        /// <param name="typeBuildingContext">A type building context used during type buiding</param>
        public ClassTypeBuilder(TypeBuildingContext typeBuildingContext)
        {
            _typeBuildingContext = typeBuildingContext;
            _mappedTypes = new Dictionary<ClassDefinition, Type>();
        }

        /// <summary>
        /// Clears the cache of mapped types registered on this ClassTypeBuilder
        /// </summary>
        public void ClearCache()
        {
            _typeBuildingContext.ResetContext();
            _mappedTypes.Clear();
            _classSuffix = 0;
        }
        
        /// <summary>
        /// Constructs and returns a type for a given class definition
        /// </summary>
        /// <param name="definition">The class definition to construct</param>
        public Type ConstructType(ClassDefinition definition)
        {
            if (_mappedTypes.ContainsKey(definition))
            {
                return _mappedTypes[definition];
            }

            // Expand super classes first
            if (definition.BaseClass != null)
            {
                ConstructType(definition.BaseClass);
            }

            TypeBuilder typeBuilder;

            try
            {
                typeBuilder = _typeBuildingContext.ModuleBuilder.DefineType(definition.Name + ClassNameSuffix);
            }
            catch (ArgumentException)
            {
                typeBuilder = _typeBuildingContext.ModuleBuilder.DefineType(definition.Name + ClassNameSuffix + _classSuffix++);
            }

            if(definition.BaseClass == null)
            {
                typeBuilder.SetParent(typeof(ZClassInstance));
            }
            else
            {
                typeBuilder.SetParent(_mappedTypes[definition.BaseClass]);
            }

            DefineConstructor(definition, typeBuilder);

            return _mappedTypes[definition] = typeBuilder.CreateType();
        }

        /// <summary>
        /// Defines the default constructor on a given class definition
        /// </summary>
        /// <param name="definition">The class definition to generate the constructor from</param>
        /// <param name="builder">The type builder to generate the constructor on</param>
        private void DefineConstructor(ClassDefinition definition, TypeBuilder builder)
        {
            var constructorDef = definition.PublicConstructor;

            var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new [] { typeof(ZClass) });
            var ilGenerator = constructor.GetILGenerator();

            FillConstructor(ilGenerator, constructorDef);
        }

        /// <summary>
        /// Fills a given IL generator with a set of tokens
        /// </summary>
        /// <param name="generator">The generator to generate the IL on</param>
        /// <param name="tokens">The body of tokens to generate the IL from</param>
        private void FillGeneratorWithTokens(ILGenerator generator, TokenList tokens)
        {
            ParameterModifier[] mod = { new ParameterModifier(1) };
            var constructor = typeof(ZClassInstance).GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.Any, new[] { typeof(ZClass) }, mod);

            if (constructor == null)
            {
                generator.Emit(OpCodes.Ret);
                return;
            }

            // For a constructor, argument zero is a reference to the new
            // instance. Push it on the stack before calling the base
            // class constructor. Specify the default constructor of the 
            // base class (System.Object) by passing an empty array of 
            // types (Type.EmptyTypes) to GetConstructor.
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, constructor);

            generator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Fills a given IL generator with a set of tokens
        /// </summary>
        /// <param name="generator">The generator to generate the tokens on</param>
        /// <param name="constructor">The constructor containing the tokens to generate the IL from</param>
        private void FillConstructor(ILGenerator generator, MethodDefinition constructor)
        {
            ParameterModifier[] mod = { new ParameterModifier(1) };
            var zConst = typeof(ZClassInstance).GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.Any, new[] { typeof(ZClass) }, mod);

            if (zConst == null)
            {
                generator.Emit(OpCodes.Ldarg_0);
                // ReSharper disable once AssignNullToNotNullAttribute
                generator.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
                generator.Emit(OpCodes.Ret);
                return;
            }

            // For a constructor, argument zero is a reference to the new
            // instance. Push it on the stack before calling the base
            // class constructor. Specify the default constructor of the 
            // base class (System.Object) by passing an empty array of 
            // types (Type.EmptyTypes) to GetConstructor.
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, zConst);

            generator.Emit(OpCodes.Ret);
        }
    }
}