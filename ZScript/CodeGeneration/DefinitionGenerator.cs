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
using Antlr4.Runtime;
using ZScript.CodeGeneration.Definitions;
using ZScript.Parsing.ANTLR;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Generator that creates definitions of all sorts by deconstructing parser contexts
    /// </summary>
    public static class DefinitionGenerator
    {
        /// <summary>
        /// Generates a new frame sequence definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the frame definition from</param>
        /// <returns>A sequence frame definition generated from the given context</returns>
        public static SequenceFrameDefinition GenerateSequenceFrameDef(ZScriptParser.SequenceFrameContext context)
        {
            var frameName = context.frameName() == null ? "" : context.frameName().GetText();

            var m = new SequenceFrameDefinition(frameName, context.functionBody(), CollectFrameRanges(context.frameRange()))
            {
                Context = context,
                HasReturnType = false,
                IdentifierContext = frameName == "" ? context.frameRange() : (ParserRuleContext)context.frameName()
            };

            return m;
        }

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
                IsOverride = context.@override != null,
                IdentifierContext = context.functionDefinition().functionName()
            };

            context.MethodDefinition = m;

            return m;
        }

        /// <summary>
        /// Generates a new function definition using the given context
        /// </summary>
        /// <param name="context">The context to generate the function definition from</param>
        /// <returns>A function definition generated from the given context</returns>
        public static TopLevelFunctionDefinition GenerateTopLevelFunctionDef(ZScriptParser.FunctionDefinitionContext context)
        {
            var f = new TopLevelFunctionDefinition(context.functionName().IDENT().GetText(), context.functionBody(),
                CollectFunctionArguments(context.functionArguments()), CollectGenericSignature(context.genericParametersDefinition()))
            {
                Context = context,
                HasReturnType = context.returnType() != null,
                ReturnTypeContext = context.returnType(),
                IdentifierContext = context.functionName()
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

            context.Definition = c;

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
                IdentifierContext = context.functionName()
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
                Context = context,
                IdentifierContext = context.argumentName()
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
        /// Returns an array of function arguments from a given function argument list definition context
        /// </summary>
        /// <param name="context">The context to collect the function arguments from</param>
        /// <returns>An array of function arguments that were collected</returns>
        public static FunctionArgumentDefinition[] CollectFunctionArguments(ZScriptParser.FunctionArgumentsContext context)
        {
            if (context?.argumentList() == null)
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
        /// Returns a generic type signature information object from a given generic parameters definition context
        /// </summary>
        /// <param name="context">The context to collect the generic signature information from</param>
        /// <returns>The generic signature information that were collected</returns>
        public static GenericSignatureInformation CollectGenericSignature(ZScriptParser.GenericParametersDefinitionContext context)
        {
            if (context == null)
                return new GenericSignatureInformation();

            // Collect the generic types
            var list = context.genericParameterDefinitionList();
            var types = list.genericType().Select(CollectGenericType).ToArray();
            GenericTypeConstraint[] constraints = new GenericTypeConstraint[0];

            if (context.genericConstraintList() != null)
            {
                constraints = context.genericConstraintList().genericConstraint().Select(CollectGenericTypeConstraint).ToArray();
            }

            return new GenericSignatureInformation(types, constraints);
        }

        /// <summary>
        /// Returns a generic type definition from a given generic parameters definition context
        /// </summary>
        /// <param name="context">The context to collect the generic type from</param>
        /// <returns>The generic type definition that were collected</returns>
        public static GenericTypeDefinition CollectGenericType(ZScriptParser.GenericTypeContext context)
        {
            return new GenericTypeDefinition(context.IDENT().GetText()) { Context = context };
        }

        /// <summary>
        /// Returns a generic type definition from a given generic parameters definition context
        /// </summary>
        /// <param name="context">The context to collect the generic type from</param>
        /// <returns>The generic type definition that were collected</returns>
        public static GenericTypeConstraint CollectGenericTypeConstraint(ZScriptParser.GenericConstraintContext context)
        {
            return new GenericTypeConstraint(context.genericType().IDENT().GetText(), context.complexTypeName().GetText(), context);
        }

        /// <summary>
        /// Returns an array of sequence frame ranges from a given frame range context
        /// </summary>
        /// <param name="context">The context to collect the frame ranges from</param>
        /// <returns>An array of sequence frame ranges that were collected</returns>
        public static SequenceFrameRange[] CollectFrameRanges(ZScriptParser.FrameRangeContext context)
        {
            // If the context is null, return a default '+1' frame range
            if (context == null)
            {
                return new [] { new SequenceFrameRange(true, 1) };
            }

            var elements = context.frameRangeElement();
            var ranges = new SequenceFrameRange[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                bool relative = elements[i].relative != null;
                var frameNumbers = elements[i].frameNumber();

                int startFrame = int.Parse(frameNumbers[0].INT().GetText());

                // Two frames detected: this is a ranged frame range
                if (frameNumbers.Length == 2)
                {
                    var endFrame = int.Parse(frameNumbers[1].INT().GetText());

                    ranges[i] = new SequenceFrameRange(relative, startFrame, endFrame);
                }
                // One frame detected: This is a single frame range
                else
                {
                    ranges[i] = new SequenceFrameRange(relative, startFrame);
                }
            }

            return ranges;
        }

        /// <summary>
        /// Creates a new local variable definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the local variable definition</param>
        /// <returns>A new local variable definition based on the given value holder declaration context</returns>
        public static LocalVariableDefinition GenerateLocalVariable(ZScriptParser.ValueHolderDeclContext context)
        {
            var def = new LocalVariableDefinition();

            FillValueHolderDef(def, context);

            return def;
        }

        /// <summary>
        /// Creates a new local variable definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the local variable definition</param>
        /// <returns>A new local variable definition based on the given value holder declaration context</returns>
        public static LocalVariableDefinition GenerateLocalVariable(ZScriptParser.ValueHolderDefineContext context)
        {
            var def = new LocalVariableDefinition();

            FillValueHolderDef(def, context);

            return def;
        }

        /// <summary>
        /// Creates a new local variable definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the local variable definition</param>
        /// <returns>A new local variable definition based on the given value holder declaration context</returns>
        public static TypeFieldDefinition GenerateClassField(ZScriptParser.ClassFieldContext context)
        {
            // TODO: Oh god what is this
            var def = new TypeFieldDefinition(context.valueDeclareStatement().valueHolderDecl().valueHolderDefine().valueHolderName().memberName().IDENT().GetText());

            FillValueHolderDef(def, context.valueDeclareStatement().valueHolderDecl());

            return def;
        }

        /// <summary>
        /// Creates a new value holder definition from a given value holder context
        /// </summary>
        /// <param name="context">The context containing the global vriable definition</param>
        /// <returns>A new global variable definition based on the given global variable declaration context</returns>
        public static GlobalVariableDefinition GenerateGlobalVariable(ZScriptParser.GlobalVariableContext context)
        {
            GlobalVariableDefinition def = new GlobalVariableDefinition();

            FillValueHolderDef(def, context.valueDeclareStatement().valueHolderDecl());

            def.Context = context.valueDeclareStatement();

            return def;
        }

        /// <summary>
        /// Fills a value holder definition with the contents of a given
        /// </summary>
        /// <param name="def">The definition to fill</param>
        /// <param name="context">The value declaration context that the definition will be filled with</param>
        public static void FillValueHolderDef(ValueHolderDefinition def, ZScriptParser.ValueHolderDeclContext context)
        {
            def.Context = context;

            FillValueHolderDef(def, context.valueHolderDefine());

            def.HasValue = context.expression() != null;
            def.ValueExpression = new Expression(context.expression());
            def.ValueDefineContext = context.valueHolderDefine();

            context.Definition = def;

            if (def.HasType)
            {
                def.TypeContext = context.valueHolderDefine().type();
            }
        }

        /// <summary>
        /// Fills a value holder definition with the contents of a given
        /// </summary>
        /// <param name="def">The definition to fill</param>
        /// <param name="context">The value declaration context that the definition will be filled with</param>
        public static void FillValueHolderDef(ValueHolderDefinition def, ZScriptParser.ValueHolderDefineContext context)
        {
            def.Context = context;

            def.Name = context.valueHolderName().memberName().IDENT().GetText();
            def.Context = context;
            def.HasType = context.type() != null;
            def.IsConstant = context.let != null;
            def.IdentifierContext = context.valueHolderName().memberName();
            def.ValueDefineContext = context;

            if (def.HasType)
            {
                def.TypeContext = context.type();
            }
        }
    }
}