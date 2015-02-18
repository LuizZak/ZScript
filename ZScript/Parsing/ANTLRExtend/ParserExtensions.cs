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

using ZScript.CodeGeneration.Definitions;
using ZScript.Runtime.Typing.Elements;

public partial class ZScriptParser
{
    /// <summary>
    /// Provides extensions to the ExpressionContext for tree weighting and storaging of
    /// context-sensitive information like marking tree portion as constant valued
    /// </summary>
    public partial class ExpressionContext
    {
        /// <summary>
        /// Gets or sets a value specifying whether this expression context has been weighted already
        /// </summary>
        public bool Weighted { get; set; }

        /// <summary>
        /// Gets or sets the weigth of this expression context.
        /// The weigth of the context depends on the weigth of child contexts, evaluating from bottom-top (leaf to root)
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the contents of this expression tree are constant in nature (e.g. they can be replaced by one single value
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the constant stored in this expression context is a primitive type.
        /// Primitive types include ints, floats, strings and bools
        /// </summary>
        public bool IsConstantPrimitive { get; set; }

        /// <summary>
        /// Gets or sets the value for the evaluated constant for this expression context
        /// </summary>
        public object ConstantValue { get; set; }

        /// <summary>
        /// Gets or sets the evaluated type associated with this expression context
        /// </summary>
        public TypeDef EvaluatedType { get; set; }
    }

    /// <summary>
    /// Provides extensions to the AssignmentExpressionContext for type definition storaging
    /// </summary>
    partial class AssignmentExpressionContext
    {
        /// <summary>
        /// Gets or sets the evaluated type associated with this assignment expression context
        /// </summary>
        public TypeDef EvaluatedType { get; set; }
    }

    /// <summary>
    /// Provides extensions to the ClosureExpressionContext for type inferring helping
    /// </summary>
    partial class ClosureExpressionContext
    {
        /// <summary>
        /// The type that was inferred to this ClosureExpressionContext while being processed by an ExpressionTypeResolver
        /// </summary>
        public TypeDef InferredType { get; set; }
    }

    /// <summary>
    /// Provides extensions to the MemberNameContext for binding it to definitions
    /// </summary>
    partial class MemberNameContext
    {
        /// <summary>
        /// Gets or sets the definition this member name context is pointing at
        /// </summary>
        public Definition Definition { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the member name is pointing to a constant value holder
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this member name context points to an existing definition
        /// </summary>
        public bool HasDefinition { get; set; }
    }

    /// <summary>
    /// Provides extensions fo the LeftValueContext for providing information about the value it is pointing at
    /// </summary>
    partial class LeftValueContext
    {
        /// <summary>
        /// Gets or sets a value specifying whether the left value is pointing to a constant value holder
        /// </summary>
        public bool IsConstant { get; set; }
    }
}