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

using Xunit;

using ZScript.Parsing;

namespace ZScriptTests.Parsing
{
    /// <summary>
    /// Tests the functionality of the ValueParser class and related components
    /// </summary>
    public class ValueParserTests
    {
        /// <summary>
        /// Tests the result of the TryParseValueBoxed method
        /// </summary>
        [Fact]
        public void TestTryParseValueBoxed()
        {
            object value;

            // null
            Assert.True(ValueParser.TryParseValueBoxed("null", out value));
            Assert.Equal(null, value);

            // false
            Assert.True(ValueParser.TryParseValueBoxed("false", out value));
            Assert.Equal(false, value);

            // true
            Assert.True(ValueParser.TryParseValueBoxed("true", out value));
            Assert.Equal(true, value);

            // Infinity
            Assert.True(ValueParser.TryParseValueBoxed("Infinity", out value));
            Assert.True(double.IsPositiveInfinity((double)value));

            // NegativeInfinity
            Assert.True(ValueParser.TryParseValueBoxed("NInfinity", out value));
            Assert.True(double.IsNegativeInfinity((double)value));

            // NaN
            Assert.True(ValueParser.TryParseValueBoxed("NaN", out value));
            Assert.True(double.IsNaN((double)value));
        }

        /// <summary>
        /// Tests the results of the ParseBinary method
        /// </summary>
        [Fact]
        public void TestParseBinary()
        {
            Assert.Equal(0, ValueParser.ParseBinary("0b00"));
            Assert.Equal(1, ValueParser.ParseBinary("0b01"));
            Assert.Equal(256, ValueParser.ParseBinary("0b100000000"));
            Assert.Equal(21515329, ValueParser.ParseBinary("0b1010010000100110001000001"));
            Assert.Equal(-2147483648, ValueParser.ParseBinary("0b10000000000000000000000000000000"));

            Assert.Equal(1L, ValueParser.ParseBinaryLong("0b1"));
            Assert.Equal(4611686018427387903L, ValueParser.ParseBinaryLong("0b0011111111111111111111111111111111111111111111111111111111111111"));
            Assert.Equal(9223372036854775807L, ValueParser.ParseBinaryLong("0b0111111111111111111111111111111111111111111111111111111111111111"));
            Assert.Equal(-1L, ValueParser.ParseBinaryLong("0b1111111111111111111111111111111111111111111111111111111111111111"));

            Assert.Throws<FormatException>(() => ValueParser.ParseBinary("BAD"));
        }

        /// <summary>
        /// Tests the results of the ParseBinary method
        /// </summary>
        [Fact]
        public void TestFailedParseBinary()
        {
            Assert.Throws<FormatException>(() => ValueParser.ParseBinary("BAD"));
        }

        /// <summary>
        /// Tests the results of the ParseBinaryUint method
        /// </summary>
        [Fact]
        public void TestParseBinaryUint()
        {
            Assert.Equal((uint)0, ValueParser.ParseBinaryUint("0b00"));
            Assert.Equal((uint)1, ValueParser.ParseBinaryUint("0b01"));
            Assert.Equal((uint)256, ValueParser.ParseBinaryUint("0b100000000"));
            Assert.Equal((uint)21515329, ValueParser.ParseBinaryUint("0b1010010000100110001000001"));
            Assert.Equal(2147483648, ValueParser.ParseBinaryUint("0b10000000000000000000000000000000"));

            Assert.Throws<FormatException>(() => ValueParser.ParseBinaryUint("BAD"));
        }

        /// <summary>
        /// Tests the results of the ParseBinaryUint method
        /// </summary>
        [Fact]
        public void TestFailedParseBinaryUint()
        {
            Assert.Throws<FormatException>(() => ValueParser.ParseBinaryUint("0bBAD"));
        }

        /// <summary>
        /// Tests the results of the ParseBinaryLong method
        /// </summary>
        [Fact]
        public void TestFailedParseBinaryLong()
        {
            Assert.Throws<FormatException>(() => ValueParser.ParseBinaryLong("0bBAD"));
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [Fact]
        public void TestParseHexadecimal()
        {
            Assert.Equal(0, ValueParser.ParseHex("0x00"));
            Assert.Equal(1, ValueParser.ParseHex("0x01"));
            Assert.Equal(256, ValueParser.ParseHex("0x100"));
            Assert.Equal(195948557, ValueParser.ParseHex("0xBADF00D"));
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [Fact]
        public void TestFailedParseHexadecimal()
        {
            Assert.Throws<FormatException>(() => ValueParser.ParseHex("0xBADHEX"));
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [Fact]
        public void TestParseHexadecimalUint()
        {
            Assert.Equal((uint)0, ValueParser.ParseHexUint("0x00"));
            Assert.Equal((uint)1, ValueParser.ParseHexUint("0x01"));
            Assert.Equal((uint)256, ValueParser.ParseHexUint("0x100"));
            Assert.Equal((uint)195948557, ValueParser.ParseHexUint("0xBADF00D"));
            Assert.Equal(4059231220, ValueParser.ParseHexUint("0xF1F2F3F4"));
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [Fact]
        public void TestFailedParseHexadecimalUint()
        {
            Assert.Throws<FormatException>(() => ValueParser.ParseHexUint("0xBADHEX"));
        }

        /// <summary>
        /// Tests boxing of numbers with the ParseNumberBoxed method
        /// </summary>
        [Fact]
        public void TestParseNumberBoxed()
        {
            Assert.Equal(1L, ValueParser.ParseNumberBoxed("1"));
            Assert.Equal(1L, ValueParser.ParseNumberBoxed("0b1"));
            Assert.Equal(1L, ValueParser.ParseNumberBoxed("0x1"));
            Assert.Equal(1.0, ValueParser.ParseNumberBoxed("1.0"));

            Assert.Equal(-1L, ValueParser.ParseNumberBoxed("-1"));
            Assert.Equal(-1L, ValueParser.ParseNumberBoxed("-0b1"));
            Assert.Equal(-1L, ValueParser.ParseNumberBoxed("-0x1"));
            Assert.Equal(-1.0, ValueParser.ParseNumberBoxed("-1.0"));

            // Test value Wrapping

            // Uint wrapping
            Assert.Equal(4059231220L, ValueParser.ParseNumberBoxed("4059231220"));
            // Long wrapping
            Assert.Equal(40592312200, ValueParser.ParseNumberBoxed("40592312200"));
            // Negative Long wrapping
            Assert.Equal(-40592312200, ValueParser.ParseNumberBoxed("-40592312200"));
            // ULong wrapping
            Assert.Equal((double)11922972036854775807, ValueParser.ParseNumberBoxed("11922972036854775807"));
            // Double wrapping
            Assert.Equal(1192297203685477580711922972036854775807.0, ValueParser.ParseNumberBoxed("1192297203685477580711922972036854775807.0"));
            // Negative double wrapping
            Assert.Equal(-1192297203685477580711922972036854775807.0, ValueParser.ParseNumberBoxed("-1192297203685477580711922972036854775807.0"));


            // Invalid values testing
            Assert.Equal(null, ValueParser.ParseNumberBoxed("-1..0"));
            Assert.Equal(null, ValueParser.ParseNumberBoxed("1a0"));
        }
    }
}