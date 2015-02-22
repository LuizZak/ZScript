using ZScript.Elements.ValueHolding;

namespace ZScript.Elements
{
    /// <summary>
    /// Represents a processed class read from a script source
    /// </summary>
    public class ZClass
    {
        /// <summary>
        /// The name for the class
        /// </summary>
        private readonly string _className;

        /// <summary>
        /// The methods for this class
        /// </summary>
        private readonly ZMethod[] _methods;

        /// <summary>
        /// The fields for this class
        /// </summary>
        private readonly ZClassField[] _fields;

        /// <summary>
        /// Gets the name for the class
        /// </summary>
        public string ClassName
        {
            get { return _className; }
        }

        /// <summary>
        /// Gets the methods for this class
        /// </summary>
        public ZMethod[] Methods
        {
            get { return _methods; }
        }

        /// <summary>
        /// Gets the fields for this class
        /// </summary>
        public ZClassField[] Fields
        {
            get { return _fields; }
        }

        /// <summary>
        /// Initializes a new instance of the ZClass class
        /// </summary>
        /// <param name="className">The name for the class</param>
        /// <param name="methods">The array of methods for the class</param>
        public ZClass(string className, ZMethod[] methods, ZClassField[] fields)
        {
            _className = className;
            _methods = methods;
            _fields = fields;
        }
    }

    /// <summary>
    /// Represents a variable for a ZClass
    /// </summary>
    public class ZClassField : Variable
    {
        /// <summary>
        /// The tokens to execute to initialize the variable's value when the class is created
        /// </summary>
        public TokenList Tokens { get; set; }

        /// <summary>
        /// Initializes a new instance of the ZClassField class
        /// </summary>
        /// <param name="tokens">The tokens to execute to initialize the variable's value when the class is created</param>
        public ZClassField(TokenList tokens)
        {
            Tokens = tokens;
        }
    }
}