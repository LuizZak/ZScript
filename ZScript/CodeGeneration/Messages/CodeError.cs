﻿namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents an error that was raised due to an error in the code
    /// </summary>
    public class CodeError : CodeMessage
    {
        /// <summary>
        /// Gets or sets the code for this error
        /// </summary>
        public ErrorCode ErrorCode;

        /// <summary>
        /// Initializes a new instance of the CodeError class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public CodeError(int line, int column, string message)
        {
            Column = column;
            Line = line;
            Message = message;
            ErrorCode = ErrorCode.Undefined;
        }

        /// <summary>
        /// Initializes a new instance of the SyntaxError class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="errorCode">The error type to raise</param>
        public CodeError(int line, int column, ErrorCode errorCode) : this(line, column, MessageForErrorType(errorCode))
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Returns the error message for the specified code error type
        /// </summary>
        /// <param name="errorType">A valid error type</param>
        /// <returns>The error message for the specified code error type</returns>
        public static string MessageForErrorType(ErrorCode errorType)
        {
            switch (errorType)
            {
                case ErrorCode.UndeclaredDefinition:
                    return "Trying to access undeclared definition";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Enumeration of possible code error types
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>An undefined error</summary>
        Undefined,
        /// <summary>Specifies an error raised when a definition that is undeclared is trying to be accessed</summary>
        UndeclaredDefinition,
        /// <summary>Specifies an error raised when a definition is found with the same name of another definition in the same reachable scope</summary>
        DuplicatedDefinition,

        /// <summary>Specifies an error raised when a constant definition has no starting value assigned to it</summary>
        ValuelessConstantDeclaration,

        #region Return statement analysis errors

        /// <summary>Some returns on a function have values, some do not</summary>
        InconsistentReturns,
        /// <summary>Some returns on a function have values, and some paths do not have returns</summary>
        IncompleteReturnPathsWithValuedReturn,
        /// <summary>Not all code paths return a value</summary>
        IncompleteReturnPaths,
        /// <summary>A return statement is missing a value in a non-void function</summary>
        MissingReturnValueOnNonvoid,
        /// <summary>A return statement contains a value in a void function</summary>
        ReturningValueOnVoidFunction,
        /// <summary>A function contains a valued return statement, but has a void return type</summary>
        MissingReturnTypeOnFunction,

        #endregion

        #region Expression analysis errors

        /// <summary>Trying to perform a binary operation with a void type</summary>
        VoidOnBinaryExpression,
        /// <summary>Trying to perform a binary operation with non-compatible types</summary>
        InvalidTypesOnOperation,
        /// <summary>Trying to perform an invalid cast</summary>
        InvalidCast,
        /// <summary>Trying to provide less arguments than a callable requires</summary>
        TooFewArguments,
        /// <summary>Trying to provide more arguments than a callable accepts</summary>
        TooManyArguments,

        #endregion

        #region Statement analysis errors

        /// <summary>Break statement not inside a for/while/switch statement context</summary>
        NoTargetForBreakStatement,
        /// <summary>Continue statement not inside a for/while statement context</summary>
        NoTargetForContinueStatement,

        /// <summary>Case label has constant value that is already defined in a previous case label of a switch statement</summary>
        RepeatedCaseLabelValue,

        /// <summary>Trying to modify the value of a constant declaration, either through assignment or through postfix/prefix operators</summary>
        ModifyingConstant

        #endregion
    }
}