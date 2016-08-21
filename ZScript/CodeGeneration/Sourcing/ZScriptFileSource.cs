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
        public string FilePath => _filePath;

        /// <summary>
        /// Gets or sets a value specifying whether this script source requires reload.
        /// Always returns true in a ZScriptFileSource instance
        /// </summary>
        public override bool ParseRequired => true;

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

            string source1 = Path.GetFullPath(_filePath);
            string source2 = Path.GetFullPath(other._filePath);

            return string.Equals(source1, source2);
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
            return (_filePath != null ? Path.GetFullPath(_filePath).GetHashCode() : 0);
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