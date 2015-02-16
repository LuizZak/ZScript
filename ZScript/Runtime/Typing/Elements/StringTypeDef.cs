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
namespace ZScript.Runtime.Typing.Elements
{
    /// <summary>
    /// Specifies a string primitive type definition
    /// </summary>
    public class StringTypeDef : TypeDef, IListTypeDef
    {
        /// <summary>
        /// Returns the type enclosed within the string type definition
        /// </summary>
        public TypeDef EnclosingType
        {
            get { return AnyType; }
        }

        /// <summary>
        /// Gets the type of objects accepted by the subscript of the string type
        /// </summary>
        public TypeDef SubscriptType
        {
            get { return IntegerType; }
        }

        /// <summary>
        /// Initilaizes a new instance of the StringTypeDef class
        /// </summary>
        public StringTypeDef() : base("string")
        {

        }
    }
}