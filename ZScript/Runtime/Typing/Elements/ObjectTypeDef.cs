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

namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies the type for a ZObject
    /// </summary>
    public class ObjectTypeDef : TypeDef, IListTypeDef
    {
        /// <summary>
        /// Gets the type of objects subscript on this ObjectTypeDef
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return AnyType; }
        }

        /// <summary>
        /// Gets the type accepted by the subscript of the ObjectTypeDef
        /// </summary>
        public TypeDef SubscriptType
        {
            get { return StringType; }
        }

        /// <summary>
        /// Initializes a new instance of the ObjectTypeDef class
        /// </summary>
        public ObjectTypeDef() : base("object")
        {

        }
    }
}