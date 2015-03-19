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

using Xunit;

using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScriptTests.Runtime.Execution.Wrappers.Members
{
    /// <summary>
    /// Tests the ClassMember class and related components
    /// </summary>
    public class ClassMemberTests
    {
        [Fact]
        public void TestMemberFetching()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var prop = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.Equal(target, field.Target);

            Assert.Equal(target.GetType().GetField("Field1"), ((FieldClassMember)field).Field);
            Assert.Equal(target.GetType().GetProperty("Property1"), ((PropertyClassMember)prop).Property);
        }

        [Fact]
        public void TestFailingFetchingNonExisting()
        {
            var target = new TestTarget();

            Assert.Throws<ArgumentException>(() => ClassMember.CreateMemberWrapper(target, "NonExistingMember"));
        }

        [Fact]
        public void TestFailingFetchingPrivate()
        {
            var target = new TestTarget();

            Assert.Throws<ArgumentException>(() => ClassMember.CreateMemberWrapper(target, "_privateField"));
        }

        [Fact]
        public void TestMemberGet()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.Equal(target.Field1, field.GetValue());
            Assert.Equal(target.Property1, property.GetValue());
        }

        [Fact]
        public void TestMemberSet()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            field.SetValue(11);
            property.SetValue(12);

            Assert.Equal(11, target.Field1);
            Assert.Equal(12, target.Property1);
        }

        [Fact]
        public void TestMemberTyping()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.Equal(target.Field1.GetType(), field.MemberType);
            Assert.Equal(target.Property1.GetType(), property.MemberType);
        }

        [Fact]
        public void TestMemberNaming()
        {
            var target = new TestTarget { Field1 = 10, Property1 = 11 };

            var field = ClassMember.CreateMemberWrapper(target, "Field1");
            var property = ClassMember.CreateMemberWrapper(target, "Property1");

            Assert.Equal("Field1", field.MemberName);
            Assert.Equal("Property1", property.MemberName);
        }

        public class TestTarget
        {
            public int Field1;
            public long Property1 { get; set; }

            private string _privateField;
        }
    }
}