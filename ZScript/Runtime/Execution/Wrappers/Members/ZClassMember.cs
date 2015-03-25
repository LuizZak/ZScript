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
using System.Linq;
using ZScript.Elements;

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
        /// The method this ZClassMember is pointing to, in case this is a method class member
        /// </summary>
        private readonly ZMethod _method;

        /// <summary>
        /// The type of field stored in this ZClassMember
        /// </summary>
        private readonly Type _fieldType;

        /// <summary>
        /// Whether the value on this ZClassMember is readonly
        /// </summary>
        private readonly bool _isReadonly;

        /// <summary>
        /// Whether this ZClassMember points to a class method
        /// </summary>
        private readonly bool _isMethod;

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

        /// <summary>
        /// Gets a value specifying whether whether the value on this ZClassMember is readonly
        /// </summary>
        public bool IsReadonly
        {
            get { return _isReadonly; }
        }

        /// <summary>
        /// Initializes a new instance of the ZClassMember class
        /// </summary>
        /// <param name="target">The ZClassInstance to wrap on ZClassMember</param>
        /// <param name="name">The name of the ZClassInstance member to wrap</param>
        public ZClassMember(ZClassInstance target, string name)
        {
            _target = target;
            _name = name;

            if (target.Class.Fields.Any(f => f.Name == name))
            {
                var field = target.Class.Fields.First(f => f.Name == name);
                _fieldType = field.Type;

                _isReadonly = false;
            }
            else
            {
                _fieldType = typeof(ICallableWrapper);

                _method = target.Class.Methods.First(m => m.Name == name);
                _isMethod = true;
                _isReadonly = true;
            }
        }

        /// <summary>
        /// Sets the value of the member
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <exception cref="InvalidOperationException">This ZClassMember is readonly</exception>
        public void SetValue(object value)
        {
            if (_isReadonly)
            {
                throw new InvalidOperationException("Cannot reassign readonly class member");
            }

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
            return _isMethod ? new Callables.ZClassMethod(_target, _method) : _target.LocalMemory.GetVariable(_name);
        }
    }
}