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

using Antlr4.Runtime;

using Xunit;

using ZScript.Parsing;

namespace ZScriptTests.Parsing
{
    /// <summary>
    /// Tests the functionality of the ConstantAtomParser class and related components
    /// </summary>
    public class ConstantAtomParserTests
    {
        [Fact]
        public void TestIntegerNumericParse()
        {
            var parser = CreateParser("2136547");
            var token = parser.numericAtom();
            var number = ConstantAtomParser.ParseNumericAtom(token, false);

            Assert.Equal(2136547L, number);
        }

        [Fact]
        public void TestFloatNumericParse()
        {
            var parser = CreateParser("12345.6789");
            var token = parser.numericAtom();
            var number = ConstantAtomParser.ParseNumericAtom(token, false);

            Assert.Equal(12345.6789, number);
        }

        [Fact]
        public void TestHexadecimalParse()
        {
            var parser = CreateParser("0x012345");
            var token = parser.hexadecimalNumber();
            var number = ConstantAtomParser.ParseHexadecimalNumber(token);

            Assert.Equal(0x12345, number);
        }

        [Fact]
        public void TestHexadecimalDetection()
        {
            var parser = CreateParser("0x012345");
            var token = parser.constantAtom();
            var number = ConstantAtomParser.ParseConstantAtom(token);

            Assert.Equal(0x12345L, number);
        }

        [Fact]
        public void TestBinaryParse()
        {
            var parser = CreateParser("0b110101");
            var token = parser.binaryNumber();
            var number = ConstantAtomParser.ParseBinaryNumber(token);

            Assert.Equal(53L, number);
        }

        [Fact]
        public void TestBinaryDetection()
        {
            var parser = CreateParser("0b110101");
            var token = parser.constantAtom();
            var number = ConstantAtomParser.ParseConstantAtom(token);

            Assert.Equal(53L, number);
        }

        [Fact]
        public void TestStringParse()
        {
            var parser = CreateParser("\"a\\\"bc\\nd\\\\\\ref\"");
            var token = parser.stringLiteral();
            var str = ConstantAtomParser.ParseStringAtom(token);

            Assert.Equal("a\"bc\nd\\\ref", str);
        }

        [Fact]
        public void TestSingleQuoteStringEscapeParse()
        {
            var parser = CreateParser("'Quo\\\\ted\\'Quoted'");
            var token = parser.stringLiteral();
            var str = ConstantAtomParser.ParseStringAtom(token);

            Assert.Equal("Quo\\ted\'Quoted", str);
        }

        [Fact]
        public void TestSingleQuotesStringParse()
        {
            var parser = CreateParser("'abcdef'");
            var token = parser.stringLiteral();
            var str = ConstantAtomParser.ParseStringAtom(token);

            Assert.Equal("abcdef", str);
        }

        [Fact]
        public void TestSingleQuoteEscapeEofException()
        {
            var parser = CreateParser("'abc\\'");
            var token = parser.stringLiteral();

            // "Trying to format a string which contains a \\ that is located at the end of the string must raise a format exception"
            Assert.Throws<FormatException>(() => ConstantAtomParser.ParseStringAtom(token));
        }

        [Fact]
        public void TestSingleQuoteInvalidEscapeException()
        {
            var parser = CreateParser("'abc\\a'");
            var token = parser.stringLiteral();

            // "Trying to format a string which contains an unrecognized that is located at the end of the string must raise a format exception"
            Assert.Throws<FormatException>(() => ConstantAtomParser.ParseStringAtom(token));
        }

        [Fact]
        public void TestDoubleQuoteEscapeEofException()
        {
            var parser = CreateParser("\"abc\\\"");
            var token = parser.stringLiteral();

            // "Trying to format a string which contains a \\ that is located at the end of the string must raise a format exception"
            Assert.Throws<FormatException>(() => ConstantAtomParser.ParseStringAtom(token));
        }

        [Fact]
        public void TestDoubleQuoteInvalidEscapeException()
        {
            var parser = CreateParser("\"abc\\a\"");
            var token = parser.stringLiteral();

            // "Trying to format a string which contains an unrecognized that is located at the end of the string must raise a format exception"
            Assert.Throws<FormatException>(() => ConstantAtomParser.ParseStringAtom(token));
        }

        [Fact]
        public void TestNegativeNumbersParse()
        {
            var parser = CreateParser("-100 -10.0");
            
            var value1 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());
            var value2 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());

            Assert.Equal((long)-100, value1);
            Assert.Equal(-10.0, value2);
        }

        [Fact]
        public void TestCompileConstants()
        {
            var parser = CreateParser("true false null 'stringyString'");

            var value1 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());
            var value2 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());
            var value3 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());
            var value4 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());

            Assert.Equal(true, value1);
            Assert.Equal(false, value2);
            Assert.Equal(null, value3);
            Assert.Equal("stringyString", value4);
        }

        /// <summary>
        /// Creates a new ZScriptParser object from a given string
        /// </summary>
        /// <param name="input">The input string to generate the ZScriptParser from</param>
        /// <returns>A ZScriptParser created from the given string</returns>
        public static ZScriptParser CreateParser(string input)
        {
            AntlrInputStream stream = new AntlrInputStream(input);
            ITokenSource lexer = new ZScriptLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            return new ZScriptParser(tokens);
        }
    }
}