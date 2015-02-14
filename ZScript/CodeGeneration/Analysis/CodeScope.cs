using System;
using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;

using ZScript.CodeGeneration.Elements;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Defines a code scope that contains definitions and definitions usage information
    /// </summary>
    public class CodeScope
    {
        /// <summary>
        /// The list of children scopes
        /// </summary>
        private readonly List<CodeScope> _childrenScopes = new List<CodeScope>();

        /// <summary>
        /// The current definitions in this scope
        /// </summary>
        private readonly List<Definition> _definitions = new List<Definition>();

        /// <summary>
        /// The current definition usages in this scope
        /// </summary>
        private readonly List<DefinitionUsage> _usages = new List<DefinitionUsage>();

        /// <summary>
        /// The context the scope is contained at
        /// </summary>
        public ParserRuleContext Context;

        /// <summary>
        /// Gets the parent scope for this scope
        /// </summary>
        public CodeScope ParentScope { get; private set; }

        /// <summary>
        /// Gets the base scope for this code scope.
        /// The value returned may be the object itself, in case it is the base scope
        /// </summary>
        public CodeScope BaseScope
        {
            get
            {
                if (ParentScope == null)
                    return this;

                return ParentScope.BaseScope;
            }
        }

        /// <summary>
        /// Gets an array of the children scopes for this scope
        /// </summary>
        public CodeScope[] ChildrenScopes
        {
            get { return _childrenScopes.ToArray(); }
        }

        /// <summary>
        /// The current variables in this scope
        /// </summary>
        public Definition[] Definitions
        {
            get { return _definitions.ToArray(); }
        }

        /// <summary>
        /// Gets the current definition usages in this scope
        /// </summary>
        public DefinitionUsage[] DefinitionUsages
        {
            get { return _usages.ToArray(); }
        }

        /// <summary>
        /// Adds a new subscope for this variable scope
        /// </summary>
        /// <param name="scope">The subscope to add</param>
        /// <exception cref="ArgumentException">The scope already has a parent, or this scope is contained within the passed scope's children list</exception>
        public void AddSubscope(CodeScope scope)
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

        /// <summary>
        /// Searches a definition by name in this, and all parent scopes recursively.
        /// If no definitions with the given name are found, null is returned instead
        /// </summary>
        /// <param name="definitionName">The name of the definition to search</param>
        /// <returns>The definition that was found</returns>
        public Definition GetDefinitionByName(string definitionName)
        {
            foreach (Definition def in _definitions)
            {
                if (def.Name == definitionName)
                {
                    return def;
                }
            }

            if (ParentScope != null)
            {
                return ParentScope.GetDefinitionByName(definitionName);
            }

            return null;
        }

        /// <summary>
        /// Searches a definition by context in this, and all children scopes recursively.
        /// If no definitions with the given context is found, null is returned instead
        /// </summary>
        /// <param name="context">The context of the definition to search</param>
        /// <returns>The definition that was found</returns>
        public Definition GetDefinitionByContextRecursive(RuleContext context)
        {
            Queue<CodeScope> scopeQueue = new Queue<CodeScope>();

            scopeQueue.Enqueue(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Dequeue();
                var def = scope.Definitions.FirstOrDefault(d => d.Context == context);
                if(def != null)
                    return def;

                foreach (var child in scope.ChildrenScopes)
                {
                    scopeQueue.Enqueue(child);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all of the definitions reachable by this scope that are of a given type
        /// </summary>
        /// <typeparam name="T">The type of the definitions to search for</typeparam>
        /// <returns>An enumerable containing all of the definitions of the given type</returns>
        public IEnumerable<T> GetDefinitionsByType<T>() where T : Definition
        {
            var scope = this;
            var definitions = new List<T>();

            while (scope != null)
            {
                definitions.AddRange(scope.Definitions.OfType<T>());

                scope = scope.ParentScope;
            }

            return definitions;
        }

        /// <summary>
        /// Gets all the definitions defined in this scope and all children scopes
        /// </summary>
        /// <returns>An enumerable containing the definitions collected</returns>
        public IEnumerable<Definition> GetAllDefinitionsRecursive()
        {
            List<Definition> definitions = new List<Definition>();

            Queue<CodeScope> scopeQueue = new Queue<CodeScope>();

            scopeQueue.Enqueue(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Dequeue();

                definitions.AddRange(scope._definitions);

                foreach (var child in scope.ChildrenScopes)
                {
                    scopeQueue.Enqueue(child);
                }
            }

            return definitions;
        }

        /// <summary>
        /// Gets all definitions reahable in this code scope by name
        /// </summary>
        /// <param name="definitionName">The name of the definitions to get</param>
        /// <returns>An enumerable containing all of the definitions with a given name reachable by this code scope</returns>
        public IEnumerable<Definition> GetDefinitionsByName(string definitionName)
        {
            var scope = this;
            var definitions = new List<Definition>();

            while (scope != null)
            {
                definitions.AddRange(scope.Definitions.Where(d => d.Name == definitionName));

                scope = scope.ParentScope;
            }

            return definitions;
        }

        /// <summary>
        /// Gets all the children scopes for this scope recursively, including this scope
        /// </summary>
        /// <returns>An enumerable containing the scopes collected</returns>
        public IEnumerable<CodeScope> GetAllScopesRecursive()
        {
            List<CodeScope> scopes = new List<CodeScope>();

            Queue<CodeScope> scopeQueue = new Queue<CodeScope>();

            scopeQueue.Enqueue(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Dequeue();
                scopes.Add(scope);

                foreach (var child in scope.ChildrenScopes)
                {
                    scopeQueue.Enqueue(child);
                }
            }

            return scopes;
        }

        /// <summary>
        /// Gets all the definition usages defined in this scope and all children scopes
        /// </summary>
        /// <returns>An enumerable containing the definition usages collected</returns>
        public IEnumerable<DefinitionUsage> GetAllUsagesRecursive()
        {
            List<DefinitionUsage> usages = new List<DefinitionUsage>();

            Queue<CodeScope> scopeQueue = new Queue<CodeScope>();

            scopeQueue.Enqueue(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Dequeue();

                usages.AddRange(_usages);

                foreach (var child in scope.ChildrenScopes)
                {
                    scopeQueue.Enqueue(child);
                }
            }

            return usages;
        }

        /// <summary>
        /// Gets all the usages for a given definition in this scope and all children scopes recursively
        /// </summary>
        /// <param name="definition">The definition to search the usages for</param>
        /// <returns>An enumerable containing all the definition usages for a given definition</returns>
        public IEnumerable<DefinitionUsage> GetUsagesForDefinition(Definition definition)
        {
            List<DefinitionUsage> usages = new List<DefinitionUsage>();

            InternalGetUsagesForDefinition(definition, usages);

            return usages;
        }

        /// <summary>
        /// Gets all the usages for a given definition in this scope and all children scopes recursively,
        /// dumping all the usages on a given list
        /// </summary>
        /// <param name="definition">The definition to search the usages for</param>
        /// <param name="target">The list to dump the usages at</param>
        /// <returns>An enumerable containing all the definition usages for a given definition</returns>
        private void InternalGetUsagesForDefinition(Definition definition, List<DefinitionUsage> target)
        {
            foreach (var usage in _usages)
            {
                if (usage.Definition == definition)
                {
                    target.Add(usage);
                }
            }

            foreach (var childrenScope in _childrenScopes)
            {
                childrenScope.InternalGetUsagesForDefinition(definition, target);
            }
        }

        /// <summary>
        /// Adds a new definition to this scope
        /// </summary>
        /// <param name="definition">The definition to register on this scope</param>
        public void AddDefinition(Definition definition)
        {
            _definitions.Add(definition);
        }

        /// <summary>
        /// Adds a new definition usage in this scope
        /// </summary>
        /// <param name="usage">The usage to register on this scope</param>
        public void AddDefinitionUsage(DefinitionUsage usage)
        {
            _usages.Add(usage);
        }
    }
}