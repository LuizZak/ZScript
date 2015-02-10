namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a warning that was generated during the generation of the code
    /// </summary>
    public class Warning : CodeMessage
    {
        /// <summary>
        /// Initializes a new instance of the Warning class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="position">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public Warning(int line, int position, string message)
        {
            Position = position;
            Line = line;
            Message = message;
        }
    }
}