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