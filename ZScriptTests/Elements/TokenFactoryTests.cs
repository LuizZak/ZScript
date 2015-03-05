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
using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScriptTests.Elements
{
    /// <summary>
    /// Tests the functionality of the TokenFactory class
    /// </summary>
    [TestClass]
    public class TokenFactoryTests
    {
        /// <summary>
        /// Tests the InstructionForOperator method
        /// </summary>
        [TestMethod]
        public void TestInstructionForOperator()
        {
            Assert.AreEqual(VmInstruction.Multiply, TokenFactory.InstructionForOperator("*"));
            Assert.AreEqual(VmInstruction.Divide, TokenFactory.InstructionForOperator("/"));
            Assert.AreEqual(VmInstruction.Modulo, TokenFactory.InstructionForOperator("%"));

            Assert.AreEqual(VmInstruction.Add, TokenFactory.InstructionForOperator("+"));
            Assert.AreEqual(VmInstruction.Subtract, TokenFactory.InstructionForOperator("-"));

            Assert.AreEqual(VmInstruction.BitwiseAnd, TokenFactory.InstructionForOperator("&"));
            Assert.AreEqual(VmInstruction.BitwiseOr, TokenFactory.InstructionForOperator("|"));
            Assert.AreEqual(VmInstruction.BitwiseXOr, TokenFactory.InstructionForOperator("^"));

            Assert.AreEqual(VmInstruction.ShiftLeft, TokenFactory.InstructionForOperator("<<"));
            Assert.AreEqual(VmInstruction.ShiftRight, TokenFactory.InstructionForOperator(">>"));

            Assert.AreEqual(VmInstruction.Less, TokenFactory.InstructionForOperator("<"));
            Assert.AreEqual(VmInstruction.LessOrEquals, TokenFactory.InstructionForOperator("<="));
            Assert.AreEqual(VmInstruction.Greater, TokenFactory.InstructionForOperator(">"));
            Assert.AreEqual(VmInstruction.GreaterOrEquals, TokenFactory.InstructionForOperator(">="));

            Assert.AreEqual(VmInstruction.Equals, TokenFactory.InstructionForOperator("=="));
            Assert.AreEqual(VmInstruction.Unequals, TokenFactory.InstructionForOperator("!="));

            Assert.AreEqual(VmInstruction.LogicalAnd, TokenFactory.InstructionForOperator("&&"));
            Assert.AreEqual(VmInstruction.LogicalOr, TokenFactory.InstructionForOperator("||"));

            Assert.AreEqual(VmInstruction.Is, TokenFactory.InstructionForOperator("is"));

            Assert.AreEqual(VmInstruction.Noop, TokenFactory.InstructionForOperator("---", false));
        }

        /// <summary>
        /// Tests exception raising when using the InstructionForOperator method with an invalid operator and with the raiseOnError parameter set to true
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to fetch an instruction for an invalid operator should raise an ArgumentException")]
        public void TestInstructionForOperatorException()
        {
            TokenFactory.InstructionForOperator("---");
        }

        /// <summary>
        /// Tests the InstructionForUnaryOperator method
        /// </summary>
        [TestMethod]
        public void TestInstructionForUnaryOperator()
        {
            Assert.AreEqual(VmInstruction.ArithmeticNegate, TokenFactory.InstructionForUnaryOperator("-"));
            Assert.AreEqual(VmInstruction.LogicalNegate, TokenFactory.InstructionForUnaryOperator("!"));
            Assert.AreEqual(VmInstruction.Noop, TokenFactory.InstructionForUnaryOperator("*", false));
        }

        /// <summary>
        /// Tests exception raising when using the InstructionForOperator method with an invalid unary operator and with the raiseOnError parameter set to true
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to fetch an instruction for an invalid unary operator should raise an ArgumentException")]
        public void TestInstructionForUnaryOperatorException()
        {
            TokenFactory.InstructionForUnaryOperator("---");
        }
    }
}