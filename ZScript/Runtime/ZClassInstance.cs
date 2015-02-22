using ZScript.Elements;
using ZScript.Runtime.Execution.VirtualMemory;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents an instance of a ZClass
    /// </summary>
    public class ZClassInstance
    {
        /// <summary>
        /// The class this object is an instance of
        /// </summary>
        private readonly ZClass _class;

        /// <summary>
        /// The local memory for this class instance
        /// </summary>
        private readonly Memory _localMemory; 

        /// <summary>
        /// Gets the class this object is an instance of
        /// </summary>
        public ZClass Class
        {
            get { return _class; }
        }

        /// <summary>
        /// Gets the local memory for this class instance
        /// </summary>
        public Memory LocalMemory
        {
            get { return _localMemory; }
        }

        /// <summary>
        /// Initializes a new instance of the ZClassInstance class
        /// </summary>
        /// <param name="zClass">The class this object is an instance of</param>
        public ZClassInstance(ZClass zClass)
        {
            _class = zClass;
            _localMemory = new Memory();
        }
    }
}