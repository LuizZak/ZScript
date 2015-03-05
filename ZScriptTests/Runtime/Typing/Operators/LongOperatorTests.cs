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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Runtime.Typing.Operators;

namespace ZScriptTests.Runtime.Typing.Operators
{
    /// <summary>
    /// Tests the functionality of the LongOperator class
    /// </summary>
    [TestClass]
    public class LongOperatorTests
    {
        /// <summary>
        /// Tests the Sum operation with longs
        /// </summary>
        [TestMethod]
        public void TestSum()
        {
            var op = new LongOperator();

            Assert.AreEqual(1L + 1L, op.Sum(1L, 1L));
            Assert.AreEqual(-1L + -1L, op.Sum(-1L, -1L));
            Assert.AreEqual(1L + -1L, op.Sum(1L, -1L));
        }

        /// <summary>
        /// Tests the Subtract operation with longs
        /// </summary>
        [TestMethod]
        public void TestSubtract()
        {
            var op = new LongOperator();

            Assert.AreEqual(1L - 1L, op.Subtract(1L, 1L));
            Assert.AreEqual(-1L - -1L, op.Subtract(-1L, -1L));
            Assert.AreEqual(1L - -1L, op.Subtract(1L, -1L));
        }

        /// <summary>
        /// Tests the Multiply operation with longs
        /// </summary>
        [TestMethod]
        public void TestMultiply()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L * 2L, op.Multiply(2L, 2L));
            Assert.AreEqual(2L * -1L, op.Multiply(2L, -1L));
            Assert.AreEqual(10L * 100L, op.Multiply(10L, 100L));
        }

        /// <summary>
        /// Tests the Divide operation with longs
        /// </summary>
        [TestMethod]
        public void TestDivide()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L / 2L, op.Divide(2L, 2L));
            Assert.AreEqual(2L / -1L, op.Divide(2L, -1L));
            Assert.AreEqual(10L / 100L, op.Divide(10L, 100L));
        }

        /// <summary>
        /// Tests the Modulo operation with longs
        /// </summary>
        [TestMethod]
        public void TestModulo()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L % 2L, op.Modulo(2L, 2L));
            Assert.AreEqual(2L % -1L, op.Modulo(2L, -1L));
            Assert.AreEqual(10L % 100L, op.Modulo(10L, 100L));
        }

        /// <summary>
        /// Tests the Bitwise And operation with longs
        /// </summary>
        [TestMethod]
        public void TestBitwiseAnd()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L & 2L, op.BitwiseAnd(2L, 2L));
            Assert.AreEqual(123L & 321L, op.BitwiseAnd(123L, 321L));
            Assert.AreEqual(0xF000L & 0xFF000L, op.BitwiseAnd(0xF000L, 0xFF000L));
        }

        /// <summary>
        /// Tests the Bitwise Or operation with longs
        /// </summary>
        [TestMethod]
        public void TestBitwiseOr()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L | 2L, op.BitwiseOr(2L, 2L));
            Assert.AreEqual(123L | 321L, op.BitwiseOr(123L, 321L));
            Assert.AreEqual(0xF000L | 0xFF000L, op.BitwiseOr(0xF000L, 0xFF000L));
        }

        /// <summary>
        /// Tests the Bitwise XOr operation with longs
        /// </summary>
        [TestMethod]
        public void TestBitwiseXOr()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L ^ 2L, op.BitwiseXOr(2L, 2L));
            Assert.AreEqual(123L ^ 321L, op.BitwiseXOr(123L, 321L));
            Assert.AreEqual(0xF000L ^ 0xFF000L, op.BitwiseXOr(0xF000L, 0xFF000L));
        }

        /// <summary>
        /// Tests the Shift Left operation with longs
        /// </summary>
        [TestMethod]
        public void TestShiftLeft()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L << 1, op.ShiftLeft(2L, 1L));
            Assert.AreEqual(1L << 10, op.ShiftLeft(1L, 10L));
            Assert.AreEqual(0xF << 5, op.ShiftLeft(0xF, 5L));
        }

        /// <summary>
        /// Tests the Shift Right operation with longs.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        public void TestShiftRight()
        {
            var op = new LongOperator();

            Assert.AreEqual(200L >> 1, op.ShiftRight(200L, 1L));
            Assert.AreEqual(0xF0000L >> 10, op.ShiftRight(0xF0000L, 10));
            Assert.AreEqual(0xF00L >> 5, op.ShiftRight(0xF00L, 5L));
        }

        /// <summary>
        /// Tests the Greater operation with longs
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreater()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L > 2L, op.Greater(2L, 2L));
            Assert.AreEqual(2L > -1L, op.Greater(2L, -1L));
            Assert.AreEqual(10L > 100L, op.Greater(10L, 100L));
        }

        /// <summary>
        /// Tests the Greater Or Equals operation with longs
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreaterOrEquals()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L >= 2L, op.GreaterOrEquals(2L, 2L));
            Assert.AreEqual(2L >= -1L, op.GreaterOrEquals(2L, -1L));
            Assert.AreEqual(10L >= 100L, op.GreaterOrEquals(10L, 100L));
        }

        /// <summary>
        /// Tests the Less operation with longs
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLess()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L < 2L, op.Less(2L, 2L));
            Assert.AreEqual(2L < -1L, op.Less(2L, -1L));
            Assert.AreEqual(10L < 100L, op.Less(10L, 100L));
        }

        /// <summary>
        /// Tests the Less Or Equals operation with longs
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLessOrEquals()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L <= 2L, op.LessOrEquals(2L, 2L));
            Assert.AreEqual(2L <= -1L, op.LessOrEquals(2L, -1L));
            Assert.AreEqual(10L <= 100L, op.LessOrEquals(10L, 100L));
        }

        /// <summary>
        /// Tests the Equals operation with longs
        /// </summary>
        [TestMethod]
        public void TestEquals()
        {
            var op = new LongOperator();

            Assert.AreEqual(2L.Equals(2L), op.Equals(2L, 2L));
            Assert.AreEqual(2L.Equals(-1L), op.Equals(2L, -1L));
            Assert.AreEqual(10L.Equals(100L), op.Equals(10L, 100L));
        }

        /// <summary>
        /// Tests the Arithmetic Negate operation with longs
        /// </summary>
        [TestMethod]
        public void TestArithmeticNegate()
        {
            var op = new LongOperator();

            Assert.AreEqual(-2L, op.ArithmeticNegate(2L));
            Assert.AreEqual(-(-2L), op.ArithmeticNegate(-2L));
            Assert.AreEqual(-100L, op.ArithmeticNegate(100L));
        }
    }
}