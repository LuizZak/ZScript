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
    }
}