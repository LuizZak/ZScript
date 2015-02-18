using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.CodeGeneration.Analysis;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the CodeScope class and related components
    /// </summary>
    [TestClass]
    public class CodeScopeTests
    {
        /// <summary>
        /// Tests the GetScopeByContext method call
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(subScope, foundScope1, "The GetScopeContainingContext failed to find the correct scope");
            Assert.AreEqual(scope, foundScope2, "The GetScopeContainingContext failed to find the correct root scope");
        }

        /// <summary>
        /// Tets cyclic parenting detection with AddSubscope
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to create cyclic scope references must raise an ArgumentException")]
        public void TestCyclicDetection()
        {
            var scope1 = new CodeScope();
            var scope2 = new CodeScope();
            var scope3 = new CodeScope();

            // Create a cyclic reference scope1 -> scope2 -> scope3 -> scope1
            scope1.AddSubscope(scope2);
            scope2.AddSubscope(scope3);
            scope3.AddSubscope(scope1);
        }

        /// <summary>
        /// Tets exception raising when trying to add a subscope to another scope while it already has a parent scope
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to add a subscope which already has a parent must raise an ArgumentException")]
        public void TestAddSubscopeWithParentException()
        {
            var scope1 = new CodeScope();
            var scope2 = new CodeScope();
            var scope3 = new CodeScope();

            // Create a cyclic reference scope1 -> scope2 -> scope3 -> scope1
            scope1.AddSubscope(scope2);
            scope3.AddSubscope(scope2);
        }
    }
}