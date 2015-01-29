namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a 
    /// </summary>
    public abstract class CodeMessage
    {
        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// The line the message is relevant at
        /// </summary>
        public int Line { get; protected set; }

        /// <summary>
        /// The offset in the line the message is relevant at
        /// </summary>
        public int Position { get; protected set; }
    }
}