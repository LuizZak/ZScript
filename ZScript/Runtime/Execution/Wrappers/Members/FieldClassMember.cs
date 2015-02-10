using System;
using System.Reflection;

namespace ZScript.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Wraps a class's field
    /// </summary>
    public class FieldClassMember : ClassMember
    {
        /// <summary>
        /// The field wrapped in this FieldClassMember
        /// </summary>
        private readonly FieldInfo _field;

        /// <summary>
        /// Gets the name of the field pointed by this FieldClassMember
        /// </summary>
        public override string MemberName
        {
            get { return _field.Name; }
        }

        /// <summary>
        /// Gets the type of the field pointed by this FieldClassMember
        /// </summary>
        public override Type MemberType
        {
            get { return _field.FieldType; }
        }

        /// <summary>
        /// Gets the field wrapped in this FieldClassMember
        /// </summary>
        public FieldInfo Field
        {
            get { return _field; }
        }

        /// <summary>
        /// Initializes a new instance of the FieldClassMember class
        /// </summary>
        /// <param name="target">The target for the get/set operations to perform</param>
        /// <param name="field">The field to wrap on this FieldClassMember</param>
        public FieldClassMember(object target, FieldInfo field) : base(target)
        {
            _field = field;
        }

        /// <summary>
        /// Sets the value of the field pointed by this FieldClassMember
        /// </summary>
        /// <param name="value">The value to set for the field pointed by this FieldClassMember</param>
        public override void SetValue(object value)
        {
            _field.SetValue(target, value);
        }

        /// <summary>
        /// Gets the value of the field pointed by this FieldClassMember
        /// </summary>
        /// <returns>The value of the field pointed by this FieldClassMember</returns>
        public override object GetValue()
        {
            return _field.GetValue(target);
        }
    }
}