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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Tests the ClassMember class and related components
    /// </summary>
    [TestClass]
    public class ClassMemberTests
    {
        [TestMethod]
        public void TestMemberFetching()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var prop = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.AreEqual(target, field.Target, "The target of the ClassMember must be the same object passed on the GetMember() method");

            Assert.AreEqual(target.GetType().GetField("Field1"), ((FieldClassMember)field).Field,
                "The field pointed by the FieldClassMember.Field property should point to the field named during the GetMember call");
            Assert.AreEqual(target.GetType().GetProperty("Property1"), ((PropertyClassMember)prop).Property,
                "The property pointed by the PropertyClassMember.Property property should point to the property named during the GetMember call");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to fetch a member that does not exists should raise an ArgumentException")]
        public void TestFailingFetchingNonExisting()
        {
            var target = new TestTarget();
            ClassMember.CreateMemberWrapper(target, "NonExistingMember");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to fetch a member that is private should raise an ArgumentException")]
        public void TestFailingFetchingPrivate()
        {
            var target = new TestTarget();
            ClassMember.CreateMemberWrapper(target, "_privateField");
        }

        [TestMethod]
        public void TestMemberGet()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.AreEqual(target.Field1, field.GetValue(), "The value of the field fetched by GetValue() does not matches the value of the target class' field");
            Assert.AreEqual(target.Property1, property.GetValue(), "The value of the property fetched by GetValue() does not matches the value of the target class' property");
        }

        [TestMethod]
        public void TestMemberSet()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            field.SetValue(11);
            property.SetValue(12);

            Assert.AreEqual(11, target.Field1, "The SetValue() method did not set the value of the pointed field correctly");
            Assert.AreEqual(12, target.Property1, "The SetValue() method did not set the value of the pointed property correctly");
        }

        [TestMethod]
        public void TestMemberTyping()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.AreEqual(target.Field1.GetType(), field.MemberType,
                "The type of the field fetched by MemberType does not matches the type of the target class' field");
            Assert.AreEqual(target.Property1.GetType(), property.MemberType,
                "The type of the property fetched by MemberType does not matches the type of the target class' property");
        }

        [TestMethod]
        public void TestMemberNaming()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.AreEqual("Field1", field.MemberName,
                "The name of the field fetched by MemberName does not matches the name of the target class' field");
            Assert.AreEqual("Property1", property.MemberName,
                "The name of the property fetched by MemberName does not matches the name of the target class' property");
        }

        public class TestTarget
        {
            public int Field1;
            public long Property1 { get; set; }

            private string _privateField;
        }
    }
}