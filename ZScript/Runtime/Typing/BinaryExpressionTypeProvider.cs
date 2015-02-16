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
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Helper class to deal with binary expression handling and transformation
    /// </summary>
    public class BinaryExpressionTypeProvider
    {
        /// <summary>
        /// The type provider to use when searching types to return
        /// </summary>
        private readonly TypeProvider _provider;

        /// <summary>
        /// Cached int type for faster check
        /// </summary>
        private readonly TypeDef _intType;

        /// <summary>
        /// Cached float type for faster check
        /// </summary>
        private readonly TypeDef _floatType;

        /// <summary>
        /// Cached boolean type for faster check
        /// </summary>
        private readonly TypeDef _boolType;

        /// <summary>
        /// Cached string type for faster check
        /// </summary>
        private readonly TypeDef _stringType;

        /// <summary>
        /// Cached null type for faster check
        /// </summary>
        private readonly TypeDef _nullType;

        /// <summary>
        /// Initializes a new instance of the BinaryExpressionTypeProvider class
        /// </summary>
        /// <param name="provider">The type provider to use when searching types to return</param>
        public BinaryExpressionTypeProvider(TypeProvider provider)
        {
            _provider = provider;
            _intType = provider.IntegerType();
            _floatType = provider.FloatType();
            _boolType = provider.BooleanType();
            _stringType = provider.StringType();
            _nullType = provider.NullType();
        }

        /// <summary>
        /// Returns whether the given operation can be performed between the two types
        /// </summary>
        /// <param name="operation">A valid binary operation to perform</param>
        /// <param name="type1">The first type to check</param>
        /// <param name="type2">The second type to check</param>
        /// <returns>true if the operation can be performed between the two values, false otherwise</returns>
        public bool CanPerformOperation(VmInstruction operation, TypeDef type1, TypeDef type2)
        {
            // Evaluate the operation
            switch (operation)
            {
                // Sum and subtraction
                case VmInstruction.Add:
                    return CanSum(type1, type2);
                case VmInstruction.Subtract:
                    return CanSubtract(type1, type2);

                // Multiplication and division
                case VmInstruction.Multiply:
                    return CanMultiply(type1, type2);
                case VmInstruction.Divide:
                    return CanDivide(type1, type2);

                // Modulo operator
                case VmInstruction.Modulo:
                    return CanModulo(type1, type2);

                // Bitwise operators
                case VmInstruction.BitwiseAnd:
                    return CanBitwiseAnd(type1, type2);
                case VmInstruction.BitwiseOr:
                    return CanBitwiseOr(type1, type2);
                case VmInstruction.BitwiseXOr:
                    return CanBitwiseXOr(type1, type2);

                // Equality/Inequality checks
                case VmInstruction.Equals:
                    return CanEqual(type1, type2);
                case VmInstruction.Unequals:
                    return CanUnequal(type1, type2);
                case VmInstruction.Less:
                    return CanLess(type1, type2);
                case VmInstruction.LessOrEquals:
                    return CanLessOrEquals(type1, type2);
                case VmInstruction.Greater:
                    return CanGreater(type1, type2);
                case VmInstruction.GreaterOrEquals:
                    return CanGreaterOrEquals(type1, type2);

                case VmInstruction.LogicalAnd:
                    return CanLogicalAnd(type1, type2);
                case VmInstruction.LogicalOr:
                    return CanLogicalOr(type1, type2);
                default:
                    throw new ArgumentException("The VM instruction " + operation + " cannot be used as a valid binary operator");
            }
        }

        /// <summary>
        /// Returns the type that is the result of a given operation on two types
        /// </summary>
        /// <param name="operation">A valid binary operation to perform</param>
        /// <param name="type1">The first type to check</param>
        /// <param name="type2">The second type to check</param>
        /// <returns>A TypeDef resulting from the operation between the two types</returns>
        public TypeDef TypeForOperation(VmInstruction operation, TypeDef type1, TypeDef type2)
        {
            // Evaluate the operation
            switch (operation)
            {
                // Sum and subtraction
                case VmInstruction.Add:
                    return TypeForSum(type1, type2);
                case VmInstruction.Subtract:
                    return TypeForSubtraction(type1, type2);

                // Multiplication and division
                case VmInstruction.Multiply:
                    return TypeForMultiplication(type1, type2);
                case VmInstruction.Divide:
                    return TypeForDivision(type1, type2);

                // Modulo operator
                case VmInstruction.Modulo:
                    return TypeForModulation(type1, type2);

                // Bitwise operators
                case VmInstruction.BitwiseAnd:
                    return TypeForBitwiseAnd(type1, type2);
                case VmInstruction.BitwiseOr:
                    return TypeForBitwiseOr(type1, type2);
                case VmInstruction.BitwiseXOr:
                    return TypeForBitwiseXOr(type1, type2);

                // Equality/Inequality checks
                case VmInstruction.Equals:
                    return TypeForEquals(type1, type2);
                case VmInstruction.Unequals:
                    return TypeForUnequals(type1, type2);
                case VmInstruction.Less:
                    return TypeForLess(type1, type2);
                case VmInstruction.LessOrEquals:
                    return TypeForLessOrEquals(type1, type2);
                case VmInstruction.Greater:
                    return TypeForGreater(type1, type2);
                case VmInstruction.GreaterOrEquals:
                    return TypeForGreaterOrEquals(type1, type2);

                case VmInstruction.LogicalAnd:
                    return TypeForLogicalAnd(type1, type2);
                case VmInstruction.LogicalOr:
                    return TypeForLogicalOr(type1, type2);
                default:
                    throw new ArgumentException("The VM instruction " + operation + " cannot be used as a valid binary operator");
            }
        }

        #region Availability test methods

        #region Multiplicative

        /// <summary>
        /// Returns whether the two given types can be multiplied
        /// </summary>
        public bool CanMultiply(TypeDef type1, TypeDef type2)
        {
            return IsNumeric(type1) && IsNumeric(type2);
        }

        /// <summary>
        /// Returns whether the two given types can be divided
        /// </summary>
        public bool CanDivide(TypeDef type1, TypeDef type2)
        {
            return IsNumeric(type1) && IsNumeric(type2);
        }

        /// <summary>
        /// Returns whether the two given types can be divided
        /// </summary>
        public bool CanModulo(TypeDef type1, TypeDef type2)
        {
            return IsNumeric(type1) && IsNumeric(type2);
        }

        #endregion

        #region Additive

        /// <summary>
        /// Returns whether the two given types can be summed
        /// </summary>
        public bool CanSum(TypeDef type1, TypeDef type2)
        {
            return (IsNumeric(type1) || type1 == _stringType) && (IsNumeric(type2) || type2 == _stringType);
        }

        /// <summary>
        /// Returns whether the two given types can be subtracted
        /// </summary>
        public bool CanSubtract(TypeDef type1, TypeDef type2)
        {
            return IsNumeric(type1) && IsNumeric(type2);
        }

        #endregion

        #region Bitwise

        /// <summary>
        /// Returns whether the two given types can be 'bitwise and'ed
        /// </summary>
        public bool CanBitwiseAnd(TypeDef type1, TypeDef type2)
        {
            return type1 == _intType && type2 == _intType;
        }

        /// <summary>
        /// Returns whether the two given types can be 'bitwise xor'ed
        /// </summary>
        public bool CanBitwiseXOr(TypeDef type1, TypeDef type2)
        {
            return type1 == _intType && type2 == _intType;
        }

        /// <summary>
        /// Returns whether the two given types can be 'bitwise or'ed
        /// </summary>
        public bool CanBitwiseOr(TypeDef type1, TypeDef type2)
        {
            return type1 == _intType && type2 == _intType;
        }

        #endregion

        #region Comparisions

        /// <summary>
        /// Returns whether the two given types can be compared for equality
        /// </summary>
        public bool CanEqual(TypeDef type1, TypeDef type2)
        {
            return !type1.IsVoid && !type2.IsVoid;
        }

        /// <summary>
        /// Returns whether the two given types can be compared for inequality
        /// </summary>
        public bool CanUnequal(TypeDef type1, TypeDef type2)
        {
            return !type1.IsVoid && !type2.IsVoid;
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'larger or equals' operation
        /// </summary>
        public bool CanGreaterOrEquals(TypeDef type1, TypeDef type2)
        {
            return (IsNumeric(type1) || type1 == _stringType) && (IsNumeric(type2) || type2 == _stringType);
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'larger than' operation
        /// </summary>
        public bool CanGreater(TypeDef type1, TypeDef type2)
        {
            return (IsNumeric(type1) || type1 == _stringType) && (IsNumeric(type2) || type2 == _stringType);
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'less or equals' operation
        /// </summary>
        public bool CanLessOrEquals(TypeDef type1, TypeDef type2)
        {
            return (IsNumeric(type1) || type1 == _stringType) && (IsNumeric(type2) || type2 == _stringType);
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'less' operation
        /// </summary>
        public bool CanLess(TypeDef type1, TypeDef type2)
        {
            return (IsNumeric(type1) || type1 == _stringType) && (IsNumeric(type2) || type2 == _stringType);
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'logical and' operation
        /// </summary>
        public bool CanLogicalAnd(TypeDef type1, TypeDef type2)
        {
            return IsLogicType(type1) && IsLogicType(type2);
        }

        /// <summary>
        /// Returns whether the two given types can be compared with a 'logical or' operation
        /// </summary>
        public bool CanLogicalOr(TypeDef type1, TypeDef type2)
        {
            return IsLogicType(type1) && IsLogicType(type2);
        }

        #endregion

        #region Prefix, postfix and unary

        /// <summary>
        /// Returns whether the given type can have a prefix operator applied to it
        /// </summary>
        public bool CanPrefix(TypeDef type)
        {
            return IsNumeric(type);
        }

        /// <summary>
        /// Returns whether the given type can have a postfix operator applied to it
        /// </summary>
        public bool CanPostfix(TypeDef type)
        {
            return IsNumeric(type);
        }

        /// <summary>
        /// Returns whether the given type can have an unary operator specified by a given VmInstruction applied to it
        /// </summary>
        public bool CanUnary(TypeDef type, VmInstruction instruction)
        {
            switch (instruction)
            {
                case VmInstruction.LogicalNegate:
                    return IsLogicType(type);
                case VmInstruction.ArithmeticNegate:
                    return IsNumeric(type);
            }

            return false;
        }

        #endregion

        #endregion

        #region Type resolving methods

        #region Multiplicative

        /// <summary>
        /// Returns the type that represents the result of the multiplication of the two given types
        /// </summary>
        public TypeDef TypeForMultiplication(TypeDef type1, TypeDef type2)
        {
            return (type1 == _floatType || type2 == _floatType ? _floatType : _intType);
        }

        /// <summary>
        /// Returns the type that represents the result of the division of the two given types
        /// </summary>
        public TypeDef TypeForDivision(TypeDef type1, TypeDef type2)
        {
            return (type1 == _floatType || type2 == _floatType ? _floatType : _intType);
        }

        /// <summary>
        /// Returns the type that represents the result of the modulation of the two given types
        /// </summary>
        public TypeDef TypeForModulation(TypeDef type1, TypeDef type2)
        {
            return (type1 == _floatType || type2 == _floatType ? _floatType : _intType);
        }

        #endregion

        #region Additive

        /// <summary>
        /// Returns the type that represents the result of the sum of the two given types
        /// </summary>
        public TypeDef TypeForSum(TypeDef type1, TypeDef type2)
        {
            return (type1 == _floatType || type2 == _floatType ? _floatType : _intType);
        }

        /// <summary>
        /// Returns the type that represents the result of the subtraction of the two given types
        /// </summary>
        public TypeDef TypeForSubtraction(TypeDef type1, TypeDef type2)
        {
            return (type1 == _floatType || type2 == _floatType ? _floatType : _intType);
        }

        #endregion

        #region Additive

        /// <summary>
        /// Returns the type that represents the result of the 'bitwise and'ing of the two given types
        /// </summary>
        public TypeDef TypeForBitwiseAnd(TypeDef type1, TypeDef type2)
        {
            return _intType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'bitwise xor'ing of the two given types
        /// </summary>
        public TypeDef TypeForBitwiseXOr(TypeDef type1, TypeDef type2)
        {
            return _intType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'bitwise or'ing of the two given types
        /// </summary>
        public TypeDef TypeForBitwiseOr(TypeDef type1, TypeDef type2)
        {
            return _intType;
        }

        #endregion

        #region Comparisions

        /// <summary>
        /// Returns the type that represents the result of the equality check of the two given types
        /// </summary>
        public TypeDef TypeForEquals(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the inequality check of the two given types
        /// </summary>
        public TypeDef TypeForUnequals(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'larger or equals' comparision of the two given types
        /// </summary>
        public TypeDef TypeForGreaterOrEquals(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'larger' comparision of the two given types
        /// </summary>
        public TypeDef TypeForGreater(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'less or equals' comparision of the two given types
        /// </summary>
        public TypeDef TypeForLessOrEquals(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the 'less' comparision of the two given types
        /// </summary>
        public TypeDef TypeForLess(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        #endregion

        #region Logical comparisions

        /// <summary>
        /// Returns the type that represents the result of the logical And check of the two given types
        /// </summary>
        public TypeDef TypeForLogicalAnd(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        /// <summary>
        /// Returns the type that represents the result of the logical Or check of the two given types
        /// </summary>
        public TypeDef TypeForLogicalOr(TypeDef type1, TypeDef type2)
        {
            return _boolType;
        }

        #endregion

        #region Prefix, postfix and unary

        /// <summary>
        /// Returns whether the given type can have a prefix operator applied to it
        /// </summary>
        public TypeDef TypeForPrefix(TypeDef type)
        {
            return type;
        }

        /// <summary>
        /// Returns whether the given type can have a postfix operator applied to it
        /// </summary>
        public TypeDef TypeForPostfix(TypeDef type)
        {
            return type;
        }

        /// <summary>
        /// Returns whether the given type can have an unary operator specified by a given VmInstruction applied to it
        /// </summary>
        public TypeDef TypeForUnary(TypeDef type, VmInstruction instruction)
        {
            switch (instruction)
            {
                case VmInstruction.LogicalNegate:
                    return _boolType;
                case VmInstruction.ArithmeticNegate:
                    return type;
            }

            throw new Exception("Instruction does not represents an unary operation: " + instruction);
        }

        #endregion

        #endregion

        /// <summary>
        /// Returns whether a given type is a numeric (int or float) type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>true if the type represents an integer or floating-point number, false otherwise</returns>
        public bool IsNumeric(TypeDef type)
        {
            return type == _intType || type == _floatType || type.IsAny;
        }

        /// <summary>
        /// Returns whether a given type is a logical (boolean) type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>true if the type represents a boolean, false otherwise</returns>
        public bool IsLogicType(TypeDef type)
        {
            return type == _boolType || type.IsAny;
        }
    }
}