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
using ZScript.Elements;
using ZScriptTests.Utils;

namespace ZScriptTests.CrashCases
{
    /// <summary>
    /// Tests for crashing tuple constructions
    /// </summary>
    [TestClass]
    public class TupleCrashCaseTests
    {
        [TestMethod]
        public void TestCrash1()
        {
            var input = @"
                    @__trace(v...)

                    func main()
                    {
                        // Main entry point
                        __trace(f( a => { return a.f1(); } ));
                    }

                    func f(a: (C->int)? = null) : (int?, int)?
                    {
                        return (a?(C()), 0);
                    }

                    class C
                    {
                        func f1() : int
                        {
                            return 0;
                        }
                    }";

            var runtimeOwner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(runtimeOwner);

            runtime.CallFunction("main");

            Assert.AreEqual(((dynamic)runtimeOwner.TraceObjects[0]).InnerValue.Field0, new Optional<long>(0));
            Assert.AreEqual(((dynamic)runtimeOwner.TraceObjects[0]).InnerValue.Field1, 0L);
        }

        [TestMethod]
        public void TestCrash2()
        {
            var input = @"@__trace(v...)

                    func main()
                    {
                        // Main entry point
                        __trace(f());
                    }

                    func f() : (int?, int)?
                    {
                        return (0, 0);
                    }";

            var runtimeOwner = new TestRuntimeOwner();
            var generator = TestUtils.CreateGenerator(input);
            var runtime = generator.GenerateRuntime(runtimeOwner);

            runtime.CallFunction("main");

            var tuple = ((IOptional)runtimeOwner.TraceObjects[0]).BaseInnerValue;
            var optional0 = tuple.GetType().GetField("Field0").GetValue(tuple);
            var nonOptional0 = tuple.GetType().GetField("Field1").GetValue(tuple);

            Assert.AreEqual(new Optional<long>(0), optional0);
            Assert.AreEqual(0L, nonOptional0);
        }
    }
}
