using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests ZObject creation/access functionality on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmObjectTests
    {
        #region Array Creation

        /// <summary>
        /// Tests a simple object created with 2 values
        /// </summary>
        [TestMethod]
        public void TestSimpleObjectCreation()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, 2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateObject)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ZObject));

            var obj = (ZObject)functionVm.Stack.Pop();

            Assert.AreEqual(10, obj["abc"], "The object was not created successfully");
            Assert.AreEqual(5, obj["def"], "The object was not created successfully");
        }

        /// <summary>
        /// Tests a simple object created with 5 values
        /// </summary>
        [TestMethod]
        public void TestObjectCreation()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 7),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, 2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateObject)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ZObject));

            var obj = (ZObject)functionVm.Stack.Pop();

            Assert.AreEqual(10, obj["abc"], "The object was not created successfully");
            Assert.AreEqual(12, obj["def"], "The object was not created successfully");
        }

        #endregion
    }
}