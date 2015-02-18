using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Elements;
using ZScript.CodeGeneration.Messages;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the FunctionParametersAnalyzer class
    /// </summary>
    [TestClass]
    public class FunctionParametersAnalyzerTests
    {
        /// <summary>
        /// Tests a valid function definition
        /// </summary>
        [TestMethod]
        public void TestValidFunction()
        {
            // Create a test function definition to use
            var arguments = new[]
            {
                new FunctionArgumentDefinition { Name = "parameter1", HasValue = false },
                new FunctionArgumentDefinition { Name = "parameter2", HasValue = true },
                new FunctionArgumentDefinition { Name = "parameter3", IsVariadic = true },
            };

            var function = new FunctionDefinition("func1", null, arguments);

            var container = new MessageContainer();
            var analyzer = new FunctionParametersAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeFunction(function);

            Assert.AreEqual(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests a valid function definition with no parameters
        /// </summary>
        [TestMethod]
        public void TestValidParameterlessFunction()
        {
            // Create a test function definition to use
            var arguments = new FunctionArgumentDefinition[]
            {

            };

            var function = new FunctionDefinition("func1", null, arguments);

            var container = new MessageContainer();
            var analyzer = new FunctionParametersAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeFunction(function);

            Assert.AreEqual(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests an invalid function definition that contains an optional paramter that comes before a required one
        /// </summary>
        [TestMethod]
        public void TestOptionalBeforeRequired()
        {
            // Create a test function definition to use
            var arguments = new[]
            {
                new FunctionArgumentDefinition { Name = "parameter1", HasValue = true },
                new FunctionArgumentDefinition { Name = "parameter2", HasValue = false }
            };

            var function = new FunctionDefinition("func1", null, arguments);

            var container = new MessageContainer();
            var analyzer = new FunctionParametersAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeFunction(function);

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidParameters));
        }

        /// <summary>
        /// Tests an invalid function definition that contains a variadic paramter that is not the last parameter
        /// </summary>
        [TestMethod]
        public void TestVariadicBeforeLastParameter()
        {
            // Create a test function definition to use
            var arguments = new[]
            {
                new FunctionArgumentDefinition { Name = "parameter1", IsVariadic = true },
                new FunctionArgumentDefinition { Name = "parameter2" }
            };

            var function = new FunctionDefinition("func1", null, arguments);

            var container = new MessageContainer();
            var analyzer = new FunctionParametersAnalyzer(new RuntimeGenerationContext(messageContainer: container));

            analyzer.AnalyzeFunction(function);

            Assert.AreEqual(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidParameters));
        }
    }
}