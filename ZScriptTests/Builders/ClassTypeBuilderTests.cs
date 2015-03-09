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
            var generationContext = new RuntimeGenerationContext();
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var classBuilder = new ClassTypeBuilder(generationContext, buildingContext);
            
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
            var generationContext = new RuntimeGenerationContext();
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var classBuilder = new ClassTypeBuilder(generationContext, buildingContext);

            // Create the class type
            ParameterModifier[] mod = { new ParameterModifier(1) };

            var classType = classBuilder.ConstructType(classDef);
            var constructor = classType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.Any, new[] { typeof(ZClass) }, mod);

            var constructors = classType.GetConstructors();

            Console.WriteLine("Found constructors: ");
            foreach (var info in constructors)
            {
                Console.WriteLine(info);
            }

            Assert.IsNotNull(constructor, "Failed to generated expected constructor");
        }
    }
}