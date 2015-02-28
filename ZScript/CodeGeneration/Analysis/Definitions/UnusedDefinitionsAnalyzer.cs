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

using System.Linq;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis.Definitions
{
    /// <summary>
    /// Class responsible for analyzing definitions and searching for unused definitions
    /// </summary>
    public static class UnusedDefinitionsAnalyzer
    {
        /// <summary>
        /// Analyzes a given scope for unused definitions
        /// </summary>
        /// <param name="scope">The scope to search the unused definitions at</param>
        /// <param name="messageContainer">The message container to report errors to</param>
        public static void Analyze(CodeScope scope, MessageContainer messageContainer)
        {
            // Collect definitions
            foreach (var definition in scope.Definitions)
            {
                // Function and global variable definitions are skipt because they can be accessed from outside
                if (definition is FunctionDefinition || definition is GlobalVariableDefinition ||
                    (definition is ValueHolderDefinition && (((ValueHolderDefinition)definition).IsInstanceValue)))
                    continue;
                // Definitions missing context are injected by the definition collector and are not analyzed
                if (definition.Context == null)
                    continue;

                var usages = scope.GetUsagesForDefinition(definition).ToArray();

                if (!usages.Any())
                {
                    RegisterDefinitionNotUsed(definition, messageContainer);
                }
                
                AnalyzeOnlySet(definition, usages, messageContainer);
            }
        }

        /// <summary>
        /// Analyzes a definition for set-only usages
        /// </summary>
        /// <param name="definition">The definition to analyze</param>
        /// <param name="usages">The usages for the definition</param>
        /// <param name="messageContainer">The message container to report messages to</param>
        private static void AnalyzeOnlySet(Definition definition, DefinitionUsage[] usages, MessageContainer messageContainer)
        {
            // Only value holder definitions can be analyzed for get/set
            if (!(definition is ValueHolderDefinition) || usages.Length == 0)
                return;

            bool get = false;
            foreach (var usage in usages)
            {
                if (usage.Context == null)
                    continue;

                if (!(usage.Context.Parent is ZScriptParser.LeftValueContext))
                {
                    get = true;
                }
            }

            if (!get)
            {
                RegisterDefinitionOnlySet(definition, messageContainer);
            }
        }

        /// <summary>
        /// Analyzes a given scope and all children scopes recursively for unused definitions
        /// </summary>
        /// <param name="scope">The scope to search the unused definitions at</param>
        /// <param name="messageContainer">The message container to report errors to</param>
        public static void AnalyzeRecursive(CodeScope scope, MessageContainer messageContainer)
        {
            Analyze(scope, messageContainer);

            // Collect definitions
            foreach (var codeScope in scope.ChildrenScopes)
            {
                AnalyzeRecursive(codeScope, messageContainer);
            }
        }

        /// <summary>
        /// Registers a warning about a definition not being used in a given message container
        /// </summary>
        /// <param name="definition">The definition to report the warning of</param>
        /// <param name="messageContainer">The message container to report the warning at</param>
        static void RegisterDefinitionNotUsed(Definition definition, MessageContainer messageContainer)
        {
            var warning = "Unused definition '" + definition.Name + "'.";
            messageContainer.RegisterWarning(definition.Context.Start.Line, definition.Context.Start.Column, warning, WarningCode.UnusedDefinition, definition.IdentifierContext);
        }

        /// <summary>
        /// Registers a warning about a definition only being set, and never used
        /// </summary>
        /// <param name="definition">The definition to report the warning of</param>
        /// <param name="messageContainer">The message container to report the warning at</param>
        static void RegisterDefinitionOnlySet(Definition definition, MessageContainer messageContainer)
        {
            var warning = "Definition '" + definition.Name + "' has its value set, but never used.";
            messageContainer.RegisterWarning(definition.Context.Start.Line, definition.Context.Start.Column, warning, WarningCode.DefinitionOnlySet, definition.IdentifierContext);
        }
    }
}