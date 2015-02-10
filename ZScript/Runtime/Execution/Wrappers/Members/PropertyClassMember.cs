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
        public override string MemberName
        {
            get { return _property.Name; }
        }

        /// <summary>
        /// Gets the type of the property pointed by this PropertyClassMember
        /// </summary>
        public override Type MemberType
        {
            get { return _property.PropertyType; }
        }

        /// <summary>
        /// Gets the property wrapped in this PropertyClassMember
        /// </summary>
        public PropertyInfo Property
        {
            get { return _property; }
        }

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