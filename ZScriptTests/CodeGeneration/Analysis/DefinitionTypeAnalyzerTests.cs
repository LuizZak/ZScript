﻿using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

using ZScriptTests.Utils;

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
            var generator = TestUtils.CreateGenerator(input);
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

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }

        /*
         * What was this test trying to achieve?
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
        */
        
        /// <summary>
        /// Tests function definition argument type checking
        /// </summary>
        [TestMethod]
        public void TestFunctionArgumentTypeChecking()
        {
            // Set up the test
            const string input = "func inventoryItemCount(_player : Player = null, itemID : int = 0) : any { return _player.CurrentInventory.GetItemNum(itemID); } func hasInventoryItem(_player : Player = null, itemID : int = 0) : any { return inventoryItemCount(_player, itemID) > 0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }

        #region Statement analysis

        /// <summary>
        /// Tests checking condition expressions on if statements
        /// </summary>
        [TestMethod]
        public void TestIfStatementTypeChecking()
        {
            // Set up the test
            const string input = "func f() { if(false) { } if(10) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking condition expressions on while statements
        /// </summary>
        [TestMethod]
        public void TestWhileStatementTypeChecking()
        {
            // Set up the test
            const string input = "func f() { while(true) { } while(10) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        #region For statement analysis

        /// <summary>
        /// Tests checking condition expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementConditionTypeChecking()
        {
            // Set up the test
            const string input = "func f() { for(var i = 0; i < 5; i++) { } for(var i = 0; i + 5; i++) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking init expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementInitTypeChecking()
        {
            // Set up the test
            const string input = "[ b = 10; ] func f() { for(b();;) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests checking increment expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementIncrementTypeChecking()
        {
            // Set up the test
            const string input = "[ b = 10; ] func f() { for(;;b()) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        #endregion

        #region Switch statement analysis

        /// <summary>
        /// Tests checking expressions contained within switch statements
        /// </summary>
        [TestMethod]
        public void TestSwitchExpressionTypeChecking()
        {
            // Set up the test
            const string input = "func f() { switch(10 > true) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnOperation), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking expressions contained within switch case statements
        /// </summary>
        [TestMethod]
        public void TestSwitchCaseExpressionTypeChecking()
        {
            // Set up the test
            const string input = "func f() { switch(true) { case true + 10: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidTypesOnOperation), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking whether a switch's expression type matches all of its cases' checking expression types
        /// </summary>
        [TestMethod]
        public void TestSwitchTypeConsistency()
        {
            // Set up the test
            const string input = "func f() { switch(10) { case true: break; case 'sneakyString': break; case 11: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking a switch's case labels against repeated constant values
        /// </summary>
        [TestMethod]
        public void TestRepeatedSwitchLabel()
        {
            // Set up the test
            const string input = "func f() { switch(10) { case 10: break; case 10: break; case 20: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.RepeatedCaseLabelValue), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking a switch's case labels against constant cases
        /// </summary>
        [TestMethod]
        public void TestConstantSwitchStatement()
        {
            // Set up the test
            const string input = "func f() { switch(10) { case 10: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.Warnings.Count(c => c.WarningCode == WarningCode.ConstantSwitchExpression), "Failed to raise expected warnings");
        }

        #endregion

        #endregion

        #region Global variable analysis

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestGlobalVariableTypeInferring()
        {
            // Set up the test
            const string input = "[ a = 10; b:bool = 10; ]";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            Assert.AreEqual(provider.IntegerType(), scope.GetDefinitionByName<GlobalVariableDefinition>("a").Type, "Faild to infer type of global variable");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestGlobalVariableTypeChecking()
        {
            // Set up the test
            const string input = "[ a = 10; b:bool = 10; ] func f() { a(); }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            Assert.AreEqual(provider.IntegerType(), scope.GetDefinitionByName<GlobalVariableDefinition>("a").Type, "Faild to infer type of global variable");
            Assert.AreEqual(1, container.Warnings.Count(w => w.WarningCode == WarningCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        #endregion
    }
}