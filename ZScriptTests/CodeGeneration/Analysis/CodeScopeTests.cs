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

using ZScript.CodeGeneration.Analysis;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the CodeScope class and related components
    /// </summary>
    public class CodeScopeTests
    {
        /// <summary>
        /// Tests the GetScopeByContext method call
        /// </summary>
        [Fact]
        public void TestGetScopeByContext()
        {
            // Create a simple scope
            var programContext = new ZScriptParser.ProgramContext(null, 0);
            var scope = new CodeScope(programContext);

            // Create the test contexts
            var context1 = new ZScriptParser.StatementContext(scope.Context, 0);
            var context2 = new ZScriptParser.ExpressionContext(context1, 0);
            
            // Create the context tree
            scope.Context.AddChild(context1);
            context1.AddChild(context2);

            // Add a sub-scope with the statement context associated with it
            var subScope = new CodeScope(context1);

            scope.AddSubscope(subScope);

            // Find the scope by context
            var foundScope1 = scope.GetScopeContainingContext(context2);
            var foundScope2 = scope.GetScopeContainingContext(programContext);

            // "The GetScopeContainingContext failed to find the correct scope"
            Assert.Equal(subScope, foundScope1);
            // "The GetScopeContainingContext failed to find the correct root scope"
            Assert.Equal(scope, foundScope2);
        }

        /// <summary>
        /// Tets cyclic parenting detection with AddSubscope
        /// </summary>
        [Fact]
        public void TestCyclicDetection()
        {
            var scope1 = new CodeScope();
            var scope2 = new CodeScope();
            var scope3 = new CodeScope();

            // Create a cyclic reference scope1 -> scope2 -> scope3 -> scope1
            scope1.AddSubscope(scope2);
            scope2.AddSubscope(scope3);

            Assert.Throws<ArgumentException>(() => { scope3.AddSubscope(scope1); });
        }

        /// <summary>
        /// Tets exception raising when trying to add a subscope to another scope while it already has a parent scope
        /// </summary>
        [Fact]
        public void TestAddSubscopeWithParentException()
        {
            var scope1 = new CodeScope();
            var scope2 = new CodeScope();
            var scope3 = new CodeScope();

            // Create a cyclic reference scope1 -> scope2 -> scope3 -> scope1
            scope1.AddSubscope(scope2);

            Assert.Throws<ArgumentException>(() => { scope3.AddSubscope(scope2); });
        }
    }
}