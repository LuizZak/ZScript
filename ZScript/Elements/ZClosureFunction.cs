using ZScript.Elements.ValueHolding;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies a closure function
    /// </summary>
    public class ZClosureFunction : ZFunction
    {
        /// <summary>
        /// Gets or sets the currently captured memory for this closure function
        /// </summary>
        public MemoryMapper CapturedMemory { get; set; }

        /// <summary>
        /// Initializes a new instance of the ZClosureFunction class
        /// </summary>
        /// <param name="name">The name for the closure</param>
        /// <param name="tokens">The list of tokens representing the closure's body</param>
        /// <param name="arguments">The list of arguments for the closure</param>
        public ZClosureFunction(string name, TokenList tokens, FunctionArgument[] arguments)
            : base(name, tokens, arguments)
        {
            IsClosure = true;
        }

        /// <summary>
        /// Performs a shallow clone of this ZClosureFunction object
        /// </summary>
        /// <returns>A shallow copy of this ZClosureFunction object</returns>
        public ZClosureFunction Clone()
        {
            return new ZClosureFunction(Name, Tokens, Arguments)
            {
                CapturedMemory = CapturedMemory == null ? CapturedMemory : CapturedMemory.Clone()
            };
        }
    }
}