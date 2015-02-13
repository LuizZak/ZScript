using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Sourcing;

namespace ZScriptTests.CodeGeneration
{
    /// <summary>
    /// Tests functionality for generating a script that is split into multiple different sources, like files
    /// </summary>
    [TestClass]
    public class MultipleSourcesTests
    {
        #region Parsing

        [TestMethod]
        public void TestMultipleSourcesParsing()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("func f1() { }");
            var source2 = new ZScriptStringSource("func f2() { }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var container = generator.MessageContainer;
            var definition = generator.GenerateRuntimeDefinition();

            Assert.AreEqual(2, definition.ZFunctionDefinitions.Length, "The definitions where not collected as expected");
            Assert.IsFalse(container.HasErrors, "Errors raised when not expected");
        }

        [TestMethod]
        public void TestCrossSourceFunctionParsing()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("func f1() { f2(); }");
            var source2 = new ZScriptStringSource("func f2() { f1(); }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var container = generator.MessageContainer;
            generator.GenerateRuntimeDefinition();

            Assert.IsFalse(container.HasErrors, "Errors raised when not expected");
        }

        [TestMethod]
        public void TestCrossSourceExportFunctionParsing()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("@_f1() func f1() { _f2(); }");
            var source2 = new ZScriptStringSource("@_f2() func f2() { _f1(); }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var container = generator.MessageContainer;
            generator.GenerateRuntimeDefinition();

            Assert.IsFalse(container.HasErrors, "Errors raised when not expected");
        }

        [TestMethod]
        public void TestCrossSourceGlobalVariableParsing()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("[ a; ] func f1() { a = b; }");
            var source2 = new ZScriptStringSource("[ b; ] func f2() { b = a; }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var container = generator.MessageContainer;
            generator.GenerateRuntimeDefinition();

            Assert.IsFalse(container.HasErrors, "Errors raised when not expected");
        }

        [TestMethod]
        public void TestGetSourceByContext()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("func f1() { }");
            var source2 = new ZScriptStringSource("func f2() { }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var definition = generator.CollectDefinitions();

            // Search the sources
            var s1 = generator.SourceProvider.SourceForContext(definition.GetDefinitionByName("f1").Context);
            var s2 = generator.SourceProvider.SourceForContext(definition.GetDefinitionByName("f2").Context);

            Assert.AreEqual(source1, s1, "The search source by context failed to locate the correct source for a definition");
            Assert.AreEqual(source2, s2, "The search source by context failed to locate the correct source for a definition");
        }

        [TestMethod]
        public void TestReportCollisions()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("func f1() { }");
            var source2 = new ZScriptStringSource("func f1() { }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var container = generator.MessageContainer;

            try
            {
                generator.GenerateRuntimeDefinition();
            }
            catch (Exception)
            {
                // ignored
            }

            Assert.AreEqual(2, container.CodeErrors.Count(c => c.ErrorCode == ErrorCode.DuplicatedDefinition), "Duplicated definition errors where not reported correctly");
        }

        #endregion

        #region Execution

        [TestMethod]
        public void TestCrossSourceFunctionExecution()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("[ a;] func f1() { f2(); }");
            var source2 = new ZScriptStringSource("func f2() { a = 10; }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var runtime = generator.GenerateRuntime(new TestRuntimeOwner());
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");

            Assert.AreEqual(10, memory.GetVariable("a"), "Cross reference of functions failed to be executed properly");
        }

        [TestMethod]
        public void TestCrossSourceGlobalVariableExecution()
        {
            var generator = new ZRuntimeGenerator();

            var source1 = new ZScriptStringSource("[ a = 10; ] func f1() { a += b; }");
            var source2 = new ZScriptStringSource("[ b = 20; ] func f2() { b += a; }");

            // Add a few sources
            generator.SourceProvider.AddSource(source1);
            generator.SourceProvider.AddSource(source2);

            var runtime = generator.GenerateRuntime(new TestRuntimeOwner());
            var memory = runtime.GlobalMemory;

            runtime.CallFunction("f1");
            runtime.CallFunction("f2");

            Assert.AreEqual(30, memory.GetVariable("a"), "Cross reference of functions failed to be executed properly");
            Assert.AreEqual(50, memory.GetVariable("b"), "Cross reference of functions failed to be executed properly");
        }

        #endregion
    }
}