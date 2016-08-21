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

namespace ZScript.Runtime.Typing.Operators
{
    /// <summary>
    /// Provides Int64 type operations
    /// </summary>
    public class LongOperator : ITypeOperator<long>
    {
        public long Sum(long v1, long v2)
        {
            return v1 + v2;
        }

        public long Subtract(long v1, long v2)
        {
            return v1 - v2;
        }

        public long Multiply(long v1, long v2)
        {
            return v1 * v2;
        }

        public long Divide(long v1, long v2)
        {
            return v1 / v2;
        }

        public long Modulo(long v1, long v2)
        {
            return v1 % v2;
        }

        public long BitwiseAnd(long v1, long v2)
        {
            return v1 & v2;
        }

        public long BitwiseXOr(long v1, long v2)
        {
            return v1 ^ v2;
        }

        public long BitwiseOr(long v1, long v2)
        {
            return v1 | v2;
        }

        public long ShiftLeft(long v1, long v2)
        {
            return v1 << (int)v2;
        }

        public long ShiftRight(long v1, long v2)
        {
            return v1 >> (int)v2;
        }

        public bool Greater(long v1, long v2)
        {
            return v1 > v2;
        }

        public bool GreaterOrEquals(long v1, long v2)
        {
            return v1 >= v2;
        }

        public bool Less(long v1, long v2)
        {
            return v1 < v2;
        }

        public bool LessOrEquals(long v1, long v2)
        {
            return v1 <= v2;
        }

        public bool Equals(long v1, long v2)
        {
            return v1 == v2;
        }

        public long ArithmeticNegate(long v1)
        {
            return -v1;
        }
    }
}