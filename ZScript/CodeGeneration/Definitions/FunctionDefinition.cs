﻿#region License information
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
using System.Text;

using ZScript.Elements;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a function definition
    /// </summary>
    public class FunctionDefinition : Definition
    {
        /// <summary>
        /// The context containing the function body's statements
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// An array of all the function arguments for this function
        /// </summary>
        private readonly FunctionArgumentDefinition[] _parameters;

        /// <summary>
        /// Cached callable definition
        /// </summary>
        private CallableTypeDef _callableTypeDef;

        /// <summary>
        /// The return type for this function
        /// </summary>
        private TypeDef _returnType;

        /// <summary>
        /// The count of required arguments for the function definition
        /// </summary>
        private readonly int _requiredCount;

        /// <summary>
        /// Gets or sets a value specifying whether this function definition has a return type associated with it
        /// </summary>
        public bool HasReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this function definition has a void return value
        /// </summary>
        public bool IsVoid { get { return ReturnType == null || ReturnType.IsVoid; } }

        /// <summary>
        /// List of return statements present in this function definition
        /// </summary>
        public List<ZScriptParser.ReturnStatementContext> ReturnStatements;

        /// <summary>
        /// Gets the context containing the function body's statements
        /// </summary>
        public ZScriptParser.FunctionBodyContext BodyContext
        {
            get { return _bodyContext; }
        }

        /// <summary>
        /// The generated tokens for the function body.
        /// May be null, if no tokens have been generated for this body
        /// </summary>
        public TokenList Tokens;

        /// <summary>
        /// Gets an array of all the function arguments for this function
        /// </summary>
        public FunctionArgumentDefinition[] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets the minimum number of arguments required for the function call
        /// </summary>
        public int RequiredParametersCount
        {
            get { return _requiredCount; }
        }

        /// ;<summary>
        /// Gets or sets the return type for the function
        /// </summary>
        public TypeDef ReturnType
        {
            get { return _returnType; }
            set
            {
                _returnType = value;
                RecreateCallableDefinition();
            }
        }

        /// <summary>
        /// Gets or sets the return type context for the function
        /// </summary>
        public ZScriptParser.ReturnTypeContext ReturnTypeContext { get; set; }

        /// <summary>
        /// Gets the callable type definition associated with this function definition
        /// </summary>
        public CallableTypeDef CallableTypeDef
        {
            get { return _callableTypeDef; }
        }

        /// <summary>
        /// Initializes a new instance of the FunctionDefinition class
        /// </summary>
        /// <param name="name">The name for the definition</param>
        /// <param name="bodyContext">The context containing the function body's statements</param>
        /// <param name="parameters">The arguments for this function definition</param>
        public FunctionDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] parameters)
        {
            Name = name;
            ReturnStatements = new List<ZScriptParser.ReturnStatementContext>();
            _bodyContext = bodyContext;
            _parameters = parameters;
            _requiredCount = parameters.Count(p => !(p.IsOptional));

            RecreateCallableDefinition();
        }

        /// <summary>
        /// Recreates the callable definition for this function
        /// </summary>
        public void RecreateCallableDefinition()
        {
            _callableTypeDef = new CallableTypeDef(_parameters.Select(a => a.ToArgumentInfo()).ToArray(), ReturnType ?? TypeDef.VoidType, HasReturnType);
        }

        /// <summary>
        /// Returns a string representation of this FunctionDefinition
        /// </summary>
        /// <returns>A string representation of this FunctionDefinition</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Name);

            // Arguments
            builder.Append("(");

            foreach (var parameter in _parameters)
            {
                builder.Append(parameter);
            }

            builder.Append(")");

            // Return type
            if (HasReturnType)
            {
                builder.Append(" : ");
                builder.Append(ReturnType);
            }

            return builder.ToString();
        }
    }
}