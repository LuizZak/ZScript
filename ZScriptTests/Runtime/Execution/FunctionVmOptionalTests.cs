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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScriptTests.Runtime.Execution
{
    /// <summary>
    /// Tests optional handling support on the FunctionVm
    /// </summary>
    [TestClass]
    public class FunctionVmOptionalTests
    {
        /// <summary>
        /// Tests wrapping of values into optionals using the Wrap instruction
        /// </summary>
        [TestMethod]
        public void TestOptionalWrap()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(10L),
                TokenFactory.CreateInstructionToken(VmInstruction.Wrap, typeof(Optional<long>)),
                TokenFactory.CreateBoxedValueToken((Optional<long>)10L),
                TokenFactory.CreateInstructionToken(VmInstruction.Wrap, typeof(Optional<Optional<long>>)),
                TokenFactory.CreateBoxedValueToken(null),
                TokenFactory.CreateInstructionToken(VmInstruction.Wrap, typeof(Optional<bool>)),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            var wrappedBool = (Optional<bool>)functionVm.Stack.Pop();
            Assert.IsFalse(wrappedBool.HasInnerValue);

            var wrappedWrappedLong = (Optional<Optional<long>>)functionVm.Stack.Pop();
            Assert.IsTrue(wrappedWrappedLong.HasInnerValue);
            Assert.AreEqual(new Optional<Optional<long>>(10L), wrappedWrappedLong);

            var wrappedLong = (Optional<long>)functionVm.Stack.Pop();
            Assert.IsTrue(wrappedLong.HasInnerValue);
            Assert.AreEqual(new Optional<long>(10L), wrappedLong);
        }

        /// <summary>
        /// Tests unwrapping of optional values using the Unwrap instruction
        /// </summary>
        [TestMethod]
        public void TestOptionalUnwrap()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken((Optional<long>)10L),
                TokenFactory.CreateInstructionToken(VmInstruction.Unwrap),
                TokenFactory.CreateBoxedValueToken((Optional<Optional<long>>)(Optional<long>)10L),
                TokenFactory.CreateInstructionToken(VmInstruction.Unwrap),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual((Optional<long>)10L, functionVm.Stack.Pop());
            Assert.AreEqual(10L, functionVm.Stack.Pop());
        }

        /// <summary>
        /// Tests SafeUnwrap instructions on optional values
        /// </summary>
        [TestMethod]
        public void TestOptionalSafeUnwrap()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken((Optional<long>)10L),
                TokenFactory.CreateInstructionToken(VmInstruction.SafeUnwrap),
                TokenFactory.CreateBoxedValueToken(Optional<long>.Empty),
                TokenFactory.CreateInstructionToken(VmInstruction.SafeUnwrap)
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();

            Assert.AreEqual(false, functionVm.Stack.Pop());
            Assert.AreEqual(true, functionVm.Stack.Pop());
            Assert.AreEqual(10L, functionVm.Stack.Pop());
        }

        /// <summary>
        /// Tests exception raising when trying to unwrap empty optional values
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestOptionalUnwrapException()
        {
            // Create the set of tokens
            IntermediaryTokenList t = new IntermediaryTokenList
            {
                TokenFactory.CreateBoxedValueToken(Optional<long>.Empty),
                TokenFactory.CreateInstructionToken(VmInstruction.Unwrap),
            };

            var tokenList = new TokenList(t);
            var memory = new Memory();
            var context = new VmContext(memory, null); // ZRuntime can be null, as long as we don't try to call a function

            var functionVm = new FunctionVM(tokenList, context);

            functionVm.Execute();
        }
    }
}