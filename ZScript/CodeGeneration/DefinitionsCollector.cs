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
        /// The current scope for the definitions
        /// </summary>
        private CodeScope _currentScope;

        /// <summary>
        /// Stack of classes used to define methods and members inside classes
        /// </summary>
        private Stack<ClassDefinition> _classStack;

        /// <summary>
        /// Stack of sequences used to define methods and members inside sequences
        /// </summary>
        private Stack<SequenceDefinition> _sequenceStack;

        /// <summary>
        /// Stack of type aliasses used to define methods and members inside type aliases
        /// </summary>
        private Stack<TypeAliasDefinition> _typeAliasStack;

        /// <summary>
        /// Stack of functions used to define arguments
        /// </summary>
        private Stack<FunctionDefinition> _functionStack; 

        /// <summary>
        /// Gets the collected base scope containing the scopes defined
        /// </summary>
        public CodeScope CollectedBaseScope { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ScopeCollector class
        /// </summary>
        /// <param name="messageContainer">The message container that errors will be reported to</param>
        public DefinitionsCollector(MessageContainer messageContainer)
        {
            CollectedBaseScope = new CodeScope();
            _currentScope = CollectedBaseScope;
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
            _sequenceStack = new Stack<SequenceDefinition>();
            _functionStack = new Stack<FunctionDefinition>();
            _typeAliasStack = new Stack<TypeAliasDefinition>();

            var walker = new ParseTreeWalker();
            walker.Walk(this, context);
        }

        #region Scope collection

        // Function definitions have their own scope, containing the parameters
        public override void EnterProgram(ZScriptParser.ProgramContext context)
        {
            // Push the default global scope
            //PushScope(context);
            _currentScope = CollectedBaseScope = new CodeScope { Context = context };
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
                _functionStack.Push(DefineTopLevelFunction(context));

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

            // Add a definition pointing to the base overriden function.
            // The analysis of whether this is a valid call is done in a separate analysis phase
            DefineMethod(new MethodDefinition("base", null, new FunctionArgumentDefinition[0]) { Class = GetClassScope() });
        }

        public override void ExitClassDefinition(ZScriptParser.ClassDefinitionContext context)
        {
            PopScope();

            _classStack.Pop().FinishDefinition();
        }

        public override void EnterSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            _sequenceStack.Push(DefineSequence(context));

            PushScope(context);
        }

        public override void ExitSequenceBlock(ZScriptParser.SequenceBlockContext context)
        {
            _sequenceStack.Pop();

            PopScope();
        }

        public override void EnterSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            _functionStack.Push(DefineSequenceFrame(context));

            PushScope(context);
        }

        public override void ExitSequenceFrame(ZScriptParser.SequenceFrameContext context)
        {
            _functionStack.Pop();

            PopScope();
        }

        public override void EnterForStatement(ZScriptParser.ForStatementContext context)
        {
            PushScope(context);
        }

        public override void ExitForStatement(ZScriptParser.ForStatementContext context)
        {
            PopScope();
        }

        public override void EnterSwitchStatement(ZScriptParser.SwitchStatementContext context)
        {
            PushScope(context);
        }

        public override void ExitSwitchStatement(ZScriptParser.SwitchStatementContext context)
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
            if (!IsInGlobalScope() && GetNearestValueHoldingDefinition() is FunctionDefinition)
                DefineLocalVariable(context);
        }

        public override void EnterClassField(ZScriptParser.ClassFieldContext context)
        {
            DefineClassField(context);
        }

        public override void EnterFunctionArg(ZScriptParser.FunctionArgContext context)
        {
            DefineFunctionArgument(context);
        }

        public override void EnterClassBody(ZScriptParser.ClassBodyContext context)
        {
            DefineHiddenVariable("this").IsConstant = true;

            PushScope(context);
        }

        public override void ExitClassBody(ZScriptParser.ClassBodyContext context)
        {
            PopScope();
        }

        public override void EnterSequenceBody(ZScriptParser.SequenceBodyContext context)
        {
            DefineHiddenVariable("this").IsConstant = true;
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
            _typeAliasStack.Push(DefineTypeAlias(context));

            PushScope(context);
        }

        public override void ExitTypeAlias(ZScriptParser.TypeAliasContext context)
        {
            _typeAliasStack.Pop();

            PopScope();
        }

        public override void EnterClassMethod(ZScriptParser.ClassMethodContext context)
        {
            PushScope(context);

            // Define the method
            _functionStack.Push(DefineMethod(context));
        }

        public override void ExitClassMethod(ZScriptParser.ClassMethodContext context)
        {
            PopScope();

            _functionStack.Pop();
        }

        public override void EnterTypeAliasVariable(ZScriptParser.TypeAliasVariableContext context)
        {
            DefineTypeAliasField(context);
        }

        public override void EnterTypeAliasFunction(ZScriptParser.TypeAliasFunctionContext context)
        {
            DefineTypeAliasFunction(context);
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
        /// Returns a definition that describes the nearest definition in the definition scopes that can hold variables.
        /// Returns null when none are found
        /// </summary>
        /// <returns>A definition that describes the nearest definition in the definition scopes that can hold variables, or null, when none are found</returns>
        Definition GetNearestValueHoldingDefinition()
        {
            // Traverse up the scopes
            CodeScope scope = _currentScope;

            while (scope != null)
            {
                if (scope.Context is ZScriptParser.ProgramContext)
                    return null;

                if (scope.Context is ZScriptParser.FunctionBodyContext ||
                    scope.Context is ZScriptParser.BlockStatementContext)
                    return _functionStack.Peek();

                if (scope.Context is ZScriptParser.ClassBodyContext ||
                    scope.Context is ZScriptParser.SequenceBodyContext)
                {
                    return scope.ParentScope.GetDefinitionByContextRecursive(scope.Context);
                }

                scope = scope.ParentScope;
            }

            return null;
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
        /// Returns the sequence definition associated with the current sequence being parsed.
        /// If the collector is currently not inside a sequence context, null is returned
        /// </summary>
        /// <returns>A sequence definition for the current scope</returns>
        SequenceDefinition GetSequenceScope()
        {
            return _sequenceStack.Count > 0 ? _sequenceStack.Peek() : null;
        }

        /// <summary>
        /// Returns the type alias definition associated with the current sequence being parsed.
        /// If the collector is currently not inside a type alias context, null is returned
        /// </summary>
        /// <returns>A type alias definition for the current scope</returns>
        TypeAliasDefinition GetTypeAliasScope()
        {
            return _typeAliasStack.Count > 0 ? _typeAliasStack.Peek() : null;
        }

        /// <summary>
        /// Defines a new variable in the current top-most scope
        /// </summary>
        /// <param name="variable">The context containing the variable to define</param>
        void DefineLocalVariable(ZScriptParser.ValueHolderDeclContext variable)
        {
            var def = DefinitionGenerator.GenerateLocalVariable(variable);

            def.IsInstanceValue = IsInInstanceScope();

            CheckCollisions(def, variable);

            _currentScope.AddDefinition(def);
        }

        /// <summary>
        /// Defines a field in a class
        /// </summary>
        /// <param name="context">The context containing the field to define</param>
        private void DefineClassField(ZScriptParser.ClassFieldContext context)
        {
            var def = DefinitionGenerator.GenerateClassField(context);

            def.IsInstanceValue = IsInInstanceScope();

            CheckCollisions(def, context);

            _currentScope.AddDefinition(def);

            if (GetSequenceScope() != null)
                GetSequenceScope().AddField(def);
            else
                GetClassScope().AddField(def);
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
        ValueHolderDefinition DefineHiddenVariable(string variableName)
        {
            var def = new GlobalVariableDefinition { Name = variableName };

            CheckCollisions(def, null);

            _currentScope.AddDefinition(def);

            return def;
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
        TopLevelFunctionDefinition DefineTopLevelFunction(ZScriptParser.FunctionDefinitionContext function)
        {
            var def = DefinitionGenerator.GenerateTopLevelFunctionDef(function);

            CheckCollisions(def, function.functionName());

            DefineTopLevelFunction(def);

            return def;
        }
        
        /// <summary>
        /// Defines a new function in the current top-most scope
        /// </summary>
        /// <param name="function">The function to define</param>
        void DefineTopLevelFunction(TopLevelFunctionDefinition function)
        {
            _currentScope.AddDefinition(function);
        }

        /// <summary>
        /// Defines a new method in the current top-most scope
        /// </summary>
        /// <param name="method">The method to define</param>
        void DefineMethod(MethodDefinition method)
        {
            // Constructor detection
            if (method.Name == GetClassScope().Name)
            {
                var constructor = new ConstructorDefinition(GetClassScope(), method.BodyContext, method.Parameters, false)
                {
                    Context = method.Context,
                    HasReturnType = method.HasReturnType,
                    IdentifierContext = method.IdentifierContext
                };
                ((ZScriptParser.ClassMethodContext)method.Context).MethodDefinition = constructor;

                if (GetClassScope().PublicConstructor != null)
                {
                    _messageContainer.RegisterError(method.BodyContext, "Duplicated constructor definition for class '" + method.Name + "'", ErrorCode.DuplicatedDefinition);
                }

                GetClassScope().PublicConstructor = constructor;

                _currentScope.AddDefinition(constructor);
            }
            else
            {
                GetClassScope().AddMethod(method);

                _currentScope.AddDefinition(method);
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
            CollectedBaseScope.AddDefinition(def);

            return def;
        }

        /// <summary>
        /// Defines a new class definition in the current top-most scope
        /// </summary>
        /// <param name="classDefinition">The class to define</param>
        /// <returns>The class that was defined</returns>
        ClassDefinition DefineClass(ZScriptParser.ClassDefinitionContext classDefinition)
        {
            var def = new ClassDefinition(classDefinition.className().IDENT().GetText())
            {
                Context = classDefinition,
                ClassContext = classDefinition,
                IdentifierContext = classDefinition.className()
            };
            
            _currentScope.AddDefinition(def);

            classDefinition.ClassDefinition = def;

            return def;
        }

        /// <summary>
        /// Defines a new sequence definition in the current top-most scope
        /// </summary>
        /// <param name="sequence">The sequence to define</param>
        /// <returns>The sequence that was defined</returns>
        SequenceDefinition DefineSequence(ZScriptParser.SequenceBlockContext sequence)
        {
            var def = new SequenceDefinition(sequence.sequenceName().IDENT().GetText())
            {
                Context = sequence,
                SequenceContext = sequence,
                IdentifierContext = sequence.sequenceName()
            };

            _currentScope.AddDefinition(def);

            sequence.SequenceDefinition = def;

            return def;
        }

        /// <summary>
        /// Defines a new sequence frame definition in the current top-most scope
        /// </summary>
        /// <param name="frame">The sequence frame to define</param>
        /// <returns>The sequence frame that was defined</returns>
        void DefineSequenceFrame(SequenceFrameDefinition frame)
        {
            _currentScope.AddDefinition(frame);

            var seq = GetSequenceScope();

            if(seq != null)
                seq.AddFrame(frame);
        }

        /// <summary>
        /// Defines a new sequence frame definition in the current top-most scope
        /// </summary>
        /// <param name="frame">The sequence frame to define</param>
        /// <returns>The sequence frame that was defined</returns>
        SequenceFrameDefinition DefineSequenceFrame(ZScriptParser.SequenceFrameContext frame)
        {
            var def = DefinitionGenerator.GenerateSequenceFrameDef(frame);

            def.Sequence = GetSequenceScope();

            CheckCollisions(def, frame.frameName());

            DefineSequenceFrame(def);

            return def;
        }

        /// <summary>
        /// Defines a new type alias definition in the current top-most scope
        /// </summary>
        /// <param name="typeAlias">The type alias to define</param>
        TypeAliasDefinition DefineTypeAlias(ZScriptParser.TypeAliasContext typeAlias)
        {
            var def = TypeAliasDefinitionGenerator.GenerateTypeAlias(typeAlias);

            _currentScope.AddTypeAliasDefinition(def);

            return def;
        }

        /// <summary>
        /// Defines a new type alias field in the current type alias context
        /// </summary>
        /// <param name="variable">The variable to define in the type alias</param>
        void DefineTypeAliasField(ZScriptParser.TypeAliasVariableContext variable)
        {
            var def = TypeAliasDefinitionGenerator.GenerateTypeField(variable);

            var typeAlias = GetTypeAliasScope();

            _currentScope.AddDefinition(def);

            typeAlias.AddField(def);
        }

        /// <summary>
        /// Defines a new type alias field in the current type alias context
        /// </summary>
        /// <param name="method">The variable to define in the type alias</param>
        void DefineTypeAliasFunction(ZScriptParser.TypeAliasFunctionContext method)
        {
            var def = TypeAliasDefinitionGenerator.GenerateTypeMethod(method);

            var typeAlias = GetTypeAliasScope();

            _currentScope.AddDefinition(def);

            typeAlias.AddMethod(def);
        }

        /// <summary>
        /// Checks collisions with the specified definition against the definitions in the available scopes
        /// </summary>
        /// <param name="definition">The definition to check</param>
        /// <param name="context">A context used during analysis to report where the error happened</param>
        void CheckCollisions(Definition definition, ParserRuleContext context)
        {
            // Unlabeled sequence frame definitions do not collide with other definitions
            if (definition is SequenceFrameDefinition && definition.Name == "")
                return;

            var defs = _currentScope.GetDefinitionsByName(definition.Name);

            foreach (var d in defs)
            {
                // Collisions between exported functions and normal functions are ignored
                if (d is ExportFunctionDefinition && !(definition is ExportFunctionDefinition))
                    continue;

                // Constructor definition
                if (d is ClassDefinition && definition is MethodDefinition && ((MethodDefinition)definition).Class == d)
                    continue;

                // Shadowing of global variables
                if (d is GlobalVariableDefinition != definition is GlobalVariableDefinition)
                    continue;

                // Type field shadowing
                if (d is TypeFieldDefinition && (definition is LocalVariableDefinition || definition is FunctionArgumentDefinition) ||
                    definition is TypeFieldDefinition && (d is LocalVariableDefinition || d is FunctionArgumentDefinition))
                    continue;

                // Value holder and class/sequence name
                if (d is ValueHolderDefinition && (definition is ClassDefinition || definition is SequenceDefinition) ||
                    definition is ValueHolderDefinition && (d is ClassDefinition || d is SequenceDefinition))
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
    }
}