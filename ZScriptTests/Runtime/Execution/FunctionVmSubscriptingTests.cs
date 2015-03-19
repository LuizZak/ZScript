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

using System.Collections;
using System.Collections.Generic;

using Xunit;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Execution.Wrappers;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests subscripting support on the FunctionVm
    /// </summary>
    public class FunctionVmSubscriptingTests
    {
        #region Array Subscripting

        /// <summary>
        /// Tests fetching an ArrayList's subscript using VM instructions
        /// </summary>
        [Fact]
        public void TestArraySubscriptGet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 5),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(object)),
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.GetSubscript)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.True(functionVm.Stack.Peek() is IndexedSubscripter);
        }

        /// <summary>
        /// Tests setting an ArrayList's value in the VM using subscripting
        /// </summary>
        [Fact]
        public void TestArraySubscriptSet()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 10),
                new Token(TokenType.Value, "abc"),
                new Token(TokenType.Value, 10.0),
                new Token(TokenType.Value, "def"),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 5),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(object)),
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

            Assert.True(context.Memory.GetVariable("a") is IList);
            Assert.Equal(1, ((List<object>)context.Memory.GetVariable("a"))[1]);
        }

        #endregion

        /// <summary>
        /// Tests fetching a Dictionary's subscript using VM instructions
        /// </summary>
        [Fact]
        public void TestDictionarySubscriptGet()
        {
            var dict = new Dictionary<string, object>();

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
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

            Assert.True(functionVm.Stack.Peek() is IndexedSubscripter);
        }

        /// <summary>
        /// Tests setting a Dictionary's value in the VM using subscripting
        /// </summary>
        [Fact]
        public void TestDictionarySubscriptSet()
        {
            var dict = new Dictionary<string, object>();

            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
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

            Assert.Equal(10, dict["0"]);
        }
    }
}