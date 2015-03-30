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

using ZScript.Runtime.Execution.VirtualMemory;
using ZScript.Runtime.Typing;

namespace ZScript.Runtime.Execution
{
    /// <summary>
    /// Defines the context of a function virtual machine
    /// </summary>
    public class VmContext
    {
        /// <summary>
        /// The memory for this VM context
        /// </summary>
        public readonly IMemory<string> Memory;

        /// <summary>
        /// The addressed memory for this VM context
        /// </summary>
        public readonly IMemory<int> AddressedMemory;

        /// <summary>
        /// The current runtime associated with this VmContext
        /// </summary>
        public readonly ZRuntime Runtime;

        /// <summary>
        /// The owner of the runtime, used to perform some operations that require elevation to the owner domain (like instance creations with 'new')
        /// </summary>
        public readonly IRuntimeOwner Owner;

        /// <summary>
        /// The type provider that is used to provide type conversions during runtime
        /// </summary>
        public readonly TypeProvider TypeProvider;

        /// <summary>
        /// The type list for the VM context
        /// </summary>
        public readonly TypeList TypeList;

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, ZRuntime runtime)
            : this(memory, new IntegerMemory(), runtime, (runtime == null ? null : runtime.Owner), null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="addressedMemory">The addressed memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, IMemory<int> addressedMemory, ZRuntime runtime)
            : this(memory, addressedMemory, runtime, (runtime == null ? null : runtime.Owner), null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="addressedMemory">The addressed memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        /// <param name="owner">The owner to associate with this VmContext</param>
        /// <param name="typeProvider">Type provider for runtime type conversions</param>
        /// <param name="typeList">A type list object containing dynamic types to be used by the virtual machine to deal with generic typing</param>
        public VmContext(IMemory<string> memory, IMemory<int> addressedMemory, ZRuntime runtime, IRuntimeOwner owner, TypeProvider typeProvider, TypeList typeList = null)
        {
            Memory = memory;
            AddressedMemory = addressedMemory;
            Runtime = runtime;
            Owner = owner;
            TypeProvider = typeProvider;
            TypeList = typeList;
        }
    }
}