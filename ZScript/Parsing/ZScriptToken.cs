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
using System.Diagnostics;
using JetBrains.Annotations;
using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// A token read by a <see cref="ZScriptLexer1"/>
    /// </summary>
    [DebuggerDisplay("TokenType: {TokenType}, Location: {Location}, TokenString: {TokenString}, Value: '{Value}'")]
    public readonly struct ZScriptToken : IEquatable<ZScriptToken>
    {
        /// <summary>
        /// Type for this token.
        /// </summary>
        public LexerTokenType TokenType { get; }

        /// <summary>
        /// Gets the location of this token on the original source code.
        /// </summary>
        public SourceLocation Location { get; }

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
        /// Initializes a new instance of a <see cref="ZScriptToken"/> with a given type and string, and an invald source location and null value.
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, string tokenString) : this()
        {
            TokenType = tokenType;
            Location = SourceLocation.Invalid;
            TokenString = tokenString;
            Value = null;
        }
        
        /// <summary>
        /// Initializes a new instance of a <see cref="ZScriptToken"/> with a given type, string and value, and an invald source location.
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, string tokenString, [CanBeNull] object value) : this()
        {
            TokenType = tokenType;
            Location = SourceLocation.Invalid;
            TokenString = tokenString;
            Value = value;
        }
        
        /// <summary>
        /// Initializes a new instance of a <see cref="ZScriptToken"/>
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, SourceLocation location, string tokenString) : this()
        {
            TokenType = tokenType;
            Location = location;
            TokenString = tokenString;
            Value = null;
        }
        
        /// <summary>
        /// Initializes a new instance of a <see cref="ZScriptToken"/>
        /// </summary>
        public ZScriptToken(LexerTokenType tokenType, SourceLocation location, string tokenString, [CanBeNull] object value) : this()
        {
            TokenType = tokenType;
            Location = location;
            TokenString = tokenString;
            Value = value;
        }
        
#pragma warning disable 1591

        public bool Equals(ZScriptToken other)
        {
            return TokenType == other.TokenType && Location == other.Location && string.Equals(TokenString, other.TokenString) && Equals(Value, other.Value);
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
            return $"{{TokenType: {TokenType}, Location: {Location}, TokenString: {TokenString}, Value: {Value?.ToString() ?? "null"}}}";
        }

#pragma warning restore 1591
    }
}