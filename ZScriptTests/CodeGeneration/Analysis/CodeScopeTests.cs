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
    }
}