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

using ZScript.CodeGeneration.Definitions;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Generator that creates definitions of all sorts by deconstructing parser contexts
    /// </summary>
    public static class DefinitionGenerator
    {
        /// <summary>
        /// Generates a new method definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the method definition from</param>
        /// <returns>A method definition generated from the given context</returns>
        public static MethodDefinition GenerateMethodDef(ZScriptParser.ClassMethodContext context)
        {
            var m = new MethodDefinition(context.functionDefinition().functionName().IDENT().GetText(), context.functionDefinition().functionBody(),
                CollectFunctionArguments(context.functionDefinition().functionArguments()))
            {
                Context = context,
                HasReturnType = context.functionDefinition().returnType() != null,
                ReturnTypeContext = context.functionDefinition().returnType(),
                IsOverride = context.@override != null
            };

            return m;
        }

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

        /// <summary>
        /// Creates a new value holder definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the value holder definition</param>
        /// <returns>A new value holder definition based on the given value holder declaration context</returns>
        public static ValueHolderDefinition GenerateValueHolderDef(ZScriptParser.ValueHolderDeclContext context)
        {
            var def = new ValueHolderDefinition();

            FillValueHolderDef(def, context);

            return def;
        }

        /// <summary>
        /// Fills a value holder definition with the contents of a given
        /// </summary>
        /// <param name="def">The definition to fill</param>
        /// <param name="context">The value declaration context that the definition will be filled with</param>
        private static void FillValueHolderDef(ValueHolderDefinition def, ZScriptParser.ValueHolderDeclContext context)
        {
            def.Name = context.valueHolderName().memberName().IDENT().GetText();
            def.Context = context;
            def.HasValue = context.expression() != null;
            def.HasType = context.type() != null;
            def.ValueExpression = new Expression(context.expression());
            def.IsConstant = context.let != null;

            if (def.HasType)
            {
                def.TypeContext = context.type();
            }
        }

        /// <summary>
        /// Creates a new value holder definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the value holder definition</param>
        /// <returns>A new value holder definition based on the given value holder declaration context</returns>
        public static GlobalVariableDefinition GenerateGlobalVariable(ZScriptParser.GlobalVariableContext context)
        {
            GlobalVariableDefinition def = new GlobalVariableDefinition();

            FillValueHolderDef(def, context.valueDeclareStatement().valueHolderDecl());

            return def;
        }
    }
}