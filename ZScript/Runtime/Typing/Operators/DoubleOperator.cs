﻿/*
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
    /// Provides double precision doubleing point type operations
    /// </summary>
    public class DoubleOperator : ITypeOperator<double>
    {
        public double Sum(double v1, double v2)
        {
            return v1 + v2;
        }

        public double Subtract(double v1, double v2)
        {
            return v1 - v2;
        }

        public double Multiply(double v1, double v2)
        {
            return v1 * v2;
        }

        public double Divide(double v1, double v2)
        {
            return v1 / v2;
        }

        public double Modulo(double v1, double v2)
        {
            return v1 % v2;
        }

        public double BitwiseAnd(double v1, double v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to doubles");
        }

        public double BitwiseXOr(double v1, double v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to doubles");
        }

        public double BitwiseOr(double v1, double v2)
        {
            throw new InvalidOperationException("Cannot apply bitwise and to doubles");
        }

        public bool Greater(double v1, double v2)
        {
            return v1 > v2;
        }

        public bool GreaterOrEquals(double v1, double v2)
        {
            return v1 >= v2;
        }

        public bool Less(double v1, double v2)
        {
            return v1 < v2;
        }

        public bool LessOrEquals(double v1, double v2)
        {
            return v1 <= v2;
        }

        public bool Equals(double v1, double v2)
        {
            return v1.Equals(v2);
        }

        public double ArithmeticNegate(double v1)
        {
            return -v1;
        }
    }
}