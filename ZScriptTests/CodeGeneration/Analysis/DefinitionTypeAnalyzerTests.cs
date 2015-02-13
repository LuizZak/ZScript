using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Elements;
using ZScriptTests.Runtime;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the definition type analyzer
    /// </summary>
    [TestClass]
    public class DefinitionTypeAnalyzerTests
    {
        /// <summary>
        /// Tests inferring of types in a closure definition that is contained within a function argument
        /// </summary>
        [TestMethod]
        public void TestClosureTypeInferringFunctionArg()
        {
            const string input = "func f() { var a = f2((i) => { return 0; }); } func f2(a:(int->int)) { }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests callable argument type checking
        /// </summary>
        [TestMethod]
        public void TestExportArgumentTypeChecking()
        {
            // Set up the test
            const string input = "@trace(args...) func f1() { trace(0); }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }

        /// <summary>
        /// Tests callable argument type checking
        /// </summary>
        [TestMethod]
        public void TestExportReturnTypeChecking()
        {
            // Set up the test
            const string input = "@trace(args...) func f1() : int { return trace(0); }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }
        
        /// <summary>
        /// Tests function definition argument type checking
        /// </summary>
        [TestMethod]
        public void TestFunctionArgumentTypeChecking()
        {
            // Set up the test
            const string input = "func inventoryItemCount(_player : Player = null, itemID : int = 0) : any { return _player.CurrentInventory.GetItemNum(itemID); } func hasInventoryItem(_player : Player = null, itemID : int = 0) : any { return inventoryItemCount(_player, itemID) > 0; }";

            var generator = ZRuntimeTests.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }
    }
}