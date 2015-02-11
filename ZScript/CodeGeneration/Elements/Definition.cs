using Antlr4.Runtime;
using ZScript.CodeGeneration.Analysis;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a common definition
    /// </summary>
    public class Definition
    {
        /// <summary>
        /// Gets or sets the name for this definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the context for the definition
        /// </summary>
        public ParserRuleContext Context { get; set; }

        /// <summary>
        /// Gets or sets the scope that contains this definition
        /// </summary>
        public CodeScope Scope { get; set; }
    }
}