using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis.Definitions
{
    /// <summary>
    /// Provides functionalities for checking the validity of function parameters
    /// </summary>
    public class FunctionParametersAnalyzer
    {
        /// <summary>
        /// The context for this analysis
        /// </summary>
        private readonly RuntimeGenerationContext _context;

        /// <summary>
        /// Initializes a new instance of the FunctionParametersAnalyzer class
        /// </summary>
        /// <param name="context">The context for the analysis</param>
        public FunctionParametersAnalyzer(RuntimeGenerationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Analysis the definitions sotred on a given context
        /// </summary>
        public void Analyze()
        {
            var definitions = _context.BaseScope.GetDefinitionsByTypeRecursive<FunctionDefinition>();

            // Analyze each function parameter individually
            foreach (var functionDefinition in definitions)
            {
                AnalyzeFunction(functionDefinition);
            }
        }

        /// <summary>
        /// Analyzes a given function's parameters
        /// </summary>
        /// <param name="func">The function definition to analyze</param>
        public void AnalyzeFunction(FunctionDefinition func)
        {
            bool endRequiredList = false;
            for (int i = 0; i < func.Parameters.Length; i++)
            {
                // Check for non-last parameter variadic arguments
                if (i < func.Parameters.Length - 1)
                {
                    if (func.Parameters[i].IsVariadic)
                    {
                        _context.MessageContainer.RegisterError(func.Parameters[i].Context, "Functions can contain only one variadic parameter that must be defined as the last paramter", ErrorCode.InvalidParameters);
                    }
                }

                // Check for required/optional parameter order
                if (func.Parameters[i].HasValue && !endRequiredList)
                {
                    endRequiredList = true;
                    continue;
                }
                
                if (!endRequiredList)
                    continue;

                if (!func.Parameters[i].HasValue && !func.Parameters[i].IsVariadic)
                {
                    _context.MessageContainer.RegisterError(func.Parameters[i].Context, "All required parameters must appear before the first required optional parameter", ErrorCode.InvalidParameters);
                }
            }
        }
    }
}