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

using ZScript.CodeGeneration.Analysis;
using ZScript.CodeGeneration.Messages;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;
using ZScriptTests.Utils;

namespace ZScriptTests.CodeGeneration.Analysis
{
    /// <summary>
    /// Tests the ConstraintTypeSolver and related components
    /// </summary>
    [TestClass]
    public class ConstraintSolverTests
    {
        [TestMethod]
        public void TestBasicSignatureSolving()
        {
            var provider = new TypeProvider();
            var cs = new ConstraintSolver(provider);
            
            cs.AddOpenType("T", ConstraintSolver.TypeDirection.In);
            cs.AddClosedType(TypeDef.IntegerType, ConstraintSolver.TypeDirection.In);

            cs.AddConstraint("T", ConstraintSolver.Constraint.SameType, "int");

            cs.Solve();

            Assert.IsTrue(cs.IsResolved);
            Assert.AreEqual(cs.ResolvedTypes?["T"], TypeDef.IntegerType);

            Assert.AreEqual(0, cs.LocalContainer.CodeErrors.Length);
        }

        [TestMethod]
        public void TestTransitiveSignatureSolving()
        {
            var provider = new TypeProvider();
            var cs = new ConstraintSolver(provider);

            cs.AddOpenType("T", ConstraintSolver.TypeDirection.In);
            cs.AddOpenType("H", ConstraintSolver.TypeDirection.In);
            cs.AddClosedType(TypeDef.IntegerType, ConstraintSolver.TypeDirection.In);

            cs.AddConstraint("T", ConstraintSolver.Constraint.SameType, "H");
            cs.AddConstraint("H", ConstraintSolver.Constraint.SameType, "int");

            cs.Solve();

            Assert.IsTrue(cs.IsResolved);
            Assert.AreEqual(cs.ResolvedTypes?["T"], TypeDef.IntegerType);
            Assert.AreEqual(cs.ResolvedTypes?["H"], TypeDef.IntegerType);

            Assert.AreEqual(0, cs.LocalContainer.CodeErrors.Length);
        }

        [TestMethod]
        public void TestTrivialUnsolvable()
        {
            var provider = new TypeProvider();
            var cs = new ConstraintSolver(provider);

            cs.AddOpenType("T", ConstraintSolver.TypeDirection.In);
            
            cs.Solve();

            Assert.IsFalse(cs.IsResolved);

            Assert.IsTrue(cs.LocalContainer.CodeErrors.Expect("Failed to deduce type T", 1));
        }

        [TestMethod]
        public void TestMismatchedTypesUnsolvable()
        {
            var provider = new TypeProvider();
            var cs = new ConstraintSolver(provider);

            cs.AddOpenType("T", ConstraintSolver.TypeDirection.In);
            cs.AddOpenType("H", ConstraintSolver.TypeDirection.In);

            cs.AddClosedType(TypeDef.IntegerType, ConstraintSolver.TypeDirection.In);
            cs.AddClosedType(TypeDef.StringType, ConstraintSolver.TypeDirection.In);

            cs.AddConstraint("T", ConstraintSolver.Constraint.SameType, "int");
            cs.AddConstraint("H", ConstraintSolver.Constraint.SameType, "String");
            cs.AddConstraint("T", ConstraintSolver.Constraint.SameType, "H");

            cs.Solve();

            Assert.IsFalse(cs.IsResolved);

            Console.WriteLine($"{cs.LocalContainer.AllMessages.JoinMessages()}");

            Assert.IsTrue(cs.LocalContainer.CodeErrors.Expect(@"Cannot satisfy constraint T (aka int) == H (aka String)", 1));
        }
    }
}
