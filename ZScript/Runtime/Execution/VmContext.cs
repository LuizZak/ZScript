using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Runtime.Execution
{
    /// <summary>
    /// Defines the context of a function virtual machine
    /// </summary>
    public class VmContext
    {
        /// <summary>
        /// The memory for this VM context
        /// </summary>
        public readonly IMemory<string> Memory;

        /// <summary>
        /// The current runtime associated with this VmContext
        /// </summary>
        public readonly ZRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, ZRuntime runtime)
        {
            Memory = memory;
            Runtime = runtime;
        }
    }
}