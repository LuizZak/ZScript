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
using ZScript.Builders;

namespace ZScriptTests.Builders
{
    /// <summary>
    /// Tests the functionality of the TypeBuildingContext class and related components
    /// </summary>
    [TestClass]
    public class TypeBuildingContextTests
    {
        /// <summary>
        /// Tests the creation of a new TypeBuildingContext
        /// </summary>
        [TestMethod]
        public void TestCreateContext()
        {
            const string name = "TestAssembly";
            var context = TypeBuildingContext.CreateBuilderContext(name);

            Assert.AreEqual(name, context.AssemblyName.Name, "The assembly failed to be generated with the expected name");
            Assert.IsNotNull(context.AssemblyBuilder.FullName, "Failed to create the assembly builder as expected");
            Assert.IsNotNull(context.ModuleBuilder, "Failed to create the module builder as expected");
        }
    }
}