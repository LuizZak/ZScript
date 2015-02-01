namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a message that is raised during code analysis
    /// </summary>
    public abstract class CodeMessage
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the line the message is relevant at
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the offset in the line the message is relevant at
        /// </summary>
        public int Position { get; set; }
    }
}