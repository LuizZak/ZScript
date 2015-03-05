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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Runtime.Execution.Wrappers.Subscripters;

namespace ZScriptTests.Runtime.Execution.Wrappers.Subscripters
{
    /// <summary>
    /// Tests the functionality of the PropertySubscripterWrapper and related components
    /// </summary>
    [TestClass]
    public class PropertySubscripterWrapperTests
    {
        /// <summary>
        /// Tests the 
        /// </summary>
        [TestMethod]
        public void TestCanSubscript()
        {
            var target = new List<int>();
            var properties = typeof(List<int>).GetProperties();
            PropertyInfo property = properties.First(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int));

            var wrapper = new PropertySubscripterWrapper(target, property);

            Assert.AreEqual(target, wrapper.Target, "The target pointed by the Target propert must be the same provided in the cosntructor");

            Assert.IsTrue(wrapper.CanSubscriptWithIndexType(typeof(int)));
            Assert.IsFalse(wrapper.CanSubscriptWithIndexType(typeof(string)));
        }
    }
}