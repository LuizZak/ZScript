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

using Xunit;

using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScriptTests.Elements
{
    /// <summary>
    /// Tests the functionality of the TokenFactory class
    /// </summary>
    public class TokenFactoryTests
    {
        /// <summary>
        /// Tests the InstructionForOperator method
        /// </summary>
        [Fact]
        public void TestInstructionForOperator()
        {
            Assert.Equal(VmInstruction.Multiply, TokenFactory.InstructionForOperator("*"));
            Assert.Equal(VmInstruction.Divide, TokenFactory.InstructionForOperator("/"));
            Assert.Equal(VmInstruction.Modulo, TokenFactory.InstructionForOperator("%"));

            Assert.Equal(VmInstruction.Add, TokenFactory.InstructionForOperator("+"));
            Assert.Equal(VmInstruction.Subtract, TokenFactory.InstructionForOperator("-"));

            Assert.Equal(VmInstruction.BitwiseAnd, TokenFactory.InstructionForOperator("&"));
            Assert.Equal(VmInstruction.BitwiseOr, TokenFactory.InstructionForOperator("|"));
            Assert.Equal(VmInstruction.BitwiseXOr, TokenFactory.InstructionForOperator("^"));

            Assert.Equal(VmInstruction.ShiftLeft, TokenFactory.InstructionForOperator("<<"));
            Assert.Equal(VmInstruction.ShiftRight, TokenFactory.InstructionForOperator(">>"));

            Assert.Equal(VmInstruction.Less, TokenFactory.InstructionForOperator("<"));
            Assert.Equal(VmInstruction.LessOrEquals, TokenFactory.InstructionForOperator("<="));
            Assert.Equal(VmInstruction.Greater, TokenFactory.InstructionForOperator(">"));
            Assert.Equal(VmInstruction.GreaterOrEquals, TokenFactory.InstructionForOperator(">="));

            Assert.Equal(VmInstruction.Equals, TokenFactory.InstructionForOperator("=="));
            Assert.Equal(VmInstruction.Unequals, TokenFactory.InstructionForOperator("!="));

            Assert.Equal(VmInstruction.LogicalAnd, TokenFactory.InstructionForOperator("&&"));
            Assert.Equal(VmInstruction.LogicalOr, TokenFactory.InstructionForOperator("||"));

            Assert.Equal(VmInstruction.Is, TokenFactory.InstructionForOperator("is"));

            Assert.Equal(VmInstruction.Noop, TokenFactory.InstructionForOperator("---", false));
        }

        /// <summary>
        /// Tests exception raising when using the InstructionForOperator method with an invalid operator and with the raiseOnError parameter set to true
        /// </summary>
        [Fact]
        public void TestInstructionForOperatorException()
        {
            Assert.Throws<ArgumentException>(() => TokenFactory.InstructionForOperator("---"));
        }

        /// <summary>
        /// Tests the InstructionForUnaryOperator method
        /// </summary>
        [Fact]
        public void TestInstructionForUnaryOperator()
        {
            Assert.Equal(VmInstruction.ArithmeticNegate, TokenFactory.InstructionForUnaryOperator("-"));
            Assert.Equal(VmInstruction.LogicalNegate, TokenFactory.InstructionForUnaryOperator("!"));
            Assert.Equal(VmInstruction.Noop, TokenFactory.InstructionForUnaryOperator("*", false));
        }

        /// <summary>
        /// Tests exception raising when using the InstructionForOperator method with an invalid unary operator and with the raiseOnError parameter set to true
        /// </summary>
        [Fact]
        public void TestInstructionForUnaryOperatorException()
        {
            Assert.Throws<ArgumentException>(() => TokenFactory.InstructionForUnaryOperator("---"));
        }
    }
}