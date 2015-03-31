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

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration
{
    /// <summary>
    /// Tests generic definition collection with the DefinitionCollector class
    /// </summary>
    [TestClass]
    public class GenericDefinitionCollectingTests
    {
        /// <summary>
        /// Tests collection of a generic top-level function definition
        /// </summary>
        [TestMethod]
        public void TestCollectGenericFunction()
        {
            const string input = "func f1<T>() { } func f2 { }";

            var parser = TestUtils.CreateParser(input);
            var container = new MessageContainer();
            var collector = new DefinitionsCollector(container);

            collector.Collect(parser.program());

            var scope = collector.CollectedBaseScope;

            // Search for the class that was parsed
            var genericFunc = scope.GetDefinitionByName<TopLevelFunctionDefinition>("f1");
            var nonGenericF = scope.GetDefinitionByName<TopLevelFunctionDefinition>("f2");

            Assert.IsFalse(container.HasErrors);

            // Check the generic types
            Assert.IsTrue(genericFunc.IsGeneric);
            Assert.AreEqual(1, genericFunc.GenericParameters.GenericTypes.Length);
            Assert.AreEqual(1, genericFunc.GenericParameters.GenericTypes.Count(g => g.Name == "T"));

            Assert.IsFalse(nonGenericF.IsGeneric);
        }
    }
}