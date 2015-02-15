using System.Collections.Generic;
using System.Linq;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Elements
{
    /// <summary>
    /// Specifies a function definition
    /// </summary>
    public class FunctionDefinition : Definition
    {
        /// <summary>
        /// The context containing the function body's statements
        /// </summary>
        private readonly ZScriptParser.FunctionBodyContext _bodyContext;

        /// <summary>
        /// An array of all the function arguments for this function
        /// </summary>
        private readonly FunctionArgumentDefinition[] _arguments;

        /// <summary>
        /// Cached callable definition
        /// </summary>
        private CallableTypeDef _callableTypeDef;

        /// <summary>
        /// The return type for this function
        /// </summary>
        private TypeDef _returnType;

        /// <summary>
        /// Gets or sets a value specifying whether this function definition has a return type associated with it
        /// </summary>
        public bool HasReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether this function definition has a void return value
        /// </summary>
        public bool IsVoid { get; set; }

        /// <summary>
        /// List of return statements present in this function definition
        /// </summary>
        public List<ZScriptParser.ReturnStatementContext> ReturnStatements;

        /// <summary>
        /// Gets the context containing the function body's statements
        /// </summary>
        public ZScriptParser.FunctionBodyContext BodyContext
        {
            get { return _bodyContext; }
        }

        /// <summary>
        /// Gets an array of all the function arguments for this function
        /// </summary>
        public FunctionArgumentDefinition[] Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        /// Gets or sets the return type for the function
        /// </summary>
        public TypeDef ReturnType
        {
            get { return _returnType; }
            set
            {
                _returnType = value;
                RecreateCallableDefinition();
            }
        }

        /// <summary>
        /// Gets or sets the return type context for the function
        /// </summary>
        public ZScriptParser.ReturnTypeContext ReturnTypeContext { get; set; }

        /// <summary>
        /// Gets the callable type definition associated with this function definition
        /// </summary>
        public CallableTypeDef CallableTypeDef
        {
            get { return _callableTypeDef; }
        }

        /// <summary>
        /// Initializes a new instance of the FunctionDefinition class
        /// </summary>
        /// <param name="name">The name for the definition</param>
        /// <param name="bodyContext">The context containing the function body's statements</param>
        /// <param name="arguments">The arguments for this function definition</param>
        public FunctionDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] arguments)
        {
            Name = name;
            ReturnStatements = new List<ZScriptParser.ReturnStatementContext>();
            _bodyContext = bodyContext;
            _arguments = arguments;
            RecreateCallableDefinition();
        }

        /// <summary>
        /// Recreates the callable definition for this function
        /// </summary>
        public void RecreateCallableDefinition()
        {
            _callableTypeDef = new CallableTypeDef(_arguments.Select(a => a.ToArgumentInfo()).ToArray(), ReturnType ?? TypeDef.VoidType, HasReturnType);
        }
    }
}