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

using Antlr4.Runtime.Sharpen;

using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Analysis.Definitions
{
    /// <summary>
    /// Class capable of analyzing generic signatures
    /// </summary>
    public class GenericSignatureAnalyzer
    {
        /// <summary>
        /// The context for this analysis
        /// </summary>
        private readonly RuntimeGenerationContext _context;

        /// <summary>
        /// Initializes a new instance of the GenericParametersAnalyzer class
        /// </summary>
        /// <param name="context">The context for the analysis</param>
        public GenericSignatureAnalyzer(RuntimeGenerationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Analysis the definitions sotred on a given context
        /// </summary>
        public void Analyze()
        {
            var definitions = _context.BaseScope.GetDefinitionsByTypeRecursive<FunctionDefinition>();

            // Analyze each function parameter individually
            foreach (var funcDef in definitions)
            {
                AnalyzeSignature(funcDef.GenericSignature);
            }
        }

        /// <summary>
        /// Analyzes a given generic signature
        /// </summary>
        /// <param name="signature">The generic signature to analyze</param>
        public void AnalyzeSignature(GenericSignatureInformation signature)
        {
            // Hashset of types constrained in the signature, used to detect duplicated constraints
            var constraints = new HashSet<string>();

            // Resolve the constraints
            foreach (var constraint in signature.Constraints)
            {
                // Get the type referenced by the constraint
                var type = signature.GenericTypes.FirstOrDefault(t => t.Name == constraint.TypeName);
                if (type == null)
                {
                    var message = "Unknown generic type " + constraint.TypeName + " that cannot be constrained in this context";
                    _context.MessageContainer.RegisterError(constraint.Context.genericType(), message, ErrorCode.UnkownGenericTypeName);
                    continue;
                }

                if (!constraints.Add(constraint.TypeName))
                {
                    var message = "Duplicated generic constraint for generic type " + constraint.TypeName;
                    _context.MessageContainer.RegisterError(constraint.Context.genericType(), message, ErrorCode.DupliatedGenericConstraint);
                    continue;
                }

                var baseType = _context.TypeProvider.TypeNamed(constraint.BaseTypeName) as IInheritableTypeDef;
                // Not resolveable at this time
                if (baseType == null)
                {
                    baseType = _context.TypeProvider.TypeNamed(constraint.BaseTypeName) as IInheritableTypeDef;

                    if(baseType == null)
                    {
                        var message = "Unkown type " + constraint.BaseTypeName;
                        _context.MessageContainer.RegisterError(constraint.Context.complexTypeName(), message, ErrorCode.UnkownType);
                        continue;
                    }
                }

                // Detect cyclical referencing
                if (baseType.IsSubtypeOf(type) || type == (baseType as GenericTypeDefinition))
                {
                    var message = "Cyclical generic constraining detected between " + type + " and " + baseType;
                    _context.MessageContainer.RegisterError(constraint.Context, message, ErrorCode.CyclicalGenericConstraint);
                    break;
                }

                type.BaseType = (TypeDef)baseType;
            }
        }
    }
}