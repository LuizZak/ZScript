using System;
using System.Linq;

namespace ZScript.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Wraps a ZClassInstance member
    /// </summary>
    public class ZClassMember : IMemberWrapper
    {
        /// <summary>
        /// The ZClassInstance wrapped in this ZClassInstance
        /// </summary>
        private readonly ZClassInstance _target;

        /// <summary>
        /// The name of the ZObject member wrapped
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The type of field stored in this ZClassMember
        /// </summary>
        private readonly Type _fieldType;

        /// <summary>
        /// Initializes a new instance of the ZClassMember class
        /// </summary>
        /// <param name="target">The ZClassInstance to wrap on ZClassMember</param>
        /// <param name="name">The name of the ZClassInstance member to wrap</param>
        public ZClassMember(ZClassInstance target, string name)
        {
            _target = target;
            _name = name;

            var field = target.Class.Fields.First(f => f.Name == name);

            _fieldType = field.Type;
        }

        /// <summary>
        /// Sets the value of the member
        /// </summary>
        /// <param name="value">The value to set</param>
        public void SetValue(object value)
        {
            _target.LocalMemory.SetVariable(_name, value);
        }

        /// <summary>
        /// Gets the value of the member referenced by this ZClassMember instance.
        /// If no member with the specified name exists, an exception is raised
        /// </summary>
        /// <returns>The value referenced by this ZObjectMember</returns>
        /// <exception cref="Exception">No member with the name referenced by this ZObjectMember exists</exception>
        public object GetValue()
        {
            return _target.LocalMemory.GetVariable(_name);
        }

        /// <summary>
        /// The name of the member referenced by this ZClassMember
        /// </summary>
        public string MemberName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the type of this member
        /// </summary>
        public Type MemberType
        {
            get { return _fieldType; }
        }
    }
}