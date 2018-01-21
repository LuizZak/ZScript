﻿#region License information
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

using JetBrains.Annotations;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Represents a method defined in a type alias definition
    /// </summary>
    public class TypeAliasMethodDefinition : FunctionDefinition
    {
        /// <summary>
        /// Initialzes a new instance of the TypeMethodDefinition class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="parameters">The parameters for the method</param>
        public TypeAliasMethodDefinition(string name, [NotNull] FunctionArgumentDefinition[] parameters)
            : base(name, null, parameters)
        {

        }
    }
}