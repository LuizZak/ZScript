using ZScript.CodeGeneration.Elements.Typing;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a variable definition
    /// </summary>
    public class FunctionArgumentDefinition : ValueHolderDefinition
    {
        /// <summary>
        /// Gets or sets a value specifying whether the function argument represented by this definition is variadic in nature.
        /// Variadic arguments allow for the caller to specify as many values as desired, separated by commas like normal arguments.
        /// Functions can only have one variadic argument, appearing at the end of the argument list
        /// </summary>
        public bool IsVariadic { get; set; }

        /// <summary>
        /// The compile-time constant defining the value for the function argument
        /// </summary>
        public ZScriptParser.CompileConstantContext DefaultValue { get; set; }

        /// <summary>
        /// Creates an argument information based on the information of this function argument
        /// </summary>
        /// <returns>A callble argument information generated from this function argument</returns>
        public CallableTypeDef.CallableParameterInfo ToArgumentInfo()
        {
            return new CallableTypeDef.CallableParameterInfo(Type, HasType, HasValue, IsVariadic);
        }
    }
}