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
using System.Globalization;
using System.Text;

namespace ZScript.Parsing
{
    /// <summary>
    /// Class containing methods used to parse constant atoms parsed from an input script
    /// </summary>
    public static class ConstantAtomParser
    {
        /// <summary>
        /// Global number format info used to convert the numeric values
        /// </summary>
        private static readonly NumberFormatInfo Nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        /// <summary>
        /// Parses a given constant atom, returning an object representing the constant
        /// </summary>
        /// <param name="context">The context containing the constant to parse</param>
        /// <returns>An object that represents the constant atom parsed</returns>
        public static object ParseCompileConstantAtom(ZScriptParser.CompileConstantContext context)
        {
            /*
             * compileConstant :  ('-')? numericAtom | T_FALSE | T_TRUE | T_NULL | stringLiteral;
            */
            if (context.numericAtom() != null)
            {
                return ParseNumericAtom(context.numericAtom(), context.GetChild(0).GetText() == "-");
            }
            if (context.T_FALSE() != null)
                return false;
            if (context.T_TRUE() != null)
                return true;
            if (context.T_NULL() != null)
                return null;
            if (context.stringLiteral() != null)
                return ParseStringAtom(context.stringLiteral());

            return null;
        }

        /// <summary>
        /// Parses a given constant atom, returning an object representing the constant
        /// </summary>
        /// <param name="context">The context containing the constant to parse</param>
        /// <returns>An object that represents the constant atom parsed</returns>
        public static object ParseConstantAtom(ZScriptParser.ConstantAtomContext context)
        {
            /*
             * constantAtom : numericAtom
             | T_FALSE | T_TRUE | T_NULL
             | stringLiteral;
            */
            if (context.numericAtom() != null)
            {
                return ParseNumericAtom(context.numericAtom(), false);
            }
            if (context.T_FALSE() != null)
                return false;
            if (context.T_TRUE() != null)
                return true;
            if (context.T_NULL() != null)
                return null;
            if (context.stringLiteral() != null)
                return ParseStringAtom(context.stringLiteral());

            return null;
        }

        /// <summary>
        /// Parses a given string atom, returning a string containing the string's content,
        /// expanding any escape character and removing the enclosing quotes on the process
        /// </summary>
        /// <param name="context">The context containing the string to parse</param>
        /// <returns>An object that represents the string atom parsed</returns>
        public static string ParseStringAtom(ZScriptParser.StringLiteralContext context)
        {
            var str = context.StringLiteral().GetText();

            // Single quotes string
            if (str[0] == '\'')
            {
                return str.Substring(1, str.Length - 2);
            }

            str = str.Substring(1, str.Length - 2);

            return EscapeString(str);
        }

        /// <summary>
        /// Escapes a given string using the language's escape rules, returning the escaped string
        /// </summary>
        /// <param name="str">The string to escape</param>
        /// <returns>An escaped version of the string</returns>
        private static string EscapeString(string str)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\\')
                {
                    if (i == str.Length - 1)
                    {
                        throw new FormatException("Expected escape sequence after '\\' character in string");
                    }

                    char next = str[i + 1];
                    i++;

                    if (next == 'n')
                    {
                        builder.Append('\n');
                        continue;
                    }
                    if (next == 'r')
                    {
                        builder.Append('\r');
                        continue;
                    }
                    if (next == '\\')
                    {
                        builder.Append('\\');
                        continue;
                    }

                    throw new FormatException("Unrecognized or invalid escape sequence in string");
                }

                builder.Append(str[i]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Parses a given numeric atom, returning an object representing the number.
        /// The returned value will be either an Int64 (long), or double precision floating point number
        /// </summary>
        /// <param name="context">The context containing the number to parse</param>
        /// <param name="negative">Whether to return a negative version of the parsed number</param>
        /// <returns>An object that represents the numeric atom parsed</returns>
        public static object ParseNumericAtom(ZScriptParser.NumericAtomContext context, bool negative)
        {
            if (context.hexadecimalNumber() != null)
            {
                long v = ParseHexadecimalNumber(context.hexadecimalNumber());
                return negative ? -v : v;
            }
            if (context.binaryNumber() != null)
            {
                long v = ParseBinaryNumber(context.binaryNumber());
                return negative ? -v : v;
            }
            if (context.FLOAT() != null)
            {
                double v = double.Parse(context.FLOAT().GetText(), Nfi);
                return negative ? -v : v;
            }
            if (context.INT() != null)
            {
                long v = long.Parse(context.INT().GetText());
                return negative ? -v : v;
            }

            return null;
        }

        /// <summary>
        /// Parses a given hexadecimal atom into an Int64 (long) value
        /// </summary>
        /// <param name="context">The context containing the hexadecimal number to parse</param>
        /// <returns>A long value representing the hexadecimal number parsed</returns>
        public static long ParseHexadecimalNumber(ZScriptParser.HexadecimalNumberContext context)
        {
            return long.Parse(context.HEX().GetText().Substring(2), NumberStyles.HexNumber);
        }

        /// <summary>
        /// Parses a given binary atom into an Int64 (long) value
        /// </summary>
        /// <param name="context">The context containing the binary number to parse</param>
        /// <returns>A long value representing the binary number parsed</returns>
        public static long ParseBinaryNumber(ZScriptParser.BinaryNumberContext context)
        {
            return ValueParser.ParseBinaryLong(context.BINARY().GetText());
        }
    }
}