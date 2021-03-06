﻿#region License information
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests Dictionary creation/access functionality on the VM
    /// </summary>
    [TestClass]
    public class FunctionVmDictionaryTests
    {
        #region Dictionary Creation

        /// <summary>
        /// Tests a simple dictionary created with 2 entries
        /// </summary>
        [TestMethod]
        public void TestDictionaryCreation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateBoxedValueToken("abc"),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateBoxedValueToken("def"),
                TokenFactory.CreateBoxedValueToken(2),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(int), typeof(string) })
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(Dictionary<int, string>));

            var array = (Dictionary<int, string>)functionVm.Stack.Pop();

            Assert.AreEqual("abc", array[0], "The dictionary was not created successfully");
            Assert.AreEqual("def", array[1], "The dictionary was not created successfully");
        }

        /// <summary>
        /// Tests a nested dictionary created with an entry that is another dictionary
        /// </summary>
        [TestMethod]
        public void TestNestedDictionaryCreation()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                // Dictionary 1 creation
                TokenFactory.CreateBoxedValueToken(0),
                // Dictionary 2 creation
                TokenFactory.CreateBoxedValueToken(0),
                TokenFactory.CreateBoxedValueToken("def"),
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(int), typeof(string) }),
                // Resume dictionary 1 creation
                TokenFactory.CreateBoxedValueToken(1),
                TokenFactory.CreateInstructionToken(VmInstruction.CreateDictionary, new [] { typeof(int), typeof(Dictionary<int,string>) })
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.IsInstanceOfType(functionVm.Stack.Peek(), typeof(Dictionary<int, Dictionary<int,string>>));

            var dict = (Dictionary<int, Dictionary<int, string>>)functionVm.Stack.Pop();

            Assert.IsInstanceOfType(dict[0], typeof(Dictionary<int, string>));

            var innerDict = dict[0];

            Assert.AreEqual("def", innerDict[0], "The dictionary was not created successfully");
        }

        #endregion
    }
}