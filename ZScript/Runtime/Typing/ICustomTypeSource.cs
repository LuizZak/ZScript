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

using ZScript.Runtime.Typing.Elements;

namespace ZScript.Runtime.Typing
{
    /// <summary>
    /// Represents a custom type source for a type provider
    /// </summary>
    public interface ICustomTypeSource
    {
        /// <summary>
        /// Returns whether this custom type source contains a type with a specified name
        /// </summary>
        /// <param name="typeName">The name to search the custom type</param>
        /// <returns>true if this ICustomTypeSource contains the given type name; false otherwise</returns>
        bool HasType(string typeName);

        /// <summary>
        /// Returns a custom type on this custom type source with the given name; or null, if none exist
        /// </summary>
        /// <param name="typeName">The name to search the custom type</param>
        /// <returns>A custom type from this ICustomTypeSource that matches the given name; or null, if none exist</returns>
        TypeDef TypeNamed(string typeName);
    }
}