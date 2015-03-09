using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Builders;

namespace ZScriptTests.Builders
{
    /// <summary>
    /// Tests the functionality of the TypeBuildingContext class and related components
    /// </summary>
    [TestClass]
    public class TypeBuildingContextTests
    {
        /// <summary>
        /// Tests the creation of a new TypeBuildingContext
        /// </summary>
        [TestMethod]
        public void TestCreateContext()
        {
            const string name = "TestAssembly";
            var context = TypeBuildingContext.CreateBuilderContext(name);

            Assert.AreEqual(name, context.AssemblyName.Name, "The assembly failed to be generated with the expected name");
            Assert.IsNotNull(context.AssemblyBuilder.FullName, "Failed to create the assembly builder as expected");
            Assert.IsNotNull(context.ModuleBuilder, "Failed to create the module builder as expected");
        }
    }
}