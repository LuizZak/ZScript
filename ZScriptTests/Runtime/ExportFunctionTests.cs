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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

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