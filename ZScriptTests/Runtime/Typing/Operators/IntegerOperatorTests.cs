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
using System.Diagnostics.CodeAnalysis;

using Xunit;

using ZScript.Runtime.Typing.Operators;

namespace ZScriptTests.Runtime.Typing.Operators
{
    /// <summary>
    /// Tests the IntegerOperator class and related components
    /// </summary>
    public class IntegerOperatorTests
    {
        /// <summary>
        /// Tests the Sum operation with integers
        /// </summary>
        [Fact]
        public void TestSum()
        {
            var op = new IntegerOperator();

            Assert.Equal(1 + 1, op.Sum(1, 1));
            Assert.Equal(-1 + -1, op.Sum(-1, -1));
            Assert.Equal(1 + -1, op.Sum(1, -1));
        }

        /// <summary>
        /// Tests the Subtract operation with integers
        /// </summary>
        [Fact]
        public void TestSubtract()
        {
            var op = new IntegerOperator();

            Assert.Equal(1 - 1, op.Subtract(1, 1));
            Assert.Equal(-1 - -1, op.Subtract(-1, -1));
            Assert.Equal(1 - -1, op.Subtract(1, -1));
        }

        /// <summary>
        /// Tests the Multiply operation with integers
        /// </summary>
        [Fact]
        public void TestMultiply()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 * 2, op.Multiply(2, 2));
            Assert.Equal(2 * -1, op.Multiply(2, -1));
            Assert.Equal(10 * 100, op.Multiply(10, 100));
        }

        /// <summary>
        /// Tests the Divide operation with integers
        /// </summary>
        [Fact]
        public void TestDivide()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 / 2, op.Divide(2, 2));
            Assert.Equal(2 / -1, op.Divide(2, -1));
            Assert.Equal(10 / 100, op.Divide(10, 100));
        }

        /// <summary>
        /// Tests the Modulo operation with integers
        /// </summary>
        [Fact]
        public void TestModulo()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 % 2, op.Modulo(2, 2));
            Assert.Equal(2 % -1, op.Modulo(2, -1));
            Assert.Equal(10 % 100, op.Modulo(10, 100));
        }

        /// <summary>
        /// Tests the Bitwise And operation with integers
        /// </summary>
        [Fact]
        public void TestBitwiseAnd()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 & 2, op.BitwiseAnd(2, 2));
            Assert.Equal(123 & 321, op.BitwiseAnd(123, 321));
            Assert.Equal(0xF000 & 0xFF000, op.BitwiseAnd(0xF000, 0xFF000));
        }

        /// <summary>
        /// Tests the Bitwise Or operation with integers
        /// </summary>
        [Fact]
        public void TestBitwiseOr()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 | 2, op.BitwiseOr(2, 2));
            Assert.Equal(123 | 321, op.BitwiseOr(123, 321));
            Assert.Equal(0xF000 | 0xFF000, op.BitwiseOr(0xF000, 0xFF000));
        }

        /// <summary>
        /// Tests the Bitwise XOr operation with integers
        /// </summary>
        [Fact]
        public void TestBitwiseXOr()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 ^ 2, op.BitwiseXOr(2, 2));
            Assert.Equal(123 ^ 321, op.BitwiseXOr(123, 321));
            Assert.Equal(0xF000 ^ 0xFF000, op.BitwiseXOr(0xF000, 0xFF000));
        }

        /// <summary>
        /// Tests the Shift Left operation with integers
        /// </summary>
        [Fact]
        public void TestShiftLeft()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 << 1, op.ShiftLeft(2, 1));
            Assert.Equal(1 << 10, op.ShiftLeft(1, 10));
            Assert.Equal(0xF << 5, op.ShiftLeft(0xF, 5));
        }

        /// <summary>
        /// Tests the Shift Right operation with integers.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [Fact]
        public void TestShiftRight()
        {
            var op = new IntegerOperator();

            Assert.Equal(200 >> 1, op.ShiftRight(200, 1));
            Assert.Equal(0xF0000 >> 10, op.ShiftRight(0xF0000, 10));
            Assert.Equal(0xF00 >> 5, op.ShiftRight(0xF00, 5));
        }

        /// <summary>
        /// Tests the Greater operation with integers
        /// </summary>
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreater()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 > 2, op.Greater(2, 2));
            Assert.Equal(2 > -1, op.Greater(2, -1));
            Assert.Equal(10 > 100, op.Greater(10, 100));
        }

        /// <summary>
        /// Tests the Greater Or Equals operation with integers
        /// </summary>
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreaterOrEquals()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 >= 2, op.GreaterOrEquals(2, 2));
            Assert.Equal(2 >= -1, op.GreaterOrEquals(2, -1));
            Assert.Equal(10 >= 100, op.GreaterOrEquals(10, 100));
        }

        /// <summary>
        /// Tests the Less operation with integers
        /// </summary>
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLess()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 < 2, op.Less(2, 2));
            Assert.Equal(2 < -1, op.Less(2, -1));
            Assert.Equal(10 < 100, op.Less(10, 100));
        }

        /// <summary>
        /// Tests the Less Or Equals operation with integers
        /// </summary>
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLessOrEquals()
        {
            var op = new IntegerOperator();

            Assert.Equal(2 <= 2, op.LessOrEquals(2, 2));
            Assert.Equal(2 <= -1, op.LessOrEquals(2, -1));
            Assert.Equal(10 <= 100, op.LessOrEquals(10, 100));
        }

        /// <summary>
        /// Tests the Equals operation with integers
        /// </summary>
        [Fact]
        public void TestEquals()
        {
            var op = new IntegerOperator();

            Assert.Equal(2.Equals(2), op.Equals(2, 2));
            Assert.Equal(2.Equals(-1), op.Equals(2, -1));
            Assert.Equal(10.Equals(100), op.Equals(10, 100));
        }

        /// <summary>
        /// Tests the Arithmetic Negate operation with integers
        /// </summary>
        [Fact]
        public void TestArithmeticNegate()
        {
            var op = new IntegerOperator();

            Assert.Equal(-2, op.ArithmeticNegate(2));
            Assert.Equal(-(-2), op.ArithmeticNegate(-2));
            Assert.Equal(-100, op.ArithmeticNegate(100));
        }
    }
}