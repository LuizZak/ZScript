using ZScript.Elements.ValueHolding;

namespace ZScript.Elements
{
    /// <summary>
    /// Describes a function that can be executed by the runtime
    /// </summary>
    public class ZFunction
    {
        /// <summary>
        /// Creates a new function definition with a given name
        /// </summary>
        /// <param name="name">The name for this function definition</param>
        /// <param name="tokens">A list of tokens containing the instructions this function will perform</param>
        /// <param name="arguments">An array of the arguments for this function</param>
        public ZFunction(string name, TokenList tokens, FunctionArgument[] arguments)
        {
            Name = name;
            Tokens = tokens;
            _arguments = arguments;
        }

        /// <summary>
        /// The name for this function definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the tokens associated with this function definition
        /// </summary>
        public TokenList Tokens { get; private set; }

        /// <summary>
        /// Gets or sets a value specifying whether this function is a closure function type
        /// </summary>
        public bool IsClosure { get; set; }

        /// <summary>
        /// An array of the arguments for this function
        /// </summary>
        private readonly FunctionArgument[] _arguments;

        /// <summary>
        /// Gets an array of the arguments for this function
        /// </summary>
        public FunctionArgument[] Arguments { get { return _arguments; } }
    }
}