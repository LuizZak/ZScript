using System;
using System.IO;

namespace ZScript.CodeGeneration.Sourcing
{
    /// <summary>
    /// Represents a ZScript file that comes from disk
    /// </summary>
    public class ZScriptFileSource : ZScriptDefinitionsSource, IEquatable<ZScriptFileSource>
    {
        /// <summary>
        /// The file path associated with this ZScriptFileSource instance
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Gets the file path containing the source for the script
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Gets or sets a value specifying whether this script source requires reload.
        /// Always returns true in a ZScriptFileSource instance
        /// </summary>
        public override bool ParseRequired
        {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of the ZScriptFileSource class
        /// </summary>
        /// <param name="filePath">The path of the file to assocaite with this ZScriptFileSource instance</param>
        public ZScriptFileSource(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Gets the script source from the file path associated with this ZScriptFile class
        /// </summary>
        /// <returns>The script source for this ZScriptFile</returns>
        public override string GetScriptSourceString()
        {
            using(FileStream stream = new FileStream(FilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
        }

        #region Equality members

        public bool Equals(ZScriptFileSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_filePath, other._filePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ZScriptFileSource)obj);
        }

        public override int GetHashCode()
        {
            return (_filePath != null ? _filePath.GetHashCode() : 0);
        }

        public static bool operator==(ZScriptFileSource left, ZScriptFileSource right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(ZScriptFileSource left, ZScriptFileSource right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}