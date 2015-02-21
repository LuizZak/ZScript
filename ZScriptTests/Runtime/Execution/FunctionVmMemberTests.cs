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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests member accessing functionality on the FunctionVm
    /// </summary>
    [TestClass]
    public class FunctionVmMemberTests
    {
        [TestMethod]
        public void TestFieldMemberAccess()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, new TestTarget { Field1 = 10 }),
                new Token(TokenType.MemberName, "Field1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ClassMember));

            var value = (ClassMember)functionVm.Stack.Pop();

            Assert.AreEqual(10, value.GetValue(), "The member was not accessed successfully");
        }

        [TestMethod]
        public void TestFieldMemberGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, new TestTarget { Field1 = 10 }),
                new Token(TokenType.MemberName, "Field1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10, functionVm.Stack.Pop(), "The member's value was not accessed successfully");
        }

        [TestMethod]
        public void TestFieldMemberSet()
        {
            var target = new TestTarget { Field1 = 10 };

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 11),
                new Token(TokenType.Value, target),
                new Token(TokenType.MemberName, "Field1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(11, target.Field1, "The member's value was not accessed successfully");
        }

        [TestMethod]
        public void TestPropertyMemberAccess()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, new TestTarget { Property1 = 10 }),
                new Token(TokenType.MemberName, "Property1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(ClassMember));

            var value = (ClassMember)functionVm.Stack.Pop();

            Assert.AreEqual(10L, value.GetValue(), "The member was not accessed successfully");
        }

        [TestMethod]
        public void TestPropertyMemberGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, new TestTarget { Property1 = 10 }),
                new Token(TokenType.MemberName, "Property1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Get)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(10L, functionVm.Stack.Pop(), "The member's value was not accessed successfully");
        }

        [TestMethod]
        public void TestPropertyMemberSet()
        {
            var target = new TestTarget { Property1 = 10 };

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 11),
                new Token(TokenType.Value, target),
                new Token(TokenType.MemberName, "Property1"),
                TokenFactory.CreateInstructionToken(VmInstruction.GetMember),
                TokenFactory.CreateInstructionToken(VmInstruction.Set)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(11, target.Property1, "The member's value was not accessed successfully");
        }

        public class TestTarget
        {
            public int Field1;
            public long Property1 { get; set; }

            private string _privateField;
        }
    }
}