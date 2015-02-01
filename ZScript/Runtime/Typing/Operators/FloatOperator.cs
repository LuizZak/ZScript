using System;
using ZScript.Runtime.Execution;

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
    }
}