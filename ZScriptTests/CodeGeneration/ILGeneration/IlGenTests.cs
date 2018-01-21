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

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZScript.Builders;
using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.ILGeneration;
using ZScript.CodeGeneration.Messages;
using ZScript.CodeGeneration.Sourcing;
using ZScript.Runtime.Typing;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.ILGeneration
{
    [TestClass]
    public class IlGenTests
    {
        [TestMethod]
        public void TestGenerateIlSum()
        {
            string input = @"
                func testSum(a: int, b: int) : int {
                    return a + b;
                }
                ";
            
            var source = new ZScriptStringSource(input);
            var generator = TestUtils.CreateGenerator(source);
            var scope = generator.CollectDefinitions();
            var context = TypeBuildingContext.CreateBuilderContext("Test");

            Debug.Assert(scope != null, nameof(scope) + " != null");

            var func = scope.GetDefinitionsByTypeRecursive<FunctionDefinition>().First();
            var ast = source.Tree.scriptBody().functionDefinition(0);
            
            var sut = new IlGen(new RuntimeGenerationContext(scope, new MessageContainer(), new TypeProvider()));

            var typeBuilder = context.ModuleBuilder.DefineType("MyType", TypeAttributes.Class);

            sut.GenerateIl(ast.functionBody(), func, scope, typeBuilder);
        }
    }
}