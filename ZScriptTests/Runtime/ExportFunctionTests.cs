using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests export function calls
    /// </summary>
    [TestClass]
    public class ExportFunctionTests
    {
        #region Parsing

        [TestMethod]
        public void TestExportFunctionParse()
        {
            const string input = "@f2(i)";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();

            Assert.IsTrue(definition.ZExportFunctionDefinitions.Any(f => f.Name == "f2"));
        }

        [TestMethod]
        public void TestExportFunctionCallInFunctionParse()
        {
            const string input = "@f2(i) func f() { f2(10); }";
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var definition = generator.GenerateRuntimeDefinition();
            
            Assert.IsTrue(definition.ZExportFunctionDefinitions.Any(f => f.Name == "f2"));
        }

        #endregion

        #region Execution

        [TestMethod]
        public void TestExportFunctionCall()
        {
            const string input = "@__trace(i) func f() { __trace(10); __trace(11); }";

            var owner = new TestRuntimeOwner();
            var generator = Utils.TestUtils.CreateGenerator(input);
            generator.ParseSources();
            var runtime = generator.GenerateRuntime(owner);

            runtime.CallFunction("f");

            Assert.AreEqual((long)10, owner.TraceObjects[0], "The export function was not called correctly");
            Assert.AreEqual((long)11, owner.TraceObjects[1], "The export function was not called correctly");
        }

        #endregion
    }
}