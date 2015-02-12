﻿using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using ZScript.CodeGeneration.Elements.Typing;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class capable of resolving the types of expressions
    /// </summary>
    public partial class ExpressionTypeResolver
    {
        /*
        expression: >>   '(' expression ')' valueAccess?
                    >> | '(' assignmentExpression ')'
                    >> |  prefixOperator leftValue
                    >> |  leftValue postfixOperator
                    >> |  closureExpression valueAccess?
                    >> |  memberName valueAccess?
                    >> |  objectLiteral objectAccess?
                    >> |  arrayLiteral valueAccess?
                    >> |  newExpression valueAccess?
                    >> |  '(' type ')' expression
                       // Unary expressions
                       |  '-' expression
                       |  '!' expression
                       // Binary expressions
                    >> |  expression multOp expression
                    >> |  expression additionOp expression
                    >> |  expression bitwiseAndXOrOp expression
                    >> |  expression bitwiseOrOp expression
                    >> |  expression comparisionOp expression
                    >> |  expression logicalOp expression
                    >> |  constantAtom objectAccess?
                       ;
        */

        /// <summary>
        /// The type provider using when resolving the type of the expressions
        /// </summary>
        private readonly TypeProvider _typeProvider;

        /// <summary>
        /// The container to report error messages to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// Gets or sets the definition type provider for this ExpressionTypeResolver
        /// </summary>
        public IDefinitionTypeProvider DefinitionTypeProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the ExpressionTypeResolver class
        /// </summary>
        /// <param name="typeProvider">The type provider using when resolving the type of the expressions</param>
        /// <param name="messageContainer">A message container to report error messages to</param>
        /// <param name="definitionTypeProvider">The definition type provider for this ExpressionTypeResolver</param>
        public ExpressionTypeResolver(TypeProvider typeProvider, MessageContainer messageContainer, IDefinitionTypeProvider definitionTypeProvider = null)
        {
            _typeProvider = typeProvider;
            _messageContainer = messageContainer;
            DefinitionTypeProvider = definitionTypeProvider;
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveExpression(ZScriptParser.ExpressionContext context)
        {
            TypeDef retType = null;

            if (context.closureExpression() != null)
            {
                retType = ResolveClosureExpression(context.closureExpression());
            }
            if (context.arrayLiteral() != null)
            {
                retType = ResolveArrayLiteral(context.arrayLiteral());
            }
            if (context.objectLiteral() != null)
            {
                retType = ResolveObjectLiteral(context.objectLiteral());
            }
            if (context.constantAtom() != null)
            {
                retType = ResolveConstantAtom(context.constantAtom());
            }

            // Binary expression
            if (context.expression().Length == 2)
            {
                return ResolveBinaryExpression(context);
            }
            // Parenthesized expression
            if (context.expression().Length == 1)
            {
                retType = ResolveExpression(context.expression()[0]);
            }

            // Member name
            if (context.memberName() != null)
            {
                retType = ResolveMemberName(context.memberName());
            }

            // New type
            if (context.newExpression() != null)
            {
                retType = ResolveNewExpression(context.newExpression());
            }

            // Type casting
            if (context.type() != null)
            {
                retType = ResolveTypeCasting(retType, context, context.type());
            }

            // If the closure is being called, return the return type of the closure instead
            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(retType, context.valueAccess());
            }

            // If the closure is being called, return the return type of the closure instead
            if (context.objectAccess() != null)
            {
                return ResolveObjectAccess(retType, context.objectAccess());
            }

            // Prefix operator
            if (context.prefixOperator() != null)
            {
                return ResolvePrefixExpression(context);
            }

            // Postfix operator
            if (context.postfixOperator() != null)
            {
                return ResolvePostfixExpression(context);
            }

            if(retType == null)
                throw new Exception("Cannot resolve type of expression '" + context.GetText() + "'");

            return retType;
        }

        /// <summary>
        /// Resolves a new expression
        /// </summary>
        /// <param name="context">The context containing the new expression</param>
        /// <returns>The type for the new expression</returns>
        private TypeDef ResolveNewExpression(ZScriptParser.NewExpressionContext context)
        {
            var typeName = context.typeName().GetText();

            return _typeProvider.TypeNamed(typeName);
        }

        #region Prefix and Postfix

        /// <summary>
        /// Resolves a prefix expression type contained within a given expression context
        /// </summary>
        /// <param name="context">The context containing the prefix operation to evaluate</param>
        /// <returns>A type resolved from the context</returns>
        public TypeDef ResolvePrefixExpression(ZScriptParser.ExpressionContext context)
        {
            // Get the type of the left value
            TypeDef leftValueType = ResolveLeftValue(context.leftValue());

            if (!_typeProvider.BinaryExpressionProvider.IsNumeric(leftValueType) && !leftValueType.IsAny)
            {
                _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Cannot perform prefix operation on values of type " + leftValueType);
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

            if (!_typeProvider.BinaryExpressionProvider.IsNumeric(leftValueType) && !leftValueType.IsAny)
            {
                _messageContainer.RegisterError(context.Start.Line, context.Start.Column, "Cannot perform postfix operation on values of type " + leftValueType);
            }

            return leftValueType;
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
            var type = ResolveMemberName(context.memberName());

            // Evaluate the access
            if (context.leftValueAccess() != null)
            {
                type = ResolveLeftValueAccess(type, context.leftValueAccess());
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
            return DefinitionTypeProvider.TypeForDefinition(context, context.IDENT().GetText());
        }

        /// <summary>
        /// Resolves a left value access
        /// </summary>
        /// <param name="leftValue">The type of the value being accessed</param>
        /// <param name="context">The context containing the left value access to resolve</param>
        /// <returns>The type for the left value access</returns>
        public TypeDef ResolveLeftValueAccess(TypeDef leftValue, ZScriptParser.LeftValueAccessContext context)
        {
            // leftValueAccess : (funcCallArguments leftValueAccess) | (fieldAccess leftValueAccess?) | (arrayAccess leftValueAccess?);
            var type = _typeProvider.AnyType();

            if (context.funcCallArguments() != null)
            {
                var callableType = leftValue as CallableTypeDef;
                if (callableType != null)
                {
                    type = callableType.ReturnType;
                }
                else if (!leftValue.IsAny)
                {
                    RegisterFunctionCallWarning(leftValue, context);
                }

                return ResolveLeftValueAccess(type, context.leftValueAccess());
            }
            // TODO: Deal with field accesses
            if (context.fieldAccess() == null)
            {
                
            }

            if (context.arrayAccess() != null)
            {
                var listType = leftValue as IListTypeDef;
                if (listType != null)
                {
                    type = listType.EnclosingType;
                }
                else if (!leftValue.IsAny)
                {
                    RegisterSubscriptWarning(leftValue, context);
                }
            }

            if (context.leftValueAccess() != null)
            {
                type = ResolveLeftValueAccess(type, context.leftValueAccess());
            }

            return type;
        }

        #endregion

        #region Accessing

        /// <summary>
        /// Resolves a value access, using a given left value as a starting point for the evaluation
        /// </summary>
        /// <param name="leftValue">The left value to access the value from</param>
        /// <param name="context">The context that contains the value access</param>
        /// <returns>A type resolved from the value access</returns>
        public TypeDef ResolveValueAccess(TypeDef leftValue, ZScriptParser.ValueAccessContext context)
        {
            TypeDef resType = _typeProvider.AnyType();

            if (context.arrayAccess() != null)
            {
                var listType = leftValue as IListTypeDef;
                if (listType != null)
                {
                    resType = listType.EnclosingType;
                }
                else if (!leftValue.IsAny)
                {
                    RegisterSubscriptWarning(leftValue, context);
                }
            }

            if (context.functionCall() != null)
            {
                var callableType = leftValue as CallableTypeDef;
                if (callableType != null)
                {
                    resType = callableType.ReturnType;
                }
                else if (!leftValue.IsAny)
                {
                    RegisterFunctionCallWarning(leftValue, context);
                }
            }

            // TODO: Deal with field access

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(resType, context.valueAccess());
            }

            return resType;
        }

        /// <summary>
        /// Resolves a value access, using a given left value as a starting point for the evaluation
        /// </summary>
        /// <param name="leftValue">The left value to access the value from</param>
        /// <param name="context">The context that contains the value access</param>
        /// <returns>A type resolved from the value access</returns>
        public TypeDef ResolveObjectAccess(TypeDef leftValue, ZScriptParser.ObjectAccessContext context)
        {
            TypeDef resType = _typeProvider.AnyType();

            if (context.arrayAccess() != null)
            {
                var listType = leftValue as IListTypeDef;
                if (listType != null)
                {
                    resType = listType.EnclosingType;
                }
                else if (!leftValue.IsAny)
                {
                    RegisterSubscriptWarning(leftValue, context);
                }
            }
            // TODO: Deal with field access

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(resType, context.valueAccess());
            }

            return resType;
        }

        #endregion

        #region Type casting

        /// <summary>
        /// Resolves a type cast attempt, retuning the resulting type
        /// </summary>
        /// <param name="valueType">The type of the value trying to be cast</param>
        /// <param name="origin">The origin of the value type tring to be cast</param>
        /// <param name="castType">The type tring to cast the value as</param>
        /// <returns>A type definition that represents the casted type</returns>
        public TypeDef ResolveTypeCasting(TypeDef valueType, ZScriptParser.ExpressionContext origin, ZScriptParser.TypeContext castType)
        {
            TypeDef target = ResolveType(castType);
            if (!_typeProvider.CanExplicitCast(valueType, target))
            {
                var message = "Invalid cast: cannot cast objects of type " + valueType + " to type " + target;
                _messageContainer.RegisterError(origin.Start.Line, origin.Start.Column, message, ErrorCode.InvalidCast, origin);
            }

            return target;
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
            var parameterTypes = new List<TypeDef>();
            var returnType = TypeDef.AnyType;

            // Iterate through each parameter type for the closure
            if(context.functionArguments().argumentList() != null)
            {
                var args = context.functionArguments().argumentList().functionArg();
                foreach (var arg in args)
                {
                    parameterTypes.Add(ResolveFunctionArgument(arg));
                }
            }

            // Check return type now
            if (context.returnType() != null)
            {
                returnType = ResolveType(context.returnType().type());
            }

            return new CallableTypeDef(parameterTypes.ToArray(), returnType);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveFunctionArgument(ZScriptParser.FunctionArgContext context)
        {
            TypeDef type;

            // Type provided
            if (context.type() != null)
            {
                type = ResolveType(context.type());
            }
            else if(context.compileConstant() != null)
            {
                // Check default type, instead
                type = ResolveCompileConstant(context.compileConstant());
            }
            else
            {
                type = _typeProvider.AnyType();
            }

            return context.variadic != null ? _typeProvider.ListForType(type) : type;
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
            return _typeProvider.ObjectType();
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public ListTypeDef ResolveArrayLiteral(ZScriptParser.ArrayLiteralContext context)
        {
            // Try to infer the type of items in the array
            var listItemsType = _typeProvider.AnyType();

            var items = context.expressionList();

            if (items != null)
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

                    listItemsType = _typeProvider.FindCommonType(itemType, listItemsType);
                }
            }

            return _typeProvider.ListForType(listItemsType);
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
                return _typeProvider.StringType();
            }
            if (context.T_FALSE() != null || context.T_TRUE() != null)
            {
                return _typeProvider.BooleanType();
            }
            if (context.T_NULL() != null)
            {
                return _typeProvider.NullType();
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
                return _typeProvider.StringType();
            }
            if (context.T_FALSE() != null || context.T_TRUE() != null)
            {
                return _typeProvider.BooleanType();
            }
            if (context.T_NULL() != null)
            {
                return _typeProvider.NullType();
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
                return _typeProvider.IntegerType();
            }
            if (context.FLOAT() != null)
            {
                return _typeProvider.FloatType();
            }

            throw new Exception("Cannot resolve type for numeric atom " + context);
        }

        #endregion

        #region Types

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveType(ZScriptParser.TypeContext context)
        {
            if (context.objectType() != null)
            {
                return _typeProvider.ObjectType();
            }
            if (context.typeName() != null)
            {
                return _typeProvider.TypeNamed(context.typeName().GetText());
            }
            if (context.callableType() != null)
            {
                return ResolveCallableType(context.callableType());
            }
            if (context.listType() != null)
            {
                return ResolveListType(context.listType());
            }

            throw new Exception("Cannot resolve type for type " + context);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public ListTypeDef ResolveListType(ZScriptParser.ListTypeContext context)
        {
            return _typeProvider.ListForType(ResolveType(context.type()));
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public CallableTypeDef ResolveCallableType(ZScriptParser.CallableTypeContext context)
        {
            var parameterTypes = new List<TypeDef>();
            var returnType = TypeDef.AnyType;

            // Iterate through each parameter type for the closure
            if (context.typeList() != null)
            {
                var args = context.typeList().type();
                foreach (var arg in args)
                {
                    parameterTypes.Add(ResolveType(arg));
                }
            }

            // Check return type now
            if (context.type() != null)
            {
                returnType = ResolveType(context.type());
            }

            return new CallableTypeDef(parameterTypes.ToArray(), returnType);
        }

        #endregion

        #region Warning raising

        /// <summary>
        /// Registers a warning about calling a non-callable type
        /// </summary>
        /// <param name="type">The type of the object trying to be called</param>
        /// <param name="context">The context in which the function call happened</param>
        private void RegisterFunctionCallWarning(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to call non-callable '" + type + "' type like a function may result in runtime errors.";
            _messageContainer.RegisterWarning(context.Start.Line, context.Start.Column, message, WarningCode.TryingToCallNonCallable, context);
        }

        /// <summary>
        /// Registers a warning about subscripting a non-subscriptable type
        /// </summary>
        /// <param name="type">The type of the object trying to be subscripted</param>
        /// <param name="context">The context in which the subscription happened</param>
        private void RegisterSubscriptWarning(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to access non-subscriptable '" + type + "' type like a list may result in runtime errors.";
            _messageContainer.RegisterWarning(context.Start.Line, context.Start.Column, message, WarningCode.TryingToSubscriptNonList, context);
        }

        #endregion
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
    }
}