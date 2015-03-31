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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScriptTests.Utils;

namespace ZScriptTests.Runtime
{
    /// <summary>
    /// Tests generic functionality on the parser and runtime
    /// </summary>
    [TestClass]
    public class GenericFunctionTests
    {
        /// <summary>
        /// Tests basic generic function parsing
        /// </summary>
        [TestMethod]
        public void TestBasicGenericFunctionParsing()
        {
            const string input = "func funca<T>(){ }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests referencing a generic type within a generic function
        /// </summary>
        [TestMethod]
        public void TestGenericTypeReferencing()
        {
            const string input = "func funca<T>(){ var a:T? = null; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }
    }
}