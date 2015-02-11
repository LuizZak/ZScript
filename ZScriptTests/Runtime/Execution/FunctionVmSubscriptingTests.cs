using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests subscripting support on the FunctionVm
    /// </summary>
    [TestClass]
    public class FunctionVmSubscriptingTests
    {
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
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(IndexedSubscripter));
        }

        /// <summary>
        /// Tests setting an ArrayList's value in the VM using subscripting
        /// </summary>
        [TestMethod]
        public void TestArraySubscriptSet()
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
                new Token(TokenType.MemberName, "a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                new Token(TokenType.Value, 1),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(context.Memory.GetVariable("a"), typeof(IList), "The value set on memory for the array created is no an IList as expected");
            Assert.AreEqual(1, ((ArrayList)context.Memory.GetVariable("a"))[1], "The subscription set operation failed to set the correct value on the underlying array");
        }

        #endregion

        /// <summary>
        /// Tests fetching a Dictionary's subscript using VM instructions
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptGet()
        {
            var dict = new Dictionary<string, object>();

            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, dict),
                new Token(TokenType.Value, "0"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(IndexedSubscripter));
        }

        /// <summary>
        /// Tests setting a Dictionary's value in the VM using subscripting
        /// </summary>
        [TestMethod]
        public void TestDictionarySubscriptSet()
        {
            var dict = new Dictionary<string, object>();

            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, dict),
                new Token(TokenType.Value, "0"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, dict["0"], "The subscripting did not work as expected");
        }
    }
}