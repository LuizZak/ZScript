using ZScript.Elements.ValueHolding;

namespace ZScript.Elements
{
    /// <summary>
    /// Specifies a function that when called fires a method call to the runtime owner
    /// </summary>
    public class ZExportFunction : ZFunction
    {
        /// <summary>
        /// Initializes a new instance of the ZExportFunction class
        /// </summary>
        /// <param name="name">The name for the export function</param>
        /// <param name="arguments">The arguments for the export function</param>
        public ZExportFunction(string name, FunctionArgument[] arguments) : base(name, null, arguments)
        {

        }
    }
}