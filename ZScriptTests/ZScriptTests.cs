using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript;
using ZScript.CodeGeneration;

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
            string input = reader.ReadToEnd();
            reader.Close();

            Stopwatch sw = Stopwatch.StartNew();

            ZScriptGenerator generator = new ZScriptGenerator(input);

            generator.ParseInputString();

            Assert.IsFalse(generator.HasSyntaxErrors);

            Assert.Fail(sw.ElapsedMilliseconds + "");
        }
    }
}