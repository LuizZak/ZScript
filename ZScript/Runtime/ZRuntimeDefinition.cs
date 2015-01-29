using System.Collections.Generic;
using ZScript.CodeGeneration;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents a runtime definition that can be instantiated into a ZRuntime
    /// </summary>
    public class ZRuntimeDefinition
    {
        /// <summary>
        /// The list of function definitions
        /// </summary>
        private readonly List<FunctionDef> _functionDefinitions = new List<FunctionDef>();

        /// <summary>
        /// Gets an array of all the function definitions stored in this ZRuntimeDefinition
        /// </summary>
        public FunctionDef[] FunctionDefinitions
        {
            get { return _functionDefinitions.ToArray(); }
        }

        /// <summary>
        /// Adds a new function definition to this ZRuntimeDefinition calss
        /// </summary>
        /// <param name="def">A valid FunctionDef</param>
        public void AddFunctionDef(FunctionDef def)
        {
            _functionDefinitions.Add(def);
        }
    }
}