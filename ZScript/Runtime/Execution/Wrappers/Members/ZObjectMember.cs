using System;

using ZScript.Elements;

namespace ZScript.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Wraps a ZObject member
    /// </summary>
    public class ZObjectMember : IMemberWrapper
    {
        /// <summary>
        /// The ZObject wrapped in this ZObjectMember
        /// </summary>
        private readonly ZObject _target;

        /// <summary>
        /// The name of the ZObject member wrapped
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the ZObjectMember class
        /// </summary>
        /// <param name="target">The ZObject to wrap on ZObjectMember</param>
        /// <param name="name">The name of the ZObject member to wrap</param>
        public ZObjectMember(ZObject target, string name)
        {
            _target = target;
            _name = name;
        }

        /// <summary>
        /// Sets the value of the member
        /// </summary>
        /// <param name="value">The value to set</param>
        public void SetValue(object value)
        {
            _target[_name] = value;
        }

        /// <summary>
        /// Gets the value of the member referenced by this ZObjectMember instance.
        /// If no member with the specified name exists, an exception is raised
        /// </summary>
        /// <returns>The value referenced by this ZObjectMember</returns>
        /// <exception cref="Exception">No member with the name referenced by this ZObjectMember exists</exception>
        public object GetValue()
        {
            return _target[_name];
        }

        /// <summary>
        /// The name of the member referenced by this ZObjectMember
        /// </summary>
        public string MemberName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the type of this member.
        /// This value is always typeof(object)
        /// </summary>
        public Type MemberType
        {
            get { return typeof(object); }
        }
    }
}