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
using System.Globalization;
using JetBrains.Annotations;

namespace ZScript.Parsing
{
    /// <summary>
    /// Static class with helper methods aimed as facilitating a unified and testable string -> value parsing
    /// </summary>
    public static class ValueParser
    {
        /// <summary>
        /// Global number format info used to convert the numeric values
        /// </summary>
        private static readonly NumberFormatInfo Nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        /// <summary>
        /// Tries to parse a given value string into a valid value.
        /// The input value string may either be a number, 'null', 'false', or 'true'
        /// </summary>
        /// <param name="valueString">The value string to parse</param>
        /// <param name="value">The output value parser</param>
        /// <returns>Whether the parsing was successful</returns>
        public static bool TryParseValueBoxed(string valueString, out object value)
        {
            switch (valueString)
            {
                case "null":
                    value = null;
                    return true;
                case "true":
                    value = true;
                    return true;
                case "false":
                    value = false;
                    return true;
                case "Infinity":
                    value = double.PositiveInfinity;
                    return true;
                case "NInfinity":
                    value = double.NegativeInfinity;
                    return true;
                case "NaN":
                    value = double.NaN;
                    return true;
            }

            value = ParseNumberBoxed(valueString);
            return value != null;
        }

        /// <summary>
        /// Converts a given value string into a numeric form and returns it as a boxed object.
        /// If the conversion fails, a null value is returned
        /// </summary>
        /// <param name="valueString">The string to convert</param>
        /// <returns>The converted string as a boxed numeric value, or null, if the conversion fails</returns>
        [CanBeNull]
        public static object ParseNumberBoxed(string valueString)
        {
            if (valueString.StartsWith("0x") || valueString.StartsWith("-0x"))
            {
                int mult = 1;
                if (valueString[0] == '-')
                {
                    valueString = valueString.Substring(1);
                    mult = -1;
                }

                return ParseHexLong(valueString) * mult;
            }
            if (valueString.StartsWith("0b") || valueString.StartsWith("-0b"))
            {
                int mult = 1;
                if (valueString[0] == '-')
                {
                    valueString = valueString.Substring(1);
                    mult = -1;
                }

                return ParseBinaryLong(valueString) * mult;
            }

            double outDouble;
            if (valueString.IndexOf('.') > 0 || valueString.IndexOf("e", StringComparison.Ordinal) > 0)
            {
                if (double.TryParse(valueString, NumberStyles.Number | NumberStyles.AllowExponent, Nfi, out outDouble))
                {
                    return outDouble;
                }

                return null;
            }

            if (long.TryParse(valueString, out var outLong))
            {
                return outLong;
            }

            if (double.TryParse(valueString, NumberStyles.Number, Nfi, out outDouble))
            {
                return outDouble;
            }

            return null;
        }

        /// <summary>
        /// Transforms the given binary number into a decimal number
        /// </summary>
        /// <param name="binary">The binary to convert</param>
        /// <returns>An integer representing the binary number</returns>
        public static int ParseBinary(string binary)
        {
            if (binary.StartsWith("0b"))
                binary = binary.Substring(2);

            int res = 0;
            int l = binary.Length;
            for (int i = 0; i < l; i++)
            {
                char c = binary[i];

                switch (c)
                {
                    case '0':
                        break;
                    case '1':
                        res |= 1 << (l - 1 - i);
                        break;
                    default:
                        throw new FormatException("The binary number has invalid characters");
                }
            }

            return res;
        }
        /// <summary>
        /// Transforms the given binary number into a decimal number
        /// </summary>
        /// <param name="binary">The binary to convert</param>
        /// <returns>An integer representing the binary number</returns>
        public static uint ParseBinaryUint(string binary)
        {
            if (binary.StartsWith("0b"))
                binary = binary.Substring(2);

            uint res = 0;
            int l = binary.Length;
            for (int i = 0; i < l; i++)
            {
                char c = binary[i];

                switch (c)
                {
                    case '0':
                        break;
                    case '1':
                        res |= (1U << (l - 1 - i));
                        break;
                    default:
                        throw new FormatException("The binary number has invalid characters");
                }
            }

            return res;
        }
        /// <summary>
        /// Transforms the given binary number into a decimal number
        /// </summary>
        /// <param name="binary">The binary to convert</param>
        /// <returns>An integer representing the binary number</returns>
        public static long ParseBinaryLong(string binary)
        {
            if (binary.StartsWith("0b"))
                binary = binary.Substring(2);

            long res = 0;
            int l = binary.Length;
            for (int i = 0; i < l; i++)
            {
                char c = binary[i];

                switch (c)
                {
                    case '0':
                        break;
                    case '1':
                        res |= (1L << (l - 1 - i));
                        break;
                    default:
                        throw new FormatException("The binary number has invalid characters");
                }
            }

            return res;
        }

        /// <summary>
        /// Transforms the given hexadecimal number into a decimal number
        /// </summary>
        /// <param name="hex">The hexadecimal to convert</param>
        /// <returns>An integer representing the hexadecimal number</returns>
        public static int ParseHex([NotNull] string hex)
        {
            // Remove '0x' leading
            return int.Parse(hex.StartsWith("0x") ? hex.Substring(2) : hex, NumberStyles.HexNumber);
        }
        /// <summary>
        /// Transforms the given hexadecimal number into a decimal number
        /// </summary>
        /// <param name="hex">The hexadecimal to convert</param>
        /// <returns>An integer representing the hexadecimal number</returns>
        public static uint ParseHexUint([NotNull] string hex)
        {
            // Remove '0x' leading
            return uint.Parse(hex.StartsWith("0x") ? hex.Substring(2) : hex, NumberStyles.HexNumber);
        }
        /// <summary>
        /// Transforms the given hexadecimal number into a decimal number
        /// </summary>
        /// <param name="hex">The hexadecimal to convert</param>
        /// <returns>An integer representing the hexadecimal number</returns>
        public static long ParseHexLong([NotNull] string hex)
        {
            // Remove '0x' leading
            return long.Parse(hex.StartsWith("0x") ? hex.Substring(2) : hex, NumberStyles.HexNumber);
        }
    }
}