using System;
using System.Text;

using ZScript.Elements;
using ZScript.Runtime.Execution;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Represents a token that contains a ZScriptParser.TypeContext enclosed within.
    /// Used in instructions that rely on a type to be performed
    /// </summary>
    public class TypedToken : Token, IEquatable<TypedToken>
    {
        /// <summary>
        /// The TypeContext associated with thi type
        /// </summary>
        private readonly ZScriptParser.TypeContext _typeContext;

        /// <summary>
        /// Gets the TypeContext associated with thi type
        /// </summary>
        public ZScriptParser.TypeContext TypeContext
        {
            get { return _typeContext; }
        }

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
        /// Returns a string representation of this typed token
        /// </summary>
        /// <returns>A string representation of this typed token</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{ TypedToken type:" + _typeContext);
            builder.Append(" }");

            return builder.ToString();
        }

        #region Equality members

        public bool Equals(TypedToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_typeContext, other._typeContext);
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
            return (_typeContext != null ? _typeContext.GetHashCode() : 0);
        }

        public static bool operator==(TypedToken left, TypedToken right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(TypedToken left, TypedToken right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}