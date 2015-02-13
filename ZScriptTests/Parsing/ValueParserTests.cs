using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Parsing;

namespace ZScriptTests.Parsing
{
    /// <summary>
    /// Tests the functionality of the Tokenizer class and related components
    /// </summary>
    [TestClass]
    public class TokenizerTests
    {
        /// <summary>
        /// Tests the results of the ParseBinary method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestParseBinary()
        {
            Assert.AreEqual(0, ValueParser.ParseBinary("0b00"), "Expected 0b00 = 0");
            Assert.AreEqual(1, ValueParser.ParseBinary("0b01"), "Expected 0b01 = 1");
            Assert.AreEqual(256, ValueParser.ParseBinary("0b100000000"), "Expected 0b100000000 = 256");
            Assert.AreEqual(21515329, ValueParser.ParseBinary("0b1010010000100110001000001"), "Expected 0b1010010000100110001000001 = 21515329");
            Assert.AreEqual(-2147483648, ValueParser.ParseBinary("0b10000000000000000000000000000000"), "Expected 0b10000000000000000000000000000000 = -2147483648");

            ValueParser.ParseBinary("BAD");
        }

        /// <summary>
        /// Tests the results of the ParseBinary method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestFailedParseBinary()
        {
            ValueParser.ParseBinary("0bBAD");
        }

        /// <summary>
        /// Tests the results of the ParseBinaryUint method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestParseBinaryUint()
        {
            Assert.AreEqual((uint)0, ValueParser.ParseBinaryUint("0b00"), "Expected 0b00 = 0");
            Assert.AreEqual((uint)1, ValueParser.ParseBinaryUint("0b01"), "Expected 0b01 = 1");
            Assert.AreEqual((uint)256, ValueParser.ParseBinaryUint("0b100000000"), "Expected 0b100000000 = 256");
            Assert.AreEqual((uint)21515329, ValueParser.ParseBinaryUint("0b1010010000100110001000001"), "Expected 0b1010010000100110001000001 = 21515329");
            Assert.AreEqual(2147483648, ValueParser.ParseBinaryUint("0b10000000000000000000000000000000"), "Expected 0b10000000000000000000000000000000 = 2147483648");

            ValueParser.ParseBinaryUint("BAD");
        }

        /// <summary>
        /// Tests the results of the ParseBinaryUint method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestFailedParseBinaryUint()
        {
            ValueParser.ParseBinaryUint("0bBAD");
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestParseHexadecimal()
        {
            Assert.AreEqual(0, ValueParser.ParseHex("0x00"), "Expected 0x00 = 0");
            Assert.AreEqual(1, ValueParser.ParseHex("0x01"), "Expected 0x01 = 1");
            Assert.AreEqual(256, ValueParser.ParseHex("0x100"), "Expected 0x100 = 256");
            Assert.AreEqual(195948557, ValueParser.ParseHex("0xBADF00D"), "Expected 0xBADF00D = 195948557");

            ValueParser.ParseHex("XBAD");
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad hexadecimal numbers should raise a format exception")]
        public void TestFailedParseHexadecimal()
        {
            ValueParser.ParseHex("0xBADHEX");
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad binary numbers should raise a format exception")]
        public void TestParseHexadecimalUint()
        {
            Assert.AreEqual((uint)0, ValueParser.ParseHexUint("0x00"), "Expected 0x00 = 0");
            Assert.AreEqual((uint)1, ValueParser.ParseHexUint("0x01"), "Expected 0x01 = 1");
            Assert.AreEqual((uint)256, ValueParser.ParseHexUint("0x100"), "Expected 0x100 = 256");
            Assert.AreEqual((uint)195948557, ValueParser.ParseHexUint("0xBADF00D"), "Expected 0xBADF00D = 195948557");
            Assert.AreEqual(4059231220, ValueParser.ParseHexUint("0xF1F2F3F4"), "Expected 0xF1F2F3F4 = 4059231220");

            ValueParser.ParseHex("XBAD");
        }

        /// <summary>
        /// Tests the results of the ParseHex method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException), "Bad hexadecimal numbers should raise a format exception")]
        public void TestFailedParseHexadecimalUint()
        {
            ValueParser.ParseHex("0xBADHEX");
        }

        /// <summary>
        /// Tests boxing of numbers with the ParseNumberBoxed method
        /// </summary>
        [TestMethod]
        public void TestParseNumberBoxed()
        {
            Assert.AreEqual((long)1, ValueParser.ParseNumberBoxed("1"), "Wrong unboxed number");
            Assert.AreEqual((long)1, ValueParser.ParseNumberBoxed("0b1"), "Wrong unboxed number");
            Assert.AreEqual((long)1, ValueParser.ParseNumberBoxed("0x1"), "Wrong unboxed number");
            Assert.AreEqual(1.0, ValueParser.ParseNumberBoxed("1.0"), "Wrong unboxed number");

            Assert.AreEqual((long)-1, ValueParser.ParseNumberBoxed("-1"), "Wrong unboxed number");
            Assert.AreEqual((long)-1, ValueParser.ParseNumberBoxed("-0b1"), "Wrong unboxed number");
            Assert.AreEqual((long)-1, ValueParser.ParseNumberBoxed("-0x1"), "Wrong unboxed number");
            Assert.AreEqual(-1.0, ValueParser.ParseNumberBoxed("-1.0"), "Wrong unboxed number");

            // Test value Wrapping

            // Uint wrapping
            Assert.AreEqual((long)4059231220, ValueParser.ParseNumberBoxed("4059231220"), "Wrong unboxed number");
            // Long wrapping
            Assert.AreEqual(40592312200, ValueParser.ParseNumberBoxed("40592312200"), "Wrong unboxed number");
            // Negative Long wrapping
            Assert.AreEqual(-40592312200, ValueParser.ParseNumberBoxed("-40592312200"), "Wrong unboxed number");
            // ULong wrapping
            Assert.AreEqual((double)11922972036854775807, ValueParser.ParseNumberBoxed("11922972036854775807"), "Wrong unboxed number");
            // Double wrapping
            Assert.AreEqual(1192297203685477580711922972036854775807.0, ValueParser.ParseNumberBoxed("1192297203685477580711922972036854775807.0"), "Wrong unboxed number");
            // Negative double wrapping
            Assert.AreEqual(-1192297203685477580711922972036854775807.0, ValueParser.ParseNumberBoxed("-1192297203685477580711922972036854775807.0"), "Wrong unboxed number");
        }
    }
}
