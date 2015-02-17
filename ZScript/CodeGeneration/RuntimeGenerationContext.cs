using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Defines a runtime generation context object that bundles a few objects with different responsibilities to be used by different code generation objects
    /// </summary>
    public class RuntimeGenerationContext
    {
        /// <summary>
        /// The message container to report errors and warnings to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// The internal type provider used to generate typing for the definitions
        /// </summary>
        private readonly TypeProvider _typeProvider;

        /// <summary>
        /// The base scope for the runtime generation, containg all of the combined definitions collected from the script sources
        /// </summary>
        private readonly CodeScope _baseScope;

        /// <summary>
        /// Gets the message container to report errors and warnings to
        /// </summary>
        public MessageContainer MessageContainer
        {
            get { return _messageContainer; }
        }

        /// <summary>
        /// Gets the internal type provider used to generate typing for the definitions
        /// </summary>
        public TypeProvider TypeProvider
        {
            get { return _typeProvider; }
        }

        /// <summary>
        /// Gets or sets the definition type provider for the code generation
        /// </summary>
        public IDefinitionTypeProvider DefinitionTypeProvider { get; set; }

        /// <summary>
        /// Gets the base scope for the runtime generation, containg all of the combined definitions collected from the script sources
        /// </summary>
        public CodeScope BaseScope
        {
            get { return _baseScope; }
        }

        /// <summary>
        /// Initializes a new instance of the RuntimeGenerationContext class
        /// </summary>
        /// <param name="baseScope">The base scope for the runtime generation, containg all of the combined definitions collected from the script sources</param>
        /// <param name="messageContainer">The message container to report errors and warnings to</param>
        /// <param name="typeProvider">The internal type provider used to generate typing for the definitions</param>
        /// <param name="definitionTypeProvider">The definition type provider for the code generation</param>
        public RuntimeGenerationContext(CodeScope baseScope = null, MessageContainer messageContainer = null,
            TypeProvider typeProvider = null, IDefinitionTypeProvider definitionTypeProvider = null)
        {
            _baseScope = baseScope;
            _messageContainer = messageContainer;
            _typeProvider = typeProvider;
            DefinitionTypeProvider = definitionTypeProvider;
        }
    }
}