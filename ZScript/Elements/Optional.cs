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

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a generic optional value
    /// </summary>
    public struct Optional<T> : IOptional, IEquatable<Optional<T>>
    {
        /// <summary>
        /// Gets the value stored in this optional
        /// </summary>
        private readonly T _value;

        /// <summary>
        /// Gets a value specifying whether this optional has a value contained within
        /// </summary>
        public bool HasInnerValue
        {
            get { return HasValue; }
        }

        /// <summary>
        /// Gets the value stored in this optional
        /// </summary>
        public object InnerValue
        {
            get { return Value; }
        }

        /// <summary>
        /// Gets a value specifying whether this optional has a value contained within
        /// </summary>
        public readonly bool HasValue;

        /// <summary>
        /// Gets the value stored in this optional. If the optional contains no value an <see cref="InvalidOperationException"/> is raised
        /// </summary>
        /// <exception cref="InvalidOperationException">The optional has no value stored</exception>
        public T Value
        {
            get
            {
                if(!HasValue)
                    throw new InvalidOperationException("This optional has no value stored");

                return _value;
            }
        }

        /// <summary>
        /// Gets an empty representation of an optional of type T
        /// </summary>
        public static Optional<T> Empty { get { return new Optional<T>(); } }

        /// <summary>
        /// Initializes a new instance of the Optional class with a starting value
        /// </summary>
        /// <param name="value">The starting value to stored in this optional.</param>
        public Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        /// <summary>
        /// Implicitly wraps a value into an optional
        /// </summary>
        /// <param name="value">The value to wrap into an optional</param>
        /// <returns>A wrapped T value</returns>
        public static implicit operator Optional<T>(T value)
        {
            return value == null ? new Optional<T>() : new Optional<T>(value);
        }

        /// <summary>
        /// Explicitly unwraps a value in the containing optional value.
        /// If the optional contains no value, an InvalidOperationException is raised
        /// </summary>
        /// <param name="optional">The value to wrap into an optional</param>
        /// <returns>The value wrapped inside the optional object</returns>
        /// <exception cref="InvalidOperationException">The optional has no value contained within</exception>
        public static explicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }

        #region Equality members

        public bool Equals(Optional<T> other)
        {
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Optional<T> && Equals((Optional<T>)obj);
        }

        public override int GetHashCode()
        {
            return HasValue || _value == null ? -1 : _value.GetHashCode();
        }

        /// <summary>Returns whether two optional values are equal</summary>
        public static bool operator==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two optional values are unequal</summary>
        public static bool operator!=(Optional<T> left, Optional<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>Returns whether an optional value and a raw value are equal</summary>
        public static bool operator ==(Optional<T> left, T right)
        {
            return left._value.Equals(right);
        }

        /// <summary>Returns whether an optional value and a raw value are unequal</summary>
        public static bool operator !=(Optional<T> left, T right)
        {
            return !left._value.Equals(right);
        }

        #endregion
    }

    /// <summary>
    /// Interface to be implemented by optional-typed objects
    /// </summary>
    public interface IOptional
    {
        /// <summary>
        /// Gets a value specifying whether this optional has a value contained within
        /// </summary>
        bool HasInnerValue { get; }

        /// <summary>
        /// Gets the value stored in this optional. If the optional contains no value an <see cref="InvalidOperationException"/> is raised
        /// </summary>
        /// <exception cref="InvalidOperationException">The optional has no value stored</exception>
        object InnerValue { get; }
    }
}