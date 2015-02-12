namespace ZScript.CodeGeneration.Messages
{
    /// <summary>
    /// Represents a warning that was generated during the generation of the code
    /// </summary>
    public class Warning : CodeMessage
    {
        /// <summary>
        /// Gets or sets the warning code for this warning
        /// </summary>
        public WarningCode Code { get; set; }

        /// <summary>
        /// Initializes a new instance of the Warning class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        public Warning(int line, int column, string message)
            : this(line, column, message, WarningCode.Undefined)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Warning class
        /// </summary>
        /// <param name="line">The line the error occurred at</param>
        /// <param name="column">The offset in the line the error occurred at</param>
        /// <param name="message">The message for the error</param>
        /// <param name="code">The code for the warning</param>
        public Warning(int line, int column, string message, WarningCode code)
        {
            Column = column;
            Line = line;
            Message = message;
            Code = code;
        }
    }

    /// <summary>
    /// Specifies the code for a warning
    /// </summary>
    public enum WarningCode
    {
        /// <summary>An undefined warning</summary>
        Undefined,
        /// <summary>A definition is created, but never user</summary>
        UnusedDefinition,
        /// <summary>A definition is created, but its value is only set and never get</summary>
        DefinitionOnlySet,

        /// <summary>Trying to access an object that is not a list with subscription</summary>
        TryingToSubscriptNonList,
        /// <summary>Trying to call an object that is not a callable</summary>
        TryingToCallNonCallable
    }
}