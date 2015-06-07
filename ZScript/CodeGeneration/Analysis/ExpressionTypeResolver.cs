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
using System.Linq;

using Antlr4.Runtime;

using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Elements;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class capable of resolving the types of expressions
    /// </summary>
    public partial class ExpressionTypeResolver : IContextTypeProvider
    {
        /// <summary>
        /// The type provider using when resolving the type of the expressions
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// Gets the type provider using when resolving the type of the expressions
        /// </summary>
        public TypeProvider TypeProvider => _generationContext.TypeProvider;

        /// <summary>
        /// Gets the container to report error messages to
        /// </summary>
        public MessageContainer MessageContainer => _generationContext.MessageContainer;

        /// <summary>
        /// Gets the definition type provider for the code generation
        /// </summary>
        public IDefinitionTypeProvider DefinitionTypeProvider => _generationContext.DefinitionTypeProvider;

        /// <summary>
        /// Gets or sets the object to notify the closure type matching to
        /// </summary>
        public IClosureExpectedTypeNotifier ClosureExpectedTypeNotifier { get; set; }

        /// <summary>
        /// Initializes a new instance of the ExpressionTypeResolver class
        /// </summary>
        /// <param name="generationContext">The generation context for this expression type resolver</param>
        public ExpressionTypeResolver(RuntimeGenerationContext generationContext)
        {
            _generationContext = generationContext;
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given assignment expression context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            if (context.HasTypeBeenEvaluated)
            {
                return context.EvaluatedType;
            }

            TypeDef variableType = ResolveLeftValue(context.leftValue());
            TypeDef valueType;

            // Find the type of the expression
            if (context.expression() != null)
            {
                // Push expected type
                context.expression().ExpectedType = variableType;

                valueType = ResolveExpression(context.expression());
            }
            else
            {
                valueType = ResolveAssignmentExpression(context.assignmentExpression());
            }

            // Get the operator
            if (!ExpressionUtils.IsCompoundAssignmentOperator(context.assignmentOperator()))
            {
                // Check the type matching
                if (!TypeProvider.CanImplicitCast(valueType, variableType))
                {
                    var message = "Cannot assign value of type " + valueType + " to variable of type " + variableType;
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else
            {
                var op = context.assignmentOperator().GetText()[0].ToString();
                var inst = TokenFactory.InstructionForOperator(op);

                // Allow any on variable types so objects can be accessed freely
                if (!variableType.IsAny && !TypeProvider.BinaryExpressionProvider.CanPerformOperation(inst, variableType, valueType))
                {
                    var message = "Cannot perform " + inst + " operation on values of type " + variableType + " and " + valueType;
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }

            // Check if the left value is not a constant
            if (context.leftValue().IsConstant)
            {
                var message = "Cannot reassign constant value " + context.leftValue().GetText();
                MessageContainer.RegisterError(context, message, ErrorCode.ModifyingConstant);
            }

            context.EvaluatedType = variableType;
            context.HasTypeBeenEvaluated = true;

            return variableType;
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveExpression(ZScriptParser.ExpressionContext context)
        {
            if (context.HasTypeBeenEvaluated)
            {
                return context.EvaluatedType;
            }

            TypeDef retType = null;

            // Assignment expressions
            if (context.constantAtom() != null)
            {
                retType = ResolveConstantAtom(context.constantAtom());
            }
            else if (context.assignmentExpression() != null)
            {
                retType = ResolveAssignmentExpression(context.assignmentExpression());
            }
            // Member name
            else if (context.memberName() != null)
            {
                retType = ResolveMemberName(context.memberName());
            }
            // Literals
            else if (context.closureExpression() != null)
            {
                // Notify of closure inferring
                if (context.ExpectedType != null)
                {
                    ClosureExpectedTypeNotifier?.ClosureTypeMatched(context.closureExpression(), context.ExpectedType);
                }

                retType = ResolveClosureExpression(context.closureExpression());
            }
            else if (context.arrayLiteral() != null)
            {
                var expectedAsList = context.ExpectedType as ListTypeDef;
                if (expectedAsList != null && (context.objectAccess() == null || context.objectAccess().arrayAccess() == null))
                {
                    context.arrayLiteral().ExpectedType = expectedAsList;
                }

                retType = ResolveArrayLiteral(context.arrayLiteral());
            }
            else if (context.arrayLiteralInit() != null)
            {
                var expectedAsList = context.ExpectedType as ListTypeDef;
                if (expectedAsList != null && (context.objectAccess() == null || context.objectAccess().arrayAccess() == null))
                {
                    context.arrayLiteralInit().ExpectedType = expectedAsList;
                }

                retType = ResolveArrayLiteralInit(context.arrayLiteralInit());
            }
            else if (context.dictionaryLiteral() != null)
            {
                var expectedAsList = context.ExpectedType as DictionaryTypeDef;
                if (expectedAsList != null && (context.objectAccess() == null || context.objectAccess().arrayAccess() == null))
                {
                    context.dictionaryLiteral().ExpectedType = expectedAsList;
                }

                retType = ResolveDictionaryLiteral(context.dictionaryLiteral());
            }
            else if (context.dictionaryLiteralInit() != null)
            {
                var expectedAsDict = context.ExpectedType as DictionaryTypeDef;
                if (expectedAsDict != null && (context.objectAccess() == null || context.objectAccess().arrayAccess() == null))
                {
                    context.dictionaryLiteralInit().ExpectedType = expectedAsDict;
                }

                retType = ResolveDictionaryLiteralInit(context.dictionaryLiteralInit());
            }
            else if (context.objectLiteral() != null)
            {
                retType = ResolveObjectLiteral(context.objectLiteral());
            }
            else if (context.tupleExpression() != null)
            {
                var expectedAsTuple = context.ExpectedType as TupleTypeDef;
                if (expectedAsTuple != null && context.objectAccess() == null)
                {
                    context.tupleExpression().ExpectedType = expectedAsTuple;
                }

                retType = ResolveTupleExpression(context.tupleExpression());
            }
            else if (context.tupleLiteralInit() != null)
            {
                retType = ResolveTupleLiteralInit(context.tupleLiteralInit());
            }
            // Type casting/checking
            else if (context.type() != null)
            {
                retType = ResolveExpression(context.expression(0));

                // Type check
                if (context.T_IS() != null)
                {
                    retType = ResolveTypeCheck(retType, context, context.type());
                }
                // Type casting
                else
                {
                    retType = ResolveTypeCasting(retType, context, context.type());
                }
            }
            // Ternary expression
            else if (context.expression().Length == 3)
            {
                retType = ResolveTernaryExpression(context);
            }
            // Binary expression
            else if (context.expression().Length == 2)
            {
                retType = ResolveBinaryExpression(context);
            }
            // Parenthesized expression
            else if (context.expression().Length == 1)
            {
                // Optional unwrapping
                if (context.unwrap != null)
                {
                    retType = ResolveOptionalUnwrapping(context);
                }
                // Unary operation
                else if (context.unaryOperator() != null)
                {
                    retType = ResolveUnaryExpression(context);
                }
            }
            // 'this' priamry expression
            else if (context.T_THIS() != null)
            {
                retType = ResolveThisType(context);
            }
            // 'base' priamry expression
            else if (context.T_BASE() != null)
            {
                retType = ResolveBaseType(context);
            }
            // New type
            else if (context.newExpression() != null)
            {
                retType = ResolveNewExpression(context.newExpression());
            }
            // Prefix operator
            else if (context.prefixOperator() != null)
            {
                retType = ResolvePrefixExpression(context);
            }
            // Postfix operator
            else if (context.postfixOperator() != null)
            {
                retType = ResolvePostfixExpression(context);
            }

            // Value/object access
            if (context.valueAccess() != null)
            {
                retType = ResolveValueAccess(retType, null, context.valueAccess());
            }
            else if (context.objectAccess() != null)
            {
                retType = ResolveObjectAccess(retType, null, context.objectAccess());
            }

            if(retType == null)
            {
                var message = "Cannot resolve type of expression '" + context.GetText() + "'. Make sure the typing is not cyclical or incomplete.";

                MessageContainer.RegisterError(context, message, ErrorCode.UnkownType);
                retType = TypeProvider.AnyType();
            }

            context.EvaluatedType = retType;

            if (context.ExpectedType != null)
            {
                context.ImplicitCastType = context.ExpectedType;
            }

            context.HasTypeBeenEvaluated = true;

            return retType;
        }

        /// <summary>
        /// Resolves a new expression
        /// </summary>
        /// <param name="context">The context containing the new expression</param>
        /// <returns>The type for the new expression</returns>
        public TypeDef ResolveNewExpression(ZScriptParser.NewExpressionContext context)
        {
            var typeName = context.typeName().GetText();
            var type =  TypeProvider.TypeNamed(typeName);

            ResolveTupleExpression(context.tupleExpression(), false);

            if (type == null)
            {
                var message = "Unknown type name '" + typeName + "'";
                _generationContext.MessageContainer.RegisterError(context.typeName(), message, ErrorCode.UnkownType);
            }

            return type ?? TypeProvider.AnyType();
        }

        #region Primary expressions

        /// <summary>
        /// Resolves the type of a 'this' expression contained within a given context
        /// </summary>
        /// <param name="context">The context containing the 'this' target</param>
        /// <exception cref="Exception">No definition type provider defined in this expression type resolver</exception>
        /// <returns>The type for the 'this' value</returns>
        public TypeDef ResolveThisType(ParserRuleContext context)
        {
            if (DefinitionTypeProvider == null)
                throw new Exception("No definition type provider exists on the context provided when constructing this ExpressionTypeResolver. No member name can be resolved to a type!");

            return DefinitionTypeProvider.TypeForThis(context);
        }

        /// <summary>
        /// Resolves the type of a 'base' expression contained within a given context
        /// </summary>
        /// <param name="context">The context containing the 'base' target</param>
        /// <exception cref="Exception">No definition type provider defined in this expression type resolver</exception>
        /// <returns>The type for the 'base' value</returns>
        public TypeDef ResolveBaseType(ParserRuleContext context)
        {
            if (DefinitionTypeProvider == null)
                throw new Exception("No definition type provider exists on the context provided when constructing this ExpressionTypeResolver. No member name can be resolved to a type!");

            if (!DefinitionTypeProvider.HasBaseTarget(context))
            {
                const string message = "There's no base method to target with the 'base' keyword";
                MessageContainer.RegisterError(context, message, ErrorCode.NoBaseTarget);
            }

            return DefinitionTypeProvider.TypeForBase(context);
        }

        #endregion

        #region Prefix, Postfix and Unary

        /// <summary>
        /// Resolves a prefix expression type contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the prefix operation to evaluate</param>
        /// <returns>A type resolved from the context</returns>
        public TypeDef ResolvePrefixExpression(ZScriptParser.ExpressionContext context)
        {
            // Get the type of the left value
            TypeDef leftValueType = ResolveLeftValue(context.leftValue());

            if (!TypeProvider.BinaryExpressionProvider.IsNumeric(leftValueType) && !leftValueType.IsAny)
            {
                MessageContainer.RegisterError(context.Start.Line, context.Start.Column, "Cannot perform prefix operation on values of type " + leftValueType);
            }
            // Check if the left value is not a constant
            if (context.leftValue().IsConstant)
            {
                var message = "Cannot reassign constant value " + context.leftValue().GetText();
                MessageContainer.RegisterError(context, message, ErrorCode.ModifyingConstant);
            }

            return leftValueType;
        }

        /// <summary>
        /// Resolves a postfix expression type contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the postfix operation to evaluate</param>
        /// <returns>A type resolved from the context</returns>
        public TypeDef ResolvePostfixExpression(ZScriptParser.ExpressionContext context)
        {
            // Get the type of the left value
            TypeDef leftValueType = ResolveLeftValue(context.leftValue());

            if (!TypeProvider.BinaryExpressionProvider.IsNumeric(leftValueType) && !leftValueType.IsAny)
            {
                MessageContainer.RegisterError(context.Start.Line, context.Start.Column, "Cannot perform postfix operation on values of type " + leftValueType);
            }
            // Check if the left value is not a constant
            if (context.leftValue().IsConstant)
            {
                var message = "Cannot reassign constant value " + context.leftValue().GetText();
                MessageContainer.RegisterError(context, message, ErrorCode.ModifyingConstant);
            }

            return leftValueType;
        }

        /// <summary>
        /// Resolves an unary expression type contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the unary operation to evaluate</param>
        /// <returns>A type resolved from the context</returns>
        public TypeDef ResolveUnaryExpression(ZScriptParser.ExpressionContext context)
        {
            var unary = context.unaryOperator();
            var expType = ResolveExpression(context.expression()[0]);

            VmInstruction inst;

            switch (unary.GetText())
            {
                case "-":
                    inst = VmInstruction.ArithmeticNegate;
                    break;
                case "!":
                    inst = VmInstruction.LogicalNegate;
                    break;
                default:
                    throw new Exception("Unrecognized unary operator " + unary.GetText());
            }

            if (TypeProvider.BinaryExpressionProvider.CanUnary(expType, inst))
                return TypeProvider.BinaryExpressionProvider.TypeForUnary(expType, inst);

            var message = "Cannot apply " + inst + " to value of type " + expType;
            MessageContainer.RegisterError(context, message, ErrorCode.InvalidTypesOnOperation);

            return TypeProvider.AnyType();
        }

        #endregion

        #region Ternary

        /// <summary>
        /// Resolves a ternary expression contained within a given expression context.
        /// The resolved expression is the most common super type of the left and right expressions of the ternary operation
        /// </summary>
        /// <param name="context">The context containing the ternary expression to resolve</param>
        /// <returns>The type definition for the expressions of the ternary</returns>
        public TypeDef ResolveTernaryExpression(ZScriptParser.ExpressionContext context)
        {
            context.expression(0).ExpectedType = TypeProvider.BooleanType();

            // Conditional expression
            var condExp = ResolveExpression(context.expression(0));

            if (!TypeProvider.CanImplicitCast(condExp, TypeProvider.BooleanType()))
            {
                const string message = "Expected boolean type opeartion on tearnary condition";
                MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
            }

            // Find type of expressions on both sides
            var type1 = ResolveExpression(context.expression(1));
            var type2 = ResolveExpression(context.expression(2));

            var commonType = TypeProvider.FindCommonType(type1, type2);

            // Adjust expected types
            context.expression(1).ExpectedType = commonType;
            context.expression(2).ExpectedType = commonType;

            return commonType;
        }

        #endregion

        #region Tuples

        /// <summary>
        /// Resolves the type of a given tuple expression
        /// </summary>
        /// <param name="context">The context containing the tuple to resolve</param>
        /// <param name="collapseSimpleExpression">
        /// Whether to collapse single-entry tuples and return the type of the inner expression. If this value is true, passing a tuple with 0 entries
        /// returns a Void type, and passing a tuple with 1 type returns a TypeDef for the contained expression.
        /// </param>
        /// <returns>The type for the tuple expression</returns>
        public TypeDef ResolveTupleExpression(ZScriptParser.TupleExpressionContext context, bool collapseSimpleExpression = true)
        {
            var expectedTuple = context.ExpectedType;
            var entries = context.tupleEntry();
            
            // Single tuple: return inner type
            if (collapseSimpleExpression)
            {
                if (entries.Length == 0)
                    return TypeProvider.VoidType();
                if (entries.Length == 1)
                    return ResolveExpression(entries[0].expression());
            }

            var names = new string[entries.Length];
            var innerTypes = new TypeDef[entries.Length];

            // Infer types
            if (expectedTuple == null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    names[i] = entries[i].IDENT() == null ? null : entries[i].IDENT().GetText();
                    innerTypes[i] = ResolveExpression(entries[i].expression());
                }

                return context.TupleType = TypeProvider.TupleForTypes(names, innerTypes);
            }

            innerTypes = expectedTuple.InnerTypes;

            var canConvert = true;
            for (int i = 0; i < entries.Length; i++)
            {
                names[i] = entries[i].IDENT() == null ? null : entries[i].IDENT().GetText();
                entries[i].expression().ExpectedType = expectedTuple.InnerTypes[i];

                var type = ResolveExpression(entries[i].expression());

                if (!TypeProvider.CanImplicitCast(type, expectedTuple.InnerTypes[i]))
                {
                    canConvert = false;
                }
            }

            var returnTuple = TypeProvider.TupleForTypes(names, innerTypes);

            if (!canConvert)
            {
                var message = "Cannot implicitly convert source tuple type " + returnTuple + " to target type " + expectedTuple;
                MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);

                returnTuple = TypeProvider.TupleForTypes(innerTypes.Select(i => TypeProvider.AnyType()).ToArray());
            }

            return context.TupleType = returnTuple;
        }

        /// <summary>
        /// Resolves the type of a givne tuple access
        /// </summary>
        /// <param name="type">The tuple type to analyze</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context containing the tuple access to perform</param>
        /// <param name="resType">The type that was resolved for the tuple access</param>
        /// <returns>The type for the tuple entry being accessed</returns>
        public void ResolveTupleAccess(TypeDef type, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.TupleAccessContext context, ref TypeDef resType)
        {
            // Can only access tuples by field
            var tuple = type as TupleTypeDef;
            if (tuple == null)
            {
                RegisterInvalidTupleAccess(type, context);
                return;
            }

            var field = tuple.InnerTypes.ElementAtOrDefault(int.Parse(context.INT().GetText()));
            if (field == null)
            {
                RegisterUndefinedTupleIndex(type, context, context.INT().GetText());
                return;
            }

            resType = field;
        }

        #endregion

        #region Optional

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveOptionalUnwrapping(ZScriptParser.ExpressionContext context)
        {
            // Resolve inner type
            var inner = ResolveExpression(context.expression(0));

            var opt = inner as OptionalTypeDef;
            if (opt != null)
            {
                return opt.WrappedType;
            }
            
            RegisterNonOptionalUnwrapping(inner, context.expression(0));

            return TypeProvider.AnyType();
        }

        #endregion

        #region Null coalescing

        /// <summary>
        /// Resolves a null coalescing expression contained within a given expression context.
        /// The resolved expression is the most common super type of the left and right expressions of the null coalescing operation
        /// </summary>
        /// <param name="context">The context containing the ternary expression to resolve</param>
        /// <returns>The type definition for the expressions of the ternary</returns>
        public TypeDef ResolveNullCoalescingExpression(ZScriptParser.ExpressionContext context)
        {
            // Find type of expressions on both sides
            var type1 = ResolveExpression(context.expression(0));
            var type2 = ResolveExpression(context.expression(1));

            // Type1 is constantly null
            if (type1 == TypeProvider.NullType())
            {
                return type2;
            }

            string message = "Cannot apply null-coalesce operator between values of type " + type1 + " and " + type2;

            var opt1 = type1 as OptionalTypeDef;
            if (opt1 == null)
            {
                RegisterNonOptionalNullCoalesceLeftSide(context);
                return type1;
            }
            var opt2 = type2 as OptionalTypeDef;
            if (opt2 != null)
            {
                if (opt1.BaseWrappedType != opt2.BaseWrappedType)
                {
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidTypesOnOperation);
                    return TypeProvider.AnyType();
                }
            }
            else
            {
                if (opt1.BaseWrappedType != type2)
                {
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidTypesOnOperation);
                    return TypeProvider.AnyType();
                }
            }

            var leftType = opt1.WrappedType;
            var result = TypeProvider.FindCommonType(leftType, type2);

            // Adjust expected types
            context.expression(0).ExpectedType = result;
            context.expression(1).ExpectedType = result;

            context.expression(1).ImplicitCastType = result;

            return result;
        }

        #endregion

        #region Left value resolving

        /// <summary>
        /// Resolves a left value type
        /// </summary>
        /// <param name="context">The context containing the left value to resolve</param>
        /// <returns>The type for the left value</returns>
        public TypeDef ResolveLeftValue(ZScriptParser.LeftValueContext context)
        {
            var memberName = context.memberName();
            TypeDef type;
            
            if(memberName != null)
            {
                type = ResolveMemberName(memberName);
            }
            else
            {
                type = ResolveThisType(context);
            }

            context.IsConstant = memberName == null || memberName.IsConstant;

            // Evaluate the access
            if (context.leftValueAccess() != null)
            {
                type = ResolveLeftValueAccess(type, context, context.leftValueAccess());
            }
            
            return type;
        }

        /// <summary>
        /// Resolves a member name type
        /// </summary>
        /// <param name="context">The context containing the left value to resolve</param>
        /// <returns>The type for the left value</returns>
        public TypeDef ResolveMemberName(ZScriptParser.MemberNameContext context)
        {
            if(DefinitionTypeProvider == null)
                throw new Exception("No definition type provider exists on the context provided when constructing this ExpressionTypeResolver. No member name can be resolved to a type!");

            return DefinitionTypeProvider.TypeForDefinition(context, context.IDENT().GetText());
        }

        /// <summary>
        /// Resolves a left value access
        /// </summary>
        /// <param name="leftValue">The type of the value being accessed</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context containing the left value access to resolve</param>
        /// <returns>The type for the left value access</returns>
        public TypeDef ResolveLeftValueAccess(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.LeftValueAccessContext context)
        {
            // leftValueAccess : (funcCallArguments leftValueAccess) | (fieldAccess leftValueAccess?) | (arrayAccess leftValueAccess?);
            var type = TypeProvider.AnyType();

            // Unwrap optional
            if (context.unwrap != null)
            {
                if (leftValue is OptionalTypeDef)
                {
                    leftValue = ((OptionalTypeDef)leftValue).WrappedType;
                }
                else
                {
                    RegisterNonOptionalUnwrapping(leftValue, leftValueContext);
                }
            }

            if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, leftValueContext, context.functionCall(), ref type);

                return ResolveLeftValueAccess(type, leftValueContext, context.leftValueAccess());
            }

            if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref type);
            }
            else if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, leftValueContext, context.arrayAccess(), ref type);
            }
            else if (context.tupleAccess() != null)
            {
                ResolveTupleAccess(leftValue, leftValueContext, context.tupleAccess(), ref type);
            }

            if (context.leftValueAccess() != null)
            {
                type = ResolveLeftValueAccess(type, leftValueContext, context.leftValueAccess());
            }

            return type;
        }

        #endregion

        #region Accessing

        /// <summary>
        /// Resolves a value access, using a given left value as a starting point for the evaluation
        /// </summary>
        /// <param name="leftValue">The left value to access the value from</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context that contains the value access</param>
        /// <returns>A type resolved from the value access</returns>
        public TypeDef ResolveValueAccess(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.ValueAccessContext context)
        {
            var type = TypeProvider.AnyType();
            // Unwrap optional
            for (int i = 0; i < context.T_NULL_CONDITIONAL().Length; i++)
            {
                if (leftValue is OptionalTypeDef)
                {
                    leftValue = ((OptionalTypeDef)leftValue).WrappedType;
                }
                else
                {
                    RegisterNonOptionalUnwrapping(leftValue, context);
                    return TypeProvider.AnyType();
                }
            }

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, leftValueContext, context.arrayAccess(), ref type);
            }
            else if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, leftValueContext, context.functionCall(), ref type);
            }
            else if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref type);
            }
            else if (context.tupleAccess() != null)
            {
                ResolveTupleAccess(leftValue, leftValueContext, context.tupleAccess(), ref type);
            }

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(type, leftValueContext, context.valueAccess());
            }

            return context.nullable != null ? TypeProvider.OptionalTypeForType(type) : type;
        }

        /// <summary>
        /// Resolves a value access, using a given left value as a starting point for the evaluation
        /// </summary>
        /// <param name="leftValue">The left value to access the value from</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context that contains the value access</param>
        /// <returns>A type resolved from the value access</returns>
        public TypeDef ResolveObjectAccess(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.ObjectAccessContext context)
        {
            TypeDef resType = TypeProvider.AnyType();

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, leftValueContext, context.arrayAccess(), ref resType);
            }
            else if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref resType);
            }
            else if (context.tupleAccess() != null)
            {
                ResolveTupleAccess(leftValue, leftValueContext, context.tupleAccess(), ref resType);
            }

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(resType, leftValueContext, context.valueAccess());
            }

            return resType;
        }

        /// <summary>
        /// Resolves a field access of a given left value, using the given field access as context
        /// </summary>
        /// <param name="leftValue">The left value to get</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context containing the field access to fetch</param>
        /// <param name="resType">The type to update the resulting field access type to</param>
        private void ResolveFieldAccess(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.FieldAccessContext context, ref TypeDef resType)
        {
            // Object and 'any' types always resolve to 'any'
            if (leftValue == TypeProvider.ObjectType())
            {
                resType = TypeProvider.OptionalTypeForType(TypeProvider.AnyType());
                return;
            }
            if (leftValue == TypeProvider.AnyType())
            {
                resType = TypeProvider.AnyType();
                return;
            }

            string memberName = context.memberName().IDENT().GetText();

            // Try to get the field info
            var memberInfo = leftValue.GetMember(memberName);
            if (memberInfo is TypeFieldDef)
            {
                resType = (memberInfo as TypeFieldDef).FieldType;

                // Mark tuple access
                var tuple = leftValue as TupleTypeDef;
                if (tuple != null)
                {
                    context.IsTupleAccess = true;
                    context.TupleIndex = tuple.IndexOfLabel(memberName);
                }
                else
                {
                    // Update constant flag
                    if (leftValueContext != null)
                        leftValueContext.IsConstant = (memberInfo as TypeFieldDef).Readonly;
                }
            }
            else if (memberInfo is TypeMethodDef)
            {
                resType = (memberInfo as TypeMethodDef).CallableTypeDef();

                // Update constant flag
                if (leftValueContext != null)
                    leftValueContext.IsConstant = true;
            }

            if(memberInfo == null)
                RegisterUndefinedMember(leftValue, context, memberName);
        }

        /// <summary>
        /// Resolves a function call of a given left value, using the given function call as context.
        /// This method raises a warning when the value is not an explicitly callable type
        /// </summary>
        /// <param name="leftValue">The type of the left value</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context of the function call</param>
        /// <param name="resType">The type to update the resulting function call return type to</param>
        private void ResolveFunctionCall(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.FunctionCallContext context, ref TypeDef resType)
        {
            var callableType = leftValue as ICallableTypeDef;
            if (callableType != null)
            {
                // Analyze type of the parameters
                ResolveFunctionCallArguments(callableType, context.tupleExpression());

                context.CallableSignature = callableType;

                resType = callableType.ReturnType;
            }
            else if (!leftValue.IsAny)
            {
                RegisterInvalidFunctionCall(leftValue, context);
            }
            else
            {
                ResolveTupleExpression(context.tupleExpression());
            }

            // Update constant flag
            if (leftValueContext != null)
                leftValueContext.IsConstant = true;
        }

        /// <summary>
        /// Returns an array of types that describe the type of the arguments contained within a given function call arguments context
        /// </summary>
        /// <param name="callableType">A callable type used to verify the argument types correctly</param>
        /// <param name="context">The context containing the function call arguments</param>
        /// <returns>An array of types related to the function call</returns>
        public TypeDef[] ResolveFunctionCallArguments(ICallableTypeDef callableType, ZScriptParser.TupleExpressionContext context)
        {
            // Collect the list of arguments
            var argTypes = new List<TypeDef>();
            context.ExpectedType = callableType.ParameterTuple;
            //var type = (TupleTypeDef)ResolveTupleExpression(context, false);

            int argCount = context.tupleEntry().Length;//type.InnerTypes.Length;

            // Whether the count of arguments is mismatched of the expected argument count
            bool mismatchedCount = false;

            // Verify argument count
            if (argCount < callableType.RequiredArgumentsCount)
            {
                var message = "Trying to pass " + argCount + " arguments to callable that requires at least " + callableType.RequiredArgumentsCount;
                MessageContainer.RegisterError(context, message, ErrorCode.TooFewArguments);
                mismatchedCount = true;
            }
            if (argCount > callableType.MaximumArgumentsCount)
            {
                var message = "Trying to pass " + argCount + " arguments to callable that accepts at most " + callableType.MaximumArgumentsCount;
                MessageContainer.RegisterError(context, message, ErrorCode.TooManyArguments);
                mismatchedCount = true;
            }

            if (callableType.ParameterInfos.Length > 0)
            {
                int ci = 0;
                var curArgInfo = callableType.ParameterInfos[ci];
                bool variadicMatched = false;
                foreach (var entry in context.tupleEntry())
                {
                    if (variadicMatched)
                    {
                        var message = "Trying to pass an argument after a variadic array";
                        MessageContainer.RegisterError(context, message, ErrorCode.TooManyArguments);
                        mismatchedCount = true;
                    }

                    var exp = entry.expression();

                    // Set expected type
                    if (!mismatchedCount)
                        exp.ExpectedType = curArgInfo.RawParameterType;

                    var argType = ResolveExpression(exp);
                    argTypes.Add(argType);

                    if (mismatchedCount)
                        continue;

                    // Match the argument types
                    var matchType = TypeProvider.AreTypesCompatible(argType, curArgInfo.ParameterType) && !TypeProvider.AreTypesCompatible(argType, curArgInfo.RawParameterType);
                    if (!matchType && !TypeProvider.CanImplicitCast(argType, curArgInfo.RawParameterType))
                    {
                        var message = "Cannot implicitly cast argument type " + argType + " to parameter type " + curArgInfo.RawParameterType;
                        MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                    }

                    if (matchType && curArgInfo.IsVariadic)
                    {
                        variadicMatched = true;

                        // Re-evaluate expression, now that the argument is supposed to be an explicit variadic array
                        exp.ExpectedType = curArgInfo.ParameterType;
                        exp.HasTypeBeenEvaluated = false;
                        ResolveExpression(exp);
                    }

                    // Jump to the next callable argument information
                    if (!curArgInfo.IsVariadic && ci < callableType.ParameterInfos.Length - 1)
                    {
                        curArgInfo = callableType.ParameterInfos[++ci];
                    }
                }
            }

            //return type.InnerTypes;
            return argTypes.ToArray();
        }

        /// <summary>
        /// Resolves subscription of a given left value, using the given array access as context.
        /// This method raises a warning when the value is not an explicitly subscriptable type
        /// </summary>
        /// <param name="leftValue">The type of the value</param>
        /// <param name="leftValueContext">The base context for the left value being analyzed</param>
        /// <param name="context">The context of the subscription</param>
        /// <param name="resType">The type to update the resulting subscripting type to</param>
        private void ResolveSubscript(TypeDef leftValue, ZScriptParser.LeftValueContext leftValueContext, ZScriptParser.ArrayAccessContext context, ref TypeDef resType)
        {
            // Get the type of object being subscripted
            var listType = leftValue as IListTypeDef;
            if (listType != null)
            {
                resType = listType.EnclosingType;

                // Check the subscript type
                var subType = listType.SubscriptType;

                // Set the expected type, so implicit type conversions can take place
                context.expression().ExpectedType = subType;

                var accessValue = ResolveExpression(context.expression());

                if (!TypeProvider.CanImplicitCast(accessValue, subType))
                {
                    var message = "Subscriptable type " + leftValue + " expects type " + subType + " for subscription but received " + accessValue + ".";
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else if (!leftValue.IsAny)
            {
                RegisterInvalidSubscript(leftValue, context);
            }
            else
            {
                ResolveExpression(context.expression());
            }

            // Update constant flag
            if (leftValueContext != null)
                leftValueContext.IsConstant = false;
        }

        #endregion

        #region Type casting/checking

        /// <summary>
        /// Resolves a type cast attempt, retuning the resulting type
        /// </summary>
        /// <param name="valueType">The type of the value trying to be cast</param>
        /// <param name="origin">The origin of the value type tring to be cast</param>
        /// <param name="castType">The type tring to cast the value as</param>
        /// <returns>A type definition that represents the casted type</returns>
        public TypeDef ResolveTypeCasting(TypeDef valueType, ZScriptParser.ExpressionContext origin, ZScriptParser.TypeContext castType)
        {
            TypeDef target = ResolveType(castType, false);
            if (!TypeProvider.CanExplicitCast(valueType, target))
            {
                var message = "Invalid cast: cannot cast objects of type " + valueType + " to type " + target;
                MessageContainer.RegisterError(origin.Start.Line, origin.Start.Column, message, ErrorCode.InvalidCast, origin);
            }

            return target;
        }

        /// <summary>
        /// Resolves a type check ('is' operator) attempt, retuning the resulting type
        /// </summary>
        /// <param name="valueType">The type of the value trying to be checked</param>
        /// <param name="origin">The origin of the value type tring to be checked</param>
        /// <param name="typeToCheck">The type tring to check the value as</param>
        /// <returns>A type definition that represents the result of the operation</returns>
        public TypeDef ResolveTypeCheck(TypeDef valueType, ZScriptParser.ExpressionContext origin, ZScriptParser.TypeContext typeToCheck)
        {
            // Resolve the type
            ResolveType(typeToCheck, true);
            return TypeProvider.BooleanType();
        }

        #endregion

        #region Closure

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public CallableTypeDef ResolveClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            if (context.IsInferred)
            {
                return context.Definition.CallableTypeDef;
            }

            var parameters = new List<FunctionArgumentDefinition>();
            var hasReturnType = context.returnType() != null;

            // Iterate through each parameter type for the closure
            if (context.functionArg() != null)
            {
                var t = DefinitionGenerator.GenerateFunctionArgumentDef(context.functionArg());

                // Resolve the type, if available
                t.Type = ResolveFunctionArgument(context.functionArg());

                parameters.Add(t);
            }
            else if(context.functionArguments().argumentList() != null)
            {
                var args = context.functionArguments().argumentList().functionArg();
                foreach (var arg in args)
                {
                    var t = DefinitionGenerator.GenerateFunctionArgumentDef(arg);

                    // Resolve the type, if available
                    t.Type = ResolveFunctionArgument(arg);

                    parameters.Add(t);
                }
            }

            TypeDef returnType;

            // Check return type now
            if (hasReturnType && context.returnType() != null)
            {
                returnType = ResolveType(context.returnType().type(), true);
            }
            else if (context.Definition != null)
            {
                returnType = context.Definition.ReturnType ?? TypeProvider.VoidType();
            }
            else
            {
                returnType = TypeProvider.VoidType();
            }
            
            if (context.Definition != null && context.Definition.HasReturnType && context.Definition.ReturnType != null)
            {
                returnType = context.Definition.ReturnType;
            }

            context.IsInferred = true;

            return new CallableTypeDef(parameters.Select(a => a.ToArgumentInfo()).ToArray(), returnType, hasReturnType);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveFunctionArgument(ZScriptParser.FunctionArgContext context)
        {
            var type = TypeProvider.AnyType();
            TypeDef defaultValueType = null;

            if (context.compileConstant() != null)
            {
                // Check default type, instead
                defaultValueType = ResolveCompileConstant(context.compileConstant());
            }

            // Type provided
            if (context.type() != null)
            {
                type = ResolveType(context.type(), false);

                // Check default value and parameter value compatibility
                if (defaultValueType != null && !TypeProvider.CanImplicitCast(type, defaultValueType))
                {
                    var message = "Cannot implicitly cast default value type " + defaultValueType + " to expected parameter type " + type;
                    MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else if (defaultValueType != null)
            {
                // Check default type, instead
                type = defaultValueType;
            }

            // Variadic parameter
            if (context.variadic != null)
            {
                type = TypeProvider.ListForType(type);
            }

            return type;
        }

        #endregion

        #region Literals

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public ObjectTypeDef ResolveObjectLiteral(ZScriptParser.ObjectLiteralContext context)
        {
            var entry = context.objectEntryList();

            if (entry != null)
            {
                foreach (var def in entry.objectEntryDefinition())
                {
                    ResolveExpression(def.expression());
                }
            }

            return TypeProvider.ObjectType();
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public ListTypeDef ResolveArrayLiteral(ZScriptParser.ArrayLiteralContext context)
        {
            // Expected type for the list
            var expectedValueType = context.ExpectedType?.EnclosingType;

            // Try to infer the type of items in the array
            var listItemsType = expectedValueType ?? TypeProvider.AnyType();

            var items = context.expressionList();

            if (items == null)
            {
                context.EvaluatedValueType = listItemsType;

                return TypeProvider.ListForType(listItemsType);
            }

            // Type is supposed to be inferred from the array's contents
            if (expectedValueType == null)
            {
                bool inferredOne = false;
                foreach (var exp in items.expression())
                {
                    var itemType = ResolveExpression(exp);
                    if (!inferredOne)
                    {
                        listItemsType = itemType;
                        inferredOne = true;
                        continue;
                    }

                    listItemsType = TypeProvider.FindCommonType(itemType, listItemsType);
                }

                foreach(var exp in items.expression())
                {
                    exp.ExpectedType = exp.ImplicitCastType = listItemsType;
                }

                context.EvaluatedValueType = listItemsType;

                return TypeProvider.ListForType(listItemsType);
            }

            // Check type compatibility
            bool canImplicit = true;
            foreach (var exp in items.expression())
            {
                exp.ExpectedType = exp.ImplicitCastType = expectedValueType;

                var itemType = ResolveExpression(exp);

                if (!TypeProvider.CanImplicitCast(itemType, expectedValueType))
                {
                    canImplicit = false;
                }
            }

            // Report an error if the types in the list cannot be implicitly cast to the list type
            if (!canImplicit)
            {
                var message = "Cannot implicitly convert source list type to target type " + expectedValueType;
                MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);

                context.EvaluatedValueType = TypeProvider.AnyType();

                return TypeProvider.ListForType(TypeProvider.AnyType());
            }

            context.EvaluatedValueType = expectedValueType;

            return context.ImplicitCastType = TypeProvider.ListForType(expectedValueType);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public ListTypeDef ResolveArrayLiteralInit(ZScriptParser.ArrayLiteralInitContext context)
        {
            var type = ResolveType(context.type(), false);

            context.EvaluatedValueType = type;

            TypeDef outType = TypeProvider.AnyType();
            ResolveFunctionCall(TypeProvider.AnyType(), null, context.functionCall(), ref outType);

            return TypeProvider.ListForType(type);
        }

        /// <summary>
        /// Returns a DictionaryTypeDef describing the type for a given dictionary literal context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The dictionary type for the context</returns>
        public DictionaryTypeDef ResolveDictionaryLiteral(ZScriptParser.DictionaryLiteralContext context)
        {
            // Expected type for the list
            var expectedValueType = context.ExpectedType?.EnclosingType;
            var expectedKeyType = context.ExpectedType?.SubscriptType;

            // Try to infer the type of items in the array
            var dictKeyType = expectedKeyType ?? TypeProvider.AnyType();
            var dictValueType = expectedValueType ?? TypeProvider.AnyType();

            var entries = context.dictionaryEntryList().dictionaryEntry();

            if (entries.Length == 0)
            {
                context.EvaluatedKeyType = dictKeyType;
                context.EvaluatedValueType = dictValueType;

                return TypeProvider.DictionaryForTypes(dictKeyType, dictValueType);
            }

            // Type is supposed to be inferred from the array's contents
            if (expectedValueType == null || expectedKeyType == null)
            {
                bool inferredOne = false;
                foreach (var exp in entries)
                {
                    var keyType = ResolveExpression(exp.expression(0));
                    var valueType = ResolveExpression(exp.expression(1));
                    if (!inferredOne)
                    {
                        dictKeyType = keyType;
                        dictValueType = valueType;
                        inferredOne = true;
                        continue;
                    }

                    dictKeyType = TypeProvider.FindCommonType(keyType, dictKeyType);
                    dictValueType = TypeProvider.FindCommonType(valueType, dictValueType);
                }

                // Update implicit casting now
                foreach (var exp in entries)
                {
                    exp.expression(0).ExpectedType = exp.expression(0).ImplicitCastType = dictKeyType;
                    exp.expression(1).ExpectedType = exp.expression(1).ImplicitCastType = dictValueType;
                }

                context.EvaluatedKeyType = dictKeyType;
                context.EvaluatedValueType = dictValueType;

                return TypeProvider.DictionaryForTypes(dictKeyType, dictValueType);
            }

            // Check type compatibility
            bool canImplicit = true;
            foreach (var exp in entries)
            {
                exp.expression(0).ExpectedType = exp.expression(0).ImplicitCastType = expectedKeyType;
                exp.expression(1).ExpectedType = exp.expression(1).ImplicitCastType = expectedValueType;

                var keyType = ResolveExpression(exp.expression(0));
                var valueType = ResolveExpression(exp.expression(1));

                if (!TypeProvider.CanImplicitCast(keyType, expectedKeyType) || !TypeProvider.CanImplicitCast(valueType, expectedValueType))
                {
                    canImplicit = false;
                }
            }

            // Report an error if the types in the list cannot be implicitly cast to the list type
            if (!canImplicit)
            {
                var message = "Cannot implicitly convert source dictionary type to target type " + TypeProvider.DictionaryForTypes(expectedKeyType, expectedValueType);
                MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);

                context.EvaluatedKeyType = TypeProvider.AnyType();
                context.EvaluatedValueType = TypeProvider.AnyType();

                return TypeProvider.DictionaryForTypes(TypeProvider.AnyType(), TypeProvider.AnyType());
            }

            context.EvaluatedKeyType = expectedKeyType;
            context.EvaluatedValueType = expectedValueType;

            return context.ImplicitCastType = TypeProvider.DictionaryForTypes(expectedKeyType, expectedValueType);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public DictionaryTypeDef ResolveDictionaryLiteralInit(ZScriptParser.DictionaryLiteralInitContext context)
        {
            var keyType = ResolveType(context.type(0), false);
            var valueType = ResolveType(context.type(1), false);

            context.EvaluatedKeyType = keyType;
            context.EvaluatedValueType = valueType;

            TypeDef outType = TypeProvider.AnyType();
            ResolveFunctionCall(TypeProvider.AnyType(), null, context.functionCall(), ref outType);

            return TypeProvider.DictionaryForTypes(keyType, valueType);
        }

        /// <summary>
        /// Returns a TupleTypeDef describing the tuple literal initializer for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The tuple type for the context</returns>
        public TupleTypeDef ResolveTupleLiteralInit(ZScriptParser.TupleLiteralInitContext context)
        {
            var tupleType = ResolveTupleType(context.tupleType());

            ResolveFunctionCallArguments(tupleType.GetInitializerSignature(), context.functionCall().tupleExpression());

            return tupleType;
        }

        #endregion

        #region Primitives

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveConstantAtom(ZScriptParser.ConstantAtomContext context)
        {
            if (context.numericAtom() != null)
            {
                return ResolveNumericAtom(context.numericAtom());
            }
            if (context.stringLiteral() != null)
            {
                return TypeProvider.StringType();
            }
            if (context.T_FALSE() != null || context.T_TRUE() != null)
            {
                return TypeProvider.BooleanType();
            }
            if (context.T_NULL() != null)
            {
                return TypeProvider.NullType();
            }

            throw new Exception("Cannot resolve type for constant atom " + context);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveCompileConstant(ZScriptParser.CompileConstantContext context)
        {
            if (context.numericAtom() != null)
            {
                return ResolveNumericAtom(context.numericAtom());
            }
            if (context.stringLiteral() != null)
            {
                return TypeProvider.StringType();
            }
            if (context.T_FALSE() != null || context.T_TRUE() != null)
            {
                return TypeProvider.BooleanType();
            }
            if (context.T_NULL() != null)
            {
                return TypeProvider.NullType();
            }

            throw new Exception("Cannot resolve type for compie time constant " + context);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        private TypeDef ResolveNumericAtom(ZScriptParser.NumericAtomContext context)
        {
            if (context.INT() != null || context.hexadecimalNumber() != null || context.binaryNumber() != null)
            {
                return TypeProvider.IntegerType();
            }
            if (context.FLOAT() != null)
            {
                return TypeProvider.FloatType();
            }

            throw new Exception("Cannot resolve type for numeric atom " + context);
        }

        #endregion

        #region Types

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <param name="allowVoid">Whether to allow void types when resolving, or raise errors when void is found</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveType(ZScriptParser.TypeContext context, bool allowVoid)
        {
            TypeDef type;

            if (context.objectType() != null)
            {
                type = TypeProvider.ObjectType();
            }
            else if (context.typeName() != null)
            {
                type = TypeProvider.TypeNamed(context.typeName().GetText());
            }
            else if (context.callableType() != null)
            {
                type = ResolveCallableType(context.callableType());
            }
            else if (context.listType() != null)
            {
                type = ResolveListType(context.listType());
            }
            else if (context.dictionaryType() != null)
            {
                type = ResolveDictionaryType(context.dictionaryType());
            }
            else if (context.tupleType() != null)
            {
                type = ResolveTupleType(context.tupleType());
            }
            else if (context.optional != null)
            {
                type = TypeProvider.OptionalTypeForType(ResolveType(context.type(), false));
            }
            else
            {
                throw new Exception("Cannot resolve type for type " + context);
            }

            // Null type - unknown type
            if (type == null)
            {
                var message = "Unkown type '" + context.GetText() + "'";
                _generationContext.MessageContainer.RegisterError(context, message, ErrorCode.UnkownType);
            }
            // Void types
            else if(type.IsVoid && !allowVoid)
            {
                const string message = "Void type is not allowed in this context";
                _generationContext.MessageContainer.RegisterError(context, message, ErrorCode.InvalidVoidType);
            }

            return type ?? TypeProvider.AnyType();
        }

        /// <summary>
        /// Returns a ListTypeDef describing the list type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The list type for the context</returns>
        public ListTypeDef ResolveListType(ZScriptParser.ListTypeContext context)
        {
            return TypeProvider.ListForType(ResolveType(context.type(), false));
        }

        /// <summary>
        /// Returns a DictionaryTypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The dictionary type for the context</returns>
        public DictionaryTypeDef ResolveDictionaryType(ZScriptParser.DictionaryTypeContext context)
        {
            // Key type
            var keyType = ResolveType(context.keyType, false);
            var valueType = ResolveType(context.valueType, false);

            return TypeProvider.DictionaryForTypes(keyType, valueType);
        }

        /// <summary>
        /// Returns a TupleTypeDef describing the tuple type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The tuple type for the context</returns>
        public TupleTypeDef ResolveTupleType(ZScriptParser.TupleTypeContext context)
        {
            var types = context.tupleTypeEntry();
            var typeNames = new string[types.Length];
            var resolvedTypes = new TypeDef[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                typeNames[i] = types[i].IDENT() == null ? null : types[i].IDENT().GetText();
                resolvedTypes[i] = ResolveType(types[i].type(), false);
            }

            return context.TupleType = TypeProvider.TupleForTypes(typeNames, resolvedTypes);
        }

        /// <summary>
        /// Returns a CallableTypeDef describing the callable type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The callable type for the context</returns>
        public CallableTypeDef ResolveCallableType(ZScriptParser.CallableTypeContext context)
        {
            var parameterTypes = new List<TypeDef>();
            var variadic = new List<bool>();
            var returnType = TypeDef.VoidType;
            var hasReturnType = context.type() != null;

            // Iterate through each parameter type for the closure
            if (context.callableTypeList() != null)
            {
                var args = context.callableTypeList().callableArgType();
                foreach (var arg in args)
                {
                    parameterTypes.Add(ResolveType(arg.type(), false));
                    variadic.Add(arg.variadic != null);
                }
            }

            // Check return type now
            if (hasReturnType)
            {
                returnType = ResolveType(context.type(), true);
            }

            return new CallableTypeDef(parameterTypes.Select((t, i) => new CallableTypeDef.CallableParameterInfo(t, true, false, variadic[i])).ToArray(), returnType, hasReturnType);
        }

        #endregion

        #region Message registering

        /// <summary>
        /// Registers an undefined member error
        /// </summary>
        /// <param name="type">The type trying to be accessed</param>
        /// <param name="context">The context of the access</param>
        /// <param name="memberName">The name of the member trying to be accessed</param>
        private void RegisterUndefinedMember(TypeDef type, ParserRuleContext context, string memberName)
        {
            _generationContext.MessageContainer.RegisterError(context, "Undefined member name '" + memberName + "' on type " + type, ErrorCode.UnrecognizedMember);
        }

        /// <summary>
        /// Registers an undefined tuple index error
        /// </summary>
        /// <param name="type">The tuple trying to be accessed</param>
        /// <param name="context">The context of the access</param>
        /// <param name="indexString">The index of the value trying to be accessed</param>
        private void RegisterUndefinedTupleIndex(TypeDef type, ParserRuleContext context, string indexString)
        {
            _generationContext.MessageContainer.RegisterError(context, "Undefined tuple index '" + indexString + "' on type " + type, ErrorCode.UnrecognizedMember);
        }

        /// <summary>
        /// Registers a message about calling a non-callable type
        /// </summary>
        /// <param name="type">The type of the object trying to be called</param>
        /// <param name="context">The context in which the function call happened</param>
        private void RegisterInvalidFunctionCall(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to call non-callable '" + type + "' type like a function.";
            MessageContainer.RegisterError(context, message, ErrorCode.TryingToCallNonCallable);
        }

        /// <summary>
        /// Registers a message about subscripting a non-subscriptable type
        /// </summary>
        /// <param name="type">The type of the object trying to be subscripted</param>
        /// <param name="context">The context in which the subscription happened</param>
        private void RegisterInvalidSubscript(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to access non-subscriptable '" + type + "' type like a list.";
            MessageContainer.RegisterError(context, message, ErrorCode.TryingToSubscriptNonList);
        }

        /// <summary>
        /// Registers a message about accessing an index on a non-tuple type
        /// </summary>
        /// <param name="type">The type of the object trying to be indexed</param>
        /// <param name="context">The context in which the subscription happened</param>
        private void RegisterInvalidTupleAccess(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to access non-indexable '" + type + "' type like a tuple.";
            MessageContainer.RegisterError(context, message, ErrorCode.TryingToIndexNonTuple);
        }

        /// <summary>
        /// Registers a message about unwrapping a non-optional type
        /// </summary>
        /// <param name="type">The type of the value trying to be unwrapped</param>
        /// <param name="context">The context in which the unwrapping happened</param>
        private void RegisterNonOptionalUnwrapping(TypeDef type, ParserRuleContext context)
        {
            var message = "Trying to unwrap non optional value type " + type;
            MessageContainer.RegisterError(context, message, ErrorCode.TryingToUnwrapNonOptional);
        }

        /// <summary>
        /// Registers a message about a non-optional null-coalesce left side
        /// </summary>
        /// <param name="context">The context in which the unwrapping happened</param>
        private void RegisterNonOptionalNullCoalesceLeftSide(ParserRuleContext context)
        {
            const string message = "Left side of null-coalesce must be an optional type";
            MessageContainer.RegisterError(context, message, ErrorCode.NonOptionalNullCoalesceLeftSize);
        }

        #endregion

        /// <summary>
        /// Returns a type for a given type context
        /// </summary>
        /// <param name="type">The type to get the type definition from</param>
        /// <returns>The type for the given type context</returns>
        public TypeDef TypeForContext(ZScriptParser.TypeContext type)
        {
            return ResolveType(type, true);
        }
    }

    /// <summary>
    /// Interface to be implemented by objects that provide types for definition names
    /// </summary>
    public interface IDefinitionTypeProvider
    {
        /// <summary>
        /// Returns a type for a given definition name
        /// </summary>
        /// <param name="context">The context the member name is contained within</param>
        /// <param name="definitionName">The name of the definition to get</param>
        /// <returns>The type for the given definition</returns>
        TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName);

        /// <summary>
        /// Returns a type for the 'this' special variable contained within a given context
        /// </summary>
        /// <param name="context">The context containing the 'this' value to parse</param>
        /// <returns>The value for the 'this' target</returns>
        TypeDef TypeForThis(ParserRuleContext context);

        /// <summary>
        /// Returns a type for the 'base' special variable contained within a given context
        /// </summary>
        /// <param name="context">The context containing the 'base' value to parse</param>
        /// <returns>The value for the 'base' target</returns>
        TypeDef TypeForBase(ParserRuleContext context);

        /// <summary>
        /// Returns a value that specifies whether the 'base' expression contained within a given parser rule
        /// context contains a valid base method target
        /// </summary>
        /// <param name="context">The context containing the 'base' value to check</param>
        /// <returns>true if the base has a target; false otherwise</returns>
        bool HasBaseTarget(ParserRuleContext context);
    }

    /// <summary>
    /// Interface to be implemented by objects capable of providing a type, given a type ontext
    /// </summary>
    public interface IContextTypeProvider
    {
        /// <summary>
        /// Returns a type for a given type context
        /// </summary>
        /// <param name="type">The type to get the type definition from</param>
        /// <returns>The type for the given type context</returns>
        TypeDef TypeForContext(ZScriptParser.TypeContext type);
    }

    /// <summary>
    /// Interface to be implemented by objects that want to be notified when closures are
    /// matched to an expected type (from an assignment expression, or function call argument)
    /// </summary>
    public interface IClosureExpectedTypeNotifier
    {
        /// <summary>
        /// Notifies that a closure context has been matched with an expected type
        /// </summary>
        /// <param name="context">The context containing the closure</param>
        /// <param name="expectedType">The expected type for the closure</param>
        void ClosureTypeMatched(ZScriptParser.ClosureExpressionContext context, TypeDef expectedType);
    }
}