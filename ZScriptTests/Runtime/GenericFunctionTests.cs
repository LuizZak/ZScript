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
            const string input = "func funca<T>(){ var b:any = null; var a:T? = b; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests referencing a generic type on a return type
        /// </summary>
        [TestMethod]
        public void TestGenericReturnType()
        {
            const string input = "func funca<T>() : T? { var b:any = null; var a:T? = b; return a; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests referencing a generic type on a function argument
        /// </summary>
        [TestMethod]
        public void TestGenericArgument()
        {
            const string input = "func funca<T>(a:T) { }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests using a variable inside a generic context
        /// </summary>
        [TestMethod]
        public void TestInternalGenericTypeUsage()
        {
            const string input = "func funca<T>(a:T) : T { return a; }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests using a variable inside a generic context
        /// </summary>
        [TestMethod]
        public void TestInternalMultipleGenericTypeUsage()
        {
            const string input = "func funca<T, U>(a: T, b: U) : (T, U) { return (a, b); }";

            // Setup owner call
            var generator = TestUtils.CreateGenerator(input);
            generator.ParseSources();
            generator.CollectDefinitions();

            // Assert the correct call was made
            generator.MessageContainer.PrintMessages();

            Assert.IsFalse(generator.MessageContainer.HasErrors);
        }

        /// <summary>
        /// Tests generic function calling
        /// </summary>
        [TestMethod]
        public void TestParseGenericCall()
        {
            const string input = "func main() { funca(10); } func funca<T>(a: T) : T { return a; }";

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