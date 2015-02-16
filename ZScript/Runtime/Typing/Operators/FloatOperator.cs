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

using System;

namespace ZScript.Runtime.Typing.Operators
{
    /// <summary>
    /// Provides single precision floating point type operations
    /// </summary>
    public class FloatOperator : ITypeOperator<float>
    {
        public float Sum(float v1, float v2)
        {
            return v1 + v2;
        }

        public float Subtract(float v1, float v2)
        {
            return v1 - v2;
        }

        public float Multiply(float v1, float v2)
        {
            return v1 * v2;
        }

        public float Divide(float v1, float v2)
        {
            return v1 / v2;
        }

        public float Modulo(float v1, float v2)
        {
            return v1 % v2;
        }

        public float BitwiseAnd(float v1, float v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to floats");
        }

        public float BitwiseXOr(float v1, float v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to floats");
        }

        public float BitwiseOr(float v1, float v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to floats");
        }

        public bool Greater(float v1, float v2)
        {
            return v1 > v2;
        }

        public bool GreaterOrEquals(float v1, float v2)
        {
            return v1 >= v2;
        }

        public bool Less(float v1, float v2)
        {
            return v1 < v2;
        }

        public bool LessOrEquals(float v1, float v2)
        {
            return v1 <= v2;
        }

        public bool Equals(float v1, float v2)
        {
            return v1.Equals(v2);
        }

        public float ArithmeticNegate(float v1)
        {
            return -v1;
        }
    }
}