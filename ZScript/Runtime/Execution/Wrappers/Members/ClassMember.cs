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
    /// Wraps a class member (field or property)
    /// </summary>
    public abstract class ClassMember : IMemberWrapper
    {
        /// <summary>
        /// The target for the get/set operations to perform
        /// </summary>
        protected readonly object target;

        public abstract string MemberName { get; }
        public abstract Type MemberType { get; }

        /// <summary>
        /// Gets the target for the get/set operations to perform
        /// </summary>
        public object Target => target;

        /// <summary>
        /// Initializes a new instance of ClassMember object
        /// </summary>
        /// <param name="target">The target for the get/set operations to perform</param>
        protected ClassMember(object target)
        {
            this.target = target;
        }

        public abstract void SetValue(object value);
        public abstract object GetValue();

        /// <summary>
        /// Returns a ClassMember that wraps a member from a target class.
        /// The name must point to either a public property or public field.
        /// If the method fails to find a valid field, an exception is raised
        /// </summary>
        /// <param name="target">The target to get the member from</param>
        /// <param name="memberName">The name of the member to get</param>
        /// <returns>A ClassMember that wraps the member</returns>
        /// <exception cref="ArgumentException">The member name provided does not points to a valid visible field or property</exception>
        public static ClassMember CreateMemberWrapper(object target, string memberName)
        {
            var field = target.GetType().GetField(memberName);
            if (field != null)
            {
                return CreateFieldWrapper(target, field);
            }

            var prop = target.GetType().GetProperty(memberName);
            if (prop != null)
            {
                return CreatePropertyWrapper(target, prop);
            }

            throw new ArgumentException("No public member of name '" + memberName + "' found on object of type '" + target.GetType() + "'", nameof(memberName));
        }

        /// <summary>
        /// Returns a FieldClassMember that wraps a field from a target class.
        /// </summary>
        /// <param name="target">The target to get the member from</param>
        /// <param name="field">The field to get</param>
        /// <returns>A ClassMember that wraps the field</returns>
        public static FieldClassMember CreateFieldWrapper(object target, FieldInfo field)
        {
            return new FieldClassMember(target, field);
        }

        /// <summary>
        /// Returns a PropertyClassMember that wraps a property from a target class.
        /// </summary>
        /// <param name="target">The target to get the member from</param>
        /// <param name="prop">The property to get</param>
        /// <returns>A ClassMember that wraps the property</returns>
        public static PropertyClassMember CreatePropertyWrapper(object target, PropertyInfo prop)
        {
            return new PropertyClassMember(target, prop);
        }
    }
}