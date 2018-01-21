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
using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// A parser for generating ASTs for the ZScript language
    /// </summary>
    public class ZScriptParser1
    {
        /// <summary>
        /// Lexer this parser is currently using
        /// </summary>
        [NotNull]
        public ZScriptLexer1 Lexer { get; }
        
        /// <summary>
        /// When a source is parsed, this points to the root AST node which all parsed AST nodes are children of.
        /// 
        /// Until the call to <see cref="Parse"/>, this property is null.
        /// </summary>
        [CanBeNull]
        public SyntaxNode RootNode { get; private set; }

        /// <summary>
        /// Gets or sets the diagnostics collector for this parser.
        /// 
        /// Defaults to an instance of <see cref="Diagnostics"/>.
        /// </summary>
        public IDiagnosticsCollector DiagnosticsCollector { get; set; } = new Diagnostics();

        /// <summary>
        /// Initializes a new instance of the <see cref="ZScriptParser1"/> class with a given lexer.
        /// </summary>
        public ZScriptParser1([NotNull] ZScriptLexer1 lexer)
        {
            Lexer = lexer;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ZScriptParser1"/> class with a given source string.
        /// 
        /// A lexer is automatically created for the source string and assigned to <see cref="Lexer"/>.
        /// </summary>
        public ZScriptParser1([NotNull] string source) : this(new ZScriptLexer1(source))
        {
            
        }

        /// <summary>
        /// Executes the parsing step, producing a root node on <see cref="RootNode"/>.
        /// </summary>
        public void Parse()
        {
            RootNode = ParseProgramBody();
        }

        /// <summary>
        /// Parses a program body node.
        /// 
        /// <code>
        /// scriptBody : (functionDefinition | globalVariable | exportDefinition | classDefinition | sequenceBlock | typeAlias)*;
        /// </code>
        /// </summary>
        private ProgramBodyNode ParseProgramBody()
        {
            return new ProgramBodyNode();
        }

        /// <summary>
        /// Parses a unction definition from the source code.
        ///
        /// <code>
        /// functionDefinition : 'func' functionName genericParametersDefinition? functionArguments? returnType? functionBody;
        /// </code>
        /// </summary>
        private FunctionDefinitionNode FunctionDefinition()
        {
            return new FunctionDefinitionNode();
        }
    }
}
