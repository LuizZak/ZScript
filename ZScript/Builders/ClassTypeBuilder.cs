using System;
using System.Reflection;
using System.Reflection.Emit;

using ZScript.CodeGeneration;
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
        /// The context used during type building
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// The type building context used during type buiding
        /// </summary>
        private readonly TypeBuildingContext _typeBuildingContext;

        /// <summary>
        /// Initializes a new instance of the ClassTypeBuilder class
        /// </summary>
        /// <param name="generationContext">A context for building the type</param>
        /// <param name="typeBuildingContext">A type building context used during type buiding</param>
        public ClassTypeBuilder(RuntimeGenerationContext generationContext, TypeBuildingContext typeBuildingContext)
        {
            _generationContext = generationContext;
            _typeBuildingContext = typeBuildingContext;
        }

        /// <summary>
        /// Constructs and returns a type for a given class definition
        /// </summary>
        /// <param name="definition">The class definition to construct</param>
        public Type ConstructType(ClassDefinition definition)
        {
            var typeBuilder = _typeBuildingContext.ModuleBuilder.DefineType(definition.Name + ClassNameSuffix);

            typeBuilder.SetParent(typeof(ZClassInstance));

            DefineConstructor(definition, typeBuilder);

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Defines the default constructor on a given class definition
        /// </summary>
        /// <param name="definition">The class definition to generate the constructor from</param>
        /// <param name="builder">The type builder to generate the constructor on</param>
        private void DefineConstructor(ClassDefinition definition, TypeBuilder builder)
        {
            var constructorDef = definition.PublicConstructor;

            var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
            var ilGenerator = constructor.GetILGenerator();

            FillGeneratorWithTokens(ilGenerator, constructorDef.Tokens);
        }

        /// <summary>
        /// Fills a given IL generator with a set of tokens
        /// </summary>
        /// <param name="generator">The generator to generate the tokens on</param>
        /// <param name="tokens">The body of tokens to generate the tokens from</param>
        private void FillGeneratorWithTokens(ILGenerator generator, TokenList tokens)
        {
            generator.Emit(OpCodes.Ret);
        }
    }
}