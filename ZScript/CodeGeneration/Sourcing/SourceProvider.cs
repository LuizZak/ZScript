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

using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;

namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Class that provides script sources to a runtime generator
    /// </summary>
    public class SourceProvider
    {
        /// <summary>
        /// Internal collection of sources registered
        /// </summary>
        private readonly ICollection<ZScriptDefinitionsSource> _sources;

        /// <summary>
        /// Gets an array containing all of the sources registered on this scripts provider
        /// </summary>
        public ZScriptDefinitionsSource[] Sources => _sources.ToArray();

        /// <summary>
        /// Initializes a new instance of the ScriptsProvider class
        /// </summary>
        public SourceProvider()
        {
            _sources = new List<ZScriptDefinitionsSource>();
        }

        /// <summary>
        /// Adds a new script source to this scripts provider, returning a value specifying whether the inclusion was successful
        /// </summary>
        /// <param name="source">The source to include in this provider</param>
        /// <returns>true if the source was added, and false it it was already present</returns>
        public bool AddSource(ZScriptDefinitionsSource source)
        {
            if (_sources.Contains(source))
                return false;

            _sources.Add(source);

            return true;
        }

        /// <summary>
        /// Removes a source from this scripts provider, returning a value specifying whether the removal was successful
        /// </summary>
        /// <param name="source">The source to remove from this scripts provider</param>
        /// <returns>true if the source was removed, and false it it was not present</returns>
        public bool RemoveSource(ZScriptDefinitionsSource source)
        {
            return _sources.Remove(source);
        }

        /// <summary>
        /// Returns a script definition source that contains the given context, or null, if none were found on this source provider
        /// </summary>
        /// <param name="context">The context to search on this source provider</param>
        /// <returns>The definition source for the given context, or null, if none were found on this source provider</returns>
        public ZScriptDefinitionsSource SourceForContext(RuleContext context)
        {
            // To simplify, traverse the context up to the Program context, if available
            while (context != null && !(context is ZScriptParser.ProgramContext))
            {
                context = context.Parent;
            }

            // If the context is null, that means the original context was not contained within a program context.
            // In such a case, no matching definition source will be found, so null is returned
            if (context == null)
                return null;

            // Search the current sources
            foreach (var source in _sources)
            {
                if (source.Tree == context)
                    return source;
            }

            return null;
        }

        /// <summary>
        /// Clears the contents of this ScriptsProvider, removing all of the script sources associated with it
        /// </summary>
        public void Clear()
        {
            _sources.Clear();
        }
    }
}