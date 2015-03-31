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
        /// The generic signature resolver used during analysis
        /// </summary>
        private readonly GenericSignatureResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the GenericParametersAnalyzer class
        /// </summary>
        /// <param name="context">The context for the analysis</param>
        public GenericSignatureAnalyzer(RuntimeGenerationContext context)
        {
            _context = context;
            _resolver = new GenericSignatureResolver();
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
            
        }
    }

    /// <summary>
    /// Class capable of resolving constraint signatures and raise diagnostics about the validity of signatures
    /// </summary>
    public class GenericSignatureResolver
    {
        /// <summary>
        /// Resolves the given generic signature information object
        /// </summary>
        /// <param name="signature">The generic signature to resolve</param>
        /// <returns>A result for the resolving</returns>
        public ResolvingResult Resolve(GenericSignatureInformation signature)
        {
            // Resolve the constraints
            foreach (var constraint in signature.Constraints)
            {
                // Get the type referenced by the constraint
                var type = signature.GenericTypes.FirstOrDefault(t => t.Name == constraint.TypeName);
                if (type == null)
                {
                    return new ResolvingResult(false, "Generic type T does not exists in current context");
                }

                var baseType = signature.GenericTypes.FirstOrDefault(t => t.Name == constraint.BaseTypeName);
                // Not resolveable at this time
                if (baseType == null)
                    continue;

                // Detect cyclical referencing
                IInheritableTypeDef b = baseType;

                while (b != null)
                {
                    if (type.Equals(b))
                    {
                        return new ResolvingResult(false, "Cyclical referencing of types " + type + " and " + baseType);
                    }

                    b = b.BaseType as IInheritableTypeDef;
                }

                type.BaseType = baseType;
            }

            return new ResolvingResult(true, "");
        }

        /// <summary>
        /// Structure that represents the result of a signature analysis
        /// </summary>
        public struct ResolvingResult
        {
            /// <summary>
            /// Gets a value specifying whether the signature was valid
            /// </summary>
            public readonly bool IsValid;

            /// <summary>
            /// A message containing additional information about a resolving
            /// </summary>
            public readonly string Message;

            /// <summary>
            /// Initializes a new ResolvingResult struct
            /// </summary>
            public ResolvingResult(bool isValid, string message)
            {
                IsValid = isValid;
                Message = message;
            }
        }
    }
}