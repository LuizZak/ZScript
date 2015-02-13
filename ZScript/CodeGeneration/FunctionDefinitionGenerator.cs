using System.Collections.Generic;
using ZScript.CodeGeneration.Elements;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Generator that creates function definitions
    /// </summary>
    public static class FunctionDefinitionGenerator
    {
        /// <summary>
        /// Generates a new function definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the function definition from</param>
        /// <returns>A function definition generated from the given context</returns>
        public static FunctionDefinition GenerateFunctionDef(ZScriptParser.FunctionDefinitionContext context)
        {
            var f = new FunctionDefinition(context.functionName().IDENT().GetText(), context.functionBody(),
                CollectFunctionArguments(context.functionArguments()))
            {
                Context = context,
                HasReturnType = context.returnType() != null,
                ReturnTypeContext = context.returnType(),
                IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
            };

            return f;
        }

        /// <summary>
        /// Generates a new closure definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the closure definition from</param>
        /// <returns>A closure definition generated from the given context</returns>
        public static ClosureDefinition GenerateClosureDef(ZScriptParser.ClosureExpressionContext context)
        {
            var args = new FunctionArgumentDefinition[0];

            if (context.functionArg() != null)
            {
                args = new[] { GenerateFunctionArgumentDef(context.functionArg()) };
            }
            else if (context.functionArguments() != null)
            {
                args = CollectFunctionArguments(context.functionArguments());
            }

            var c = new ClosureDefinition("", context.functionBody(), args)
            {
                Context = context,
                HasReturnType = context.returnType() != null,
                ReturnTypeContext = context.returnType(),
                IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
            };

            return c;
        }

        /// <summary>
        /// Generates a new export function definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the export function definition from</param>
        /// <returns>An export function definition generated from the given context</returns>
        public static ExportFunctionDefinition GenerateExportFunctionDef(ZScriptParser.ExportDefinitionContext context)
        {
            var e = new ExportFunctionDefinition(context.functionName().IDENT().GetText(),
                CollectFunctionArguments(context.functionArguments()))
            {
                Context = context,
                HasReturnType = context.returnType() != null,
                ReturnTypeContext = context.returnType(),
                IsVoid = (context.returnType() != null && context.returnType().type().GetText() == "void")
            };

            return e;
        }

        /// <summary>
        /// Generates a new function argument definition from a given function argument context
        /// </summary>
        /// <param name="context">The context containing the function argument to create</param>
        /// <returns>A FunctionArgumentDefinition for the given context</returns>
        public static FunctionArgumentDefinition GenerateFunctionArgumentDef(ZScriptParser.FunctionArgContext context)
        {
            var a = new FunctionArgumentDefinition
            {
                Name = context.argumentName().IDENT().GetText(),
                HasValue = context.compileConstant() != null,
                Context = context
            };

            if (context.type() != null)
            {
                a.HasType = true;
                a.TypeContext = context.type();
            }

            if (context.variadic != null)
            {
                a.IsVariadic = true;
            }
            else if (context.compileConstant() != null)
            {
                a.HasValue = true;
                a.DefaultValue = context.compileConstant();
            }
            return a;
        }

        /// <summary>
        /// Returns an array of function arguments from a given function definition context
        /// </summary>
        /// <param name="context">The context to collect the function arguments from</param>
        /// <returns>An array of function arguments that were collected</returns>
        private static FunctionArgumentDefinition[] CollectFunctionArguments(ZScriptParser.FunctionArgumentsContext context)
        {
            if (context == null || context.argumentList() == null)
                return new FunctionArgumentDefinition[0];

            var argList = context.argumentList().functionArg();
            var args = new FunctionArgumentDefinition[argList.Length];

            int i = 0;
            foreach (var arg in argList)
            {
                var a = GenerateFunctionArgumentDef(arg);

                args[i++] = a;
            }

            return args;
        }
    }
}