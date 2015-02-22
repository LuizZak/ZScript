using ZScript.Elements.ValueHolding;

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a class method
    /// </summary>
    public class ZMethod : ZFunction
    {
        /// <summary>
        /// Initializes a new instance of the ZMethod class
        /// </summary>
        /// <param name="name">The name for the method</param>
        /// <param name="tokens">The tokens for the method</param>
        /// <param name="arguments">The arguments for the method call</param>
        public ZMethod(string name, TokenList tokens, FunctionArgument[] arguments)
            : base(name, tokens, arguments)
        {

        }
    }
}