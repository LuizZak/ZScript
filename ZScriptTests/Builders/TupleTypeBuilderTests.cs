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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZScript.Builders;
using ZScript.Runtime;
using ZScript.Runtime.Typing;

namespace ZScriptTests.Builders
{
    /// <summary>
    /// Tests the functionality of the TupleTypeBuilder class and related components
    /// </summary>
    [TestClass]
    public class TupleTypeBuilderTests
    {
        /// <summary>
        /// Tests that a create tuple has the correct naming scheme, that it implements the ITuple interface, and that the created type's IsValueType is set to true
        /// </summary>
        [TestMethod]
        public void TestCreatedTupleTypeNameAndAttributes()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);
            
            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1);

            Assert.IsFalse(tupleType.IsValueType);
            Assert.IsTrue(tupleType.GetInterfaces().Any(t => t == typeof(ITuple)));
        }

        /// <summary>
        /// Tests that a create tuple has the expected tuple fields in it
        /// </summary>
        [TestMethod]
        public void TestCreatedTupleFields()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1).MakeGenericType(typeof(long), typeof(double));

            var field1 = tupleType.GetField("Field0");
            var field2 = tupleType.GetField("Field1");

            Assert.IsNotNull(field1);
            Assert.IsNotNull(field2);

            Assert.AreEqual(typeof(long), field1.FieldType);
            Assert.AreEqual(typeof(double), field2.FieldType);
        }

        /// <summary>
        /// Tests that a create tuple has the expected tuple field constructor in it
        /// </summary>
        [TestMethod]
        public void TestCreatedTupleFieldConstructor()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1).MakeGenericType(typeof(long), typeof(double));
            
            var constructor = tupleType.GetConstructor(new[] { typeof(long), typeof(double) });

            Assert.IsNotNull(constructor);

            var parameters = constructor.GetParameters();

            Assert.AreEqual(typeof(long), parameters[0].ParameterType);
            Assert.AreEqual(typeof(double), parameters[1].ParameterType);
        }

        /// <summary>
        /// Tests that a create tuple has the expected tuple copy constructor in it
        /// </summary>
        [TestMethod]
        public void TestCreatedTupleCopyConstructor()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1);

            var constructor = tupleType.GetConstructor(new[] { tupleType });

            Assert.IsNotNull(constructor);
        }

        /// <summary>
        /// Tests that a creates an instance of a generated tuple
        /// </summary>
        [TestMethod]
        public void TestCreateInstanceOfGeneratedTuple()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1).MakeGenericType(typeof(long), typeof(double));

            var tuple = (ITuple)Activator.CreateInstance(tupleType, 5L, 10.0);

            Assert.AreEqual(5L, tupleType.GetField("Field0").GetValue(tuple));
            Assert.AreEqual(10.0, tupleType.GetField("Field1").GetValue(tuple));
        }

        /// <summary>
        /// Tests that a creates an instance of a generated tuple with its copy constructor
        /// </summary>
        [TestMethod]
        public void TestCreateCopyInstanceOfGeneratedTuple()
        {
            var provider = new TypeProvider();

            var tupleTypeDef = provider.TupleForTypes(provider.IntegerType(), provider.FloatType());

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tupleTypeDef).MakeGenericType(typeof(long), typeof(double));
            var c = tupleType.GetConstructors();
            var tuple1 = (ITuple)Activator.CreateInstance(tupleType, 5L, 10.0);
            var tuple2 = (ITuple)Activator.CreateInstance(tupleType, tuple1);

            Assert.AreEqual(5L, tupleType.GetField("Field0").GetValue(tuple1));
            Assert.AreEqual(10.0, tupleType.GetField("Field1").GetValue(tuple1));

            Assert.AreEqual(5L, tupleType.GetField("Field0").GetValue(tuple2));
            Assert.AreEqual(10.0, tupleType.GetField("Field1").GetValue(tuple2));
        }

        /// <summary>
        /// Tests that a create tuple has a nested tuple within
        /// </summary>
        [TestMethod]
        public void TestCreateNestedTuple()
        {
            var provider = new TypeProvider();

            var tuple1 = provider.TupleForTypes(provider.IntegerType(), provider.TupleForTypes(provider.IntegerType(), provider.FloatType()));

            // Boilerplate
            var buildingContext = TypeBuildingContext.CreateBuilderContext("TestAssembly");

            var tupleBuilder = new TupleTypeBuilder(buildingContext);

            // Create the class type
            var tupleType = tupleBuilder.ConstructType(tuple1);

            var nestedTuple = tupleType.MakeGenericType(typeof(long), typeof(double));
            tupleType = tupleType.MakeGenericType(typeof(long), nestedTuple);

            var field1 = tupleType.GetField("Field0");
            var field2 = tupleType.GetField("Field1");

            Assert.IsNotNull(field1);
            Assert.IsNotNull(field2);

            Assert.AreEqual(typeof(long), field1.FieldType);
            Assert.AreEqual(nestedTuple, field2.FieldType);
        }
    }
}