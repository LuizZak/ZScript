//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Users/Luiz Fernando/Documents/Visual Studio 2015/Engines/ZScript/ZScript/Parsing/ANTLR\ZScript.g4 by ANTLR 4.5.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="ZScriptParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.1")]
[System.CLSCompliant(false)]
public interface IZScriptVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] ZScriptParser.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.scriptBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScriptBody([NotNull] ZScriptParser.ScriptBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.classDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDefinition([NotNull] ZScriptParser.ClassDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.classInherit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassInherit([NotNull] ZScriptParser.ClassInheritContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.className"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassName([NotNull] ZScriptParser.ClassNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBody([NotNull] ZScriptParser.ClassBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.classField"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassField([NotNull] ZScriptParser.ClassFieldContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.classMethod"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassMethod([NotNull] ZScriptParser.ClassMethodContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.globalVariable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGlobalVariable([NotNull] ZScriptParser.GlobalVariableContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.sequenceBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSequenceBlock([NotNull] ZScriptParser.SequenceBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.sequenceName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSequenceName([NotNull] ZScriptParser.SequenceNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.sequenceBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSequenceBody([NotNull] ZScriptParser.SequenceBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.sequenceFrame"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSequenceFrame([NotNull] ZScriptParser.SequenceFrameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.sequenceFrameChange"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSequenceFrameChange([NotNull] ZScriptParser.SequenceFrameChangeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.frameName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFrameName([NotNull] ZScriptParser.FrameNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.frameRange"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFrameRange([NotNull] ZScriptParser.FrameRangeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.frameRangeElement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFrameRangeElement([NotNull] ZScriptParser.FrameRangeElementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.frameNumber"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFrameNumber([NotNull] ZScriptParser.FrameNumberContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDefinition([NotNull] ZScriptParser.FunctionDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.exportDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExportDefinition([NotNull] ZScriptParser.ExportDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionName([NotNull] ZScriptParser.FunctionNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionBody([NotNull] ZScriptParser.FunctionBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionArguments([NotNull] ZScriptParser.FunctionArgumentsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.argumentList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentList([NotNull] ZScriptParser.ArgumentListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.returnType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnType([NotNull] ZScriptParser.ReturnTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionArg([NotNull] ZScriptParser.FunctionArgContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.argumentName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentName([NotNull] ZScriptParser.ArgumentNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericParametersDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericParametersDefinition([NotNull] ZScriptParser.GenericParametersDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericParameterDefinitionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericParameterDefinitionList([NotNull] ZScriptParser.GenericParameterDefinitionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericConstraintList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericConstraintList([NotNull] ZScriptParser.GenericConstraintListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericConstraint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericConstraint([NotNull] ZScriptParser.GenericConstraintContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericType([NotNull] ZScriptParser.GenericTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericParameters([NotNull] ZScriptParser.GenericParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.genericParameterList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericParameterList([NotNull] ZScriptParser.GenericParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAlias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAlias([NotNull] ZScriptParser.TypeAliasContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAliasBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAliasBody([NotNull] ZScriptParser.TypeAliasBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAliasVariable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAliasVariable([NotNull] ZScriptParser.TypeAliasVariableContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAliasFunction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAliasFunction([NotNull] ZScriptParser.TypeAliasFunctionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAliasName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAliasName([NotNull] ZScriptParser.TypeAliasNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeAliasInherit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAliasInherit([NotNull] ZScriptParser.TypeAliasInheritContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] ZScriptParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.blockStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockStatement([NotNull] ZScriptParser.BlockStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.trailingIfStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTrailingIfStatement([NotNull] ZScriptParser.TrailingIfStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.ifStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStatement([NotNull] ZScriptParser.IfStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.elseStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseStatement([NotNull] ZScriptParser.ElseStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.switchStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchStatement([NotNull] ZScriptParser.SwitchStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.switchBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchBlock([NotNull] ZScriptParser.SwitchBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.caseBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCaseBlock([NotNull] ZScriptParser.CaseBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.defaultBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDefaultBlock([NotNull] ZScriptParser.DefaultBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.whileStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStatement([NotNull] ZScriptParser.WhileStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForStatement([NotNull] ZScriptParser.ForStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forInit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForInit([NotNull] ZScriptParser.ForInitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forCondition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForCondition([NotNull] ZScriptParser.ForConditionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forIncrement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForIncrement([NotNull] ZScriptParser.ForIncrementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forEachStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForEachStatement([NotNull] ZScriptParser.ForEachStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.forEachHeader"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForEachHeader([NotNull] ZScriptParser.ForEachHeaderContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.returnStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] ZScriptParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.breakStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBreakStatement([NotNull] ZScriptParser.BreakStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.continueStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitContinueStatement([NotNull] ZScriptParser.ContinueStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.valueDeclareStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueDeclareStatement([NotNull] ZScriptParser.ValueDeclareStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.valueHolderDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueHolderDecl([NotNull] ZScriptParser.ValueHolderDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.valueHolderDefine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueHolderDefine([NotNull] ZScriptParser.ValueHolderDefineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.valueHolderName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueHolderName([NotNull] ZScriptParser.ValueHolderNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType([NotNull] ZScriptParser.TypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.objectType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjectType([NotNull] ZScriptParser.ObjectTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.typeName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeName([NotNull] ZScriptParser.TypeNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.complexTypeName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComplexTypeName([NotNull] ZScriptParser.ComplexTypeNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.primitiveType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimitiveType([NotNull] ZScriptParser.PrimitiveTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.callableType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCallableType([NotNull] ZScriptParser.CallableTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.listType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitListType([NotNull] ZScriptParser.ListTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.dictionaryType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionaryType([NotNull] ZScriptParser.DictionaryTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.callableTypeList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCallableTypeList([NotNull] ZScriptParser.CallableTypeListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.callableArgType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCallableArgType([NotNull] ZScriptParser.CallableArgTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleType([NotNull] ZScriptParser.TupleTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleTypeEntry"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleTypeEntry([NotNull] ZScriptParser.TupleTypeEntryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] ZScriptParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.assignmentExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentExpression([NotNull] ZScriptParser.AssignmentExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.multOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultOp([NotNull] ZScriptParser.MultOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.additionOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAdditionOp([NotNull] ZScriptParser.AdditionOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.bitwiseShift"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseShift([NotNull] ZScriptParser.BitwiseShiftContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.bitwiseAndOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseAndOp([NotNull] ZScriptParser.BitwiseAndOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.bitwiseXOrOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseXOrOp([NotNull] ZScriptParser.BitwiseXOrOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.bitwiseOrOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseOrOp([NotNull] ZScriptParser.BitwiseOrOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.relationalOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelationalOp([NotNull] ZScriptParser.RelationalOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.equalityOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEqualityOp([NotNull] ZScriptParser.EqualityOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.logicalAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalAnd([NotNull] ZScriptParser.LogicalAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.logicalOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalOr([NotNull] ZScriptParser.LogicalOrContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleExpression([NotNull] ZScriptParser.TupleExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleEntry"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleEntry([NotNull] ZScriptParser.TupleEntryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.newExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNewExpression([NotNull] ZScriptParser.NewExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.closureExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClosureExpression([NotNull] ZScriptParser.ClosureExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.prefixOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrefixOperator([NotNull] ZScriptParser.PrefixOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.postfixOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfixOperator([NotNull] ZScriptParser.PostfixOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.unaryOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryOperator([NotNull] ZScriptParser.UnaryOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.assignmentOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentOperator([NotNull] ZScriptParser.AssignmentOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.leftValue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLeftValue([NotNull] ZScriptParser.LeftValueContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.leftValueAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLeftValueAccess([NotNull] ZScriptParser.LeftValueAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.functionCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCall([NotNull] ZScriptParser.FunctionCallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.fieldAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFieldAccess([NotNull] ZScriptParser.FieldAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleAccess([NotNull] ZScriptParser.TupleAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.arrayAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayAccess([NotNull] ZScriptParser.ArrayAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.objectAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjectAccess([NotNull] ZScriptParser.ObjectAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.valueAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueAccess([NotNull] ZScriptParser.ValueAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.memberName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMemberName([NotNull] ZScriptParser.MemberNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.expressionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionList([NotNull] ZScriptParser.ExpressionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.arrayLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayLiteral([NotNull] ZScriptParser.ArrayLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.dictionaryLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionaryLiteral([NotNull] ZScriptParser.DictionaryLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.objectLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjectLiteral([NotNull] ZScriptParser.ObjectLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.stringLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringLiteral([NotNull] ZScriptParser.StringLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.dictionaryEntryList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionaryEntryList([NotNull] ZScriptParser.DictionaryEntryListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.dictionaryEntry"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionaryEntry([NotNull] ZScriptParser.DictionaryEntryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.tupleLiteralInit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTupleLiteralInit([NotNull] ZScriptParser.TupleLiteralInitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.arrayLiteralInit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayLiteralInit([NotNull] ZScriptParser.ArrayLiteralInitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.dictionaryLiteralInit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionaryLiteralInit([NotNull] ZScriptParser.DictionaryLiteralInitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.objectEntryList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjectEntryList([NotNull] ZScriptParser.ObjectEntryListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.objectEntryDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjectEntryDefinition([NotNull] ZScriptParser.ObjectEntryDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.entryName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEntryName([NotNull] ZScriptParser.EntryNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.compileConstant"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompileConstant([NotNull] ZScriptParser.CompileConstantContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.constantAtom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstantAtom([NotNull] ZScriptParser.ConstantAtomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.numericAtom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumericAtom([NotNull] ZScriptParser.NumericAtomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.hexadecimalNumber"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHexadecimalNumber([NotNull] ZScriptParser.HexadecimalNumberContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZScriptParser.binaryNumber"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinaryNumber([NotNull] ZScriptParser.BinaryNumberContext context);
}
