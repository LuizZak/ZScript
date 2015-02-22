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
        /// Expands the classes on the generation context this class was initialized with
        /// </summary>
        public void Expand()
        {
            // Get the classes to expand
            var classes = _baseScope.GetDefinitionsByTypeRecursive<ClassDefinition>().ToArray();

            foreach (var classDefinition in classes)
            {
                SetupInheritance(classDefinition);
            }

            foreach (var classDefinition in classes)
            {
                CheckMethodCollisions(classDefinition);
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
        }

        /// <summary>
        /// Checks collisions of methods of a given class definition with its parent classes
        /// </summary>
        /// <param name="definition">The definition to check collisions on</param>
        private void CheckMethodCollisions(ClassDefinition definition)
        {
            var methods = definition.Methods;
            var inheritedMethods = definition.GetAllMethods(true);

            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];

                // Ignore 'base' methods
                if (method.Name == "base")
                    continue;

                // Check inner collisions
                for (int j = i + 1; j < methods.Count; j++)
                {
                    if (methods[j].Name == method.Name)
                    {
                        var message = "Duplicated method definition '" + method.Name + "'";
                        _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.DuplicatedDefinition);
                    }
                }

                if (inheritedMethods.Any(m => m.Name == method.Name))
                {
                    // Check if method is not marked as override
                    if (method.IsOverride)
                        continue;

                    var message = "Duplicated method definition '" + method.Name + "' not marked as override";
                    _generationContext.MessageContainer.RegisterError(method.Context, message, ErrorCode.DuplicatedDefinition);
                }
            }
        }
    }
}