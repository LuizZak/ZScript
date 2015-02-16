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

using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a value holder definition
    /// </summary>
    public class ValueHolderDefinition : Definition
    {
        /// <summary>
        /// Gets or sets a value that represents the expression containing the value for this variable definition
        /// </summary>
        public Expression ValueExpression { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this variable has a expression specifying its value
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this value holder definition has a type associated with it.
        /// If the value is false, the value holder has a type infered from the value expression
        /// </summary>
        public bool HasType { get; set; }

        /// <summary>
        /// Gets or sets the type associated with this value holder definition
        /// </summary>
        public TypeDef Type { get; set; }

        /// <summary>
        /// Gets or sets the context for the type of this value holder definition
        /// </summary>
        public ZScriptParser.TypeContext TypeContext { get; set; }

        /// <summary>
        /// Whether this value holder is constant
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Whether this value holder is an instance value
        /// </summary>
        public bool IsInstanceValue;
    }
}