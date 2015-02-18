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
using Antlr4.Runtime;

namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a message that is raised during code analysis
    /// </summary>
    public abstract class CodeMessage
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the line the message is relevant at
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the offset in the line the message is relevant at
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the context the message is contained at
        /// </summary>
        public ParserRuleContext Context { get; set; }

        /// <summary>
        /// Gets a friendly version of the context for this code message
        /// </summary>
        public string ContextName
        {
            get
            {
                // Try to roll up until a function definition context
                RuleContext context = Context;
                while (context != null)
                {
                    var fd = context as ZScriptParser.FunctionDefinitionContext;
                    if (fd != null)
                    {
                        return "function '" + fd.functionName().IDENT().GetText() + "'";
                    }

                    var od = context as ZScriptParser.ObjectDefinitionContext;
                    if (od != null)
                    {
                        return "object '" + od.objectName().IDENT().GetText() + "'";
                    }

                    var sf = context as ZScriptParser.SequenceFrameContext;
                    if (sf != null)
                    {
                        return "sequence '" + ((ZScriptParser.SequenceBlockContext)sf.Parent.Parent).sequenceName().IDENT() + "' frame range " + sf.frameRange().GetText();
                    }

                    var sb = context as ZScriptParser.SequenceBodyContext;
                    if (sb != null)
                    {
                        return "sequence '" + ((ZScriptParser.SequenceBlockContext)sb.Parent).sequenceName().IDENT() + "'";
                    }

                    context = context.Parent;

                    if (context == null)
                    {
                        return "";
                    }
                }

                return "";
            }
        }
    }
}