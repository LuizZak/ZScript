using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets the owner of this ZRuntime object
        /// </summary>
        public IRuntimeOwner Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Initializes a new instance of the ZRuntime class with an owner to specify
        /// </summary>
        /// <param name="owner">The owner of this ZRuntime</param>
        public ZRuntime(IRuntimeOwner owner)
        {
            _owner = owner;
        }
    }
}