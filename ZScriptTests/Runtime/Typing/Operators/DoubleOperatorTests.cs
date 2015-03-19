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
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Runtime.Typing.Operators;

namespace ZScriptTests.Runtime.Typing.Operators
{
    /// <summary>
    /// Tests the DoubleOperator class and related components
    /// </summary>
    [TestClass]
    public class DoubleOperatorTests
    {
        /// <summary>
        /// Tests the Sum operation with doubles
        /// </summary>
        [TestMethod]
        public void TestSum()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(1.0 + 1.0, op.Sum(1.0, 1.0));
            Assert.AreEqual(-1.0 + -1.0, op.Sum(-1.0, -1.0));
            Assert.AreEqual(1.0 + -1.0, op.Sum(1.0, -1.0));
        }

        /// <summary>
        /// Tests the Subtract operation with doubles
        /// </summary>
        [TestMethod]
        public void TestSubtract()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(1.0 - 1.0, op.Subtract(1.0, 1.0));
            Assert.AreEqual(-1.0 - -1.0, op.Subtract(-1.0, -1.0));
            Assert.AreEqual(1.0 - -1.0, op.Subtract(1.0, -1.0));
        }

        /// <summary>
        /// Tests the Multiply operation with doubles
        /// </summary>
        [TestMethod]
        public void TestMultiply()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 * 2.0, op.Multiply(2.0, 2.0));
            Assert.AreEqual(2.0 * -1.0, op.Multiply(2.0, -1.0));
            Assert.AreEqual(10.0 * 100.0, op.Multiply(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Divide operation with doubles
        /// </summary>
        [TestMethod]
        public void TestDivide()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 / 2.0, op.Divide(2.0, 2.0));
            Assert.AreEqual(2.0 / -1.0, op.Divide(2.0, -1.0));
            Assert.AreEqual(10.0 / 100.0, op.Divide(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Modulo operation with doubles
        /// </summary>
        [TestMethod]
        public void TestModulo()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 % 2.0, op.Modulo(2.0, 2.0));
            Assert.AreEqual(2.0 % -1.0, op.Modulo(2.0, -1.0));
            Assert.AreEqual(10.0 % 100.0, op.Modulo(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Bitwise And operation with doubles.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Trying to apply a bitwise operation on doubles should raise an exception")]
        public void TestBitwiseAnd()
        {
            var op = new DoubleOperator();

            op.BitwiseAnd(2.0, 2.0);
        }

        /// <summary>
        /// Tests the Bitwise Or operation with doubles.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Trying to apply a bitwise operation on doubles should raise an exception")]
        public void TestBitwiseOr()
        {
            var op = new DoubleOperator();

            op.BitwiseOr(2.0, 2.0);
        }

        /// <summary>
        /// Tests the Bitwise XOr operation with doubles.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Trying to apply a bitwise operation on doubles should raise an exception")]
        public void TestBitwiseXOr()
        {
            var op = new DoubleOperator();

            op.BitwiseXOr(2.0, 2.0);
        }

        /// <summary>
        /// Tests the Shift Left operation with doubles.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Trying to apply a bitwise operation on doubles should raise an exception")]
        public void TestShiftLeft()
        {
            var op = new DoubleOperator();

            op.ShiftLeft(2.0, 2.0);
        }

        /// <summary>
        /// Tests the Shift Right operation with doubles.
        /// This test should raise an exception due to incompatible types
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Trying to apply a bitwise operation on doubles should raise an exception")]
        public void TestShiftRight()
        {
            var op = new DoubleOperator();

            op.ShiftRight(2.0, 2.0);
        }

        /// <summary>
        /// Tests the Greater operation with doubles
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreater()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 > 2.0, op.Greater(2.0, 2.0));
            Assert.AreEqual(2.0 > -1.0, op.Greater(2.0, -1.0));
            Assert.AreEqual(10.0 > 100.0, op.Greater(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Greater Or Equals operation with doubles
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestGreaterOrEquals()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 >= 2.0, op.GreaterOrEquals(2.0, 2.0));
            Assert.AreEqual(2.0 >= -1.0, op.GreaterOrEquals(2.0, -1.0));
            Assert.AreEqual(10.0 >= 100.0, op.GreaterOrEquals(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Less operation with doubles
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLess()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 < 2.0, op.Less(2.0, 2.0));
            Assert.AreEqual(2.0 < -1.0, op.Less(2.0, -1.0));
            Assert.AreEqual(10.0 < 100.0, op.Less(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Less Or Equals operation with doubles
        /// </summary>
        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestLessOrEquals()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0 <= 2.0, op.LessOrEquals(2.0, 2.0));
            Assert.AreEqual(2.0 <= -1.0, op.LessOrEquals(2.0, -1.0));
            Assert.AreEqual(10.0 <= 100.0, op.LessOrEquals(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Equals operation with doubles
        /// </summary>
        [TestMethod]
        public void TestEquals()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(2.0.Equals(2.0), op.Equals(2.0, 2.0));
            Assert.AreEqual(2.0.Equals(-1.0), op.Equals(2.0, -1.0));
            Assert.AreEqual(10.0.Equals(100.0), op.Equals(10.0, 100.0));
        }

        /// <summary>
        /// Tests the Arithmetic Negate operation with doubles
        /// </summary>
        [TestMethod]
        public void TestArithmeticNegate()
        {
            var op = new DoubleOperator();

            Assert.AreEqual(-2.0, op.ArithmeticNegate(2.0));
            Assert.AreEqual(-(-2.0), op.ArithmeticNegate(-2.0));
            Assert.AreEqual(-100.0, op.ArithmeticNegate(100.0));
        }
    }
}