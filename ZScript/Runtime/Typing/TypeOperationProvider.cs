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
using ZScript.Runtime.Typing.Operators;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Provides type operation interfacing to clients
    /// </summary>
    public class TypeOperationProvider : ITypeOperator<object>
    {
        /// <summary>
        /// The operator to use when performing 32-bit integer operations
        /// </summary>
        private readonly ITypeOperator<int> _intTypeOperator;

        /// <summary>
        /// The operator to use when performing 64-bit integer operations
        /// </summary>
        private readonly ITypeOperator<long> _longTypeOperator;

        /// <summary>
        /// The operator to use when performing single precision floating-point operations
        /// </summary>
        private readonly ITypeOperator<double> _doubleTypeOperator;

        /// <summary>
        /// Initializes a new instance of the TypeOperationProvider class
        /// </summary>
        public TypeOperationProvider()
        {
            _intTypeOperator = new IntegerOperator();
            _longTypeOperator = new LongOperator();
            _doubleTypeOperator = new DoubleOperator();
        }

        public object Sum(object v1, object v2)
        {
            // Special case: One of the values is a string object
            if (v1 is string || v2 is string)
            {
                return v1.ToString() + v2;
            }

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Sum((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Sum(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Sum((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Sum(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Sum((double)v1, (double)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Sum((float)v1, (float)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Sum(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Sum operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object Subtract(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Subtract((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Subtract(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Subtract((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Subtract(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Subtract((float)v1, (float)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Subtract((double)v1, (double)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Subtract(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Subtract operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object Multiply(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Multiply((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Multiply(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Multiply((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Multiply(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Multiply((double)v1, (double)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Multiply((float)v1, (float)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Multiply(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Multiply operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object Divide(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Divide((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Divide(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Divide((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Divide(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Divide((double)v1, (double)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Divide((float)v1, (float)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Divide(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Divide operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object Modulo(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Modulo((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Modulo(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Modulo((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Modulo(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Modulo((double)v1, (double)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Modulo((float)v1, (float)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Modulo(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Modulo operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object BitwiseAnd(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 & (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.BitwiseAnd((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.BitwiseAnd(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.BitwiseAnd((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.BitwiseAnd(Convert.ToInt64(v1), Convert.ToInt64(v2));
            }

            throw new Exception("Cannot apply BitwiseAnd operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object BitwiseXOr(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 ^ (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.BitwiseXOr((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.BitwiseXOr(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.BitwiseXOr((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.BitwiseXOr(Convert.ToInt64(v1), Convert.ToInt64(v2));
            }

            throw new Exception("Cannot apply BitwiseXOr operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object BitwiseOr(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 | (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.BitwiseOr((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.BitwiseOr(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.BitwiseOr((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.BitwiseOr(Convert.ToInt64(v1), Convert.ToInt64(v2));
            }

            throw new Exception("Cannot apply BitwiseOr operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object ShiftLeft(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 ^ (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.ShiftLeft((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.ShiftLeft(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.ShiftLeft((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.ShiftLeft(Convert.ToInt64(v1), Convert.ToInt64(v2));
            }

            throw new Exception("Cannot apply Shift Left operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object ShiftRight(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 | (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.ShiftRight((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.ShiftRight(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.ShiftRight((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.ShiftRight(Convert.ToInt64(v1), Convert.ToInt64(v2));
            }

            throw new Exception("Cannot apply Shift Right operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public bool Greater(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Greater((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Greater(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Greater((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Greater(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Greater((float)v1, (float)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Greater((double)v1, (double)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Greater(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Greater operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public bool GreaterOrEquals(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.GreaterOrEquals((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.GreaterOrEquals(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.GreaterOrEquals((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.GreaterOrEquals(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.GreaterOrEquals((float)v1, (float)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.GreaterOrEquals((double)v1, (double)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.GreaterOrEquals(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Greater operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public bool Less(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Less((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Less(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Less((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Less(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.Less((float)v1, (float)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Less((double)v1, (double)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Less(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply Less operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public bool LessOrEquals(object v1, object v2)
        {
            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.LessOrEquals((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.LessOrEquals(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.LessOrEquals((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.LessOrEquals(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.LessOrEquals((float)v1, (float)v2);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.LessOrEquals((double)v1, (double)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.LessOrEquals(Convert.ToDouble(v1), Convert.ToDouble(v2));
            }

            throw new Exception("Cannot apply LessOrEquals operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public new bool Equals(object v1, object v2)
        {
            if (v1 is bool && v2 is bool)
                return (bool)v1 && (bool)v2;

            var c = BestFitForTypes(v1, v2);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.Equals((int)v1, (int)v2);
                case NumberClass.Integer:
                    return _intTypeOperator.Equals(Convert.ToInt32(v1), Convert.ToInt32(v2));

                case NumberClass.ExactLong:
                    return _longTypeOperator.Equals((long)v1, (long)v2);
                case NumberClass.Long:
                    return _longTypeOperator.Equals(Convert.ToInt64(v1), Convert.ToInt64(v2));

                case NumberClass.ExactFloat:
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.Equals((float)v1, (float)v2);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.Equals(Convert.ToSingle(v1), Convert.ToSingle(v2));
            }

            throw new Exception("Cannot apply Equals operation on objects of type " + v1.GetType() + " and " + v2.GetType());
        }

        public object ArithmeticNegate(object v1)
        {
            var c = NumberClassForNumber(v1);

            switch (c)
            {
                case NumberClass.ExactInteger:
                    return _intTypeOperator.ArithmeticNegate((int)v1);
                case NumberClass.Integer:
                    return _intTypeOperator.ArithmeticNegate(Convert.ToInt32(v1));

                case NumberClass.ExactLong:
                    return _longTypeOperator.ArithmeticNegate((long)v1);
                case NumberClass.Long:
                    return _longTypeOperator.ArithmeticNegate(Convert.ToInt64(v1));

                case NumberClass.ExactFloat:
                    return _doubleTypeOperator.ArithmeticNegate((float)v1);
                case NumberClass.ExactDouble:
                    return _doubleTypeOperator.ArithmeticNegate((double)v1);
                case NumberClass.Float:
                case NumberClass.Double:
                    return _doubleTypeOperator.ArithmeticNegate(Convert.ToDouble(v1));
            }

            throw new Exception("Cannot apply ArithmeticNegate operation on objects of type " + v1.GetType());
        }

        public object LogicalNegate(object v1)
        {
            if (v1 is bool)
                return !(bool)v1;

            throw new Exception("Cannot apply LogicalNegate operation on objects of type " + v1.GetType());
        }

        /// <summary>
        /// Tries to cast a given number object into either an Int64 (long) value or double precision floating-point value.
        /// If the method fails to cast the number, the same value is returned
        /// </summary>
        /// <param name="numberObject">The boxed number object to cast</param>
        /// <returns>A boxed long or double number cast from the given number object</returns>
        public static object TryCastNumber(object numberObject)
        {
            var c = NumberClassForNumber(numberObject, false);
            if (c == NumberClass.NotANumber)
            {
                return numberObject;
            }

            // Number already casted
            if (c == NumberClass.ExactLong || c == NumberClass.ExactDouble)
                return numberObject;

            if (c == NumberClass.ExactFloat || c == NumberClass.Float || c == NumberClass.Double)
                return Convert.ChangeType(numberObject, typeof(double));

            return Convert.ChangeType(numberObject, typeof(long));
        }

        /// <summary>
        /// Returns the number type for a given boxed number type
        /// </summary>
        /// <param name="boxedNumber">The boxed number</param>
        /// <param name="throwOnError">Whether to throw an exception when the boxed value is not a valid number</param>
        /// <returns>The number type for the given boxed number</returns>
        public static NumberClass NumberClassForNumber(object boxedNumber, bool throwOnError = true)
        {
            if (boxedNumber is int)
                return NumberClass.ExactInteger;
            if (boxedNumber is long)
                return NumberClass.ExactLong;
            if (boxedNumber is float)
                return NumberClass.ExactFloat;
            if (boxedNumber is double)
                return NumberClass.ExactDouble;

            if (boxedNumber is byte || boxedNumber is sbyte || boxedNumber is short || boxedNumber is ushort || boxedNumber is uint)
                return NumberClass.Long;
            if (boxedNumber is ulong)
                return NumberClass.Double;

            if (throwOnError)
                throw new ArgumentException("The provided boxed object is not a valid numeric type", "boxedNumber");

            return NumberClass.NotANumber;
        }

        /// <summary>
        /// Returns the best fit number class that can deal with both number types
        /// </summary>
        /// <param name="type1">A valid number class</param>
        /// <param name="type2">A valid number class</param>
        /// <returns>A number class that can be best used to represent the two number classes</returns>
        public static NumberClass BestFitForTypes(NumberClass type1, NumberClass type2)
        {
            if (type1 == NumberClass.NotANumber || type1 == NumberClass.NotANumber)
                return NumberClass.NotANumber;

            if (type1 == NumberClass.ExactFloat && type2 == NumberClass.ExactFloat)
                return NumberClass.ExactFloat;
            if (type1 == NumberClass.ExactDouble && type2 == NumberClass.ExactDouble)
                return NumberClass.ExactDouble;
            if (type1 == NumberClass.ExactLong && type2 == NumberClass.ExactLong)
                return NumberClass.ExactLong;
            if (type1 == NumberClass.ExactInteger && type2 == NumberClass.ExactInteger)
                return NumberClass.ExactInteger;

            if (type1 == NumberClass.Double || type2 == NumberClass.Double || type1 == NumberClass.ExactDouble || type2 == NumberClass.ExactDouble)
                return NumberClass.Double;

            if (type1 == NumberClass.Float || type2 == NumberClass.Float || type1 == NumberClass.ExactFloat || type2 == NumberClass.ExactFloat)
                return NumberClass.Float;

            if (type1 == NumberClass.Long || type2 == NumberClass.Long || type1 == NumberClass.ExactLong || type2 == NumberClass.ExactLong)
                return NumberClass.Long;

            return NumberClass.Integer;
        }

        /// <summary>
        /// Returns the best fit number class that can deal with both number types
        /// </summary>
        /// <param name="boxed1">A valid boxed number</param>
        /// <param name="boxed2">A valid boxed number</param>
        /// <returns>A number class that can be best used to represent the two number classes</returns>
        public static NumberClass BestFitForTypes(object boxed1, object boxed2)
        {
            var c1 = NumberClassForNumber(boxed1);
            var c2 = NumberClassForNumber(boxed2);

            return BestFitForTypes(c1, c2);
        }

        /// <summary>
        /// Returns the type that equates to the specified number type
        /// </summary>
        /// <param name="type">The type of number to get the type of</param>
        /// <returns>The type that equates to the specified number type</returns>
        public static Type GetTypeForNumberType(NumberClass type)
        {
            switch (type)
            {
                case NumberClass.Integer:
                    return typeof(int);
                case NumberClass.Long:
                    return typeof(long);
                case NumberClass.Float:
                    return typeof(float);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Specified the type of a number
        /// </summary>
        public enum NumberClass
        {
            /// <summary>A value that can be used as an Int32 type</summary>
            Integer,
            /// <summary>An exact Int32 type</summary>
            ExactInteger,
            /// <summary>A value that can be used as an Int64 type</summary>
            Long,
            /// <summary>An exact Int64 type</summary>
            ExactLong,
            /// <summary>A value that can be used as a single precision floating point type</summary>
            Float,
            /// <summary>An exact single precision floating type</summary>
            ExactFloat,
            /// <summary>A value that can be used as a double precision floating point type</summary>
            Double,
            /// <summary>An exact double precision floating type</summary>
            ExactDouble,
            /// <summary>A non-number type</summary>
            NotANumber
        }
    }
}