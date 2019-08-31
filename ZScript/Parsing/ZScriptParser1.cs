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
using JetBrains.Annotations;
using ZScript.Parsing.AST;

namespace ZScript.Parsing
{
    /// <summary>
    /// A parser for generating ASTs for the ZScript language
    /// </summary>
    public class ZScriptParser1
    {
        internal ParseNodeContext Context;

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
        /// Defaults to an instance of <see cref="Parsing.Diagnostics"/>.
        /// </summary>
        public IDiagnosticsCollector Diagnostics { get; set; } = new Diagnostics();

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
        public ZScriptParser1([NotNull] ISource source, [NotNull] string sourceCode) : this(new ZScriptLexer1(source, sourceCode))
        {
            
        }

        /// <summary>
        /// Executes the parsing step, producing a root node on <see cref="RootNode"/>.
        /// </summary>
        public void Parse()
        {
            ParseProgramBody();

            RootNode = Context.TopNode;
        }

        /// <summary>
        /// Parses a program body node.
        /// 
        /// <code>
        /// scriptBody : (functionDefinition | globalVariable | exportDefinition | classDefinition | sequenceBlock | typeAlias)*;
        /// </code>
        /// </summary>
        private void ParseProgramBody()
        {
            Context.PushContext<ProgramBodyNode>();



            Context.PopContext();
        }

        /// <summary>
        /// Parses a unction definition from the source code.
        ///
        /// <code>
        /// functionDefinition : 'func' functionName genericParametersDefinition? functionArguments? returnType? functionBody;
        /// </code>
        /// </summary>
        private void FunctionDefinition()
        {
            Context.PushContext<FunctionDefinitionNode>();

            // 'func'
            if (!Lexer.IsCurrentTokenType(LexerTokenType.FunctionKeyword))
            {
                Diagnostics.Error("Expected 'func' keyword before method", Location());
            }
            else
            {
                Context.AddToken(Lexer.Next());
            }

            // functionName
            Identifier("Expected identifier for function name");


            Context.PopContext();
        }

        private void Identifier(string errorMessage = "Expected identifier")
        {
            Context.PushContext<IdentifierNode>();

            // functionName
            if (Lexer.IsCurrentTokenType(LexerTokenType.Identifier))
            {
                var tok = Lexer.Next();
                Context.AddToken(tok);
            }
            else
                Diagnostics.Error(errorMessage, Location());

            Context.PopContext();
        }

        private SourceLocation Location()
        {
            return Lexer.Location();
        }
    }
    
    internal class ParseNodeContext
    {
        private readonly Stack<SyntaxNode> _nodeStack = new Stack<SyntaxNode>();
        
        [CanBeNull]
        public SyntaxNode TopNode => _nodeStack.Count == 0 ? null : _nodeStack.Peek();

        public T PushContext<T>() where T: SyntaxNode, new()
        {
            var node = new T();
            PushContext(node);

            return node;
        }

        public void AddChild([NotNull] SyntaxNode node)
        {
            if(TopNode == null)
                throw new InvalidOperationException("Missing context to add node to. Did you forget to call PushContext()?");

            TopNode?.AddChild(node);
        }

        public void AddToken([NotNull] TokenNode node)
        {
            
            if(TopNode == null)
                throw new InvalidOperationException("Missing context to add node to. Did you forget to call PushContext()?");

            TopNode?.AddToken(node);
        }
        
        public void AddToken(ZScriptToken token)
        {
            AddToken(new TokenNode(token));
        }

        public void PushContext(SyntaxNode node)
        {
            TopNode?.AddChild(node);

            _nodeStack.Push(node);
        }

        public SyntaxNode PopContext()
        {
            return _nodeStack.Pop();
        }
    }
}
