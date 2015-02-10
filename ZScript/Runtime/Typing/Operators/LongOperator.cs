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
    }
}
