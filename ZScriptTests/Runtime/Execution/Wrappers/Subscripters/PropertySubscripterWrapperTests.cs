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