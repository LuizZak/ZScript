namespace ZScript.CodeGeneration.Messages
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
                case ErrorCode.AccessUndeclaredDefinition:
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
        /// <summary>Specifies an error raised when a definitio that is undeclared is trying to be accessed</summary>
        AccessUndeclaredDefinition,

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

        #endregion

        #region Expression analysis errors

        /// <summary>Trying to perform a binary operation with a void type</summary>
        VoidOnBinaryExpression,
        /// <summary>Trying to perform a binary operation with non-compatible types</summary>
        InvalidTypesOnBinaryExpression,
        /// <summary>Trying to perform an invalid cast</summary>
        InvalidCast

        #endregion
    }
}