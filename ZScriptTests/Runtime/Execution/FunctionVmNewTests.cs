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

using Xunit;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

using ZScriptTests.Utils;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests usage of the New instruction on the VM
    /// </summary>
    public class FunctionVmNewTests
    {
        /// <summary>
        /// Tests the New instruction
        /// </summary>
        [Fact]
        public void TestCreateInteger()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                new Token(TokenType.String, "System.Collections.ArrayList"),
                new Token(TokenType.Value, 24),
                new Token(TokenType.Value, 1),
                TokenFactory.CreateInstructionToken(VmInstruction.New)
            };
            
            var owner = new TestRuntimeOwner();

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, new IntegerMemory(), null, owner, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.Equal(24, ((ArrayList)functionVm.Stack.Peek()).Capacity);

            Assert.True(functionVm.Stack.Peek() is ArrayList);
        }
    }
}