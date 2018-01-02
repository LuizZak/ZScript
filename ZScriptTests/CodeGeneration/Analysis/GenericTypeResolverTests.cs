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

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the GenericTypeResolver class and related components
    /// </summary>
    [TestClass]
    public class GenericTypeResolverTests
    {
        /*
        /// <summary>
        /// Tests resolving a simple function call
        /// </summary>
        [TestMethod]
        public void TestResolveSimpleCall()
        {
            var parser = TestUtils.CreateParser("a: T");
            var parserCall = TestUtils.CreateParser("(10)");
            var genericType = new GenericTypeDefinition("T");
            var genericSignature = new GenericSignatureInformation(new[] { genericType });

            var argument = new FunctionArgumentDefinition
            {
                Name = "arg0",
                HasValue = false,
                Context = parser.functionArg(),
                IdentifierContext = parser.functionArg().argumentName(),
                Type = genericType,
                TypeContext = parser.functionArg().type()
            };
            
            var functionCall = parserCall.functionCall();

            functionCall.CallableSignature = new CallableTypeDef(new [] { new CallableTypeDef.CallableParameterInfo(genericType, true, false, false) }, TypeDef.VoidType, true, genericSignature);
            
            var function = new FunctionDefinition("test", null, new [] { argument }, genericSignature);
            var context = new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider(), new TestDefinitionTypeProvider());

            var genericResolver = new GenericTypeResolver(context);

            genericResolver.ResolveFunctionCall(functionCall);
        }

        /// <summary>
        /// Tests storing function calls on a generic type resolver
        /// </summary>
        [TestMethod]
        public void TestFunctionCallStoring()
        {
            var parserCall = TestUtils.CreateParser("(10)");
            var genericType = new GenericTypeDefinition("T");
            var genericSignature = new GenericSignatureInformation(new[] { genericType });

            var functionCall = parserCall.functionCall();

            functionCall.CallableSignature = new CallableTypeDef(new[] { new CallableTypeDef.CallableParameterInfo(genericType, true, false, false) }, TypeDef.VoidType, true, genericSignature);
            
            var context = new RuntimeGenerationContext(null, new MessageContainer(), new TypeProvider(), new TestDefinitionTypeProvider());

            var genericResolver = new GenericTypeResolver(context);

            Assert.AreEqual(0, genericResolver.EnqueuedFunctionCalls.Length);

            genericResolver.PushGenericCallContext(functionCall);

            Assert.AreEqual(1, genericResolver.EnqueuedFunctionCalls.Length);
            Assert.AreEqual(genericResolver.EnqueuedFunctionCalls[0], functionCall);
        }
        */
    }
}
