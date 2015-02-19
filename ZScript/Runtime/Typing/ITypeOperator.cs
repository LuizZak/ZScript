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
namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Interface that when implemented by a child class provides single-type arithmetic operations
    /// </summary>
    /// <typeparam name="T">The type of the values that can be operated on this type operator</typeparam>
    public interface ITypeOperator<T>
    {
        /// <summary>Sums two values</summary>
        /// <returns>The result of the operation</returns>
        T Sum(T v1, T v2);

        /// <summary>Subtracts two values</summary>
        /// <returns>The result of the operation</returns>
        T Subtract(T v1, T v2);

        /// <summary>Multiplies two values</summary>
        /// <returns>The result of the operation</returns>
        T Multiply(T v1, T v2);

        /// <summary>Divides v1 by v2</summary>
        /// <returns>The result of the operation</returns>
        T Divide(T v1, T v2);

        /// <summary>Returns the rest of the division of v1 by v2</summary>
        /// <returns>The result of the operation</returns>
        T Modulo(T v1, T v2);

        /// <summary>Returns the bitwise AND operation between v1 and v2</summary>
        /// <returns>The result of the operation</returns>
        T BitwiseAnd(T v1, T v2);

        /// <summary>Returns the bitwise Exclusive-OR operation between v1 and v2</summary>
        /// <returns>The result of the operation</returns>
        T BitwiseXOr(T v1, T v2);

        /// <summary>Returns the bitwise OR operation between v1 and v2</summary>
        /// <returns>The result of the operation</returns>
        T BitwiseOr(T v1, T v2);

        /// <summary>Returns the shift left (&lt;&lt;) operation between v1 and v2</summary>
        /// <returns>The result of the operation</returns>
        T ShiftLeft(T v1, T v2);

        /// <summary>Returns the shift right (&gt;&gt;) operation between v1 and v2</summary>
        /// <returns>The result of the operation</returns>
        T ShiftRight(T v1, T v2);

        /// <summary>Returns whether v1 is greater than v2</summary>
        /// <returns>The result of the operation</returns>
        bool Greater(T v1, T v2);

        /// <summary>Returns whether v1 is greater than or equals to v2</summary>
        /// <returns>The result of the operation</returns>
        bool GreaterOrEquals(T v1, T v2);

        /// <summary>Returns whether v1 is less than v2</summary>
        /// <returns>The result of the operation</returns>
        bool Less(T v1, T v2);

        /// <summary>Returns whether v1 is less than or equals to v2</summary>
        /// <returns>The result of the operation</returns>
        bool LessOrEquals(T v1, T v2);

        /// <summary>Returns whether v1 equals to v2</summary>
        /// <returns>The result of the operation</returns>
        bool Equals(T v1, T v2);

        /// <summary>Returns an arithmetic negation of a given value</summary>
        /// <returns>The given value, negated arithmetically</returns>
        T ArithmeticNegate(T v1);
    }
}