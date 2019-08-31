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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Parsing.AST;

namespace ZScriptTests.Parsing.AST
{
    [TestClass]
    public class SyntaxNodeTests
    {
        [TestMethod]
        public void TestInitialState()
        {
            var sut = new TestRootNode();

            Assert.AreEqual(0, sut.Children.Count);
            Assert.IsNull(sut.Parent);
            Assert.AreEqual(SourceLocation.Invalid, sut.SourceLocation);
            Assert.IsFalse(sut.SourceLocation.IsValid);
        }

        [TestMethod]
        public void TestAddChild()
        {
            var sut = new TestRootNode();
            var child = new TestRootNode();

            sut.AddChild(child);

            Assert.AreEqual(child, sut.Children[0]);
            Assert.AreEqual(sut, child.Parent);
        }

        [TestMethod]
        public void TestAddChildDetachesFromPreviousParent()
        {
            var sut = new TestRootNode();
            var oldParent = new TestRootNode();
            var child = new TestRootNode();
            oldParent.AddChild(child);

            sut.AddChild(child);
            
            Assert.AreEqual(0, oldParent.Children.Count);
            Assert.AreEqual(child, sut.Children[0]);
            Assert.AreEqual(sut, child.Parent);
        }

        [TestMethod]
        public void TestAddChildRaisesExceptionOnCyclicReference()
        {
            var sut = new TestRootNode();
            var child = new TestRootNode();
            sut.AddChild(child);

            Assert.ThrowsException<ArgumentException>(() => child.AddChild(sut));
        }

        [TestMethod]
        public void TestAddChildRaisesExceptionOnCyclicReferenceOfArbitraryDepths()
        {
            var sut = new TestRootNode();
            var child = new TestRootNode();
            var grandchild = new TestRootNode();
            var grandGrandchild = new TestRootNode();
            child.AddChild(grandchild);
            grandchild.AddChild(grandGrandchild);
            sut.AddChild(child);

            Assert.ThrowsException<ArgumentException>(() => grandGrandchild.AddChild(sut));
        }
        
        [TestMethod]
        public void TestRemoveFromParent()
        {
            var sut = new TestRootNode();
            var child = new TestRootNode();
            sut.AddChild(child);

            child.RemoveFromParent();

            Assert.AreEqual(0, sut.Children.Count);
            Assert.IsNull(child.Parent);
        }

        [TestMethod]
        public void TestGetParentOfType()
        {
            var root = new TestNode();
            var parent = new TestRootNode();
            var sut = new TestRootNode();
            root.AddChild(parent);
            parent.AddChild(sut);

            var result = sut.GetParentOfType<TestNode>();

            Assert.AreEqual(root, result);
        }

        [TestMethod]
        public void TestGetParentOfTypeNone()
        {
            var parent = new TestRootNode();
            var sut = new TestRootNode();
            parent.AddChild(sut);

            var result = sut.GetParentOfType<TestNode>();

            Assert.IsNull(result);
        }
        
        [TestMethod]
        public void TestGetFirstChildOfType()
        {
            var sut = new TestRootNode();
            var child = new TestNode();
            sut.AddChild(child);

            var result = sut.GetFirstChildOfType<TestNode>();

            Assert.AreEqual(child, result);
        }
        
        [TestMethod]
        public void TestGetFirstChildOfTypeNone()
        {
            var sut = new TestRootNode();
            var child = new TestRootNode();
            sut.AddChild(child);

            var result = sut.GetFirstChildOfType<TestNode>();

            Assert.IsNull(result);
        }

        private class TestRootNode : SyntaxNode
        {

        }

        private class TestNode : SyntaxNode
        {

        }
    }
}