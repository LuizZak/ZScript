using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Performs analysis in scopes to guarantee that variables are scoped correctly
    /// </summary>
    public class ScopeAnalyzer : ZScriptBaseListener
    {
        /// <summary>
        /// A list of all the errors raised during the current analysis
        /// </summary>
        private List<CodeError> _errorList = new List<CodeError>();

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private Stack<VariableScope> _scopes;

        /// <summary>
        /// An array of all the errors that were found
        /// </summary>
        public CodeError[] Errors
        {
            get { return _errorList.ToArray(); }
        }

        /// <summary>
        /// Analyzes a given program context for undeclared variable errors
        /// </summary>
        /// <param name="context">The context of the program to analyze</param>
        public void AnalyzeProgram(ZScriptParser.ProgramContext context)
        {
            _errorList = new List<CodeError>();
            _scopes = new Stack<VariableScope>();

            ParseTreeWalker walker = new ParseTreeWalker();

            walker.Walk(this, context);
        }

        // Function definitions have their own scope, containing the parameters
        public override void EnterFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            PushScope();
        }

        public override void ExitFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            PopScope();
        }

        public override void EnterFunctionArg(ZScriptParser.FunctionArgContext context)
        {
            // Define the function argumenta
            var name = context.argumentName().IDENT().GetText();
            DefineVariable(name);
        }

        public override void EnterBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PushScope();
        }

        public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PopScope();
        }

        public override void EnterValueHolderDecl(ZScriptParser.ValueHolderDeclContext context)
        {
            var name = context.valueHolderName().IDENT().GetText();
            DefineVariable(name);
        }

        public override void EnterExpression(ZScriptParser.ExpressionContext context)
        {
            // First entrance
            if (context.memberName() != null)
            {
                var name = context.memberName().IDENT().GetText();

                if (!IsVariableDefined(name))
                {
                    RegistervariableNotFound(context.memberName());
                }
            }
        }

        public override void EnterAssignmentExpression(ZScriptParser.AssignmentExpressionContext context)
        {
            // First entrance
            if (context.leftValue().memberName() != null)
            {
                var name = context.leftValue().memberName().IDENT().GetText();

                if (!IsVariableDefined(name))
                {
                    RegistervariableNotFound(context.leftValue().memberName());
                }
            }
        }

        /// <summary>
        /// Pushes a new scope of variables
        /// </summary>
        void PushScope()
        {
            _scopes.Push(new VariableScope());
        }
        
        /// <summary>
        /// Pops the current scope, removing along all the variables from the current scope
        /// </summary>
        void PopScope()
        {
            _scopes.Pop();
        }

        /// <summary>
        /// Defines a new variable in the current top-most scope
        /// </summary>
        /// <param name="variableName">The name of the variable to define</param>
        void DefineVariable(string variableName)
        {
            var def = new VariableDefinition {Name = variableName};

            _scopes.Peek().Variables.Add(def);
        }

        /// <summary>
        /// Returns whether a given variable name is defined in any of the current scopes
        /// </summary>
        /// <param name="variableName">The variable name to verify</param>
        bool IsVariableDefined(string variableName)
        {
            return _scopes.Any(s => s.Variables.Any(v => v.Name == variableName));
        }

        /// <summary>
        /// Registers a new variable not found error
        /// </summary>
        /// <param name="member">The member containing the name of the variable that was not found</param>
        void RegistervariableNotFound(ZScriptParser.MemberNameContext member)
        {
            CodeError error = new CodeError(member.IDENT().Symbol.Line, member.IDENT().Symbol.StartIndex, CodeError.CodeErrorType.AccessUndeclaredVariable);
            error.Message += " '" + member.IDENT().GetText() + "'";

            _errorList.Add(error);
        }

        /// <summary>
        /// Defines a variables scope
        /// </summary>
        class VariableScope
        {
            /// <summary>
            /// The current variables in this scope
            /// </summary>
            public List<VariableDefinition> Variables = new List<VariableDefinition>();
        }

        /// <summary>
        /// Specifies a variable definition
        /// </summary>
        class VariableDefinition
        {
            /// <summary>
            /// The name for this variable
            /// </summary>
            public string Name;
        }
    }
}