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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace ZScript.Parsing
{
    /// <summary>
    /// A lexer for a ZScript source file
    /// </summary>
    public sealed class ZScriptLexer1
    {
        private delegate bool CharacterTest(char c);

        /// <summary>
        /// The source string this lexer is currently using
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Current character offset that the lexer is at on the source string.
        /// </summary>
        public int Offset { get; private set; }
        
        /// <summary>
        /// Gets the current token under the current string position.
        /// </summary>
        public ZScriptToken Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZScriptLexer1"/> class with a given string source to parse with.
        /// </summary>
        public ZScriptLexer1(string source)
        {
            Source = source;

            // Start by reading the current token.
            LexNextToken();
        }
        
        /// <summary>
        /// Reads the token and advances the token value by one.
        /// </summary>
        public ZScriptToken Next()
        {
            var token = Current;

            _advanceWhile(IsWhitespace);

            LexNextToken();

            return token;
        }

        /// <summary>
        /// Returns a list containing all remaining tokens up to the end of the string.
        /// </summary>
        public List<ZScriptToken> AllTokens()
        {
            var tokens = new List<ZScriptToken>();

            while (!_isEof() && Current.TokenType != LexerTokenType.Eof)
            {
                tokens.Add(Next());
            }

            return tokens;
        }

        private void LexNextToken()
        {
            if (_isEof())
            {
                Current = new ZScriptToken(LexerTokenType.Eof, "", null);
                return;
            }

            if (_isLetter() || _isNextCharacter('_'))
            {
                LexAlphanumeric();
            }
            else if (_isNextCharacter('-') && _isDigit(1) || _isDigit())
            {
                LexNumber();
            }
            else if (_isNextCharacter('"'))
            {
                LexString();
            }
            else
            {
                LexOperator();
            }
        }

        private void LexNumber()
        {
            var stringRange = StartStringRange();
            var tokenType = LexerTokenType.IntegerLiteral;

            // Try an integer first
            if(_isNextCharacter('-'))
                _advance();

            while (_isDigit())
            {
                _advance();
            }

            // Check for floating-point (either '.', or 'e' followed by a valid exponent suffix)
            var backtrack = CreateBacktrack();
            
            // Decimal places
            if (_isNextCharacter('.'))
            {
                _advance();

                if (_isDigit())
                {
                    tokenType = LexerTokenType.FloatingPointLiteral;

                    while (_isDigit())
                    {
                        _advance();
                    }

                    // Backtrack from end of floating-point, now.
                    backtrack = CreateBacktrack();
                }
                else
                {
                    backtrack.Backtrack();
                }
            }

            // Exponent
            if (_isNextCharacter('e') || _isNextCharacter('E'))
            {
                _advance();

                if (_isNextCharacter('+') || _isNextCharacter('-'))
                    _advance();

                if (_isDigit())
                {
                    tokenType = LexerTokenType.FloatingPointLiteral;

                    while (_isDigit())
                    {
                        _advance();
                    }
                }
                else
                {
                    backtrack.Backtrack();
                }
            }

            string source = stringRange.GetString();
            object value =
                tokenType == LexerTokenType.IntegerLiteral
                    ? (object)int.Parse(source, NumberStyles.Integer)
                    : float.Parse(source, NumberStyles.Float, new CultureInfo("en-US"));

            Current = new ZScriptToken(tokenType, source, value);
        }

        private void LexString()
        {
            var stringRange = StartStringRange();
            var value = new StringBuilder();

            // Opening quotes
            _advance();

            while (!_isEof())
            {
                if (_isNextCharacter('"'))
                {
                    break;
                }

                // Escape sequence
                if (_isNextCharacter('\\'))
                {
                    char p = _peekChar(1);
                    switch (p)
                    {
                        case 'n':
                            value.Append('\n');
                            _advance(2);
                            break;
                        case 'r':
                            value.Append('\r');
                            _advance(2);
                            break;
                        case 'b':
                            value.Append('\b');
                            _advance(2);
                            break;
                        case 't':
                            value.Append('\t');
                            _advance(2);
                            break;
                        case '"':
                            value.Append('"');
                            _advance(2);
                            break;
                        default: // TODO: Should throw/diagnose error here.
                            _advance(2);
                            break;
                    }
                }
                else
                {
                    value.Append(_nextChar());
                }
            }

            // Closing quotes
            _advance();

            string str = stringRange.GetString();
            Current = new ZScriptToken(LexerTokenType.StringLiteral, str, value.ToString());
        }

        private void LexAlphanumeric()
        {
            var range = StartStringRange();

            while (_isLetter() || _isDigit() || _isNextCharacter('_'))
            {
                _advance();
            }

            string result = range.GetString();

            var keywordMapping = new Dictionary<string, LexerTokenType>
            {
                {"func", LexerTokenType.Function},
                {"class", LexerTokenType.Class},
                {"if", LexerTokenType.If},
                {"else", LexerTokenType.Else},
                {"for", LexerTokenType.For},
                {"while", LexerTokenType.While},
                {"switch", LexerTokenType.Switch},
                {"break", LexerTokenType.Break},
                {"continue", LexerTokenType.Continue},
                {"return", LexerTokenType.Return},
            };

            if(keywordMapping.TryGetValue(result, out var keyword))
            {
                Current = new ZScriptToken(keyword, result);
            }
            else
            {
                Current = new ZScriptToken(LexerTokenType.Identifier, result);
            }
        }

        private void LexOperator()
        {
            var operatorMapping = new Dictionary<char, LexerTokenType>
            {
                {'(', LexerTokenType.OpenParens},
                {')', LexerTokenType.CloseParens},
                {'[', LexerTokenType.OpenSquareBracket},
                {']', LexerTokenType.CloseSquareBracket},
                {'{', LexerTokenType.OpenBraces},
                {'}', LexerTokenType.CloseBraces},
                {':', LexerTokenType.Colon},
                {';', LexerTokenType.Semicolon}
            };

            var marker = StartStringRange();

            char c = _peekChar();

            if (operatorMapping.TryGetValue(c, out var oper))
            {
                _advance();
                Current = new ZScriptToken(oper, marker.GetString());
                return;
            }

            LexerTokenType op;

            if (c == '+')
            {
                _advance();

                op = LexerTokenType.PlusSign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.PlusEqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '-')
            {
                _advance();

                op = LexerTokenType.MinusSign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.MinusEqualsSign;
                }
                else if (_peekChar() == '>')
                {
                    _advance();
                    op = LexerTokenType.Arrow;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '*')
            {
                _advance();

                op = LexerTokenType.MultiplySign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.MultiplyEqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '/')
            {
                _advance();

                op = LexerTokenType.DivideSign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.DivideEqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '<')
            {
                _advance();

                op = LexerTokenType.LessThanSign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.LessThanOrEqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '>')
            {
                _advance();

                op = LexerTokenType.GreaterThanSign;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.GreaterThanOrEqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '=')
            {
                _advance();

                op = LexerTokenType.AssignOperator;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.EqualsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '!')
            {
                _advance();

                op = LexerTokenType.BooleanNegateOperator;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.UnequalsSign;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '&')
            {
                _advance();

                op = LexerTokenType.BitwiseAndOperator;

                if (_peekChar() == '&')
                {
                    _advance();
                    op = LexerTokenType.AndOperator;
                }
                else if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.BitwiseAndAssignOperator;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '|')
            {
                _advance();

                op = LexerTokenType.BitwiseOrOperator;

                if (_peekChar() == '|')
                {
                    _advance();
                    op = LexerTokenType.OrOperator;
                }
                else if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.BitwiseOrAssignOperator;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
            else if(c == '^')
            {
                _advance();

                op = LexerTokenType.XorOperator;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.XorAssignOperator;
                }
                
                Current = new ZScriptToken(op, marker.GetString());
            }
        }

        private BacktrackPoint CreateBacktrack()
        {
            return new BacktrackPoint(this);
        }

        private StringRangeMaker StartStringRange()
        {
            return new StringRangeMaker(this);
        }

        #region String scanning

        /// <summary>
        /// Returns true iff (<see cref="Offset"/> + lookahead) &gt;= <see cref="Source"/>.Length
        /// </summary>
        private bool _isEof(int lookahead = 0)
        {
            return Offset + lookahead >= Source.Length;
        }

        /// <summary>
        /// Returns the current character on the source string and forwards the offset
        /// by one.
        /// </summary>
        private char _nextChar()
        {
            if (_isEof())
                return '\0';
            
            return Source[Offset++];
        }

        /// <summary>
        /// Advances the source string by 'forward' characters.
        /// </summary>
        /// <remarks>'forward' must be &gt;= 1.</remarks>
        private void _advance(int forward = 1)
        {
            Offset += forward;
        }

        /// <summary>
        /// Peeks the current character without advancing the reading offset.
        /// </summary>
        private char _peekChar(int lookahead = 0)
        {
            if (_isEof(lookahead))
                return '\0';

            return Source[Offset + lookahead];
        }

        /// <summary>
        /// Advances the offset on the string while a given character testing method returns true.
        /// </summary>
        private void _advanceWhile([NotNull] CharacterTest test)
        {
            while (test(_peekChar()))
            {
                _advance();
            }
        }
        
        #endregion

        #region Character class checking
        
        private bool _isDigit(int lookahead = 0)
        {
            return IsDigit(_peekChar(lookahead));
        }

        private bool _isLetter(int lookahead = 0)
        {
            return IsLetter(_peekChar(lookahead));
        }

        private bool _isAlphanumeric(int lookahead = 0)
        {
            return IsAlphanumeric(_peekChar(lookahead));
        }

        private bool _isWhitespace(int lookahead = 0)
        {
            return IsWhitespace(_peekChar(lookahead));
        }

        private bool _isNextCharacter(char c, int lookahead = 0)
        {
            return _peekChar(lookahead) == c;
        }

        private static bool IsDigit(char c)
        {
            return char.IsDigit(c);
        }

        private static bool IsLetter(char c)
        {
            return char.IsLetter(c);
        }

        private static bool IsAlphanumeric(char c)
        {
            return char.IsLetterOrDigit(c);
        }

        private static bool IsWhitespace(char c)
        {
            return char.IsWhiteSpace(c);
        }
        
        #endregion

        private class BacktrackPoint
        {
            private readonly ZScriptLexer1 _lexer;
            private readonly int _offset;
            private bool _enabled = true;

            public BacktrackPoint([NotNull] ZScriptLexer1 lexer)
            {
                _lexer = lexer;
                _offset = lexer.Offset;
            }

            public void Backtrack()
            {
                if (!_enabled)
                    return;

                _lexer.Offset = _offset;
                _enabled = false;
            }
        }

        private class StringRangeMaker
        {
            private readonly ZScriptLexer1 _lexer;
            private readonly int _offset;

            public StringRangeMaker([NotNull] ZScriptLexer1 lexer)
            {
                _lexer = lexer;
                _offset = lexer.Offset;
            }

            public string GetString()
            {
                return _lexer.Source.Substring(_offset, _lexer.Offset - _offset);
            }
        }
    }

    /// <summary>
    /// A token read by a <see cref="ZScriptLexer1"/>
    /// </summary>
    [DebuggerDisplay("TokenType: {TokenType}, TokenString: {TokenString}, Value: {Value}")]
    public readonly struct ZScriptToken : IEquatable<ZScriptToken>
    {
        /// <summary>
        /// Type for this token.
        /// </summary>
        public LexerTokenType TokenType { get; }

        /// <summary>
        /// Input string for this token. Is an empty string, in case <see cref="LexerTokenType"/> is <see cref="LexerTokenType.Eof"/>
        /// </summary>
        public string TokenString { get; }

        /// <summary>
        /// An object value that represents this token's semantic value.
        /// 
        /// For example, for <see cref="LexerTokenType.IntegerLiteral"/> this is an <see cref="int"/> value, for <see cref="LexerTokenType.FloatingPointLiteral"/>
        /// this is a <see cref="float"/>, etc.
        /// </summary>
        [CanBeNull]
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="ZScriptToken"/>
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, string tokenString) : this()
        {
            TokenType = tokenType;
            TokenString = tokenString;
            Value = null;
        }
        
        /// <summary>
        /// Initializes a new instance of a <see cref="ZScriptToken"/>
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, string tokenString, [CanBeNull] object value) : this()
        {
            TokenType = tokenType;
            TokenString = tokenString;
            Value = value;
        }
        
#pragma warning disable 1591

        public bool Equals(ZScriptToken other)
        {
            return TokenType == other.TokenType && string.Equals(TokenString, other.TokenString) && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            else
            return obj is ZScriptToken token && Equals(token);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) TokenType;
                hashCode = (hashCode * 397) ^ (TokenString != null ? TokenString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ZScriptToken left, ZScriptToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ZScriptToken left, ZScriptToken right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{{TokenType: {TokenType}, TokenString: {TokenString}, Value: {Value}}}";
        }

#pragma warning restore 1591
    }
    
    /// <summary>
    /// Specifies the type of a token read by the lexer
    /// </summary>
    public enum LexerTokenType
    {
        /// <summary>
        /// An end-of-file token. Represents the end of the input stream when no more tokens can be read.
        /// </summary>
        Eof = 0,

        /// <summary>
        /// Identifier token.
        /// 
        /// <code>
        /// Identifier:
        ///     [a-zA-Z_][a-zA-Z_0-9]+
        /// </code>
        /// </summary>
        Identifier,

        /// <summary>
        /// Integer literal value.
        /// 
        /// <code>
        /// Integer:
        ///     '-'? [0-9]+
        /// </code>
        /// </summary>
        IntegerLiteral,

        /// <summary>
        /// Floating-point literal value.
        /// 
        /// <code>
        /// FloatingPoint:
        ///     '-'? [0-9]+ '.' [0-9]+ Exponent?
        ///     '-'? [0-9]+ Exponent
        /// 
        /// Exponent:
        ///     ('e' | 'E') ('+' | '-')? [0-9]+
        /// </code>
        /// </summary>
        FloatingPointLiteral,

        /// <summary>
        /// Double-quoted string literal.
        /// 
        /// <code>
        /// StringLiteral:
        ///     '"' (EscapeLiteral | .)* '"'
        /// EscapeLiteral:
        ///    '\' ('n' | 'r' | 't' | 'b' | '"' | ''')
        /// </code>
        /// </summary>
        StringLiteral,

        /// <summary>
        /// 'func' keyword
        /// </summary>
        Function,

        /// <summary>
        /// 'class' keyword
        /// </summary>
        Class,

        /// <summary>
        /// An 'if' statement keyword
        /// </summary>
        If,

        /// <summary>
        /// An 'else' statement keyword
        /// </summary>
        Else,

        /// <summary>
        /// 'for' statement keyword
        /// </summary>
        For,

        /// <summary>
        /// 'while' statement keyword
        /// </summary>
        While,

        /// <summary>
        /// 'switch' statement keyword
        /// </summary>
        Switch,

        /// <summary>
        /// 'continue' statement keyword
        /// </summary>
        Continue,

        /// <summary>
        /// 'break' statement keyword
        /// </summary>
        Break,

        /// <summary>
        /// 'return' statement keyword
        /// </summary>
        Return,
        
        /// <summary>'('</summary>
        OpenParens,
        /// <summary>')'</summary>
        CloseParens,
        /// <summary>'{'</summary>
        OpenBraces,
        /// <summary>'}'</summary>
        CloseBraces,
        /// <summary>'['</summary>
        OpenSquareBracket,
        /// <summary>']'</summary>
        CloseSquareBracket,
        
        /// <summary>'->', return value separator</summary>
        Arrow,
        /// <summary>':'</summary>
        Colon,
        /// <summary>';'</summary>
        Semicolon,

        /// <summary>'=' (assignment operator)</summary>
        AssignOperator,
        
        /// <summary>'!' (boolean negation operator)</summary>
        BooleanNegateOperator,

        /// <summary>'||' (boolean OR operator)</summary>
        OrOperator,
        /// <summary>'&amp;&amp;' (boolean AND operator)</summary>
        AndOperator,
        /// <summary>'|' (bitwise OR operator)</summary>
        BitwiseOrOperator,
        /// <summary>'|=' (bitwise OR assignment operator)</summary>
        BitwiseOrAssignOperator,
        /// <summary>'&amp;' (bitwise AND operator)</summary>
        BitwiseAndOperator,
        /// <summary>'&amp;=' (bitwise AND assignment operator)</summary>
        BitwiseAndAssignOperator,
        /// <summary>'^' (bitwise XOR operator)</summary>
        XorOperator,
        /// <summary>'^=' (bitwise XOR assignment operator)</summary>
        XorAssignOperator,
        
        /// <summary>'+' (add operator)</summary>
        PlusSign,
        /// <summary>'+=' (in-place add operator)</summary>
        PlusEqualsSign,
        /// <summary>'-' (subtract operator)</summary>
        MinusSign,
        /// <summary>'-=' (in-place subtract operator)</summary>
        MinusEqualsSign,
        /// <summary>'*' (multiplication operator)</summary>
        MultiplySign,
        /// <summary>'*=' (in-place multiplication operator)</summary>
        MultiplyEqualsSign,
        /// <summary>'/' (division operator)</summary>
        DivideSign,
        /// <summary>'/=' (in-place division operator)</summary>
        DivideEqualsSign,
        /// <summary>'==' (equality operator)</summary>
        EqualsSign,
        /// <summary>'!=' (inequality operator)</summary>
        UnequalsSign,
        /// <summary>'&gt;' (greater-than operator)</summary>
        GreaterThanSign,
        /// <summary>'&lt;' (less-than operator)</summary>
        LessThanSign,
        /// <summary>'&gt;=' (greater-than-or-equals operator)</summary>
        GreaterThanOrEqualsSign,
        /// <summary>'&lt;=' (less-than-or-equals operator)</summary>
        LessThanOrEqualsSign,
    }
}