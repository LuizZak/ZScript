using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests Array creation/access functionality on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmArrayTests
    {
        #region Array Creation

        /// <summary>
        /// Tests a simple array created with 5 values
        /// </summary>
        [TestMethod]
        public void TestArrayCreation()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 5),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ArrayList));

            var array = (ArrayList)functionVm.Stack.Pop();

            Assert.AreEqual(10,    array[0], "The array was not created successfully");
            Assert.AreEqual("abc", array[1], "The array was not created successfully");
            Assert.AreEqual(10.0,  array[2], "The array was not created successfully");
            Assert.AreEqual("def", array[3], "The array was not created successfully");
            Assert.AreEqual(true,  array[4], "The array was not created successfully");
        }

        /// <summary>
        /// Tests a nested array created with 2 values, one being an array containing 4 values
        /// </summary>
        [TestMethod]
        public void TestNestedArrayCreation()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, 4),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ArrayList));

            var array = (ArrayList)functionVm.Stack.Pop();

            Assert.IsInstanceOfType(array[0], typeof(ArrayList));
            Assert.AreEqual(true, array[1], "The array was not created successfully");

            array = (ArrayList)array[0];

            Assert.AreEqual(10, array[0], "The array was not created successfully");
            Assert.AreEqual("abc", array[1], "The array was not created successfully");
            Assert.AreEqual(10.0, array[2], "The array was not created successfully");
            Assert.AreEqual("def", array[3], "The array was not created successfully");
        }

        #endregion

        #region Array Subscripting

        /// <summary>
        /// Tests fetching an ArrayList's subscript using VM instructions
        /// </summary>
        [TestMethod]
        public void TestArraySubscriptGet()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 5),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ListSubscripter));
        }

        #endregion
    }
}