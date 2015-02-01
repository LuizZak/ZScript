namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents an error that was raised due to an error in the code
    /// </summary>
    public class CodeError : CodeMessage
    {
        /// <summary>
        /// Initializes a new instance of the CodeError class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="position">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public CodeError(int line, int position, string message)
        {
            Position = position;
            Line = line;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the SyntaxError class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="position">The offset in the line the error occurred at</param>
        /// <param name="errorType">The error type to raise</param>
        public CodeError(int line, int position, CodeErrorType errorType) : this(line, position, MessageForErrorType(errorType))
        {
            
        }

        /// <summary>
        /// Returns the error message for the specified code error type
        /// </summary>
        /// <param name="errorType">A valid error type</param>
        /// <returns>The error message for the specified code error type</returns>
        public static string MessageForErrorType(CodeErrorType errorType)
        {
            switch (errorType)
            {
                case CodeErrorType.AccessUndeclaredVariable:
                    return "Trying to access undeclared variable";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Enumeration of possible code error types
        /// </summary>
        public enum CodeErrorType
        {
            /// <summary>
            /// Specifies an error raised when a variable that is undeclared is trying to be accessed
            /// </summary>
            AccessUndeclaredVariable
        }
    }
}