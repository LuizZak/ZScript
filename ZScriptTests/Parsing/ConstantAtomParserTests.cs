﻿/*
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
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Parsing;

namespace ZScriptTests.Parsing
{
    /// <summary>
    /// Tests the functionality of the ConstantAtomParser class and related components
    /// </summary>
    [TestClass]
    public class ConstantAtomParserTests
    {
        [TestMethod]
        public void TestIntegerNumericParse()
        {
            var parser = CreateParser("2136547");
            var token = parser.numericAtom();
            var number = ConstantAtomParser.ParseNumericAtom(token, false);

            Assert.AreEqual((long)2136547, number, "The integer number was not parsed correctly");
        }

        [TestMethod]
        public void TestFloatNumericParse()
        {
            var parser = CreateParser("12345.6789");
            var token = parser.numericAtom();
            var number = ConstantAtomParser.ParseNumericAtom(token, false);

            Assert.AreEqual(12345.6789, number, "The double number was not parsed correctly");
        }

        [TestMethod]
        public void TestHexadecimalParse()
        {
            var parser = CreateParser("0x012345");
            var token = parser.hexadecimalNumber();
            var number = ConstantAtomParser.ParseHexadecimalNumber(token);

            Assert.AreEqual(0x12345, number, "The hexadecimal number was not parsed correctly");
        }

        [TestMethod]
        public void TestBinaryParse()
        {
            var parser = CreateParser("0b110101");
            var token = parser.binaryNumber();
            var number = ConstantAtomParser.ParseBinaryNumber(token);

            Assert.AreEqual(53, number, "The hexadecimal number was not parsed correctly");
        }

        [TestMethod]
        public void TestStringParse()
        {
            var parser = CreateParser("\"abc\\nd\\\\\\ref\"");
            var token = parser.stringLiteral();
            var str = ConstantAtomParser.ParseStringAtom(token);

            Assert.AreEqual("abc\nd\\\ref", str, "The string was not parsed correctly");
        }

        [TestMethod]
        public void TestSingleQuotesStringParse()
        {
            var parser = CreateParser("'abc\\ndef'");
            var token = parser.stringLiteral();
            var str = ConstantAtomParser.ParseStringAtom(token);

            Assert.AreEqual("abc\\ndef", str, "The string was not parsed correctly");
        }

        [TestMethod]
        public void TestNegativeNumbersParse()
        {
            var parser = CreateParser("-100 -10.0");
            
            var value1 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());
            var value2 = ConstantAtomParser.ParseCompileConstantAtom(parser.compileConstant());

            Assert.AreEqual((long)-100, value1, "The numeric constant was not parsed correctly");
            Assert.AreEqual(-10.0, value2, "The string was not parsed correctly");
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