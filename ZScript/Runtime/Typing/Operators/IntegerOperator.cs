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

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

namespace ZScript.Runtime.Typing.Operators
{
    /// <summary>
    /// Provides integer type operations
    /// </summary>
    public class IntegerOperator : ITypeOperator<int>
    {
        public int Sum(int v1, int v2)
        {
            return v1 + v2;
        }

        public int Subtract(int v1, int v2)
        {
            return v1 - v2;
        }

        public int Multiply(int v1, int v2)
        {
            return v1 * v2; 
        }

        public int Divide(int v1, int v2)
        {
            return v1 / v2;
        }

        public int Modulo(int v1, int v2)
        {
            return v1 % v2;
        }

        public int BitwiseAnd(int v1, int v2)
        {
            return v1 & v2;
        }

        public int BitwiseXOr(int v1, int v2)
        {
            return v1 ^ v2;
        }

        public int BitwiseOr(int v1, int v2)
        {
            return v1 | v2;
        }

        public int ShiftLeft(int v1, int v2)
        {
            return v1 << v2;
        }

        public int ShiftRight(int v1, int v2)
        {
            return v1 >> v2;
        }

        public bool Greater(int v1, int v2)
        {
            return v1 > v2;
        }

        public bool GreaterOrEquals(int v1, int v2)
        {
            return v1 >= v2;
        }

        public bool Less(int v1, int v2)
        {
            return v1 < v2;
        }

        public bool LessOrEquals(int v1, int v2)
        {
            return v1 <= v2;
        }

        public bool Equals(int v1, int v2)
        {
            return v1 == v2;
        }

        public int ArithmeticNegate(int v1)
        {
            return -v1;
        }
    }
}