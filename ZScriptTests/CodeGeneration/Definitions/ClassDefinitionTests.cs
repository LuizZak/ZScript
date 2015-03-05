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
using ZScript.CodeGeneration.Definitions;
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.CodeGeneration.Definitions
{
    /// <summary>
    /// Tests the functionality of the ClassDefinition and related components
    /// </summary>
    [TestClass]
    public class ClassDefinitionTests
    {
        /// <summary>
        /// Tests setup of an empty class definition
        /// </summary>
        [TestMethod]
        public void TestEmptyDefinition()
        {
            var definition = new ClassDefinition("test1");

            definition.FinishDefinition();

            Assert.IsNotNull(definition.PublicConstructor, "When no constructor is provided, calling FinishDefinition() should create a new parameterless constructor");
        }

        /// <summary>
        /// Tests fetching a field's information with the ClassTypeDef property
        /// </summary>
        [TestMethod]
        public void TestClassTypeDefField()
        {
            var definition = new ClassDefinition("test1");

            definition.AddField(new TypeFieldDefinition("field1") { Type = TypeDef.IntegerType });

            definition.FinishDefinition();

            var classTypeDef = definition.ClassTypeDef;

            Assert.AreEqual(TypeDef.IntegerType, classTypeDef.GetField("field1").FieldType);
        }

        /// <summary>
        /// Tests fetching a method's information with the ClassTypeDef property
        /// </summary>
        [TestMethod]
        public void TestClassTypeDefMethod()
        {
            var definition = new ClassDefinition("test1");

            definition.AddMethod(new MethodDefinition("func1", null,
                new[] {new FunctionArgumentDefinition {Name = "param1", Type = TypeDef.IntegerType}})
            {
                ReturnType = TypeDef.BooleanType
            });

            definition.FinishDefinition();

            var classTypeDef = definition.ClassTypeDef;

            Assert.AreEqual(TypeDef.IntegerType, classTypeDef.GetMethod("func1").Parameters[0].ParameterType);
            Assert.AreEqual("param1", classTypeDef.GetMethod("func1").Parameters[0].ParameterName);
            Assert.AreEqual(TypeDef.BooleanType, classTypeDef.GetMethod("func1").ReturnType);
        }
    }
}