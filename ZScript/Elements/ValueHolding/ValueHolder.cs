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
namespace ZScript.Elements.ValueHolding
{
    /// <summary>
    /// Specifies a definition for an object that points to a value
    /// </summary>
    public class ValueHolder
    {
        /// <summary>
        /// Gets or sets the name for this value holder
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the value contained within this value holder definition
        /// </summary>
        public object Type { get; set; }
    }
}