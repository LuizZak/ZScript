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

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies an expression composed from an expression context read from a script parser
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// The expression context for this expression
        /// </summary>
        private readonly ZScriptParser.ExpressionContext _expressionContext;

        /// <summary>
        /// Gets the expression context for this expression
        /// </summary>
        public ZScriptParser.ExpressionContext ExpressionContext
        {
            get { return _expressionContext; }
        }

        /// <summary>
        /// Initializes a new instance of the Expression class
        /// </summary>
        /// <param name="expressionContext">An expression context read from a script parser</param>
        public Expression(ZScriptParser.ExpressionContext expressionContext)
        {
            _expressionContext = expressionContext;
        }
    }
}