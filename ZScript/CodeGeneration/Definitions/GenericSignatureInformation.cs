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

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Represents definitions for generic parametrization of a function signature
    /// </summary>
    public class GenericSignatureInformation
    {
        /// <summary>
        /// The array of generic types defined within this generic parameters definition
        /// </summary>
        private readonly GenericTypeDefinition[] _genericTypes;

        /// <summary>
        /// Gets an array of generic types defined within this generic parameters definition
        /// </summary>
        public GenericTypeDefinition[] GenericTypes
        {
            get { return _genericTypes; }
        }

        /// <summary>
        /// Initializes a new instance of the GenericParametersDefinition class with an empty list of generic types
        /// </summary>
        public GenericSignatureInformation()
            : this(new GenericTypeDefinition[0])
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the GenericParametersDefinition class with a specified set of generic types
        /// </summary>
        /// <param name="genericTypes">The generic types for this generic parameters definition</param>
        public GenericSignatureInformation(GenericTypeDefinition[] genericTypes)
        {
            _genericTypes = genericTypes;
        }
    }

    /// <summary>
    /// Represents a generic type definition for a function signature
    /// </summary>
    public class GenericTypeDefinition : TypeDef
    {
        /// <summary>
        /// Initializes a new instance of the GenericTypeDefinition class with a generic name specified
        /// </summary>
        /// <param name="name">The name for the generic type</param>
        public GenericTypeDefinition(string name)
            : base(name, false)
        {
            
        }
    }
}