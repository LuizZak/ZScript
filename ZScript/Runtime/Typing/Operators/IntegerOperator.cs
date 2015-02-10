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
    }
}