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
using System.Reflection;

namespace ZScript.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Wraps a class's property
    /// </summary>
    public class PropertyClassMember : ClassMember
    {
        /// <summary>
        /// The property wrapped in this PropertyClassMember
        /// </summary>
        private readonly PropertyInfo _property;

        /// <summary>
        /// Gets the name of the property pointed by this PropertyClassMember
        /// </summary>
        public override string MemberName => _property.Name;

        /// <summary>
        /// Gets the type of the property pointed by this PropertyClassMember
        /// </summary>
        public override Type MemberType => _property.PropertyType;

        /// <summary>
        /// Gets the property wrapped in this PropertyClassMember
        /// </summary>
        public PropertyInfo Property => _property;

        /// <summary>
        /// Initializes a new instance of the PropertyClassMember class
        /// </summary>
        /// <param name="target">The target for the get/set operations to perform</param>
        /// <param name="property">The property to wrap on this PropertyClassMember</param>
        public PropertyClassMember(object target, PropertyInfo property)
            : base(target)
        {
            _property = property;
        }

        /// <summary>
        /// Sets the value of the property pointed by this PropertyClassMember
        /// </summary>
        /// <param name="value">The value to set for the property pointed by this PropertyClassMember</param>
        public override void SetValue(object value)
        {
            _property.SetValue(target, value);
        }

        /// <summary>
        /// Gets the value of the property pointed by this PropertyClassMember
        /// </summary>
        /// <returns>The value of the field property by this PropertyClassMember</returns>
        public override object GetValue()
        {
            return _property.GetValue(target);
        }
    }
}