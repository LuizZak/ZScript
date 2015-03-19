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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Builders;
using ZScript.CodeGeneration;
using ZScript.Elements;
using ZScript.Runtime;

using ZScriptTests.Utils;

namespace ZScriptTests.Builders
{
    /// <summary>
    /// Tests the functionality of the ClassTypeBuilder class and related components
    /// </summary>
    [TestClass]
    public class ClassTypeBuilderTests
    {
        /// <summary>
        /// Tests simple construction of a class definition into a type
        /// </summary>
        [TestMethod]
        public void TestConstructClass()
        {
            // Define a simple class to build
            var classDef = TestUtils.CreateTestClassDefinition();

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var classBuilder = new ClassTypeBuilder(buildingContext);
            
            // Create the class type
            var classType = classBuilder.ConstructType(classDef);

            Assert.AreEqual(classDef.Name + ClassTypeBuilder.ClassNameSuffix, classType.Name, "Failed to generate the expected type");
            Assert.IsTrue(typeof(ZClassInstance).IsAssignableFrom(classType), "Generated class type should inherit ZClassInstance");
            Assert.IsTrue(classType.BaseType == typeof(ZClassInstance), "Generated class type should inherit ZClassInstance");
        }

        /// <summary>
        /// Tests verification of constructors for generated ZClassInstances
        /// </summary>
        [TestMethod]
        public void TestClassConstructor()
        {
            // Define a simple class to build
            var classDef = TestUtils.CreateTestClassDefinition();

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var classBuilder = new ClassTypeBuilder(buildingContext);

            // Create the class type
            var classType = classBuilder.ConstructType(classDef);

            // Search the constructor
            ParameterModifier[] mod = { new ParameterModifier(1) };
            var constructor = classType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.Any, new[] { typeof(ZClass) }, mod);

            var constructors = classType.GetConstructors();

            Console.WriteLine("Found constructors: ");
            foreach (var info in constructors)
            {
                Console.WriteLine(info);
            }

            Assert.IsNotNull(constructor, "Failed to generated expected constructor");
        }

        /// <summary>
        /// Tests verification of constructors for generated ZClassInstances
        /// </summary>
        [TestMethod]
        public void TestGeneratedClassInheritance()
        {
            // Define a simple class to build
            var classDef1 = TestUtils.CreateTestClassDefinition("class1");
            var classDef2 = TestUtils.CreateTestClassDefinition("class2");

            classDef2.BaseClass = classDef1;

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var classBuilder = new ClassTypeBuilder(buildingContext);

            var classType1 = classBuilder.ConstructType(classDef1);
            var classType2 = classBuilder.ConstructType(classDef2);

            Assert.IsTrue(classType1.IsAssignableFrom(classType2), "Failed to manage inheritance correctly");
            Assert.IsFalse(classType2.IsAssignableFrom(classType1), "Failed to manage inheritance correctly");
        }
    }
}