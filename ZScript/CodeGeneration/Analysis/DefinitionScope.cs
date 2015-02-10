using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using ZScript.CodeGeneration.Elements;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Defines a variables scope
    /// </summary>
    public class DefinitionScope
    {
        /// <summary>
        /// The list of children scopes
        /// </summary>
        private readonly List<DefinitionScope> _childrenScopes = new List<DefinitionScope>();

        /// <summary>
        /// The current variables in this scope
        /// </summary>
        public readonly List<Definition> Definitions = new List<Definition>();

        /// <summary>
        /// The context the scope is contained at
        /// </summary>
        public ParserRuleContext Context;

        /// <summary>
        /// The parent scope for this scope
        /// </summary>
        public DefinitionScope ParentScope { get; private set; }

        /// <summary>
        /// Gets an array of the children scopes for this scope
        /// </summary>
        public DefinitionScope[] ChildrenScopes
        {
            get { return _childrenScopes.ToArray(); }
        }

        /// <summary>
        /// Adds a new subscope for this variable scope
        /// </summary>
        /// <param name="scope">The subscope to add</param>
        /// <exception cref="ArgumentException">The scope already has a parent, or this scope is contained within the passed scope's children list</exception>
        public void AddSubscope(DefinitionScope scope)
        {
            if (scope.ParentScope != null)
            {
                throw new ArgumentException("The subscope already has a parent!");
            }

            var s = this;
            while (s.ParentScope != null)
            {
                if (s.ParentScope == scope)
                {
                    throw new ArgumentException("Cyclic scope parenting detected");
                }
                s = s.ParentScope;
            }

            scope.ParentScope = this;

            _childrenScopes.Add(scope);
        }
    }
}