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
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Error listener for parsing of script snippets
    /// </summary>
    public class ZScriptSyntaxErrorListener : BaseErrorListener
    {
        /// <summary>
        /// The container errors will be reported to
        /// </summary>
        private MessageContainer _messageContainer;

        /// <summary>
        /// Gets or sets the container errors will be reported to
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
            set { _messageContainer = value; }
        }

        /// <summary>
        /// Initializes a new instance of the ZScriptErrorListener class
        /// </summary>
        /// <param name="messageContainer">The container errors will be reported to</param>
        public ZScriptSyntaxErrorListener(MessageContainer messageContainer)
        {
            _messageContainer = messageContainer;
        }

        // 
        // SintaxError listener
        // 
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _messageContainer.RegisterSyntaxError(new SyntaxError(line, charPositionInLine, msg) { Token = offendingSymbol });
        }
    }
}