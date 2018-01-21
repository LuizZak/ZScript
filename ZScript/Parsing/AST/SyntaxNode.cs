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

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ZScript.Parsing.AST
{
    /// <summary>
    /// Base class for abstract syntax tree nodes.
    /// </summary>
    public class SyntaxNode
    {
        private readonly List<SyntaxNode> _children = new List<SyntaxNode>();

        /// <summary>
        /// Gets or sets the parent for this node.
        /// </summary>
        [CanBeNull]
        public SyntaxNode Parent { get; private set; }

        /// <summary>
        /// Gets or sets the original source location for this syntax node.
        /// </summary>
        public SourceLocation SourceLocation { get; set; } = SourceLocation.Invalid;

        /// <summary>
        /// Gets the list of children nodes on this node
        /// </summary>
        public IReadOnlyList<SyntaxNode> Children => _children;

        /// <summary>
        /// Adds a given node as a child of this <see cref="SyntaxNode"/>.
        /// 
        /// An exception is raised, if cyclic referencing in hierarchy is detected.
        /// 
        /// If child is already a child of another node, it is first removed from that node
        /// before attempting to add as a child of this node.
        /// </summary>
        public void AddChild([NotNull] SyntaxNode child)
        {
            var parent = Parent;
            while (parent != null)
            {
                if (parent == child)
                    throw new ArgumentException($"Specified node in {nameof(AddChild)} is an ancestor of this node and cannot be added as a child.", nameof(child));

                parent = parent.Parent;
            }

            child.RemoveFromParent();

            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>
        /// Detaches this node from its parent, if it has one.
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?._children.Remove(this);
            Parent = null;
        }

        /// <summary>
        /// Searches upwards through the ancestor chain until a node with a specified type
        /// is found.
        /// 
        /// Returns null, if a suitable parent type is not found.
        /// </summary>
        [CanBeNull]
        public T GetParentOfType<T>() where T : SyntaxNode
        {
            var parent = Parent;

            while (parent != null)
            {
                if (parent is T p)
                    return p;

                parent = parent?.Parent;
            }

            return null;
        }

        /// <summary>
        /// Returns the first child of this node that matches a given type.
        /// 
        /// Returns null, if no child matches the type <see cref="T"/>.
        /// </summary>
        [CanBeNull]
        public T GetFirstChildOfType<T>() where T : SyntaxNode
        {
            return _children.OfType<T>().FirstOrDefault();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyntaxNodeRef<T> where T : SyntaxNode
    {
        /// <summary>
        /// The node contained within this syntax node reference
        /// </summary>
        [CanBeNull]
        public T Node { get; }
    }
}
