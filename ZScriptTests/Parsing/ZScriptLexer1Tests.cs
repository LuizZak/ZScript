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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Parsing;

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
            TestLex("class", new ZScriptToken(LexerTokenType.Class, "class"));
            TestLex("func", new ZScriptToken(LexerTokenType.Function, "func"));
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
            
            TestLex("1malformed", new ZScriptToken(LexerTokenType.IntegerLiteral, "1", 1));
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
            TestLex("^", new ZScriptToken(LexerTokenType.XorOperator, "^"));
            TestLex("^=", new ZScriptToken(LexerTokenType.XorAssignOperator, "^="));

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

        private static void TestLex(string input, ZScriptToken expected)
        {
            var lexer = new ZScriptLexer1(input);
            Assert.AreEqual(expected, lexer.Next());
        }
    }
}