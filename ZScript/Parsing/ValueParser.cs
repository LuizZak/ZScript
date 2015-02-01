using System;
using System.Globalization;

namespace ZScript.Parsing
{
    /// <summary>
    /// Static class with helper methods aimed as facilitating a unified and testable string -> value parsing
    /// </summary>
    public static class ValueParser
    {
        /// <summary>
        /// Parses the given string into a number, taking into consideration decimal, hexadecimal (0xXX) and binary (0bXX) notation
        /// </summary>
        /// <param name="num">The number to parse</param>
        /// <returns>A valid floating point number parsed from the string</returns>
        public static float ParseNumber(string num)
        {
            int mult = 1;
            if (num[0] == '-')
            {
                num = num.Substring(1);
                mult = -1;
            }

            // Test for hexadecimal numbers
            if (num.StartsWith("0x"))
                return ParseHex(num) * mult;
            // Test for binary numbers
            if (num.StartsWith("0b"))
                return ParseBinary(num) * mult;

            return float.Parse(num.Replace('.', ',')) * mult;
        }

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
                    value = float.PositiveInfinity;
                    return true;
                case "NInfinity":
                    value = float.NegativeInfinity;
                    return true;
                case "NaN":
                    value = float.NaN;
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

                uint value = ParseHexUint(valueString);
                if (value > int.MaxValue)
                {
                    return value * mult;
                }

                return (int)value * mult;
            }
            if (valueString.StartsWith("0b") || valueString.StartsWith("-0b"))
            {
                int mult = 1;
                if (valueString[0] == '-')
                {
                    valueString = valueString.Substring(1);
                    mult = -1;
                }

                uint value = ParseBinaryUint(valueString);
                if (value > int.MaxValue)
                {
                    return value * mult;
                }

                return (int)value * mult;
            }

            if (valueString.IndexOf('.') > 0)
            {
                float outFloat;
                double outDouble;
                if (float.TryParse(valueString.Replace('.', ','), out outFloat))
                {
                    return outFloat;
                }
                if (double.TryParse(valueString.Replace('.', ','), out outDouble))
                {
                    return outDouble;
                }

                return null;
            }

            long outLong;
            ulong outULong;
            if (long.TryParse(valueString, out outLong))
            {
                // Downgrade to uint32
                if (outLong >= int.MaxValue && outLong < uint.MaxValue)
                {
                    return (uint)outLong;
                }
                // Dowgrade to int32
                if (outLong < int.MaxValue && outLong > int.MinValue)
                {
                    return (int)outLong;
                }

                return outLong;
            }
            if (ulong.TryParse(valueString, out outULong))
            {
                return outULong;
            }

            return null;
        }

        /// <summary>
        /// Parses the given string into a number, taking into consideration decimal, hexadecimal (0xXX) and binary (0bXX) notation
        /// </summary>
        /// <param name="num">The number to parse</param>
        /// <returns>A valid integer number parsed from the string</returns>
        public static int ParseInt(string num)
        {
            int mult = 1;
            if (num[0] == '-')
            {
                num = num.Substring(1);
                mult = -1;
            }

            // Test for hexadecimal numbers
            if (num.StartsWith("0x"))
                return ParseHex(num) * mult;
            // Test for binary numbers
            if (num.StartsWith("0b"))
                return ParseBinary(num) * mult;

            return int.Parse(num) * mult;
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
        /// Transforms the given hexadecimal number into a decimal number
        /// </summary>
        /// <param name="hex">The hexadecimal to convert</param>
        /// <returns>An integer representing the hexadecimal number</returns>
        public static int ParseHex(string hex)
        {
            // Remove '0x' leading
            return int.Parse(hex.StartsWith("0x") ? hex.Substring(2) : hex, NumberStyles.HexNumber);
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
                        res |= (uint)(1 << (l - 1 - i));
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
        public static uint ParseHexUint(string hex)
        {
            // Remove '0x' leading
            return uint.Parse(hex.StartsWith("0x") ? hex.Substring(2) : hex, NumberStyles.HexNumber);
        }
    }
}