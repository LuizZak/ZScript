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
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.CodeGeneration.Definitions
{
    [TestClass]
    public class CallableTypeDefTests
    {
        [TestMethod]
        public void TestToString()
        {
            // Simple empty signature
            {
                var parameters = new CallableTypeDef.CallableParameterInfo[0];

                var callableType = new CallableTypeDef(parameters, TypeDef.VoidType, false);

                Assert.AreEqual("(->void)", callableType.ToString());
            }

            // Simple void-returning signature
            {
                var parameters = new[]
                {
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false)
                };

                var callableType = new CallableTypeDef(parameters, TypeDef.VoidType, false);

                Assert.AreEqual("(int->void)", callableType.ToString());
            }

            // Multi-parameter signature with return type
            {
                var parameters = new[]
                {
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false),
                    new CallableTypeDef.CallableParameterInfo(TypeDef.StringType, true, false, false)
                };

                var callableType = new CallableTypeDef(parameters, TypeDef.FloatType, false);

                Assert.AreEqual("(int,String->float)", callableType.ToString());
            }

            // Variadic signature
            {
                var parameters = new[]
                {
                    new CallableTypeDef.CallableParameterInfo(TypeDef.IntegerType, true, false, false),
                    new CallableTypeDef.CallableParameterInfo(TypeDef.StringType, true, false, true)
                };
                
                var callableType = new CallableTypeDef(parameters, TypeDef.FloatType, false);

                Assert.AreEqual("(int,String...->float)", callableType.ToString());
            }
        }
    }
}
