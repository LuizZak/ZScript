namespace ZScript.Elements
{
    /// <summary>
    /// Describes a function definition that was read from the script and can be called by the runtime
    /// </summary>
    public class FunctionDef
    {
        /// <summary>
        /// Creates a new function definition with a given name
        /// </summary>
        /// <param name="name">The name for this function definition</param>
        public FunctionDef(string name)
        {
            Name = name;
            Tokens = new TokenList { Tokens = new Token[0] };
        }

        /// <summary>
        /// The name for this function definition
        /// </summary>
        public string Name;

        /// <summary>
        /// The tokens associated with this function definition
        /// </summary>
        public TokenList Tokens;
    }
}