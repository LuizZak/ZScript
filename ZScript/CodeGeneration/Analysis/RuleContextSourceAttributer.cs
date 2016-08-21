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

using Antlr4.Runtime.Tree;
using ZScript.CodeGeneration.Sourcing;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class used to attribute sources to parser rules ina  freshly-parsed AST tree
    /// </summary>
    public class RuleContextSourceAttributer : ZScriptBaseVisitor<bool>
    {
        /// <summary>
        /// Gets the source to attribute to objects visited by this listener
        /// </summary>
        public ZScriptDefinitionsSource Source { get; }

        /// <summary>
        /// Initializes a new instance of the RuleContextSourceAttributer class
        /// </summary>
        /// <param name="source">The soruce to define on this rule context source attributer</param>
        public RuleContextSourceAttributer(ZScriptDefinitionsSource source)
        {
            Source = source;
        }

        /// <summary>
        /// Visits the given node, attributing the source to it, in case it's an ISourceAttributedContext object
        /// </summary>
        public override bool VisitChildren(IRuleNode node)
        {
            var sourceAttributable = node as ZScriptParser.ISourceAttributedContext;
            if (sourceAttributable != null)
            {
                sourceAttributable.Source = Source;
            }

            return base.VisitChildren(node);
        }
    }
}
