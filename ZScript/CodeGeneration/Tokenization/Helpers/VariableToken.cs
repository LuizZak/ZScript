using ZScript.CodeGeneration.Elements;
using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization.Helpers
{
    /// <summary>
    /// Specifies a token that points to a local/global variable in the memory
    /// </summary>
    public class VariableToken : Token
    {
        /// <summary>
        /// The name of the variable this token is referring to
        /// </summary>
        public string VariableName;

        /// <summary>
        /// Whether this variable token is pointing at a global (variable or function) definition
        /// </summary>
        public bool GlobalDefinition;

        /// <summary>
        /// The definition this variable token is pointing at
        /// </summary>
        public Definition PointingDefinition;

        /// <summary>
        /// Whether the variable is currently being get
        /// </summary>
        public bool IsGet;

        /// <summary>
        /// Initializes a new instance of the VariableToken class
        /// </summary>
        /// <param name="variableName">The name of the variable this token is referring to</param>
        /// <param name="isGet">Whether the variable is currently being get</param>
        public VariableToken(string variableName, bool isGet = true) : base(TokenType.MemberName, variableName)
        {
            VariableName = variableName;
            IsGet = isGet;
        }
    }
}