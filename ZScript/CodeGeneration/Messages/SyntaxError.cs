namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents an structure that encapsulates syntax errors that were reported during the parsing of the script
    /// </summary>
    public class SyntaxError : CodeMessage
    {
        /// <summary>
        /// Initializes a new SyntaxError struct
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="position">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public SyntaxError(int line, int position, string message)
        {
            Position = position;
            Line = line;
            Message = message;
        }
    }
}