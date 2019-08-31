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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Parsing;
using ZScript.Parsing.AST;

namespace ZScriptTests.Parsing
{
    [TestClass]
    public class ZScriptLexer1Tests
    {
        [TestMethod]
        public void TestTokenizeInteger()
        {
            TestLex("123", new ZScriptToken(LexerTokenType.IntegerLiteral, "123", 123));
            TestLex("-123", new ZScriptToken(LexerTokenType.IntegerLiteral, "-123", -123));
        }

        [TestMethod]
        public void TestTokenizeFloatingPoint()
        {
            TestLex("123.456", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123.456", 123.456f));
            TestLex("-123.456", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "-123.456", -123.456f));
        }

        [TestMethod]
        public void TestTokenizeFloatingPointExponentOnly()
        {
            TestLex("123e0", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123e0", 123e0f));
            TestLex("123e+1", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123e+1", 123e+1f));
            TestLex("123e-1", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123e-1", 123e-1f));
        }

        [TestMethod]
        public void TestTokenizeFloatingPointDecimalAndExponent()
        {
            TestLex("123.4e5", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123.4e5", 123.4e5f));
            TestLex("123.4e+5", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123.4e+5", 123.4e+5f));
            TestLex("123.4e-5", new ZScriptToken(LexerTokenType.FloatingPointLiteral, "123.4e-5", 123.4e-5f));
        }

        [TestMethod]
        public void TestTokenizeStringLiteral()
        {
            TestLex("\"abc\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"abc\"", "abc"));
            TestLex("\"ab\\nc\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"ab\\nc\"", "ab\nc"));
            TestLex("\"ab\\rc\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"ab\\rc\"", "ab\rc"));
            TestLex("\"ab\\tc\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"ab\\tc\"", "ab\tc"));
            TestLex("\"ab\\bc\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"ab\\bc\"", "ab\bc"));
            TestLex("\"ab\\\"c\\\"\"", new ZScriptToken(LexerTokenType.StringLiteral, "\"ab\\\"c\\\"\"", "ab\"c\""));
        }

        [TestMethod]
        public void TestTokenizeKeyword()
        {
            TestLex("class", new ZScriptToken(LexerTokenType.ClassKeyword, "class"));
            TestLex("func", new ZScriptToken(LexerTokenType.FunctionKeyword, "func"));
            TestLex("if", new ZScriptToken(LexerTokenType.If, "if"));
            TestLex("else", new ZScriptToken(LexerTokenType.Else, "else"));
            TestLex("for", new ZScriptToken(LexerTokenType.For, "for"));
            TestLex("while", new ZScriptToken(LexerTokenType.While, "while"));
            TestLex("switch", new ZScriptToken(LexerTokenType.Switch, "switch"));
            TestLex("break", new ZScriptToken(LexerTokenType.Break, "break"));
            TestLex("continue", new ZScriptToken(LexerTokenType.Continue, "continue"));
            TestLex("return", new ZScriptToken(LexerTokenType.Return, "return"));
        }

        [TestMethod]
        public void TestTokenizeIdentifier()
        {
            TestLex("myVar1", new ZScriptToken(LexerTokenType.Identifier, "myVar1"));
            TestLex("class1", new ZScriptToken(LexerTokenType.Identifier, "class1"));
            TestLex("Class", new ZScriptToken(LexerTokenType.Identifier, "Class"));
            TestLex("_", new ZScriptToken(LexerTokenType.Identifier, "_"));
            TestLex("_i_", new ZScriptToken(LexerTokenType.Identifier, "_i_"));
            
            TestLex("1malformed", new ZScriptToken(LexerTokenType.IntegerLiteral, "1", 1), 1);
        }

        [TestMethod]
        public void TestTokenizeOperators()
        {
            TestLex("(", new ZScriptToken(LexerTokenType.OpenParens, "("));
            TestLex(")", new ZScriptToken(LexerTokenType.CloseParens, ")"));
            TestLex("{", new ZScriptToken(LexerTokenType.OpenBraces, "{"));
            TestLex("}", new ZScriptToken(LexerTokenType.CloseBraces, "}"));
            TestLex("[", new ZScriptToken(LexerTokenType.OpenSquareBracket, "["));
            TestLex("]", new ZScriptToken(LexerTokenType.CloseSquareBracket, "]"));
            
            TestLex("->", new ZScriptToken(LexerTokenType.Arrow, "->"));

            TestLex(":", new ZScriptToken(LexerTokenType.Colon, ":"));
            TestLex(";", new ZScriptToken(LexerTokenType.Semicolon, ";"));

            TestLex("=", new ZScriptToken(LexerTokenType.AssignOperator, "="));

            TestLex("!", new ZScriptToken(LexerTokenType.BooleanNegateOperator, "!"));
            TestLex("&&", new ZScriptToken(LexerTokenType.AndOperator, "&&"));
            TestLex("||", new ZScriptToken(LexerTokenType.OrOperator, "||"));
            TestLex("&", new ZScriptToken(LexerTokenType.BitwiseAndOperator, "&"));
            TestLex("&=", new ZScriptToken(LexerTokenType.BitwiseAndAssignOperator, "&="));
            TestLex("|", new ZScriptToken(LexerTokenType.BitwiseOrOperator, "|"));
            TestLex("|=", new ZScriptToken(LexerTokenType.BitwiseOrAssignOperator, "|="));
            TestLex("^", new ZScriptToken(LexerTokenType.BitwiseXorOperator, "^"));
            TestLex("^=", new ZScriptToken(LexerTokenType.BitwiseXorAssignOperator, "^="));

            TestLex("+", new ZScriptToken(LexerTokenType.PlusSign, "+"));
            TestLex("+=", new ZScriptToken(LexerTokenType.PlusEqualsSign, "+="));
            TestLex("-", new ZScriptToken(LexerTokenType.MinusSign, "-"));
            TestLex("-=", new ZScriptToken(LexerTokenType.MinusEqualsSign, "-="));
            TestLex("*", new ZScriptToken(LexerTokenType.MultiplySign, "*"));
            TestLex("*=", new ZScriptToken(LexerTokenType.MultiplyEqualsSign, "*="));
            TestLex("/", new ZScriptToken(LexerTokenType.DivideSign, "/"));
            TestLex("/=", new ZScriptToken(LexerTokenType.DivideEqualsSign, "/="));
            TestLex("<", new ZScriptToken(LexerTokenType.LessThanSign, "<"));
            TestLex("<=", new ZScriptToken(LexerTokenType.LessThanOrEqualsSign, "<="));
            TestLex(">", new ZScriptToken(LexerTokenType.GreaterThanSign, ">"));
            TestLex(">=", new ZScriptToken(LexerTokenType.GreaterThanOrEqualsSign, ">="));
            TestLex("==", new ZScriptToken(LexerTokenType.EqualsSign, "=="));
            TestLex("!=", new ZScriptToken(LexerTokenType.UnequalsSign, "!="));
        }

        [TestMethod]
        public void TestTokenizeSequence()
        {
            TestLexMany(
                "+ <= = >= < == > func ident1 class",
                LexerTokenType.PlusSign, 
                LexerTokenType.LessThanOrEqualsSign, 
                LexerTokenType.AssignOperator, 
                LexerTokenType.GreaterThanOrEqualsSign,
                LexerTokenType.LessThanSign,
                LexerTokenType.EqualsSign,
                LexerTokenType.GreaterThanSign,
                LexerTokenType.FunctionKeyword,
                LexerTokenType.Identifier,
                LexerTokenType.ClassKeyword
                );
        }

        [TestMethod]
        public void TestLineAtOffset()
        {
            var sut = CreateTestLexer("abc\ndef\n\ng");

            Assert.AreEqual(1, sut.LineAtOffset(0));
            Assert.AreEqual(1, sut.LineAtOffset(1));
            Assert.AreEqual(1, sut.LineAtOffset(2));
            Assert.AreEqual(1, sut.LineAtOffset(3));
            Assert.AreEqual(2, sut.LineAtOffset(4));
            Assert.AreEqual(2, sut.LineAtOffset(5));
            Assert.AreEqual(2, sut.LineAtOffset(6));
            Assert.AreEqual(2, sut.LineAtOffset(7));
            Assert.AreEqual(3, sut.LineAtOffset(8));
            Assert.AreEqual(4, sut.LineAtOffset(9));
        }

        [TestMethod]
        public void TestColumnAtOffset()
        {
            var sut = CreateTestLexer("abc\ndef\n\ng");

            Assert.AreEqual(1, sut.ColumnAtOffset(0));
            Assert.AreEqual(2, sut.ColumnAtOffset(1));
            Assert.AreEqual(3, sut.ColumnAtOffset(2));
            Assert.AreEqual(4, sut.ColumnAtOffset(3));
            Assert.AreEqual(1, sut.ColumnAtOffset(4));
            Assert.AreEqual(2, sut.ColumnAtOffset(5));
            Assert.AreEqual(3, sut.ColumnAtOffset(6));
            Assert.AreEqual(4, sut.ColumnAtOffset(7));
            Assert.AreEqual(1, sut.ColumnAtOffset(8));
            Assert.AreEqual(1, sut.ColumnAtOffset(9));
        }

        private static void TestLex(string input, ZScriptToken expected, int expectedLength = -1)
        {
            var testSource = new TestSource();
            var loc = new SourceLocation(testSource, 0, expectedLength == -1 ? input.Length : expectedLength, 1, 1);
            var expWithLoc = new ZScriptToken(expected.TokenType, loc, expected.TokenString, expected.Value);
            var lexer = new ZScriptLexer1(testSource, input);
            Assert.AreEqual(expWithLoc, lexer.Next());
        }

        private static void TestLexMany(string input, params LexerTokenType[] expected)
        {
            var lexer = CreateTestLexer(input);

            var toks = lexer.ReadAllTokens();
            
            for (int i = 0; i < Math.Min(toks.Count, expected.Length); i++)
            {
                var tok = toks[i];

                Assert.AreEqual(expected[i], tok.TokenType, $"Expected token {i + 1} to tokenize as {expected[i]}, but received {tok.TokenType}");
            }

            Assert.AreEqual(expected.Length, toks.Count, $"Expected {expected.Length} tokens, but tokenized {toks.Count}");
        }

        private static ZScriptLexer1 CreateTestLexer(string input)
        {
            var testSource = new TestSource();
            return new ZScriptLexer1(testSource, input);
        }

        private class TestSource : ISource
        {
            public bool Equals(ISource other)
            {
                return other is TestSource;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((ISource) obj);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
            }

            public static bool operator ==(TestSource left, TestSource right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TestSource left, TestSource right)
            {
                return !Equals(left, right);
            }
        }
    }
}