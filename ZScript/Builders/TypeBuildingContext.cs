using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ZScript.Builders
{
    /// <summary>
    /// Specifies context for dynamic building of types during compilation
    /// </summary>
    public class TypeBuildingContext
    {
        /// <summary>
        /// The name of the assembly to build on
        /// </summary>
        public AssemblyName AssemblyName { get; private set; }

        /// <summary>
        /// The the assembly builder to build on
        /// </summary>
        public AssemblyBuilder AssemblyBuilder { get; private set; }

        /// <summary>
        /// The module builder to build on
        /// </summary>
        public ModuleBuilder ModuleBuilder { get; set; }

        /// <summary>
        /// Private constructor 
        /// </summary>
        private TypeBuildingContext()
        {
            
        }

        /// <summary>
        /// Creates a new type builder context with a given assembly name
        /// </summary>
        /// <param name="name">The name of the assembly to generate</param>
        /// <returns>A new TypeBuildingContext to use on type builders</returns>
        public static TypeBuildingContext CreateBuilderContext(string name)
        {
            var assemblyName = new AssemblyName(name);

            var context = new TypeBuildingContext
            {
                AssemblyName = assemblyName,
                AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave),
            };

            context.ModuleBuilder = context.AssemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");

            return context;
        }
    }
}