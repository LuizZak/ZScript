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

using System.IO;

using Xunit;

using ZScript.CodeGeneration.Sourcing;

namespace ZScriptTests.CodeGeneration.Sourcing
{
    /// <summary>
    /// Tests the ZScriptFileSource class and related components
    /// </summary>
    public class ZScriptFileSourceTests
    {
        /// <summary>
        /// Tests creating a source from a file
        /// </summary>
        [Fact]
        public void TestFileSourceCreation()
        {
            const string path = "ZScript sample.zs";
            var source = new ZScriptFileSource(path);

            Assert.Equal(path, source.FilePath); // "The file path on a ZScriptFileSource should match the path provided in the constructor"
        }

        /// <summary>
        /// Tests reading a source from a file
        /// </summary>
        [Fact]
        public void TestFileSourceReading()
        {
            const string path = "ZScript sample.zs";

            var source = new ZScriptFileSource(path);

            var reader = new StreamReader(path);
            var readerInput = reader.ReadToEnd();
            reader.Close();

            var sourceInput = source.GetScriptSourceString();

            Assert.Equal(readerInput, sourceInput); // "The contents of a file source read from GetScriptSourceString() should match the contents of the original file"
        }

        /// <summary>
        /// Tests the ParseRequired flag
        /// </summary>
        [Fact]
        public void TestParseRequired()
        {
            const string path = "ZScript sample.zs";
            var source = new ZScriptFileSource(path);

            Assert.True(source.ParseRequired, "The ParseRequired flag from a file source should always be true");

            Assert.Equal(path, source.FilePath); // "The file path on a ZScriptFileSource should match the path provided in the constructor"

            Assert.True(source.ParseRequired, "The ParseRequired flag from a file source should always be true");
        }

        #region Equality members tests

        /// <summary>
        /// Tests equality comparision of ZScriptFileSource objects
        /// </summary>
        [Fact]
        public void TestEquality()
        {
            var source1 = new ZScriptFileSource("Source1");
            var source2 = new ZScriptFileSource("Source2");
            var source3 = new ZScriptFileSource("Source2");
            var source4 = new ZScriptFileSource(".\\Source2");

            Assert.True(source1.Equals(source1), "Equals with the same object should always result in true");
            Assert.True(source2.Equals(source2), "Equals with the same object should always result in true");
            Assert.True(source3.Equals(source3), "Equals with the same object should always result in true");
            Assert.True(source4.Equals(source4), "Equals with the same object should always result in true");
            
            Assert.True(source1.Equals((object)source1), "Equals with the same object should always result in true");
            Assert.True(source2.Equals((object)source2), "Equals with the same object should always result in true");
            Assert.True(source3.Equals((object)source3), "Equals with the same object should always result in true");
            Assert.True(source4.Equals((object)source4), "Equals with the same object should always result in true");

            Assert.False(source1.Equals(source2), "Failed to detect different sources");
            Assert.False(source1.Equals(source2), "Failed to detect different sources");

            Assert.True(source2.Equals(source3), "Failed to detect equal sources");
            Assert.True(source2.Equals(source4), "The equivalent path should be used to compare the file on the Equals() method");
            Assert.True(source2.Equals((object)source4), "The equivalent path should be used to compare the file on the Equals() method");

            Assert.True(source2 == source4, "The equivalent path should be used to compare the file on the Equals() method");
            Assert.True(source2 != source1, "The equivalent path should be used to compare the file on the Equals() method");

            // Different objects
            Assert.False(source2.Equals(null));
            Assert.False(source2.Equals((object)null));
            Assert.False(source2.Equals(new object()));
        }

        /// <summary>
        /// Tests hash code of file source objects
        /// </summary>
        [Fact]
        public void TestHashCode()
        {
            var source1 = new ZScriptFileSource("Source1");
            var source2 = new ZScriptFileSource("Source2");
            var source4 = new ZScriptFileSource("Source2");
            var source5 = new ZScriptFileSource(".\\Source2");

            Assert.NotEqual(source1.GetHashCode(), source2.GetHashCode()); // "Failed to generate different hashcodes for different source objects"
            Assert.Equal(source2.GetHashCode(), source5.GetHashCode()); // "The absolute filepath should be used to generate hashcodes"

            Assert.Equal(source2.GetHashCode(), source4.GetHashCode()); // "Failed to generate equal hashcodes for equal source objects"
        }

        #endregion
    }
}