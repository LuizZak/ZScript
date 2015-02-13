namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Abstract class that can be used as a source of ZScript definitions
    /// </summary>
    public abstract class ZScriptDefinitionsSource
    {
        /// <summary>
        /// Gets or sets the definitions contained within this ZScript source
        /// </summary>
        public DefinitionsCollector Definitions { get; set; }

        /// <summary>
        /// Gets or sets a pre-parsed context for the contents of this file
        /// </summary>
        public ZScriptParser.ProgramContext Tree { get; set; }

        /// <summary>
        /// Gets or sets a value whether this script source has changed since the last definition collection and requires a re-parsing
        /// </summary>
        public virtual bool ParseRequired { get; set; }

        /// <summary>
        /// Gets the string containing the script source to parse
        /// </summary>
        /// <returns>The string containing the script source to parse</returns>
        public abstract string GetScriptSourceString();
    }
}