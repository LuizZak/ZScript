using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Runtime;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the unused definitions analyzer class and related components
    /// </summary>
    [TestClass]
    public class UnusedDefinitionAnalyzerTests
    {
        /// <summary>
        /// Tests the analyzer by providing a scope with a definition with no definition usages associated with it
        /// </summary>
        [TestMethod]
        public void TestUnusedReporting()
        {
            var definition = GenerateDefinition();
            var scope = new CodeScope();
            var messages = new MessageContainer();

            scope.AddDefinition(definition);

            UnusedDefinitionsAnalyzer.Analyze(scope, messages);

            Assert.AreEqual(1, messages.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests the analyzer by providing a scope with a definition with no definition usages associated with it
        /// </summary>
        [TestMethod]
        public void TestUnusedNestedReporting()
        {
            var definition = GenerateDefinition();
            var usage = new DefinitionUsage(definition, null);

            var scope = new CodeScope();
            var childScope = new CodeScope();
            var messages = new MessageContainer();

            // Add the edfinition on the child scope, but the usage on the parent scope
            scope.AddSubscope(childScope);
            childScope.AddDefinition(definition);
            scope.AddDefinitionUsage(usage);

            UnusedDefinitionsAnalyzer.AnalyzeRecursive(scope, messages);

            Assert.AreEqual(1, messages.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests the analyzer by providing a scope with a definition and a definition usage associated with it
        /// </summary>
        [TestMethod]
        public void TestUnusedNonReporting()
        {
            var definition = GenerateDefinition();
            var usage = new DefinitionUsage(definition, null);
            var scope = new CodeScope();
            var messages = new MessageContainer();

            scope.AddDefinition(definition);
            scope.AddDefinitionUsage(usage);

            UnusedDefinitionsAnalyzer.Analyze(scope, messages);

            Assert.AreEqual(0, messages.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests the analyzer by providing a scope with a definition and a subscope containing the definition usage in it
        /// </summary>
        [TestMethod]
        public void TestUnusedNestedNonReporting()
        {
            var definition = GenerateDefinition();
            var usage = new DefinitionUsage(definition, null);

            var scope = new CodeScope();
            var childScope = new CodeScope();
            var messages = new MessageContainer();

            // Add the edfinition on the child scope, but the usage on the parent scope
            scope.AddSubscope(childScope);
            scope.AddDefinition(definition);
            childScope.AddDefinitionUsage(usage);

            UnusedDefinitionsAnalyzer.Analyze(scope, messages);

            Assert.AreEqual(0, messages.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests reporting an unused definition on a real analyze context
        /// </summary>
        [TestMethod]
        public void TestParseUnusedReporting()
        {
            const string input = "func f() { var a = 5; var b; var c = b; c = b; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests reporting an unused definition on a real analyze context
        /// </summary>
        [TestMethod]
        public void TestParsedUnusedNonReporting()
        {
            const string input = "func f() { var a = 5; var b; var c = a + b; if(c == 10) { a = b.a; } }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(0, generator.MessageContainer.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Tests reporting a definition that is only set on a real analyze context
        /// </summary>
        [TestMethod]
        public void TestParseOnlySetReporting()
        {
            const string input = "func f() { var a = 5; var b = a; b = 5; b = a; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.Warnings.Count(w => w.Code == WarningCode.DefinitionOnlySet));
        }

        /// <summary>
        /// Tests reporting an object definition that is never used
        /// </summary>
        [TestMethod]
        public void TestUnusedObjectReporting()
        {
            const string input = "object o { var a; }";
            var generator = ZRuntimeTests.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            Assert.AreEqual(1, generator.MessageContainer.Warnings.Count(w => w.Code == WarningCode.UnusedDefinition));
        }

        /// <summary>
        /// Generates a generic definition that can be used in tests
        /// </summary>
        private static ValueHolderDefinition GenerateDefinition()
        {
            const string input = "var b;";
            var parser = ZRuntimeTests.CreateParser(input);

            return new ValueHolderDefinition { Name = "Test", Context = parser.valueDecl() };
        }
    }
}