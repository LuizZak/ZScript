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

using ZScript.Elements;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents an interface to be implemented by objects that will provide interop between a ZRuntime and the program
    /// </summary>
    public interface IRuntimeOwner
    {
        /// <summary>
        /// Called by the runtime when an export function was invoked by the script
        /// </summary>
        /// <param name="func">The function that was invoked</param>
        /// <param name="parameters">The list of parameters the function was called with</param>
        /// <returns>The return value for the function call</returns>
        object CallFunction(ZExportFunction func, params object[] parameters);

        /// <summary>
        /// Called by the runtime when a 'new' instruction has been hit
        /// </summary>
        /// <param name="typeName">The name of the type trying to be instantiated</param>
        /// <param name="parameters">The parameters collected from the function call</param>
        /// <returns>The newly created object</returns>
        object CreateType(string typeName, params object[] parameters);
    }
}