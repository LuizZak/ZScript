namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Represents a ZScript source that comes from a provided string
    /// </summary>
    public class ZScriptStringSource : ZScriptDefinitionsSource
    {
        /// <summary>
        /// The script string
        /// </summary>
        private string _script;

        /// <summary>
        /// Gets or sets the script string
        /// </summary>
        public string Script
        {
            get { return _script; }
            set
            {
                _script = value;
                ParseRequired = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ZScriptString class
        /// </summary>
        /// <param name="script">The string containing the script to create</param>
        public ZScriptStringSource(string script)
        {
            Script = script;
            ParseRequired = true;
        }

        /// <summary>
        /// Gets the script source for this ZScriptString
        /// </summary>
        /// <returns>The script source for this ZScriptString</returns>
        public override string GetScriptSourceString()
        {
            return Script;
        }
    }
}