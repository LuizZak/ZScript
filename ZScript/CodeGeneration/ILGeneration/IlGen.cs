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
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Definitions;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration.ILGeneration
{
    /// <summary>
    /// A class capable of generating IL from code
    /// </summary>
    public class IlGen
    {
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// Inits a new ILGen instance with the given runtime generation context
        /// </summary>
        public IlGen(RuntimeGenerationContext generationContext)
        {
            _generationContext = generationContext;
        }

        /// <summary>
        /// Returns whether the given function body context can be tokenized into IL.
        /// </summary>
        public bool CanGenerateIl(ZScriptParser.FunctionBodyContext context, [NotNull] FunctionDefinition functionDefinition, CodeScope scope)
        {
            // Cannot convert closures, methods or export functions
            if (functionDefinition is ClosureDefinition || functionDefinition is MethodDefinition ||
                functionDefinition is ExportFunctionDefinition || functionDefinition is SequenceFrameDefinition)
                return false;

            var typeProvider = _generationContext.TypeProvider;

            // Verify native types are available for all parameters and the return type of the function definition
            if (!typeProvider.HasNativeType(functionDefinition.ReturnType))
                return false;
            if (!functionDefinition.Parameters.All(parameter => typeProvider.HasNativeType(parameter.Type)))
                return false;

            // Verify all locals can be defined using native types
            var definitions = scope.GetAllDefinitionsRecursive();
            if (!definitions.OfType<ValueHolderDefinition>().Select(d => d.Type).All(local => typeProvider.HasNativeType(local)))
                return false;

            // Verify all usages are of local variables or parameters of the function on the context
            var usages = scope.GetAllUsagesRecursive();

            foreach (var usage in usages)
            {
                var definition = usage.Definition;

                // Local variable that is local to the function being converted
                if (definition is LocalVariableDefinition local && local.FunctionDefinition == functionDefinition)
                {
                    continue;
                }

                // A function argument that is not referencing another function argument (e.g. closure)
                if (definition is FunctionArgumentDefinition argument && argument.Function == functionDefinition)
                {
                    continue;
                }

                /* TODO: Support function calling.

                // All function calls require proper native types on each parameter as well as the return type, even if it's not used.
                if (definition is FunctionDefinition funcDef && funcDef.Parameters.All(p => typeProvider.HasNativeType(p.Type)))
                {
                    if (funcDef.HasReturnType && typeProvider.HasNativeType(funcDef.ReturnType))
                        continue;
                }
                */
                
                return false;
            }

            return false;
        }
        
        /// <summary>
        /// Generates an IL-assembly instruction set from the given function definition, on a given target type
        /// </summary>
        public MethodBuilder GenerateIl([NotNull] ZScriptParser.FunctionBodyContext context, [NotNull] FunctionDefinition functionDefinition, [NotNull] CodeScope scope, [NotNull] TypeBuilder targetType)
        {
            var returnType = _generationContext.TypeProvider.NativeTypeForTypeDef(functionDefinition.ReturnType);
            var parameterTypes = functionDefinition.Parameters.Select(parameter => _generationContext.TypeProvider.NativeTypeForTypeDef(parameter.Type)).ToArray();

            var method = targetType.DefineMethod(functionDefinition.Name, MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes);

            var gen = method.GetILGenerator();

            var generator = new MethodIlGen(gen, _generationContext, scope, context.blockStatement());

            generator.Generate();
            
            return method;
        }
    }

    internal class MethodIlGen
    {
        private readonly ILGenerator _builder;
        private readonly RuntimeGenerationContext _generationContext;
        private readonly CodeScope _scope;
        private readonly ZScriptParser.BlockStatementContext _context;
        
        public MethodIlGen([NotNull] ILGenerator builder, [NotNull] RuntimeGenerationContext generationContext,
            [NotNull] CodeScope scope, [NotNull] ZScriptParser.BlockStatementContext context)
        {
            _builder = builder;
            _generationContext = generationContext;
            _scope = scope;
            _context = context;
        }

        public void Generate()
        {
            
        }
    }
}
