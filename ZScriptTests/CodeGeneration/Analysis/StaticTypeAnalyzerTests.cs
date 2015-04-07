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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Utils;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the static type analyzer
    /// </summary>
    [TestClass]
    public class StaticTypeAnalyzerTests
    {
        #region Closure resolving

        /// <summary>
        /// Tests inferring of types in a closure definition that is contained within a function argument
        /// </summary>
        [TestMethod]
        public void TestClosureTypeInferringFunctionArg()
        {
            const string input = "func f() { f2(i => { return 1; }); } func f2(a:(int->int)) { }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();
            generator.MessageContainer.PrintMessages();

            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            Assert.IsFalse(generator.HasErrors);
            Assert.AreEqual(provider.IntegerType(), closure.Parameters[0].Type, "The parameter type of the closure was not inferred correctly");
            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests preventing inferring of types in a closure definition that is contained within a function argument
        /// </summary>
        [TestMethod]
        public void TestPreventedClosureTypeInferringFunctionArg()
        {
            const string input = "func f() { f2(i : any => { return 0; }); } func f2(a:(int->int)) { }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();
            
            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.HasErrors);
            Assert.AreEqual(provider.AnyType(), closure.Parameters[0].Type, "The parameter type of the closure was not inferred correctly");
            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests inferring of types in a closure definition that is passed as the return value of a function
        /// </summary>
        [TestMethod]
        public void TestClosureTypeInferringReturn()
        {
            const string input = "func f2() : (int->int) { return i => { return 0; }; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            Assert.IsFalse(generator.HasErrors);
            Assert.AreEqual(provider.IntegerType(), closure.Parameters[0].Type, "The parameter type of the closure was not inferred correctly");
            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests inferring of types in a closure definition that is used as a value of a variable
        /// </summary>
        [TestMethod]
        public void TestClosureTypeInferringVariable()
        {
            const string input = "func f2() { var a: (int->int) = i => { return 0; }; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.HasErrors);
            Assert.AreEqual(provider.IntegerType(), closure.Parameters[0].Type, "The parameter type of the closure was not inferred correctly");
            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests inferring of types in a closure definition that is used as a value of a variable
        /// </summary>
        [TestMethod]
        public void TestClosureTypeInferringAssignment()
        {
            const string input = "func f2() { var a: (int->int)?; a = i => { return 0; }; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            var closure = scope.GetDefinitionsByType<ClosureDefinition>().First();

            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.HasErrors);
            Assert.AreEqual(provider.IntegerType(), closure.Parameters[0].Type, "The parameter type of the closure was not inferred correctly");
            Assert.AreEqual(provider.IntegerType(), closure.ReturnType, "The return type of the closure was not inferred correctly");
        }

        /// <summary>
        /// Tests raising errors when resolving implicit closures to void
        /// </summary>
        [TestMethod]
        public void TestVoidClosureParsing()
        {
            const string input = "func f2() { var a = () => { }(); }";
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();

            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, generator.MessageContainer.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        #endregion

        #region Class type support

        /// <summary>
        /// Tests using a class name as a valid type
        /// </summary>
        [TestMethod]
        public void TestClassTypeNaming()
        {
            const string input = "var a:TestClass?; class TestClass { }";
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.HasErrors);
        }

        /// <summary>
        /// Tests using a base class and assigning a derived class
        /// </summary>
        [TestMethod]
        public void TestClassInheritanceSupport()
        {
            const string input = "var a:Base = Derived(); class Base { } class Derived : Base { }";
            var generator = TestUtils.CreateGenerator(input);
            generator.CollectDefinitions();
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.HasErrors);
        }

        /// <summary>
        /// Tests correct type verification of 'this' expressions
        /// </summary>
        [TestMethod]
        public void TestThisExpressionTyping()
        {
            const string input = "class TestClass { var field:int; func f1() { this.field = 'sneakyString'; } }";
            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();
            generator.MessageContainer.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        #endregion

        #region Implicit casting

        /// <summary>
        /// Tests emission of implicit casts to function arguments
        /// </summary>
        [TestMethod]
        public void TestFunctionArgumentImplicitCasting()
        {
            const string input = "var a:int = 0; func f() { f2(a); } func f2(i:float) { }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var definition = generator.GenerateRuntimeDefinition();

            var function = definition.ZFunctionDefinitions[0];

            // Fetch the tokens now
            var generatedTokens = function.Tokens.Tokens;

            // Compare to the expected emitted tokens
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateGlobalFunctionToken(1), // 1 is the global indexer of the 'f2' function
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, provider.NativeTypeForTypeDef(provider.FloatType())),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, "Failed to generate expected implicit cast tokens");
        }

        /// <summary>
        /// Tests non-emission of implicit casts when providing values of matching types
        /// </summary>
        [TestMethod]
        public void TestFunctionArgumentNoImplicitCasting()
        {
            const string input = "var a:int = 0; func f() { f2(a); } func f2(i:int) { }";
            var generator = TestUtils.CreateGenerator(input);
            var definition = generator.GenerateRuntimeDefinition();

            var function = definition.ZFunctionDefinitions[0];

            // Fetch the tokens now
            var generatedTokens = function.Tokens.Tokens;

            // Compare to the expected emitted tokens
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateGlobalFunctionToken(1), // 1 is the global indexer of the 'f2' function
                TokenFactory.CreateVariableToken("a", true),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.Call),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, "Failed to generate expected implicit cast tokens");
        }

        /// <summary>
        /// Tests implicit casting on assignments
        /// </summary>
        [TestMethod]
        public void TestAssignmentImplicitCasting()
        {
            const string input = "var a:float = 0; var b:int = 0; func f() { a = b; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var definition = generator.GenerateRuntimeDefinition();

            var function = definition.ZFunctionDefinitions[0];

            // Fetch the tokens now
            var generatedTokens = function.Tokens.Tokens;

            // Compare to the expected emitted tokens
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, provider.NativeTypeForTypeDef(provider.FloatType())),
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, "Failed to generate expected implicit cast tokens");
        }

        /// <summary>
        /// Tests implicit casting on assignments
        /// </summary>
        [TestMethod]
        public void TestCompoundAssignmentImplicitCasting()
        {
            const string input = "var a:float = 0; var b:int = 0; func f() { a += b; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var definition = generator.GenerateRuntimeDefinition();

            var function = definition.ZFunctionDefinitions[0];

            // Fetch the tokens now
            var generatedTokens = function.Tokens.Tokens;

            // Compare to the expected emitted tokens
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateInstructionToken(VmInstruction.Duplicate),
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, provider.NativeTypeForTypeDef(provider.FloatType())),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                TokenFactory.CreateInstructionToken(VmInstruction.Swap),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, "Failed to generate expected implicit cast tokens");
        }

        /// <summary>
        /// Tests implicit casting on assignments
        /// </summary>
        [TestMethod]
        public void TestListAssignmentImplicitCasting()
        {
            const string input = "var a:[float] = [0]; var b:int = 0; func f() { a[0] = b; }";
            var generator = TestUtils.CreateGenerator(input);
            var provider = generator.TypeProvider;
            var definition = generator.GenerateRuntimeDefinition();

            var function = definition.ZFunctionDefinitions[0];

            // Fetch the tokens now
            var generatedTokens = function.Tokens.Tokens;

            // Compare to the expected emitted tokens
            var expectedTokens = new List<Token>
            {
                TokenFactory.CreateVariableToken("b", true),
                TokenFactory.CreateOperatorToken(VmInstruction.Cast, provider.NativeTypeForTypeDef(provider.FloatType())),
                TokenFactory.CreateVariableToken("a", false),
                TokenFactory.CreateBoxedValueToken(0L),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                TokenFactory.CreateInstructionToken(VmInstruction.ClearStack),
            };

            Console.WriteLine("Dump of tokens: ");
            Console.WriteLine("Expected:");
            TokenUtils.PrintTokens(expectedTokens);
            Console.WriteLine("Actual:");
            TokenUtils.PrintTokens(generatedTokens);

            // Assert the tokens where generated correctly
            TestUtils.AssertTokenListEquals(expectedTokens, generatedTokens, "Failed to generate expected implicit cast tokens");
        }

        #endregion

        #region General function resolving

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
            const string input = "func inventoryItemCount(_player : any = null, itemID : int = 0) : int { return ((any)_player).CurrentInventory.GetItemNum(itemID); }" +
                                 "func hasInventoryItem(_player : any = null, itemID : int = 0) : bool { return inventoryItemCount(_player, itemID) > 0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(0, container.CodeErrors.Length, "Errors where raised when not expected");
        }

        #endregion

        #region Statement analysis

        #region General expression statement analysis

        /// <summary>
        /// Tests raising errors when trying to assign values to constant local variables
        /// </summary>
        [TestMethod]
        public void TestConstantLocalVariableAssignmentCheck()
        {
            // Set up the test
            const string input = "func f() { let a = 0; a = 1; a += 0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests raising errors when trying to assign values to constant global variables
        /// </summary>
        [TestMethod]
        public void TestConstantGlobalVariableAssignmentCheck()
        {
            // Set up the test
            const string input = "let b = 0; func f() { b = 10; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests raising errors when creating global constants with no starting value
        /// </summary>
        [TestMethod]
        public void TestValuelessGlobalConstantDefinition()
        {
            // Set up the test
            const string input = "let b;";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ValuelessConstantDeclaration), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests raising errors when trying to increment/decrement the contents of a constant value
        /// </summary>
        [TestMethod]
        public void TestIncrementDecrementConstantValue()
        {
            // Set up the test
            const string input = "func f() { let a = 0; a++; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests raising errors when trying to infer types of tuples containing null literals
        /// </summary>
        [TestMethod]
        public void TestInferTupleWithNullLiteralsError()
        {
            // Set up the test
            const string input = "var b = (0, null);";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteType));
        }

        #endregion

        #region If statement analysis

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
        /// Tests checking constant condition expressions on if statements
        /// </summary>
        [TestMethod]
        public void TestConstantIfStatementChecking()
        {
            // Set up the test
            const string input = "func f() { if(false) { } else if(true) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(2, container.Warnings.Count(w => w.WarningCode == WarningCode.ConstantIfCondition), "Failed to raise expected warnings");
        }

        #endregion

        #region While statement analysis

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

        #endregion

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

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking condition expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementConditionConstantChecking()
        {
            // Set up the test
            const string input = "func f() { for(let i = 0;i++;) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests checking init expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementInitTypeChecking()
        {
            // Set up the test
            const string input = "let b = 10; func f() { for(b();;) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(e => e.ErrorCode == ErrorCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests checking increment expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementIncrementTypeChecking()
        {
            // Set up the test
            const string input = "let b = 10; func f() { for(;;b()) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(e => e.ErrorCode == ErrorCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests checking increment expressions on for statements
        /// </summary>
        [TestMethod]
        public void TestForStatementIncrementConstantChecking()
        {
            // Set up the test
            const string input = "func f() { for(let i = 0;;i++) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.ModifyingConstant), "Failed to raise expected warnings");
        }

        #endregion

        #region For each statement analysis

        /// <summary>
        /// Tests checking iterable expressions on for-each statements
        /// </summary>
        [TestMethod]
        public void TestForEachStatementTypeChecking()
        {
            // Set up the test
            const string input = "func f() { for(var a in [0, 1]) { } for(var a in 0) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests checking varaible type on for-each statements
        /// </summary>
        [TestMethod]
        public void TestForEachStatementHardTypeChecking()
        {
            // Set up the test
            const string input = "func f() { for(var a:int in [0, 1]) { } for(var a:float in [0, 1]) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests proper propagation of loop variable type
        /// </summary>
        [TestMethod]
        public void TestForEachStatementLoopVariableTypePropagation()
        {
            // Set up the test
            const string input = "func f() { for(var a:int in [0, 1]) { a = 0.0; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
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
        /// Tests checking expressions contained within switch statements which have a variable declaration as an expression
        /// </summary>
        [TestMethod]
        public void TestSwitchValuedExpressionTypeChecking()
        {
            // Set up the test
            const string input = "func f() { switch(let a = 10 > true) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

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

        /// <summary>
        /// Tests checking a switch's case labels against constant cases
        /// </summary>
        [TestMethod]
        public void TestValidConstantSwitchStatement()
        {
            // Set up the test
            const string input = "func f() { switch(10) { case f2(): break; case f2(): break; } } func f2() : int { return 0; }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(0, container.Warnings.Count(c => c.WarningCode == WarningCode.ConstantSwitchExpression), "Warnings raised when not expected");
        }

        /// <summary>
        /// Tests checking a switch's case labels against complete constant cases that never match the switch expression
        /// </summary>
        [TestMethod]
        public void TestNonMatchingConstantSwitchStatement()
        {
            // Set up the test
            const string input = "func f() { switch(10) { case 11: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(2, container.Warnings.Count(c => c.WarningCode == WarningCode.ConstantSwitchExpression), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests reporting errors when a switch expression is a value declaration with no value specified
        /// </summary>
        [TestMethod]
        public void TestValuelessValuedSwitchStatement()
        {
            // Set up the test
            const string input = "func f() { switch(let a) { } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.MissingValueOnSwitchValueDefinition), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests the raising of warnings when using the switch variable as case labels
        /// </summary>
        [TestMethod]
        public void TestCaseValueIsSwitchVariableSwitchStatementWarning()
        {
            // Set up the test
            const string input = "func f() { switch(var a = 10) { case a: break; } }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.Warnings.Count(c => c.WarningCode == WarningCode.ConstantSwitchExpression), "Failed to raise expected errors");
        }

        #endregion

        #endregion

        #region Parameters

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestParameterAnalysis()
        {
            // Set up the test
            const string input = "func f(a = null) { }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            container.PrintMessages();

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteType), "Failed to raise expected errors");
        }

        #endregion

        #region Global variable analysis

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestGlobalVariableTypeInferring()
        {
            // Set up the test
            const string input = "let a = 10; let b:bool = 10; let c = null;";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            Assert.AreEqual(provider.IntegerType(), scope.GetDefinitionByName<GlobalVariableDefinition>("a").Type, "Faild to infer type of global variable");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidCast), "Failed to raise expected errors");
            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.IncompleteType), "Failed to raise expected errors");
        }

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestGlobalVariableTypeChecking()
        {
            // Set up the test
            const string input = "let a = 10; let b:bool = 10; func f() { a(); }";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            var provider = generator.TypeProvider;
            var scope = generator.CollectDefinitions();

            Assert.AreEqual(provider.IntegerType(), scope.GetDefinitionByName<GlobalVariableDefinition>("a").Type, "Faild to infer type of global variable");
            Assert.AreEqual(1, container.CodeErrors.Count(e => e.ErrorCode == ErrorCode.TryingToCallNonCallable), "Failed to raise expected warnings");
        }

        /// <summary>
        /// Tests global variable type expanding and inferring
        /// </summary>
        [TestMethod]
        public void TestEmptyNonOptional()
        {
            // Set up the test
            const string input = "var a:int; var b:int?;";

            var generator = TestUtils.CreateGenerator(input);
            var container = generator.MessageContainer;
            generator.CollectDefinitions();

            Assert.AreEqual(1, container.CodeErrors.Count(e => e.ErrorCode == ErrorCode.ValuelessNonOptionalDeclaration), "Failed to raise expected warnings");
        }

        #endregion
    }
}