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

using ZScript.CodeGeneration.Sourcing;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Sourcing
{
    /// <summary>
    /// Tests the functionaliy of the SourceProvider class and related components
    /// </summary>
    public class SourceProviderTests
    {
        /// <summary>
        /// Tests addition of a new source to a source provider
        /// </summary>
        [Fact]
        public void TestAddSource()
        {
            var provider = new SourceProvider();
            var stringSource1 = new ZScriptStringSource("");
            var stringSource2 = new ZScriptStringSource("");

            Assert.Equal(0, provider.Sources.Length); // "A newly created source provider should have 0 sources registered"

            provider.AddSource(stringSource1);

            Assert.True(provider.Sources.Contains(stringSource1), "Adding a source to a source provider should reflect in the .Sources property");

            provider.AddSource(stringSource2);

            Assert.True(provider.Sources.Contains(stringSource2), "Adding a source to a source provider should reflect in the .Sources property");

            provider.AddSource(stringSource2);

            Assert.Equal(2, provider.Sources.Length); // "Sources cannot appear more than once in the .Sources property after multiple sequential additions"
        }

        /// <summary>
        /// Tests removal of a new source to a source provider
        /// </summary>
        [Fact]
        public void TestRemoveSource()
        {
            var provider = new SourceProvider();
            var stringSource1 = new ZScriptStringSource("");
            var stringSource2 = new ZScriptStringSource("");

            provider.AddSource(stringSource1);
            provider.AddSource(stringSource2);

            provider.RemoveSource(stringSource1);
            provider.RemoveSource(stringSource2);

            Assert.False(provider.Sources.Contains(stringSource1), "Removing a source from a source provider should reflect in the .Sources property");
            Assert.False(provider.Sources.Contains(stringSource2), "Removing a source from a source provider should reflect in the .Sources property");
        }

        /// <summary>
        /// Tests clearing all the sources from a source provider
        /// </summary>
        [Fact]
        public void TestClearSources()
        {
            var provider = new SourceProvider();
            var stringSource1 = new ZScriptStringSource("");
            var stringSource2 = new ZScriptStringSource("");

            provider.AddSource(stringSource1);
            provider.AddSource(stringSource2);

            provider.Clear();

            Assert.Equal(0, provider.Sources.Length); // "After clearing a source provider, its source count must be reset to 0"
            Assert.False(provider.Sources.Contains(stringSource1), "Removing a source from a source provider should reflect in the .Sources property");
            Assert.False(provider.Sources.Contains(stringSource2), "Removing a source from a source provider should reflect in the .Sources property");
        }

        /// <summary>
        /// Tests the retrieval of sources based on a parser rule context
        /// </summary>
        [Fact]
        public void TestSourceForContext()
        {
            var provider = new SourceProvider();

            var source1 = new ZScriptStringSource("var a;");
            var source2 = new ZScriptStringSource("var b;");

            provider.AddSource(source1);
            provider.AddSource(source2);

            var parser1 = TestUtils.CreateParser(source1.GetScriptSourceString());
            var parser2 = TestUtils.CreateParser(source2.GetScriptSourceString());
            var parser3 = TestUtils.CreateParser("var c;");
            var parser4 = TestUtils.CreateParser("var d;");

            source1.Tree = parser1.program();
            source2.Tree = parser2.program();
            var program3 = parser3.program();
            var program4 = parser4.globalVariable();

            Assert.Equal(source1, provider.SourceForContext(source1.Tree.scriptBody().globalVariable(0))); // "Failed to locate the correct source for the context"
            Assert.Equal(source2, provider.SourceForContext(source2.Tree.scriptBody().globalVariable(0))); // "Failed to locate the correct source for the context"
            Assert.Same(null, provider.SourceForContext(program3.scriptBody().globalVariable(0))); // "Failed to locate the correct source for the context"
            Assert.Same(null, provider.SourceForContext(program4)); // "Failed to locate the correct source for the context"
        }
    }
}