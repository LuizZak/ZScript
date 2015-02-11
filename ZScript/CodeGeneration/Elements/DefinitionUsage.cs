using Antlr4.Runtime;
using ZScript.CodeGeneration.Analysis;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Declares the usage context of a definition
    /// </summary>
    public class DefinitionUsage
    {
        /// <summary>
        /// The definition that was used
        /// </summary>
        private readonly Definition _definition;

        /// <summary>
        /// The parser rule context that represents the definition's usage
        /// </summary>
        private readonly ParserRuleContext _context;

        /// <summary>
        /// Gets the definition that was used
        /// </summary>
        public Definition Definition
        {
            get { return _definition; }
        }

        /// <summary>
        /// Gets the parser rule context that represents the definition's usage
        /// </summary>
        public ParserRuleContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets or sets the scope that contains this definition usage
        /// </summary>
        public CodeScope Scope { get; set; }

        /// <summary>
        /// Initializes a new instance of the DefinitionUsage class
        /// </summary>
        /// <param name="definition">The definition that was used</param>
        /// <param name="context">The context in which the definition was used</param>
        public DefinitionUsage(Definition definition, ParserRuleContext context)
        {
            _definition = definition;
            _context = context;
        }
    }
}