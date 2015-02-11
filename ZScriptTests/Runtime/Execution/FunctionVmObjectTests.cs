using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests ZObject creation/access functionality on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmObjectTests
    {
        #region Object Creation

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
                new Token(TokenType.String, "abc"),
                new Token(TokenType.Value, 5),
                new Token(TokenType.String, "def"),
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
        /// Tests a simple object created with 2 values, with one being computed
        /// </summary>
        [TestMethod]
        public void TestObjectCreation()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.String, "abc"),
                new Token(TokenType.Value, 5),
                new Token(TokenType.Value, 7),
                TokenFactory.CreateOperatorToken(VmInstruction.Add),
                new Token(TokenType.String, "def"),
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

        #region Object Accessing

        /// <summary>
        /// Tests fetching ZObject values using GetMember
        /// </summary>
        [TestMethod]
        public void TestObjectMemberAccess()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.String, "abc"),
                new Token(TokenType.Value, 5),
                new Token(TokenType.String, "def"),
                new Token(TokenType.Value, 2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateObject),
                new Token(TokenType.String, "def"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ZObjectMember));

            var obj = (ZObjectMember)functionVm.Stack.Pop();

            Assert.AreEqual(5, obj.GetValue(), "The object member wrapper was not created successfully");
        }

        /// <summary>
        /// Tests setting ZObject values using GetMember
        /// </summary>
        [TestMethod]
        public void TestObjectMemberGet()
        {
            // Create the set of tokens
            List<Token> t = new List<Token>
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.String, "abc"),
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateObject),
                new Token(TokenType.MemberName, "a"),
                TokenFactory.CreateInstructionToken(VmInstruction.Set),
                new Token(TokenType.Value, 5),
                new Token(TokenType.MemberName, "a"),
                new Token(TokenType.MemberName, "def"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(5, ((ZObject)context.Memory.GetVariable("a"))["def"], "The Set operation on the ZObjectMember failed");
        }

        #endregion
    }
}