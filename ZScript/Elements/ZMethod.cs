using ZScript.Elements.ValueHolding;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a class method
    /// </summary>
    public class ZMethod : ZFunction
    {
        /// <summary>
        /// Gets or sets the local memory for the class running this method
        /// </summary>
        public Memory LocalMemory { get; set; }

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

        /// <summary>
        /// Performs a shallow clone of this ZMethod class, copying field values
        /// </summary>
        /// <returns>A shallow clone of this ZMethod class</returns>
        public ZMethod Clone()
        {
            var newMethod = (ZMethod)MemberwiseClone();

            return newMethod;
        }
    }

    /// <summary>
    /// Represents a function that when executed creates an instance of a class
    /// </summary>
    public class ZConstructor : ZMethod
    {
        /// <summary>
        /// The class instance this constructor will create
        /// </summary>
        private readonly ZClassInstance _classInstance;

        /// <summary>
        /// Gets the class instance this constructor will create
        /// </summary>
        public ZClassInstance ClassInstance
        {
            get { return _classInstance; }
        }

        /// <summary>
        /// Initializes a new instance of the ZConstructor class
        /// </summary>
        /// <param name="classInstance">The class instance to create a constructor of</param>
        public ZConstructor(ZClassInstance classInstance)
            : base(classInstance.Class.ClassName, classInstance.Class.Constructor.Tokens, classInstance.Class.Constructor.Arguments)
        {
            _classInstance = classInstance;
        }

        /// <summary>
        /// Performs the initialization of the fields of the class, using a given VmContext as context for the initialization
        /// </summary>
        public void InitFields(VmContext context)
        {
            foreach (var field in _classInstance.Class.Fields)
            {
                if (field.HasValue)
                {
                    // Execute a function VM and extract the value
                    var functionVm = new FunctionVM(field.Tokens, context);

                    functionVm.Execute();

                    _classInstance.LocalMemory.SetVariable(field.Name, functionVm.Stack.Peek());
                }
            }
        }
    }
}