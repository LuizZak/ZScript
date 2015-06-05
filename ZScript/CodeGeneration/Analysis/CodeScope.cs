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

using Antlr4.Runtime;
using ZScript.CodeGeneration.Definitions;

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
        /// List of type aliases registered on this code scope
        /// </summary>
        private readonly List<TypeAliasDefinition> _typeAlias = new List<TypeAliasDefinition>();

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
        public CodeScope[] ChildrenScopes => _childrenScopes.ToArray();

        /// <summary>
        /// The current variables in this scope
        /// </summary>
        public Definition[] Definitions => _definitions.ToArray();

        /// <summary>
        /// Gets the current definition usages in this scope
        /// </summary>
        public DefinitionUsage[] DefinitionUsages => _usages.ToArray();

        /// <summary>
        /// Initializes a new instance of the CodeScope class
        /// </summary>
        public CodeScope()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the CodeScope class with a context associated with it
        /// </summary>
        /// <param name="context">A context to associate this CodeScope with</param>
        public CodeScope(ParserRuleContext context)
        {
            Context = context;
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
        /// Scans through all definitions, returning the first definition that matches a given selector
        /// </summary>
        /// <param name="selector">The selector to apply to the definitions. The method returns the first definition that made the selector return true</param>
        /// <returns>The first definition that returns true on the given selector</returns>
        public Definition GetDefinition(Func<Definition, bool> selector)
        {
            var scope = this;

            while (scope != null)
            {
                foreach (var definition in scope._definitions)
                {
                    if (selector(definition))
                        return definition;
                }

                scope = scope.ParentScope;
            }

            return null;
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

            return ParentScope?.GetDefinitionByName(definitionName);
        }

        /// <summary>
        /// Searches a definition with a given type by name in this, and all parent scopes recursively.
        /// If no definitions with the given name are found, null is returned instead
        /// </summary>
        /// <param name="definitionName">The name of the definition to search</param>
        /// <returns>The definition that was found</returns>
        public T GetDefinitionByName<T>(string definitionName) where T : Definition
        {
            foreach (var definition in _definitions)
            {
                var def = definition as T;
                if (def != null && def.Name == definitionName)
                {
                    return def;
                }
            }

            return ParentScope?.GetDefinitionByName<T>(definitionName);
        }

        /// <summary>
        /// Searches a definition by context in this, and all children scopes recursively.
        /// If no definitions with the given context is found, null is returned instead
        /// </summary>
        /// <param name="context">The context of the definition to search</param>
        /// <returns>The definition that was found</returns>
        public Definition GetDefinitionByContextRecursive(RuleContext context)
        {
            Stack<CodeScope> scopeQueue = new Stack<CodeScope>();

            scopeQueue.Push(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Pop();
                var def = scope.Definitions.FirstOrDefault(d => d.Context == context);
                if(def != null)
                    return def;

                foreach (var child in scope._childrenScopes)
                {
                    scopeQueue.Push(child);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all of the definitions reachable by this scope that are of a given type
        /// </summary>
        /// <typeparam name="TDefinition">The type of the definitions to search for</typeparam>
        /// <returns>An enumerable containing all of the definitions of the given type</returns>
        public IEnumerable<TDefinition> GetDefinitionsByType<TDefinition>() where TDefinition : Definition
        {
            var scope = this;
            var definitions = new List<TDefinition>();

            while (scope != null)
            {
                definitions.AddRange(scope._definitions.OfType<TDefinition>());

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

            Stack<CodeScope> scopeQueue = new Stack<CodeScope>();

            scopeQueue.Push(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Pop();

                definitions.AddRange(scope._definitions);

                foreach (var child in scope._childrenScopes)
                {
                    scopeQueue.Push(child);
                }
            }

            return definitions;
        }

        /// <summary>
        /// Gets all the definitions of a given type defined in this scope and all children scopes
        /// </summary>
        /// <typeparam name="TDefinition">The type of definition to get</typeparam>
        /// <returns>An enumerable containing the definitions collected</returns>
        public IEnumerable<TDefinition> GetDefinitionsByTypeRecursive<TDefinition>() where TDefinition : Definition
        {
            List<TDefinition> definitions = new List<TDefinition>();

            Stack<CodeScope> scopeQueue = new Stack<CodeScope>();

            scopeQueue.Push(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Pop();

                definitions.AddRange(scope._definitions.OfType<TDefinition>());

                foreach (var child in scope._childrenScopes)
                {
                    scopeQueue.Push(child);
                }
            }

            return definitions;
        }

        /// <summary>
        /// Gets all definitions reachable in this code scope by name
        /// </summary>
        /// <param name="definitionName">The name of the definitions to get</param>
        /// <returns>An enumerable containing all of the definitions with a given name reachable by this code scope</returns>
        public IList<Definition> GetDefinitionsByName(string definitionName)
        {
            var scope = this;
            var definitions = new List<Definition>();

            while (scope != null)
            {
                foreach (var definition in scope._definitions)
                {
                    if(definition.Name == definitionName)
                        definitions.Add(definition);
                }

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
            var scopes = new List<CodeScope>();

            var scopeQueue = new Stack<CodeScope>();

            scopeQueue.Push(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Pop();
                scopes.Add(scope);

                foreach (var child in scope._childrenScopes)
                {
                    scopeQueue.Push(child);
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

            Stack<CodeScope> scopeQueue = new Stack<CodeScope>();

            scopeQueue.Push(this);

            while (scopeQueue.Count > 0)
            {
                var scope = scopeQueue.Pop();

                usages.AddRange(scope._usages);

                foreach (var child in scope.ChildrenScopes)
                {
                    scopeQueue.Push(child);
                }
            }

            return usages;
        }

        /// <summary>
        /// Returns a code scope from this code scope tree that has a given context nested inside of it.
        /// If no matching code scope is found, null is returned instead
        /// </summary>
        /// <param name="context">The context to search in this CodeScope</param>
        /// <returns>A CodeScope which contains the given scope, or null, if none is found</returns>
        public CodeScope GetScopeContainingContext(RuleContext context)
        {
            // Assume that if this scope has no parent, it is the root scope, and return it when the query is for a Program context (the root context)
            if (context is ZScriptParser.ProgramContext && ParentScope == null)
                return this;

            var scopes = GetAllScopesRecursive();

            var p = context;
            while (p != null)
            {
                // If the context is a program context, return the base global scope
                if (p is ZScriptParser.ProgramContext && ParentScope == null)
                    return this;

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var scope in scopes)
                {
                    if (p == scope.Context)
                    {
                        return scope;
                    }
                }

                p = p.Parent;
            }
            
            return null;
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

        /// <summary>
        /// Adds a new type alias to this code scope
        /// </summary>
        /// <param name="typeAlias">The type alias to add to this scope</param>
        public void AddTypeAliasDefinition(TypeAliasDefinition typeAlias)
        {
            _typeAlias.Add(typeAlias);
        }
    }
}