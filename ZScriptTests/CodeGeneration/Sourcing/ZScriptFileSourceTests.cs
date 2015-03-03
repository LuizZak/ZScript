using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Sourcing;

namespace ZScriptTests.CodeGeneration.Sourcing
{
    /// <summary>
    /// Tests the ZScriptFileSource class and related components
    /// </summary>
    [TestClass]
    public class ZScriptFileSourceTests
    {
        /// <summary>
        /// Tests creating a source from a file
        /// </summary>
        [TestMethod]
        public void TestFileSourceCreation()
        {
            const string path = "ZScript sample.zs";
            var source = new ZScriptFileSource(path);

            Assert.AreEqual(path, source.FilePath, "The file path on a ZScriptFileSource should match the path provided in the constructor");
        }

        /// <summary>
        /// Tests reading a source from a file
        /// </summary>
        [TestMethod]
        public void TestFileSourceReading()
        {
            const string path = "ZScript sample.zs";

            var source = new ZScriptFileSource(path);

            var reader = new StreamReader(path);
            var readerInput = reader.ReadToEnd();
            reader.Close();

            var sourceInput = source.GetScriptSourceString();

            Assert.AreEqual(readerInput, sourceInput, "The contents of a file source read from GetScriptSourceString() should match the contents of the original file");
        }

        /// <summary>
        /// Tests the ParseRequired flag
        /// </summary>
        [TestMethod]
        public void TestParseRequired()
        {
            const string path = "ZScript sample.zs";
            var source = new ZScriptFileSource(path);

            Assert.IsTrue(source.ParseRequired, "The ParseRequired flag from a file source should always be true");

            Assert.AreEqual(path, source.FilePath, "The file path on a ZScriptFileSource should match the path provided in the constructor");

            Assert.IsTrue(source.ParseRequired, "The ParseRequired flag from a file source should always be true");
        }

        #region Equality members tests

        /// <summary>
        /// Tests equality comparision of ZScriptFileSource objects
        /// </summary>
        [TestMethod]
        public void TestEquality()
        {
            var source1 = new ZScriptFileSource("Source1");
            var source2 = new ZScriptFileSource("Source2");
            var source3 = new ZScriptFileSource("Source2");
            var source4 = new ZScriptFileSource(".\\Source2");

            Assert.IsTrue(source1.Equals(source1), "Equals with the same object should always result in true");
            Assert.IsTrue(source2.Equals(source2), "Equals with the same object should always result in true");
            Assert.IsTrue(source3.Equals(source3), "Equals with the same object should always result in true");
            Assert.IsTrue(source4.Equals(source4), "Equals with the same object should always result in true");
            
            Assert.IsTrue(source1.Equals((object)source1), "Equals with the same object should always result in true");
            Assert.IsTrue(source2.Equals((object)source2), "Equals with the same object should always result in true");
            Assert.IsTrue(source3.Equals((object)source3), "Equals with the same object should always result in true");
            Assert.IsTrue(source4.Equals((object)source4), "Equals with the same object should always result in true");

            Assert.IsFalse(source1.Equals(source2), "Failed to detect different sources");
            Assert.IsFalse(source1.Equals(source2), "Failed to detect different sources");

            Assert.IsTrue(source2.Equals(source3), "Failed to detect equal sources");
            Assert.IsTrue(source2.Equals(source4), "The equivalent path should be used to compare the file on the Equals() method");
            Assert.IsTrue(source2.Equals((object)source4), "The equivalent path should be used to compare the file on the Equals() method");

            Assert.IsTrue(source2 == source4, "The equivalent path should be used to compare the file on the Equals() method");
            Assert.IsTrue(source2 != source1, "The equivalent path should be used to compare the file on the Equals() method");

            // Different objects
            Assert.IsFalse(source2.Equals(null));
            Assert.IsFalse(source2.Equals((object)null));
            Assert.IsFalse(source2.Equals(new object()));
        }

        /// <summary>
        /// Tests hash code of file source objects
        /// </summary>
        [TestMethod]
        public void TestHashCode()
        {
            var source1 = new ZScriptFileSource("Source1");
            var source2 = new ZScriptFileSource("Source2");
            var source4 = new ZScriptFileSource("Source2");
            var source5 = new ZScriptFileSource(".\\Source2");

            Assert.AreNotEqual(source1.GetHashCode(), source2.GetHashCode(), "Failed to generate different hashcodes for different source objects");
            Assert.AreEqual(source2.GetHashCode(), source5.GetHashCode(), "The absolute filepath should be used to generate hashcodes");

            Assert.AreEqual(source2.GetHashCode(), source4.GetHashCode(), "Failed to generate equal hashcodes for equal source objects");
        }

        #endregion
    }
}