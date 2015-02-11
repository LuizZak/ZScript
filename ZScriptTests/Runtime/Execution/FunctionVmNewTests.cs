using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests usage of the New instruction on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmNewTests
    {
        /// <summary>
        /// Tests the New instruction
        /// </summary>
        [TestMethod]
        public void TestCreateInteger()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.String, "System.Collections.ArrayList"),
                new Token(TokenType.Value, 24),
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.New)
            };
            
            var owner = new TestRuntimeOwner();

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, new IntegerMemory(), null, owner); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(24, ((ArrayList)functionVm.Stack.Peek()).Capacity,
                "The ArrayList created should have the capacity specified by the instructions, as passed to its constructor");

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ArrayList),
                "The New instruction failed to execute as expected");
        }
    }
}