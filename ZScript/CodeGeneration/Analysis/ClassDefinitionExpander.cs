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
using System.Linq;

using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Class capable of analyzing and concretizing class definitions
    /// </summary>
    public class ClassDefinitionExpander
    {
        /// <summary>
        /// The generation context to use when expanding the classes
        /// </summary>
        private readonly RuntimeGenerationContext _generationContext;

        /// <summary>
        /// The code scope containing the classes to expand
        /// </summary>
        private readonly CodeScope _baseScope;

        /// <summary>
        /// Initializes a new instance of the ClassDefinitionExpander class
        /// </summary>
        /// <param name="generationContext">The generation context for this static type analyzer</param>
        public ClassDefinitionExpander(RuntimeGenerationContext generationContext)
        {
            _baseScope = generationContext.BaseScope;

            _generationContext = generationContext;
        }

        /// <summary>
        /// Expands the classes on the generation context this object was initialized with
        /// </summary>
        public void Expand()
        {
            // Get the classes to expand
            var classes = _baseScope.GetDefinitionsByTypeRecursive<ClassDefinition>().ToArray();

            foreach (var classDefinition in classes)
            {
                SetupInheritance(classDefinition);
                VerifyConstructor(classDefinition);
            }
        }

        /// <summary>
        /// Verification supposed to be made after types have been expanded on the runtime generator
        /// </summary>
        public void PostVerification()
        {
            VerifyMethods();
            VerifyFields();
        }

        /// <summary>
        /// Verifies the validity of the methods from the classes on the generation context this object was initialized with
        /// </summary>
        private void VerifyMethods()
        {
            // Get the classes to expand
            var classes = _baseScope.GetDefinitionsByTypeRecursive<ClassDefinition>().ToArray();

            foreach (var classDefinition in classes)
            {
                CheckMethodCollisions(classDefinition);
            }
        }

        /// <summary>
        /// Verifies the validity of the fields from the classes on the generation context this object was initialized with
        /// </summary>
        private void VerifyFields()
        {
            // Get the classes to expand
            var classes = _baseScope.GetDefinitionsByTypeRecursive<ClassDefinition>().ToArray();

            foreach (var classDefinition in classes)
            {
                CheckFieldCollisions(classDefinition);
                VerifyFieldType(classDefinition);
            }
        }

        /// <summary>
        /// Updates the inheritance of the given class
        /// </summary>
        /// <param name="definition">The class definition to set the inheritance of</param>
        private void SetupInheritance(ClassDefinition definition)
        {
            // Detect inheritance
            ClassDefinition baseClass = null;

            if (definition.ClassContext.classInherit() != null)
            {
                string baseClassName = definition.ClassContext.classInherit().className().IDENT().GetText();

                baseClass = _baseScope.GetDefinitionByName<ClassDefinition>(baseClassName);

                if (baseClass == null)
                {
                    string message = "Cannot find class definition '" + baseClassName + "' to inherit from.";
                    _generationContext.MessageContainer.RegisterError(definition.ClassContext.classInherit(), message, ErrorCode.UndeclaredDefinition);
                }
            }

            // Test circular inheritance
            var c = baseClass;
            while (c != null)
            {
                if (c == definition)
                {
                    string message = "Circular inheritance detected: ";

                    // Iterate from baseClass until 'c'
                    c = baseClass;

                    while (c != definition && c != null)
                    {
                        message += "'" + c.Name + "' <- ";

                        c = c.BaseClass;
                    }

                    message += "'" + definition.Name + "' <- ";
                    message += "'" + baseClass.Name + "' <- ...";

                    _generationContext.MessageContainer.RegisterError(definition.ClassContext.classInherit(), message, ErrorCode.CircularInheritanceChain);

                    baseClass = null;
                    break;
                }

                c = c.BaseClass;
            }

            // Set inheritance
            definition.BaseClass = baseClass;
            // Set constructor inheritance
            definition.PublicConstructor.BaseMethod = (baseClass != null ? baseClass.PublicConstructor : null);

            // Setup method overloading
            var methods = definition.Methods;
            var inheritedMethods = definition.GetAllMethods(TypeMemberAttribute.Inherited);

            foreach (var method in methods)
            {
                // Ignore 'base' methods
                if (method.Name == "base")
                    continue;

                var method1 = method;
                var baseMethod = inheritedMethods.FirstOrDefault(m => m.Name == method1.Name);

                method.BaseMethod = baseMethod;
            }
        }

        /// <summary>
        /// Verifies the constructor of the given class
        /// </summary>
        /// <param name="definition">The class definition to verify the constructor of</param>
        private void VerifyConstructor(ClassDefinition definition)
        {
            if (definition.PublicConstructor.Context == null)
                return;

            var constructor = ((ZScriptParser.ClassMethodContext)definition.PublicConstructor.Context);
            if (constructor.functionDefinition().returnType() != null)
            {
                const string message = "Constructors cannot specify a return type";
                _generationContext.MessageContainer.RegisterError(constructor.functionDefinition().returnType(), message, ErrorCode.ReturnTypeOnConstructor);

                // Remove the constructor's type
                constructor.MethodDefinition.ReturnType = null;
                constructor.MethodDefinition.HasReturnType = false;
            }
        }

        /// <summary>
        /// Checks collisions of methods of a given class definition with its parent classes
        /// </summary>
        /// <param name="definition">The definition to check collisions on</param>
        private void CheckMethodCollisions(ClassDefinition definition)
        {
            var methods = definition.Methods;
            var inheritedMethods = definition.GetAllMethods(TypeMemberAttribute.Inherited);

            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];

                // Ignore 'base' methods
                if (method.Name == "base")
                    continue;

                // Check inner collisions
                for (int j = i + 1; j < methods.Length; j++)
                {
                    if (methods[j].Name == method.Name)
                    {
                        var message = "Duplicated method definition '" + method.Name + "'";
                        _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.DuplicatedDefinition);
                    }
                }

                var baseMethod = inheritedMethods.FirstOrDefault(m => m.Name == method.Name);

                method.BaseMethod = baseMethod;

                if (baseMethod != null)
                {
                    string message;

                    // Check overriden type signature matching
                    if (method.IsOverride)
                    {
                        if (baseMethod.CallableTypeDef != method.CallableTypeDef)
                        {
                            message = "Overriden method definition '" + method.Name + "' has a different signature than base method";
                            _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.MismatchedOverrideSignatures);
                        }

                        continue;
                    }

                    message = "Duplicated method definition '" + method.Name + "' not marked as override";
                    _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.DuplicatedDefinition);
                }
                // Missing override target
                else if (method.IsOverride)
                {
                    var message = "Method definition '" + method.Name + "' is marked as override but there's no base method to override";
                    _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.NoOverrideTarget);
                }
            }
        }

        /// <summary>
        /// Checks collisions of fields of a given class definition with its parent classes
        /// </summary>
        /// <param name="definition">The definition to check collisions on</param>
        private void CheckFieldCollisions(ClassDefinition definition)
        {
            var fields = definition.Fields;
            var inheritedFields = definition.GetAllFields(TypeMemberAttribute.Inherited);

            foreach (var field in fields)
            {
                var field1 = field;
                if (inheritedFields.Any(f => f.Name == field1.Name))
                {
                    var message = "Duplicated field definition '" + field.Name + "'";
                    _generationContext.MessageContainer.RegisterError(field.Context, message, ErrorCode.DuplicatedDefinition);
                }
            }
        }

        /// <summary>
        /// Verifies that the field of a class is compile-time resolvable
        /// </summary>
        /// <param name="definition">The definition to check collisions on</param>
        private void VerifyFieldType(ClassDefinition definition)
        {
            var fields = definition.Fields;

            foreach (var field in fields)
            {
                // Can only verify type-less fields
                if (field.HasType)
                    continue;

                if (field.TypeContext == null && !field.HasInferredType)
                {
                    var message = "Cannot resolve type for field '" + field.Name + "'. Type has to be explicitly stated or compile-time resolvable.";
                    _generationContext.MessageContainer.RegisterError(field.Context, message, ErrorCode.MissingFieldType);
                }
            }
        }
    }
}