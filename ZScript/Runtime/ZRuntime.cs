using ZScript.CodeGeneration;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents a runtime that is encapsulated
    /// </summary>
    public class ZRuntime
    {
        /// <summary>
        /// The owner of this ZRuntime
        /// </summary>
        private readonly IRuntimeOwner _owner;

        /// <summary>
        /// The list of all functions defined in this ZRuntime instance
        /// </summary>
        private FunctionDef[] _functionDefs;

        /// <summary>
        /// Gets the owner of this ZRuntime object
        /// </summary>
        public IRuntimeOwner Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Initializes a new instance of the ZRuntime class from the following definition, and with an owner to specify
        /// </summary>
        /// <param name="definition">The runtime definition object to create this runtime from</param>
        /// <param name="owner">The owner of this ZRuntime</param>
        public ZRuntime(ZRuntimeDefinition definition, IRuntimeOwner owner)
        {
            _functionDefs = definition.FunctionDefinitions;
            _owner = owner;
        }
    }
}