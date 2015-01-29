using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript;
using ZScript.CodeGeneration;
using ZScript.Runtime;

namespace ZScriptTests
{
    /// <summary>
    /// Tests the functionality of the ZScript class and related components
    /// </summary>
    [TestClass]
    public class ZScriptTests
    {
        [TestMethod]
        public void TestCodeGeneration()
        {
            //const string input = "{ @__trace(args...) funca { __trace(10); } }";
            var reader = new StreamReader(@"C:\Users\Luiz Fernando\Desktop\ZHScript test 2.txt");
            var input = reader.ReadToEnd();
            reader.Close();

            var owner = new TestRuntimeOwner();

            var sw = Stopwatch.StartNew();

            var generator = new ZScriptGenerator(input);

            generator.ParseInputString();

            // Generate the runtime now
            generator.GenerateRuntime(owner);

            Assert.IsFalse(generator.HasSyntaxErrors);

            Assert.Fail(sw.ElapsedMilliseconds + "");
        }

        /// <summary>
        /// Runtime owner used in tests
        /// </summary>
        class TestRuntimeOwner : IRuntimeOwner
        {
            
        }
    }
}