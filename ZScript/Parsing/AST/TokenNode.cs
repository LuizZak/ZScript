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

using JetBrains.Annotations;

namespace ZScript.Parsing.AST
{
    /// <summary>
    /// A token node hosts a single <see cref="ZScriptToken"/> instance.
    /// </summary>
    public class TokenNode
    {
        /// <summary>
        /// Gets or sets the parent for this node.
        /// </summary>
        [CanBeNull]
        public SyntaxNode Parent { get; internal set; }

        /// <summary>
        /// Gets the token associated with this token node
        /// </summary>
        public ZScriptToken Token { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenNode"/> class.
        /// </summary>
        public TokenNode(ZScriptToken token)
        {
            Token = token;
        }

        /// <summary>
        /// Detaches this node from its parent, if it has one.
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?.ChildTokens.Remove(this);
            Parent = null;
        }
    }
}
