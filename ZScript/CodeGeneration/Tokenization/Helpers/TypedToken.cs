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
using System.Linq;
using System.Text;

using ZScript.Elements;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Represents a token that contains a ZScriptParser.TypeContext enclosed within.
    /// Used in instructions that rely on a type to be performed
    /// </summary>
    public class TypedToken : Token, IEquatable<TypedToken>
    {
        /// <summary>
        /// The TypeContext associated with this typed token
        /// </summary>
        private readonly ZScriptParser.TypeContext _typeContext;

        /// <summary>
        /// The TypeDef associated with this typed token
        /// </summary>
        private readonly TypeDef _typeDef;

        /// <summary>
        /// The type associated with this typed token
        /// </summary>
        private readonly Type _type;

        /// <summary>
        /// The types associated with this typed token
        /// </summary>
        private readonly Type[] _types;

        /// <summary>
        /// Gets the TypeContext associated with thi type
        /// </summary>
        public ZScriptParser.TypeContext TypeContext => _typeContext;

        /// <summary>
        /// Gets the TypeDef associated with this typed token
        /// </summary>
        public TypeDef TypeDef => _typeDef;

        /// <summary>
        /// Gets type associated with this typed token
        /// </summary>
        public Type RawType => _type;

        /// <summary>
        /// Gets the raw types associated with this typed token
        /// </summary>
        public Type[] RawTypes => _types;

        /// <summary>
        /// Initializes a new instance of the TypedToken class
        /// </summary>
        /// <param name="type">The type for this token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="typeContext">The type context to associate with this typed token</param>
        public TypedToken(TokenType type, VmInstruction instruction, ZScriptParser.TypeContext typeContext)
            : base(type, null, instruction)
        {
            _typeContext = typeContext;
        }

        /// <summary>
        /// Initializes a new instance of the TypedToken class
        /// </summary>
        /// <param name="type">The type for this token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="typeDef">The type to associate with this typed token</param>
        public TypedToken(TokenType type, VmInstruction instruction, TypeDef typeDef)
            : base(type, null, instruction)
        {
            _typeDef = typeDef;
        }

        /// <summary>
        /// Initializes a new instance of the TypedToken class
        /// </summary>
        /// <param name="tokenType">The type for this token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="type">The type to associate with this typed token</param>
        public TypedToken(TokenType tokenType, VmInstruction instruction, Type type)
            : base(tokenType, null, instruction)
        {
            _type = type;
        }

        /// <summary>
        /// Initializes a new instance of the TypedToken class
        /// </summary>
        /// <param name="tokenType">The type for this token</param>
        /// <param name="instruction">The instruction to associate with the token</param>
        /// <param name="types">The type to associate with this typed token</param>
        public TypedToken(TokenType tokenType, VmInstruction instruction, Type[] types)
            : base(tokenType, null, instruction)
        {
            _types = types;
        }

        /// <summary>
        /// Returns a string representation of this typed token
        /// </summary>
        /// <returns>A string representation of this typed token</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{ TypedToken ");

            if (_types != null)
            {
                builder.Append("types: [");
                builder.Append(string.Join(", ", (IEnumerable<object>)_types));
                builder.Append("]");
            }
            else if (_type != null)
            {
                builder.Append("rawType: " + _type);
            }
            else if (_typeContext != null)
            {
                builder.Append("type: " + _typeContext);
            }
            else
            {
                builder.Append("typeDef: " + _typeDef);
            }

            builder.Append(" }");

            return builder.ToString();
        }

        #region Equality members

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        public bool Equals(TypedToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_types != null && other._types != null && !_types.SequenceEqual(other._types))
                return false;
            if ((_types != null) != (other._types != null))
                return false;

            return base.Equals(other) && Equals(_typeContext, other._typeContext) && Equals(_typeDef, other._typeDef) && _type == other._type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypedToken)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_typeContext?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_typeDef?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_type?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_types?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator==(TypedToken left, TypedToken right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(TypedToken left, TypedToken right)
        {
            return !Equals(left, right);
        }

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        #endregion
    }
}