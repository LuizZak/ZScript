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
        public TypeProvider TypeProvider
        {
            get { return _generationContext.TypeProvider; }
        }

        /// <summary>
        /// Gets the container to report error messages to
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _generationContext.MessageContainer; }
        }

        /// <summary>
        /// Gets the definition type provider for the code generation
        /// </summary>
        public IDefinitionTypeProvider DefinitionTypeProvider
        {
            get { return _generationContext.DefinitionTypeProvider; }
        }

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
            if (context.assignmentExpression() != null)
            {
                retType = ResolveAssignmentExpression(context.assignmentExpression());
            }
            // Literals
            if (context.closureExpression() != null)
            {
                retType = ResolveClosureExpression(context.closureExpression());
            }
            if (context.arrayLiteral() != null)
            {
                var expectedAsList = context.ExpectedType as ListTypeDef;
                if (expectedAsList != null && (context.valueAccess() == null || context.valueAccess().arrayAccess() == null))
                {
                    context.arrayLiteral().ExpectedType = expectedAsList;
                }

                retType = ResolveArrayLiteral(context.arrayLiteral());
            }
            if (context.dictionaryLiteral() != null)
            {
                var expectedAsList = context.ExpectedType as DictionaryTypeDef;
                if (expectedAsList != null && (context.valueAccess() == null || context.valueAccess().arrayAccess() == null))
                {
                    context.dictionaryLiteral().ExpectedType = expectedAsList;
                }

                retType = ResolveDictionaryLiteral(context.dictionaryLiteral());
            }
            if (context.objectLiteral() != null)
            {
                retType = ResolveObjectLiteral(context.objectLiteral());
            }
            if (context.constantAtom() != null)
            {
                retType = ResolveConstantAtom(context.constantAtom());
            }

            // Ternary expression
            if (context.expression().Length == 3)
            {
                retType = ResolveTernaryExpression(context);
            }
            // Binary expression
            if (context.expression().Length == 2)
            {
                retType = ResolveBinaryExpression(context);
            }
            // Parenthesized expression
            if (context.expression().Length == 1)
            {
                // Unary operation
                if (context.unaryOperator() != null)
                {
                    retType = ResolveUnaryExpression(context);
                }
                // Parenthesized expression/type check/type cast
                else
                {
                    retType = ResolveExpression(context.expression(0));
                }
            }

            // Member name
            if (context.memberName() != null)
            {
                retType = ResolveMemberName(context.memberName());
            }

            // 'this' priamry expression
            if (context.T_THIS() != null)
            {
                retType = ResolveThisType(context);
            }
            // 'base' priamry expression
            if (context.T_BASE() != null)
            {
                retType = ResolveBaseType(context);
            }

            // New type
            if (context.newExpression() != null)
            {
                retType = ResolveNewExpression(context.newExpression());
            }

            // Type casting/checking
            if (context.type() != null)
            {
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

            // If the closure is being called, return the return type of the closure instead
            if (context.valueAccess() != null)
            {
                retType = ResolveValueAccess(retType, null, context.valueAccess());
            }

            // If the closure is being called, return the return type of the closure instead
            if (context.objectAccess() != null)
            {
                retType = ResolveObjectAccess(retType, null, context.objectAccess());
            }

            // Prefix operator
            if (context.prefixOperator() != null)
            {
                retType = ResolvePrefixExpression(context);
            }

            // Postfix operator
            if (context.postfixOperator() != null)
            {
                retType = ResolvePostfixExpression(context);
            }

            if(retType == null)
                throw new Exception("Cannot resolve type of expression '" + context.GetText() + "'");

            context.EvaluatedType = retType;

            if (context.ExpectedType != null)
                context.ImplicitCastType = context.ExpectedType;

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

            return TypeProvider.TypeNamed(typeName);
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

            return TypeProvider.FindCommonType(type1, type2);
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

                // Disable constant checking when making accesses
                context.IsConstant = false;
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

            if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, leftValueContext, context.functionCall(), ref type);

                return ResolveLeftValueAccess(type, leftValueContext, context.leftValueAccess());
            }

            if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref type);
            }

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, leftValueContext, context.arrayAccess(), ref type);
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

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, leftValueContext, context.arrayAccess(), ref type);
            }

            if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, leftValueContext, context.functionCall(), ref type);
            }

            if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref type);
            }

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(type, leftValueContext, context.valueAccess());
            }

            return type;
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
            if (leftValue == _generationContext.TypeProvider.ObjectType() || leftValue == _generationContext.TypeProvider.AnyType())
            {
                resType = _generationContext.TypeProvider.AnyType();
                return;
            }

            string memberName = context.memberName().IDENT().GetText();

            // Try to get the field info
            var memberInfo = leftValue.GetMember(memberName);
            if (memberInfo is TypeFieldDef)
            {
                resType = (memberInfo as TypeFieldDef).FieldType;

                // Update constant flag
                if (leftValueContext != null)
                    leftValueContext.IsConstant = (memberInfo as TypeFieldDef).Readonly;
            }
            else if (memberInfo is TypeMethodDef)
            {
                resType = (memberInfo as TypeMethodDef).CallableTypeDef();

                // Update constant flag
                if (leftValueContext != null)
                    leftValueContext.IsConstant = true;
            }

            // TODO: Decide what to do on failure. Raise error? Warning? Do nothing?
            if(memberInfo == null)
                _generationContext.MessageContainer.RegisterError(context, "Undefined member name '" + memberName + "' on type " + leftValue, ErrorCode.UnrecognizedMember);
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
                ResolveFunctionCallArguments(callableType, context.funcCallArguments());

                resType = callableType.ReturnType;
            }
            else if (!leftValue.IsAny)
            {
                RegisterFunctionCallWarning(leftValue, context);
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
        public TypeDef[] ResolveFunctionCallArguments(ICallableTypeDef callableType, ZScriptParser.FuncCallArgumentsContext context)
        {
            // Collect the list of arguments
            var argTypes = new List<TypeDef>();

            if (context.expressionList() != null)
            {
                int argCount = context.expressionList().expression().Length;

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
                    foreach (var exp in context.expressionList().expression())
                    {
                        // Set expected type
                        if (!mismatchedCount)
                            exp.ExpectedType = curArgInfo.RawParameterType;

                        var argType = ResolveExpression(exp);
                        argTypes.Add(argType);

                        if (mismatchedCount)
                            continue;

                        // Match the argument types
                        if (!TypeProvider.CanImplicitCast(argType, curArgInfo.RawParameterType))
                        {
                            var message = "Cannot implicitly cast argument type " + argType + " to parameter type " + curArgInfo.RawParameterType;
                            MessageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                        }

                        // Jump to the next callable argument information
                        if (!curArgInfo.IsVariadic && ci < callableType.ParameterInfos.Length - 1)
                        {
                            curArgInfo = callableType.ParameterInfos[++ci];
                        }
                    }
                }
            }
            else if (callableType.RequiredArgumentsCount > 0)
            {
                var message = "Trying to pass 0 arguments to callable that requires at least " + callableType.RequiredArgumentsCount;
                MessageContainer.RegisterError(context, message, ErrorCode.TooFewArguments);
            }

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
                RegisterSubscriptWarning(leftValue, context);
            }

            // Update constant flag
            if (leftValueContext != null)
                leftValueContext.IsConstant = false;
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
            
            if (context.fieldAccess() != null)
            {
                ResolveFieldAccess(leftValue, leftValueContext, context.fieldAccess(), ref resType);
            }

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(resType, leftValueContext, context.valueAccess());
            }

            return resType;
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
            TypeDef target = ResolveType(castType);
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
            var parameters = new List<FunctionArgumentDefinition>();
            var returnType = TypeDef.AnyType;
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

            // Check return type now
            if (hasReturnType)
            {
                returnType = ResolveType(context.returnType().type());
            }
            
            if (context.Definition != null && context.Definition.HasReturnType && context.Definition.ReturnType != null)
            {
                returnType = context.Definition.ReturnType;
            }

            return new CallableTypeDef(parameters.Select(a => a.ToArgumentInfo()).ToArray(), returnType, hasReturnType);
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveFunctionArgument(ZScriptParser.FunctionArgContext context)
        {
            TypeDef type;
            TypeDef defaultValueType = null;

            if (context.compileConstant() != null)
            {
                // Check default type, instead
                defaultValueType = ResolveCompileConstant(context.compileConstant());
            }

            // Type provided
            if (context.type() != null)
            {
                type = ResolveType(context.type());

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
            else
            {
                type = TypeProvider.AnyType();
            }

            return context.variadic != null ? TypeProvider.ListForType(type) : type;
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
            var expectedValueType = context.ExpectedType == null ? null : context.ExpectedType.EnclosingType;

            // Try to infer the type of items in the array
            var listItemsType = expectedValueType ?? TypeProvider.AnyType();

            var items = context.expressionList();

            if (items == null)
            {
                context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(listItemsType, true);

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

                context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(listItemsType, true);

                return TypeProvider.ListForType(listItemsType);
            }

            // Check type compatibility
            bool canImplicit = true;
            foreach (var exp in items.expression())
            {
                exp.ExpectedType = expectedValueType;

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

                context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(TypeProvider.AnyType(), true);

                return TypeProvider.ListForType(TypeProvider.AnyType());
            }

            context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(expectedValueType, true);

            return context.ImplicitCastType = TypeProvider.ListForType(expectedValueType);
        }

        /// <summary>
        /// Returns a DictionaryTypeDef describing the type for a given dictionary literal context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The dictionary type for the context</returns>
        public DictionaryTypeDef ResolveDictionaryLiteral(ZScriptParser.DictionaryLiteralContext context)
        {
            // Expected type for the list
            var expectedValueType = context.ExpectedType == null ? null : context.ExpectedType.EnclosingType;
            var expectedKeyType = context.ExpectedType == null ? null : context.ExpectedType.SubscriptType;

            // Try to infer the type of items in the array
            var dictKeyType = expectedKeyType ?? TypeProvider.AnyType();
            var dictValueType = expectedValueType ?? TypeProvider.AnyType();

            var entries = context.dictionaryEntryList().dictionaryEntry();

            if (entries.Length == 0)
            {
                context.EvaluatedKeyType = TypeProvider.NativeTypeForTypeDef(dictKeyType, true);
                context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(dictValueType, true);

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

                context.EvaluatedKeyType = TypeProvider.NativeTypeForTypeDef(dictKeyType, true);
                context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(dictValueType, true);

                return TypeProvider.DictionaryForTypes(dictKeyType, dictValueType);
            }

            // Check type compatibility
            bool canImplicit = true;
            foreach (var exp in entries)
            {
                exp.expression(0).ExpectedType = expectedKeyType;
                exp.expression(1).ExpectedType = expectedValueType;

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

                context.EvaluatedKeyType = typeof(object);
                context.EvaluatedValueType = typeof(object);

                return TypeProvider.DictionaryForTypes(TypeProvider.AnyType(), TypeProvider.AnyType());
            }

            context.EvaluatedKeyType = TypeProvider.NativeTypeForTypeDef(expectedKeyType, true);
            context.EvaluatedValueType = TypeProvider.NativeTypeForTypeDef(expectedValueType, true);

            return context.ImplicitCastType = TypeProvider.DictionaryForTypes(expectedKeyType, expectedValueType);
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
                return TypeProvider.AnyType();
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
                return TypeProvider.AnyType();
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
        /// <returns>The type for the context</returns>
        public TypeDef ResolveType(ZScriptParser.TypeContext context)
        {
            if (context.objectType() != null)
            {
                return TypeProvider.ObjectType();
            }
            if (context.typeName() != null)
            {
                return TypeProvider.TypeNamed(context.typeName().GetText());
            }
            if (context.callableType() != null)
            {
                return ResolveCallableType(context.callableType());
            }
            if (context.listType() != null)
            {
                return ResolveListType(context.listType());
            }
            if (context.dictionaryType() != null)
            {
                return ResolveDictionaryType(context.dictionaryType());
            }

            throw new Exception("Cannot resolve type for type " + context);
        }

        /// <summary>
        /// Returns a ListTypeDef describing the list type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The list type for the context</returns>
        public ListTypeDef ResolveListType(ZScriptParser.ListTypeContext context)
        {
            return TypeProvider.ListForType(ResolveType(context.type()));
        }

        /// <summary>
        /// Returns a DictionaryTypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The dictionary type for the context</returns>
        public DictionaryTypeDef ResolveDictionaryType(ZScriptParser.DictionaryTypeContext context)
        {
            // Key type
            var keyType = ResolveType(context.keyType);
            var valueType = ResolveType(context.valueType);

            return TypeProvider.DictionaryForTypes(keyType, valueType);
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
            var returnType = TypeDef.AnyType;
            var hasReturnType = context.type() != null;

            // Iterate through each parameter type for the closure
            if (context.callableTypeList() != null)
            {
                var args = context.callableTypeList().callableArgType();
                foreach (var arg in args)
                {
                    parameterTypes.Add(ResolveType(arg.type()));
                    variadic.Add(arg.variadic != null);
                }
            }

            // Check return type now
            if (hasReturnType)
            {
                returnType = ResolveType(context.type());
            }

            return new CallableTypeDef(parameterTypes.Select((t, i) => new CallableTypeDef.CallableParameterInfo(t, true, false, variadic[i])).ToArray(), returnType, hasReturnType);
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
            MessageContainer.RegisterWarning(context.Start.Line, context.Start.Column, message, WarningCode.TryingToCallNonCallable, context);
        }

        /// <summary>
        /// Registers a warning about subscripting a non-subscriptable type
        /// </summary>
        /// <param name="type">The type of the object trying to be subscripted</param>
        /// <param name="context">The context in which the subscription happened</param>
        private void RegisterSubscriptWarning(TypeDef type, ParserRuleContext context)
        {
            string message = "Trying to access non-subscriptable '" + type + "' type like a list may result in runtime errors.";
            MessageContainer.RegisterWarning(context.Start.Line, context.Start.Column, message, WarningCode.TryingToSubscriptNonList, context);
        }

        #endregion

        /// <summary>
        /// Returns a type for a given type context
        /// </summary>
        /// <param name="type">The type to get the type definition from</param>
        /// <returns>The type for the given type context</returns>
        public TypeDef TypeForContext(ZScriptParser.TypeContext type)
        {
            return ResolveType(type);
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