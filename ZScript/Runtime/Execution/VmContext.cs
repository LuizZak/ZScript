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
        /// The addressed memory for this VM context
        /// </summary>
        public readonly IMemory<int> AddressedMemory;

        /// <summary>
        /// The current runtime associated with this VmContext
        /// </summary>
        public readonly ZRuntime Runtime;

        /// <summary>
        /// The owner of the runtime, used to perform some operations that require elevation to the owner domain (like instance creations with 'new')
        /// </summary>
        public readonly IRuntimeOwner Owner;

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, ZRuntime runtime)
            : this(memory, new IntegerMemory(), runtime, (runtime == null ? null : runtime.Owner))
        {

        }

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="addressedMemory">The addressed memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, IMemory<int> addressedMemory, ZRuntime runtime)
            : this(memory, addressedMemory, runtime, (runtime == null ? null : runtime.Owner))
        {

        }

        /// <summary>
        /// Initializes a new instance of the VmContext class
        /// </summary>
        /// <param name="memory">The memory for this VM context</param>
        /// <param name="addressedMemory">The addressed memory for this VM context</param>
        /// <param name="runtime">The current runtime to associate with this VmContext</param>
        /// <param name="owner">The owner to associate with this VmContext</param>
        public VmContext(IMemory<string> memory, IMemory<int> addressedMemory, ZRuntime runtime, IRuntimeOwner owner)
        {
            Memory = memory;
            AddressedMemory = addressedMemory;
            Runtime = runtime;
            Owner = owner;
        }
    }
}