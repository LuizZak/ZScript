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
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// A lexer for a ZScript source file
    /// </summary>
    public sealed class ZScriptLexer1
    {
        private delegate bool CharacterTest(char c);

        private bool _hasLexedToken;
        private ZScriptToken _current;

        /// <summary>
        /// The source string this lexer is currently using
        /// </summary>
        public string SourceCode { get; }

        /// <summary>
        /// The source string this lexer is currently using
        /// </summary>
        public ISource Source { get; }

        /// <summary>
        /// Current character offset that the lexer is at on the source string.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the current token under the current string position.
        /// </summary>
        public ZScriptToken Current
        {
            get
            {
                if(!_hasLexedToken)
                    LexNextToken();

                return _current;
            }
            private set => _current = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZScriptLexer1"/> class with a given string source to parse with.
        /// </summary>
        public ZScriptLexer1(ISource source, string sourceCode)
        {
            Source = source;
            SourceCode = sourceCode;
        }
        
        /// <summary>
        /// Reads the token and advances the token value by one.
        /// </summary>
        public ZScriptToken Next()
        {
            var token = Current;

            LexNextToken();

            return token;
        }
        
        /// <summary>
        /// Skips the current token on the lexer
        /// </summary>
        public void SkipToken()
        {
            LexNextToken();
        }
        
        /// <summary>
        /// Returns true iff the current token's type matches the one specified.
        /// </summary>
        public bool IsCurrentTokenType(LexerTokenType type)
        {
            return Current.TokenType == type;
        }

        /// <summary>
        /// Returns true iff the current token's type matches an operator token.
        /// </summary>
        public bool IsCurrentTokenOperator()
        {
            return Current.TokenType.IsOperator();
        }
        
        /// <summary>
        /// Returns a list containing all remaining tokens up to the end of the string.
        /// </summary>
        public List<ZScriptToken> ReadAllTokens()
        {
            var tokens = new List<ZScriptToken>();

            while (Current.TokenType != LexerTokenType.Eof)
            {
                tokens.Add(Current);
                SkipToken();
            }

            return tokens;
        }

        private void LexNextToken()
        {
            _advanceWhile(IsWhitespace);

            _hasLexedToken = true;

            if (_isEof())
            {
                Current = new ZScriptToken(LexerTokenType.Eof, Location(), "", null);
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
            var range = StartStringRange();
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

            string source = range.CreateString();
            object value =
                tokenType == LexerTokenType.IntegerLiteral
                    ? (object)int.Parse(source, NumberStyles.Integer)
                    : float.Parse(source, NumberStyles.Float, new CultureInfo("en-US"));

            Current = new ZScriptToken(tokenType, range.CreateLocation(), source, value);
        }

        private void LexString()
        {
            var range = StartStringRange();
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

            string str = range.CreateString();
            Current = new ZScriptToken(LexerTokenType.StringLiteral, range.CreateLocation(), str, value.ToString());
        }

        private void LexAlphanumeric()
        {
            var range = StartStringRange();

            while (_isLetter() || _isDigit() || _isNextCharacter('_'))
            {
                _advance();
            }

            string result = range.CreateString();

            var keywordMapping = new Dictionary<string, LexerTokenType>
            {
                {"func", LexerTokenType.FunctionKeyword},
                {"class", LexerTokenType.ClassKeyword},
                {"if", LexerTokenType.If},
                {"else", LexerTokenType.Else},
                {"for", LexerTokenType.For},
                {"while", LexerTokenType.While},
                {"switch", LexerTokenType.Switch},
                {"break", LexerTokenType.Break},
                {"continue", LexerTokenType.Continue},
                {"return", LexerTokenType.Return}
            };

            if(keywordMapping.TryGetValue(result, out var keyword))
            {
                Current = new ZScriptToken(keyword, range.CreateLocation(), result);
            }
            else
            {
                Current = new ZScriptToken(LexerTokenType.Identifier, range.CreateLocation(), result);
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
                Current = new ZScriptToken(oper, marker.CreateLocation(), marker.CreateString());
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
            }
            else if(c == '^')
            {
                _advance();

                op = LexerTokenType.BitwiseXorOperator;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.BitwiseXorAssignOperator;
                }
            }
            else if (c == '~')
            {
                _advance();

                op = LexerTokenType.BitwiseNegateOperator;

                if (_peekChar() == '=')
                {
                    _advance();
                    op = LexerTokenType.BitwiseNegateAssignOperator;
                }
            }
            else
            {
                op = LexerTokenType.Unknown;
            }
                
            Current = new ZScriptToken(op, marker.CreateLocation(), marker.CreateString());
        }

        private void LexCommentToken()
        {
            var range = StartStringRange();

            if (_advanceIfSubstring("//"))
            {
                _advanceWhile(c => c != '\n');
                _advance();

                Current = new ZScriptToken(LexerTokenType.SingleLineComment, range.CreateLocation(), range.CreateString());
            }
            else if (_advanceIfSubstring("/*"))
            {
                while (!_isEof(1))
                {
                    if (_peekChar() == '*' && _peekChar() == '/')
                    {
                        _advance(2);
                        break;
                    }
                }

                Current = new ZScriptToken(LexerTokenType.MultiLineComment, range.CreateLocation(), range.CreateString());
            }
        }

        /// <summary>
        /// Requests a backtrack for the current point.
        /// </summary>
        /// <returns></returns>
        public IBacktrackPoint CreateBacktrack()
        {
            return new BacktrackPoint(this);
        }

        /// <summary>
        /// Requests the start of a a range marker at the current offset.
        /// </summary>
        public ISourceRangeMarker StartStringRange()
        {
            return new SourceRangeMarker(this, Offset);
        }

        /// <summary>
        /// Returns the current offset, as a zero-length <see cref="SourceLocation"/> instance.
        /// </summary>
        public SourceLocation Location()
        {
            return new SourceLocation(Source, Offset, 0, LineAtOffset(Offset), ColumnAtOffset(Offset));
        }

        /// <summary>
        /// Returns the line at a given string offset
        /// </summary>
        public int LineAtOffset(int offset)
        {
            int line = 1;

            for (int i = 0; i < offset; i++)
            {
                if (SourceCode[i] == '\n')
                    line += 1;
            }

            return line;
        }

        /// <summary>
        /// Returns the column offset until the start of the leading line at a given offset.
        /// </summary>
        public int ColumnAtOffset(int offset)
        {
            int column = 1;

            for (int i = offset - 1; i >= 0; i--)
            {
                if (SourceCode[i] == '\n')
                    return column;

                column += 1;
            }

            return column;
        }

        #region String scanning

        /// <summary>
        /// Returns true iff (<see cref="Offset"/> + lookahead) &gt;= <see cref="SourceCode"/>.Length
        /// </summary>
        private bool _isEof(int lookahead = 0)
        {
            return Offset + lookahead >= SourceCode.Length;
        }

        /// <summary>
        /// Returns the current character on the source string and forwards the offset
        /// by one.
        /// </summary>
        private char _nextChar()
        {
            if (_isEof())
                return '\0';
            
            return SourceCode[Offset++];
        }
        
        /// <summary>
        /// Peeks the current character without advancing the reading offset.
        /// </summary>
        private char _peekChar(int lookahead = 0)
        {
            if (_isEof(lookahead))
                return '\0';

            return SourceCode[Offset + lookahead];
        }

        /// <summary>
        /// Returns whether the current offset on the buffer has the given string.
        /// </summary>
        private bool _isSubstring([NotNull] string str)
        {
            if (_isEof(str.Length))
            {
                return false;
            }

            return SourceCode.IndexOf(str, Offset, str.Length, StringComparison.Ordinal) == 0;
        }

        /// <summary>
        /// Advances the string offset by a given string's length iff <see cref="_isSubstring(string)"/>.
        /// </summary>
        private bool _advanceIfSubstring([NotNull] string str)
        {
            if (_isSubstring(str))
            {
                Offset += str.Length;
                return true;
            }

            return false;
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
        /// Advances the offset on the string while a given character testing predicate returns true.
        /// 
        /// The offset then points to the first character which failed the predicate test.
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

        private class BacktrackPoint : IBacktrackPoint
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

        private class SourceRangeMarker : ISourceRangeMarker
        {
            private readonly ZScriptLexer1 _lexer;
            private readonly int _startOffset;

            public SourceRangeMarker([NotNull] ZScriptLexer1 lexer, int startOffset)
            {
                _lexer = lexer;
                _startOffset = startOffset;
            }

            /// <summary>
            /// Using the start/end offsets of a lexer, computates the total source location
            /// of the range.
            /// </summary>
            public SourceLocation CreateLocation()
            {
                return new SourceLocation(_lexer.Source, _startOffset, _lexer.Offset - _startOffset, _lexer.LineAtOffset(_startOffset), _lexer.ColumnAtOffset(_startOffset));
            }

            /// <summary>
            /// Creates a string that matches the interval this source range represents.
            /// </summary>
            public string CreateString()
            {
                return _lexer.SourceCode.Substring(_startOffset, _lexer.Offset - _startOffset);
            }

            public string GetString()
            {
                return _lexer.SourceCode.Substring(_startOffset, _lexer.Offset - _startOffset);
            }
        }
    }
    
    /// <summary>
    /// Represents an object that encapsulates a backtracking operation for a lexer.
    /// </summary>
    public interface IBacktrackPoint
    {
        /// <summary>
        /// Backtracks the lexer to the point at which this backtrack point was created.
        /// </summary>
        void Backtrack();
    }

    /// <summary>
    /// Specifies a source range marker that a lexer produces when requested by a call to <see cref="ZScriptLexer1.StartStringRange"/>.
    /// </summary>
    public interface ISourceRangeMarker
    {
        /// <summary>
        /// Using the start/end offsets of a lexer, computes the total source location
        /// of the range.
        /// </summary>
        SourceLocation CreateLocation();

        /// <summary>
        /// Creates a string that matches the interval this source range represents.
        /// </summary>
        string CreateString();
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
        /// An unknown token that the lexer cannot recognize.
        /// </summary>
        Unknown,

        /// <summary>
        /// A single line comment
        /// </summary>
        SingleLineComment,

        /// <summary>
        /// A C-like multi-line comment
        /// </summary>
        MultiLineComment,

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
        FunctionKeyword,

        /// <summary>
        /// 'class' keyword
        /// </summary>
        ClassKeyword,

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
        BitwiseXorOperator,
        /// <summary>'^=' (bitwise XOR assignment operator)</summary>
        BitwiseXorAssignOperator,
        /// <summary>'~' (bitwise NOT operator)</summary>
        BitwiseNegateOperator,
        /// <summary>'~=' (bitwise NOT assignment operator)</summary>
        BitwiseNegateAssignOperator,
        
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

    /// <summary>
    /// Helper operations for token types
    /// </summary>
    public static class LexerTokenHelpers
    {
        /// <summary>
        /// Returns true iff the token represents a binary/unary operator.
        /// </summary>
        public static bool IsOperator(this LexerTokenType tok)
        {
            switch (tok)
            {
                // Signs (work as operators, as well)
                case LexerTokenType.PlusSign:
                case LexerTokenType.PlusEqualsSign:
                case LexerTokenType.MinusSign:
                case LexerTokenType.MinusEqualsSign:
                case LexerTokenType.MultiplySign:
                case LexerTokenType.MultiplyEqualsSign:
                case LexerTokenType.DivideSign:
                case LexerTokenType.DivideEqualsSign:
                case LexerTokenType.LessThanSign:
                case LexerTokenType.LessThanOrEqualsSign:
                case LexerTokenType.GreaterThanSign:
                case LexerTokenType.GreaterThanOrEqualsSign:
                case LexerTokenType.EqualsSign:
                case LexerTokenType.UnequalsSign:
                    
                // Operators
                case LexerTokenType.AssignOperator:
                case LexerTokenType.AndOperator:
                case LexerTokenType.OrOperator:
                case LexerTokenType.BooleanNegateOperator:
                case LexerTokenType.BitwiseAndOperator:
                case LexerTokenType.BitwiseOrOperator:
                case LexerTokenType.BitwiseXorOperator:
                case LexerTokenType.BitwiseNegateOperator:
                case LexerTokenType.BitwiseAndAssignOperator:
                case LexerTokenType.BitwiseXorAssignOperator:
                case LexerTokenType.BitwiseOrAssignOperator:
                case LexerTokenType.BitwiseNegateAssignOperator:
                    return true;

                default:
                    return false;
            }
        }
    }
}