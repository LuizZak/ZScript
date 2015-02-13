using System;
using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;

using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Elements.Typing;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Tokenization;
using ZScript.Elements;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class capable of resolving the types of expressions
    /// </summary>
    public partial class ExpressionTypeResolver
    {
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
        /// Gets or sets the closure expected type notifying
        /// </summary>
        public IClosureExpectedTypeNotifier ClosureExpectedTypeNotifier { get; set; }

        /// <summary>
        /// Gets or sets a type that specifies an expected type when dealing with function arguments and assignment operations, used mostly to infer types to closures
        /// </summary>
        public TypeDef ExpectedType { get; set; }

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
        /// Returns a TypeDef describing the type for a given assignment expression context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            TypeDef variableType = ResolveLeftValue(context.leftValue());
            TypeDef valueType;

            // Find the type of the expression
            if (context.expression() != null)
            {
                // Push expected type
                ExpectedType = context.expression().closureExpression() != null ? variableType : null;

                valueType = ResolveExpression(context.expression());

                ExpectedType = null;
            }
            else
            {
                valueType = ResolveAssignmentExpression(context.assignmentExpression());
            }

            // Get the operator
            if (!PostfixExpressionTokenizer.IsCompoundAssignmentOperator(context.assignmentOperator()))
            {
                // Check the type matching
                if (!_typeProvider.CanImplicitCast(valueType, variableType))
                {
                    var message = "Cannot assign value of type " + valueType + " to variable of type " + variableType;
                    _messageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else
            {
                var op = context.assignmentOperator().GetText()[0].ToString();
                var inst = TokenFactory.InstructionForOperator(op);

                if (!_typeProvider.BinaryExpressionProvider.CanPerformOperation(inst, variableType, valueType))
                {
                    var message = "Cannot perform " + inst + " operation on values of type " + variableType + " and " + valueType;
                    _messageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }

            return variableType;
        }

        /// <summary>
        /// Returns a TypeDef describing the type for a given context
        /// </summary>
        /// <param name="context">The context to resolve</param>
        /// <returns>The type for the context</returns>
        public TypeDef ResolveExpression(ZScriptParser.ExpressionContext context)
        {
            TypeDef retType = null;

            if (context.assignmentExpression() != null)
            {
                return ResolveAssignmentExpression(context.assignmentExpression());
            }
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
                if (context.unaryOperator() != null)
                {
                    retType = ResolveUnaryExpression(context);
                }
                else
                {
                    retType = ResolveExpression(context.expression()[0]);
                }
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

            if (_typeProvider.BinaryExpressionProvider.CanUnary(expType, inst))
                return _typeProvider.BinaryExpressionProvider.TypeForUnary(expType, inst);

            var message = "Cannot apply " + inst + " to value of type " + expType;
            _messageContainer.RegisterError(context, message, ErrorCode.InvalidTypesOnOperation);

            return _typeProvider.AnyType();
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

            if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, context.functionCall(), ref type);

                return ResolveLeftValueAccess(type, context.leftValueAccess());
            }
            // TODO: Deal with field accesses
            if (context.fieldAccess() != null)
            {
                
            }

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, context.arrayAccess(), ref type);
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
            var type = _typeProvider.AnyType();

            if (context.arrayAccess() != null)
            {
                ResolveSubscript(leftValue, context.arrayAccess(), ref type);
            }

            if (context.functionCall() != null)
            {
                ResolveFunctionCall(leftValue, context.functionCall(), ref type);
            }

            // TODO: Deal with field access

            if (context.valueAccess() != null)
            {
                return ResolveValueAccess(type, context.valueAccess());
            }

            return type;
        }

        /// <summary>
        /// Resolves a function call of a given left value, using the given function call as context.
        /// This method raises a warning when the value is not an explicitly callable type
        /// </summary>
        /// <param name="leftValue">The type of the left value</param>
        /// <param name="context">The context of the function call</param>
        /// <param name="resType">The type to update the resulting function call return type to</param>
        private void ResolveFunctionCall(TypeDef leftValue, ZScriptParser.FunctionCallContext context, ref TypeDef resType)
        {
            var callableType = leftValue as CallableTypeDef;
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
        }

        /// <summary>
        /// Returns an array of types that describe the type of the arguments contained within a given function call arguments context
        /// </summary>
        /// <param name="callableType">A callable type used to verify the argument types correctly</param>
        /// <param name="context">The context containing the function call arguments</param>
        /// <returns>An array of types related to the function call</returns>
        public TypeDef[] ResolveFunctionCallArguments(CallableTypeDef callableType, ZScriptParser.FuncCallArgumentsContext context)
        {
            // Collect the list of arguments
            var argTypes = new List<TypeDef>();

            // TODO: Move argument type checking inside the argument collecting loop so we can effectively use the _expectedTypeStack stack here
            if (context.expressionList() != null)
            {
                int argCount = context.expressionList().expression().Length;

                // Whether the count of arguments is mismatched of the expected argument count
                bool mismatchedCount = false;

                // Verify argument count
                if (argCount < callableType.RequiredArgumentsCount)
                {
                    var message = "Trying to pass " + argTypes.Count + " arguments to callable that requires at least " + callableType.RequiredArgumentsCount;
                    _messageContainer.RegisterError(context, message, ErrorCode.TooFewArguments);
                    mismatchedCount = true;
                }
                if (argCount > callableType.MaximumArgumentsCount)
                {
                    var message = "Trying to pass " + argTypes.Count + " arguments to callable that accepts at most " + callableType.MaximumArgumentsCount;
                    _messageContainer.RegisterError(context, message, ErrorCode.TooManyArguments);
                    mismatchedCount = true;
                }


                int ci = 0;
                var curArgInfo = callableType.ParameterInfos[ci];
                foreach (var exp in context.expressionList().expression())
                {
                    // Set expected type
                    if(!mismatchedCount && exp.closureExpression() != null)
                        ExpectedType = curArgInfo.RawParameterType;

                    var argType = ResolveExpression(exp);
                    argTypes.Add(argType);

                    // Clear expected type
                    ExpectedType = null;

                    if (mismatchedCount)
                        continue;

                    // Match the argument types
                    if (!_typeProvider.CanImplicitCast(argType, curArgInfo.RawParameterType))
                    {
                        var message = "Cannot implicitly cast argument type " + argType + " to parameter type " + curArgInfo.RawParameterType;
                        _messageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                    }

                    // Jump to the next callable argument information
                    if (!curArgInfo.IsVariadic && ci < callableType.ParameterInfos.Length - 1)
                    {
                        curArgInfo = callableType.ParameterInfos[++ci];
                    }
                }
            }

            return argTypes.ToArray();
        }

        /// <summary>
        /// Resolves subscription of a given left value, using the given array access as context.
        /// This method raises a warning when the value is not an explicitly subscriptable type
        /// </summary>
        /// <param name="leftValue">The type of the value</param>
        /// <param name="context">The context of the subscription</param>
        /// <param name="resType">The type to update the resulting subscripting type to</param>
        private void ResolveSubscript(TypeDef leftValue, ZScriptParser.ArrayAccessContext context, ref TypeDef resType)
        {
            // Get the type of object being subscripted
            var listType = leftValue as IListTypeDef;
            if (listType != null)
            {
                resType = listType.EnclosingType;

                // Check the subscript type
                var subType = listType.SubscriptType;
                var accessValue = ResolveExpression(context.expression());

                if (!_typeProvider.CanImplicitCast(accessValue, subType))
                {
                    var message = "Subscriptable type " + leftValue + " expects type " + subType + " for subscription but received " + accessValue + ".";
                    _messageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else if (!leftValue.IsAny)
            {
                RegisterSubscriptWarning(leftValue, context);
            }
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
                ResolveSubscript(leftValue, context.arrayAccess(), ref resType);
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
            var parameters = new List<FunctionArgumentDefinition>();
            var returnType = TypeDef.AnyType;
            var hasReturnType = context.returnType() != null;

            // Iterate through each parameter type for the closure
            if (context.functionArg() != null)
            {
                var t = FunctionDefinitionGenerator.GenerateFunctionArgumentDef(context.functionArg());

                // Resolve the type, if available
                t.Type = ResolveFunctionArgument(context.functionArg());

                parameters.Add(t);
            }
            else if(context.functionArguments().argumentList() != null)
            {
                var args = context.functionArguments().argumentList().functionArg();
                foreach (var arg in args)
                {
                    var t = FunctionDefinitionGenerator.GenerateFunctionArgumentDef(arg);

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

            // Notify closure expected types
            if (ClosureExpectedTypeNotifier != null && ExpectedType != null)
            {
                ClosureExpectedTypeNotifier.ClosureTypeMatched(context, ExpectedType);
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
                if (defaultValueType != null && !_typeProvider.CanImplicitCast(type, defaultValueType))
                {
                    var message = "Cannot implicitly cast default value type " + defaultValueType + " to expected parameter type " + type;
                    _messageContainer.RegisterError(context, message, ErrorCode.InvalidCast);
                }
            }
            else if (defaultValueType != null)
            {
                // Check default type, instead
                type = defaultValueType;
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
                return _typeProvider.AnyType();
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
                return _typeProvider.AnyType();
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