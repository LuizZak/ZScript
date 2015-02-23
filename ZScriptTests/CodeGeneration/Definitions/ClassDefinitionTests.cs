using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Definitions;
using ZScript.Runtime.Typing.Elements;

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

        /// <summary>
        /// Tests fetching a field's information with the ClassTypeDef property
        /// </summary>
        [TestMethod]
        public void TestClassTypeDefField()
        {
            var definition = new ClassDefinition("test1");

            definition.AddField(new ClassFieldDefinition("field1") { Type = TypeDef.IntegerType });

            definition.FinishDefinition();

            var classTypeDef = definition.ClassTypeDef;

            Assert.AreEqual(TypeDef.IntegerType, classTypeDef.GetField("field1").FieldType);
        }

        /// <summary>
        /// Tests fetching a method's information with the ClassTypeDef property
        /// </summary>
        [TestMethod]
        public void TestClassTypeDefMethod()
        {
            var definition = new ClassDefinition("test1");

            definition.AddMethod(new MethodDefinition("func1", null,
                new[] {new FunctionArgumentDefinition {Name = "param1", Type = TypeDef.IntegerType}})
            {
                ReturnType = TypeDef.BooleanType
            });

            definition.FinishDefinition();

            var classTypeDef = definition.ClassTypeDef;

            Assert.AreEqual(TypeDef.IntegerType, classTypeDef.GetMethod("func1").Parameters[0].ParameterType);
            Assert.AreEqual("param1", classTypeDef.GetMethod("func1").Parameters[0].ParameterName);
            Assert.AreEqual(TypeDef.BooleanType, classTypeDef.GetMethod("func1").ReturnType);
        }
    }
}