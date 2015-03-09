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
        public ModuleBuilder ModuleBuilder { get; private set; }

        /// <summary>
        /// Private constructor 
        /// </summary>
        private TypeBuildingContext()
        {
            
        }

        /// <summary>
        /// Resets the assembly context of this type building context
        /// </summary>
        public void ResetContext()
        {
            AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name, AssemblyName.Name + ".dll");
        }

        /// <summary>
        /// Creates a new type builder context with a given assembly name
        /// </summary>
        /// <param name="name">The name of the assembly to generate</param>
        /// <returns>A new TypeBuildingContext to use on type builders</returns>
        public static TypeBuildingContext CreateBuilderContext(string name)
        {
            var context = new TypeBuildingContext
            {
                AssemblyName = new AssemblyName(name)
            };

            context.ResetContext();

            return context;
        }
    }
}