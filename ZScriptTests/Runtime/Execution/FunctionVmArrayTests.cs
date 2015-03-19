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

using System.Collections.Generic;

using Xunit;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests Array creation/access functionality on the VM
    /// </summary>
    public class FunctionVmArrayTests
    {
        #region Array Creation

        /// <summary>
        /// Tests a simple array created with 5 values
        /// </summary>
        [Fact]
        public void TestArrayCreation()
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
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(object))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.True(functionVm.Stack.Peek() is List<object>);

            var array = (List<object>)functionVm.Stack.Pop();

            Assert.Equal(10,    array[0]);
            Assert.Equal("abc", array[1]);
            Assert.Equal(10.0,  array[2]);
            Assert.Equal("def", array[3]);
            Assert.Equal(true,  array[4]);
        }

        /// <summary>
        /// Tests a nested array created with 2 values, one being an array containing 4 values
        /// </summary>
        [Fact]
        public void TestNestedArrayCreation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.Value, 1),
                new Token(TokenType.Value, 2),
                new Token(TokenType.Value, 3),
                new Token(TokenType.Value, 4),
                new Token(TokenType.Value, 4),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(int)),
                new Token(TokenType.Value, true),
                new Token(TokenType.Value, 2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateArray, typeof(object))
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.True(functionVm.Stack.Peek() is List<object>);

            var array = (List<object>)functionVm.Stack.Pop();

            Assert.True(array[0] is List<int>);
            Assert.Equal(true, array[1]);

            var innerArray = (List<int>)array[0];

            Assert.Equal(1, innerArray[0]);
            Assert.Equal(2, innerArray[1]);
            Assert.Equal(3, innerArray[2]);
            Assert.Equal(4, innerArray[3]);
        }

        #endregion
    }
}