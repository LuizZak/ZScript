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
using Xunit;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis.Definitions;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the functionality of the FunctionParametersAnalyzer class
    /// </summary>
    public class FunctionParametersAnalyzerTests
    {
        /// <summary>
        /// Tests a valid function definition
        /// </summary>
        [Fact]
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

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests a valid function definition with no parameters
        /// </summary>
        [Fact]
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

            Assert.Equal(0, container.CodeErrors.Length);
        }

        /// <summary>
        /// Tests an invalid function definition that contains an optional paramter that comes before a required one
        /// </summary>
        [Fact]
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

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidParameters));
        }

        /// <summary>
        /// Tests an invalid function definition that contains a variadic paramter that is not the last parameter
        /// </summary>
        [Fact]
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

            Assert.Equal(1, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.InvalidParameters));
        }
    }
}