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
        /// <param name="arguments">The argument information for the function call, containing the list of arguments and list of generic types passed</param>
        /// <returns>The return value for the function call</returns>
        object CallFunction(ZExportFunction func, CallArguments arguments);

        /// <summary>
        /// Called by the runtime when an export function was invoked by the script, used to verify whether
        /// the runtime owner can atually respond to a given export function
        /// </summary>
        /// <param name="func">The export function to verify</param>
        /// <returns>Whether the runtime owner responds to this function</returns>
        bool RespondsToFunction(ZExportFunction func);

        /// <summary>
        /// Called by the runtime when a 'new' instruction has been hit
        /// </summary>
        /// <param name="typeName">The name of the type trying to be instantiated</param>
        /// <param name="arguments">The argumetns for the constructor call, containing the list of arguments and list of generic types passed</param>
        /// <returns>The newly created object</returns>
        object CreateType(string typeName, CallArguments arguments);
    }
}