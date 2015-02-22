using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Definitions;

namespace ZScriptTests.CodeGeneration.Definitions
{
    /// <summary>
    /// Tests the functionality of the ClassDefinition and related components
    /// </summary>
    [TestClass]
    public class ClassDefinitionTests
    {
        /// <summary>
        /// Tests setup of an empty class definition
        /// </summary>
        [TestMethod]
        public void TestEmptyDefinition()
        {
            var definition = new ClassDefinition("test1");

            definition.FinishDefinition();

            Assert.IsNotNull(definition.PublicConstructor, "When no constructor is provided, calling FinishDefinition() should create a new parameterless constructor");
        }
    }
}