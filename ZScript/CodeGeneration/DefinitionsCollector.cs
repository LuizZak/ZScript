#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion

using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration
{
    /// <summary>
    /// Class capable of creating a tree of definition scopes, containing variables, functions and objects for contexts
    /// </summary>
    public class DefinitionsCollector : ZScriptBaseListener
    {
        /// <summary>
        /// The count of closures that were created
        /// </summary>
        private int _closuresCount;

        /// <summary>
        /// The message container that errors will be reported to
        /// </summary>
        private readonly MessageContainer _messageContainer;

        /// <summary>
        /// The current stack of variable scopes
        /// </summary>
        private CodeScope _baseScope;

        /// <summary>
        /// The current scope for the definitions
        /// </summary>
        private CodeScope _currentScope;

        /// <summary>
        /// Stack of classes used to define methods and members inside classes
        /// </summary>
        private Stack<ClassDefinition> _classStack;

        /// <summary>
        /// Stack of functions used to define arguments
        /// </summary>
        private Stack<FunctionDefinition> _functionStack; 

        /// <summary>
        /// Gets the collected base scope containing the scopes defined
        /// </summary>
        public CodeScope CollectedBaseScope
        {
            get { return _baseScope; }
        }

        /// <summary>
        /// Initializes a new instance of the ScopeCollector class
        /// </summary>
        /// <param name="messageContainer">The message container that errors will be reported to</param>
        public DefinitionsCollector(MessageContainer messageContainer)
        {
            _baseScope = new CodeScope();
            _currentScope = _baseScope;
            _messageContainer = messageContainer;
        }

        /// <summary>
        /// Analyzes a given subset of a parser rule context on this definitions collector
        /// </summary>
        /// <param name="context">The context to analyze</param>
        public void Collect(ParserRuleContext context)
        {
            // Clear stacks
            _classStack = new Stack<ClassDefinition>();
            _functionStack = new Stack<FunctionDefinition>();

            var walker = new ParseTreeWalker();
            walker.Walk(this, context);
        }

        #region Scope collection

        // Function definitions have their own scope, containing the parameters
        public override void EnterProgram(ZScriptParser.ProgramContext context)
        {
            // Push the default global scope
            //PushScope(context);
            _currentScope = _baseScope = new CodeScope { Context = context };
        }

        public override void EnterExportDefinition(ZScriptParser.ExportDefinitionContext context)
        {
            // Define the function
            _functionStack.Push(DefineExportFunction(context));

            PushScope(context);
        }

        public override void ExitExportDefinition(ZScriptParser.ExportDefinitionContext context)
        {
            PopScope();

            _functionStack.Pop();
        }

        public override void EnterFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            // Define the function, but only if it's not a class method
            if(!(context.Parent is ZScriptParser.ClassMethodContext))
                _functionStack.Push(DefineFunction(context));

            PushScope(context);
        }

        public override void ExitFunctionDefinition(ZScriptParser.FunctionDefinitionContext context)
        {
            PopScope();

            // Pop the function stack
            if (!(context.Parent is ZScriptParser.ClassMethodContext))
                _functionStack.Pop();
        }

        public override void EnterBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PushScope(context);
        }

        public override void ExitBlockStatement(ZScriptParser.BlockStatementContext context)
        {
            PopScope();
        }

        public override void EnterClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            _classStack.Push(DefineClass(context));

            PushScope(context);
        }

        public override void ExitClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            PopScope();

            _classStack.Pop().FinishDefinition();
        }

        public override void EnterSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            PushScope(context);
        }

        public override void ExitSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            PopScope();
        }

        public override void EnterSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            PushScope(context);
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            PopScope();
        }

        #endregion

        #region Definition collection

        public override void EnterGlobalVariable(ZScriptParser.GlobalVariableContext context)
        {
            DefineGlobalVariable(context);
        }

        public override void EnterValueHolderDecl(ZScriptParser.ValueHolderDeclContext context)
        {
            if (!IsInGlobalScope())
                DefineValueHolder(context);
        }

        public override void EnterFunctionArg(ZScriptParser.FunctionArgContext context)
        {
            DefineFunctionArgument(context);
        }

        public override void EnterClassBody(ZScriptParser.ClassBodyContext context)
        {
            DefineHiddenVariable("this");

            PushScope(context);
        }

        public override void ExitClassBody(ZScriptParser.ClassBodyContext context)
        {
            PopScope();
        }

        public override void EnterSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            DefineHiddenVariable("this");
            DefineHiddenVariable("T");
            DefineHiddenVariable("async");

            PushScope(context);
        }

        public override void ExitSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            PopScope();
        }

        public override void EnterClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            _functionStack.Push(DefineClosure(context));

            PushScope(context);
        }

        public override void ExitClosureExpression(ZScriptParser.ClosureExpressionContext context)
        {
            PopScope();
            // Pop the scope
            _functionStack.Pop();
        }

        public override void EnterTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            DefineTypeAlias(context);
            PushScope(context);
        }

        public override void ExitTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            PopScope();
        }

        public override void EnterClassMethod(ZScriptParser.ClassMethodContext context)
        {
            PushScope(context);

            // Add a definition pointing to the base overriden function.
            // The analysis of whether this is a valid call is done in a separate analysis phase
            DefineMethod(new MethodDefinition("base", null, new FunctionArgumentDefinition[0]) { Class = GetClassScope() });

            // Define the method
            _functionStack.Push(DefineMethod(context));
        }

        public override void ExitClassMethod(ZScriptParser.ClassMethodContext context)
        {
            PopScope();

            _functionStack.Pop();
        }

        #endregion

        /// <summary>
        /// Pushes a new scope of variables
        /// </summary>
        /// <param name="context">A context binded to the scope</param>
        void PushScope(ParserRuleContext context)
        {
            var newScope = new CodeScope { Context = context };

            _currentScope.AddSubscope(newScope);
            _currentScope = newScope;
        }

        /// <summary>
        /// Pops the current scope, removing along all the variables from the current scope
        /// </summary>
        void PopScope()
        {
            _currentScope = _currentScope.ParentScope;
        }

        /// <summary>
        /// Returns whether the current scope is the global scope
        /// </summary>
        /// <returns>A boolean value specifying whether the current scope is the global scope</returns>
        bool IsInGlobalScope()
        {
            return _currentScope.Context is ZScriptParser.ProgramContext;
        }

        /// <summary>
        /// Returns whether the current scope is an instance (object or sequence) scope
        /// </summary>
        /// <returns>A boolean value specifying whether the current scope is instance (object or sequence) scope</returns>
        bool IsInInstanceScope()
        {
            return _classStack.Count > 0 || _currentScope.Context is ZScriptParser.ClassBodyContext ||
                   _currentScope.Context is ZScriptParser.SequenceBodyContext;
        }

        /// <summary>
        /// Returns the class definition associated with the current class being parsed.
        /// If the collector is currently not inside a class context, null is returned
        /// </summary>
        /// <returns>A class definition for the current scope</returns>
        ClassDefinition GetClassScope()
        {
            return _classStack.Count > 0 ? _classStack.Peek() : null;
        }

        /// <summary>
        /// Defines a new variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The context containing the variable to define</param>
        void DefineValueHolder(ZScriptParser.ValueHolderDeclContext variable)
        {
            var def = DefinitionGenerator.GenerateValueHolderDef(variable);

            def.IsInstanceValue = IsInInstanceScope();

            CheckCollisions(def, variable);

            _currentScope.AddDefinition(def);

            // Class field collection
            if (GetClassScope() != null)
            {
                GetClassScope().Fields.Add(def);
            }
        }

        /// <summary>
        /// Defines a new global variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The global variable to define</param>
        void DefineGlobalVariable(ZScriptParser.GlobalVariableContext variable)
        {
            var def = DefinitionGenerator.GenerateGlobalVariable(variable);

            if (!def.HasValue && def.IsConstant)
            {
                _messageContainer.RegisterError(variable, "Constants require a value to be assigned on declaration", ErrorCode.ValuelessConstantDeclaration);
            }

            CheckCollisions(def, variable);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a function argument on the top-most scope
        /// </summary>
        /// <param name="argument">The argument to define</param>
        void DefineFunctionArgument(ZScriptParser.FunctionArgContext argument)
        {
            var def = DefinitionGenerator.GenerateFunctionArgumentDef(argument);

            CheckCollisions(def, argument.argumentName());

            // Try to find the definition in the current function definition for the argument, and store that instead
            var funcDef = _functionStack.Peek();

            _currentScope.AddDefinition(funcDef.Parameters.First(a => a.Context == argument));
        }

        /// <summary>
        /// Defines a variable that is hidden, that is, it can be accessed, but does not comes from the script source
        /// </summary>
        /// <param name="variableName">The name of the variable to define</param>
        void DefineHiddenVariable(string variableName)
        {
            var def = new ValueHolderDefinition { Name = variableName };

            CheckCollisions(def, null);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a new export function on the current top-most scope
        /// </summary>
        /// <param name="exportFunction">The export function to define</param>
        ExportFunctionDefinition DefineExportFunction(ZScriptParser.ExportDefinitionContext exportFunction)
        {
            var def = DefinitionGenerator.GenerateExportFunctionDef(exportFunction);

            CheckCollisions(def, exportFunction.functionName());

            _currentScope.AddDefinition(def);

            return def;
        }

        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        FunctionDefinition DefineFunction(ZScriptParser.FunctionDefinitionContext function)
        {
            var def = DefinitionGenerator.GenerateFunctionDef(function);

            CheckCollisions(def, function.functionName());

            DefineFunction(def);

            return def;
        }
        
        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineFunction(FunctionDefinition function)
        {
            _currentScope.AddDefinition(function);
        }

        /// <summary>
        /// Defines a new method in the current top-most scope
        /// </summary>
        /// <param name="method">The method to define</param>
        void DefineMethod(MethodDefinition method)
        {
            // Define the function inside a class context as a method, if any class context is available
            if (GetClassScope() == null)
            {
                _currentScope.AddDefinition(method);
                return;
            }

            // Constructor detection
            if (method.Name == GetClassScope().Name)
            {
                var constructor = new ConstructorDefinition(GetClassScope(), method.BodyContext, method.Parameters);

                if (GetClassScope().PublicConstructor != null)
                {
                    _messageContainer.RegisterError(method.BodyContext, "Duplicated constructor definition for class '" + method.Name + "'", ErrorCode.DuplicatedDefinition);
                }

                GetClassScope().PublicConstructor = constructor;

                _currentScope.AddDefinition(constructor);
            }
            else
            {
                GetClassScope().Methods.Add(method);
            }
        }

        /// <summary>
        /// Defines a new class method in the current top-most scope
        /// </summary>
        /// <param name="method">The method to define</param>
        MethodDefinition DefineMethod(ZScriptParser.ClassMethodContext method)
        {
            var def = DefinitionGenerator.GenerateMethodDef(method);

            def.Class = GetClassScope();

            CheckCollisions(def, method.functionDefinition().functionName());

            DefineMethod(def);

            return def;
        }

        /// <summary>
        /// Defines a new closure in the current top-most scope
        /// </summary>
        /// <param name="closure">The closure to define</param>
        ClosureDefinition DefineClosure(ZScriptParser.ClosureExpressionContext closure)
        {
            var def = DefinitionGenerator.GenerateClosureDef(closure);

            def.Name = ClosureDefinition.ClosureNamePrefix + (_closuresCount++);

            // Closures are always defined at the base scope
            _baseScope.AddDefinition(def);

            return def;
        }

        /// <summary>
        /// Defines a new class definition in the current top-most scope
        /// </summary>
        /// <param name="classDefinition">The object to define</param>
        /// <returns>The class that was defined</returns>
        ClassDefinition DefineClass(ZScriptParser.ClassDefinitionContext classDefinition)
        {
            var def = new ClassDefinition(classDefinition.className().IDENT().GetText())
            {
                Context = classDefinition,
                ClassContext = classDefinition
            };

            _currentScope.AddDefinition(def);

            return def;
        }

        /// <summary>
        /// Defines a new type alias definition in the current top-most scope
        /// </summary>
        /// <param name="typeAlias">The type alias to define</param>
        void DefineTypeAlias(ZScriptParser.TypeAliasContext typeAlias)
        {
            var def = TypeAliasDefinitionGenerator.GenerateTypeAlias(typeAlias);

            _currentScope.AddTypeAliasDefinition(def);
        }

        /// <summary>
        /// Checks collisions with the specified definition against the definitions in the available scopes
        /// </summary>
        /// <param name="definition">The definition to check</param>
        /// <param name="context">A context used during analysis to report where the error happened</param>
        void CheckCollisions(Definition definition, ParserRuleContext context)
        {
            var defs = _currentScope.GetDefinitionsByName(definition.Name);

            foreach (var d in defs)
            {
                // Collisions between exported definitions are ignored
                if (d is ExportFunctionDefinition && !(definition is ExportFunctionDefinition))
                    continue;

                // Constructor definition
                if (d is ClassDefinition && definition is FunctionDefinition && IsContextChildOf(definition.Context, d.Context))
                    continue;

                int defLine = definition.Context == null ? 0 : definition.Context.Start.Line;
                int defColumn = definition.Context == null ? 0 : definition.Context.Start.Column;

                int dLine = d.Context == null ? 0 : d.Context.Start.Line;
                int dColumn = d.Context == null ? 0 : d.Context.Start.Column;

                string message = "Duplicated definition of '" + definition.Name + "' at line " + defLine +
                                 " column " + defColumn + " collides with definition " + d + " at line " + dLine +
                                 " column " + dColumn;

                _messageContainer.RegisterError(context, message, ErrorCode.DuplicatedDefinition);
            }
        }

        /// <summary>
        /// Returns whether a rule context is child of another context.
        /// </summary>
        /// <param name="context">The context to check</param>
        /// <param name="parent">The context to check for parenting</param>
        /// <returns>Whether 'context' is child of 'parent'</returns>
        bool IsContextChildOf(RuleContext context, RuleContext parent)
        {
            while (context != null)
            {
                context = context.Parent;

                if (context == parent)
                    return true;
            }

            return false;
        }
    }
}