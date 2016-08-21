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

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Interface to be implemented by objects that have values that can be get/set
    /// </summary>
    public interface IValueHolder
    {
        /// <summary>
        /// Sets the value of the member wrapped by this IValueHolder
        /// </summary>
        /// <param name="value">The value to set the member as</param>
        void SetValue(object value);

        /// <summary>
        /// Gets the value of the member wrapped by this IValueHolder
        /// </summary>
        /// <returns>The value of the member wrapped by this IValueHolder</returns>
        object GetValue();
    }
}