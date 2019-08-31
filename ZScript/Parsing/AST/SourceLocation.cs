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

namespace ZScript.Parsing.AST
{
    /// <summary>
    /// Specifies a location in a source code file
    /// </summary>
    [DebuggerDisplay("Source = {Source}, Offset = {Offset}, Length = {Length}, Line = {Line}, Column = {Column}")]
    public readonly struct SourceLocation : IEquatable<SourceLocation>
    {
        private static readonly SourceLocation InvalidNode = new SourceLocation(null, -1, -1, -1, -1);

        /// <summary>
        /// Gets a source location value that represents an invalid source location
        /// </summary>
        public static ref readonly SourceLocation Invalid => ref InvalidNode;

        /// <summary>
        /// Gets a boolean value specifying whether this source location structure represents
        /// an invalid source location.
        /// </summary>
        public bool IsValid => Offset > -1 && Length > -1 && Line > -1 && Column > -1;

        /// <summary>
        /// The origin of this location.
        /// </summary>
        [CanBeNull]
        public ISource Source { get; }

        /// <summary>
        /// Total offset (in characters) on the file.
        /// </summary>
        public int Offset { get; }
        
        /// <summary>
        /// If positive non-zero, this specifies a range of the source location.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Line this location originated from.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Zero-based column this location originated from.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Creates a new source location structure.
        /// </summary>
        public SourceLocation(ISource source, int offset, int length, int line, int column)
        {
            Source = source;
            Offset = offset;
            Length = length;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Returns true if this instance equals another <see cref="SourceLocation"/> instance down to each value.
        /// </summary>
        public bool Equals(SourceLocation other)
        {
            return Equals(Source, other.Source) && Offset == other.Offset && Line == other.Line && Column == other.Column && Length == other.Length;
        }

        /// <summary>
        /// Returns true if this instance equals a given object.
        /// 
        /// Returns false, if obj is not an instance of <see cref="SourceLocation"/>.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SourceLocation location && Equals(location);
        }

        /// <summary>
        /// Gets a hashcode for this source location structure.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Offset;
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Column;
                hashCode = (hashCode * 397) ^ Length;
                return hashCode;
            }
        }

        /// <summary>
        /// Returns true iff left and right represent the same source location value.
        /// </summary>
        public static bool operator ==(SourceLocation left, SourceLocation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns true iff left and right do not represent the same source location value.
        /// </summary>
        public static bool operator !=(SourceLocation left, SourceLocation right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{{Source: {Source}, Offset: {Offset}, Length: {Length} Line: {Line}, Column: {Column}}}";
        }
    }

    /// <summary>
    /// Interface for source-code origin objects.
    /// </summary>
    public interface ISource : IEquatable<ISource>
    {

    }
}