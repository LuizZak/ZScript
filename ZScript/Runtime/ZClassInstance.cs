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

using ZScript.Elements;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents an instance of a ZClass
    /// </summary>
    public class ZClassInstance
    {
        /// <summary>
        /// The local memory for this class instance
        /// </summary>
        private readonly Memory _localMemory; 

        /// <summary>
        /// Gets the class this object is an instance of
        /// </summary>
        public ZClass Class { get; }

        /// <summary>
        /// Gets the local memory for this class instance
        /// </summary>
        public Memory LocalMemory => _localMemory;

        /// <summary>
        /// Initializes a new instance of the ZClassInstance class
        /// </summary>
        /// <param name="zClass">The class this object is an instance of</param>
        public ZClassInstance(ZClass zClass)
        {
            Class = zClass;
            _localMemory = new Memory();

            // Init the local memory with null values for the variables
            foreach (var field in zClass.Fields)
            {
                _localMemory.SetVariable(field.Name, null);
            }
            
            // Init special variables
            _localMemory.SetVariable("this", this);
        }
    }
}