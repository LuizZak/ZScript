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
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

using ZScript.CodeGeneration;
using ZScript.CodeGeneration.Definitions;
using ZScript.CodeGeneration.Tokenization;
using ZScript.CodeGeneration.Tokenization.Helpers;
using ZScript.Elements;
using ZScript.Elements.ValueHolding;
using ZScript.Parsing.ANTLR;
using ZScript.Runtime;
using ZScript.Runtime.Execution;
using ZScript.Runtime.Typing.Elements;
using ZScript.Utils;

namespace ZScriptTests.Utils
{
    /// <summary>
    /// Static class that contains utillity methods utilized through the unit tests
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Returns whether the two given token lists are equivalent.
        /// The equivalence takes in consideration the token types, instructions,
        /// values, and in case of jump tokens, the equivalence of the jump
        /// </summary>
        /// <param name="expected">The expected token</param>
        /// <param name="actual">The actual token</param>
        /// <param name="message">The message to display in case the tokens mismatch</param>
        /// <returns>true if the token lists are equivalent, false otherwise</returns>
        /// <exception cref="Exception">The token lists did not match</exception>
        public static void AssertTokenListEquals(IEnumerable<Token> expected, IEnumerable<Token> actual, string message = "Failed to generate expected token list")
        {
            var expList = expected.ToList();
            var actualList = actual.ToList();

            // Compare the tokens one by one
            for (int i = 0; i < Math.Min(expList.Count, actualList.Count); i++)
            {
                Token t1 = expList[i];
                Token t2 = actualList[i];

                // Unequality of types
                if (t1.GetType() != t2.GetType())
                    throw new Exception(message + "; Tokens at index " + i + " have different types: expected " + t1.GetType() + " actual: " + t2.GetType());

                var jt1 = t1 as JumpToken;
                var jt2 = t2 as JumpToken;
                if (jt1 != null && jt2 != null)
                {
                    if (jt1.ConditionToJump != jt2.ConditionToJump ||
                        jt1.Conditional != jt2.Conditional ||
                        jt1.ConsumesStack != jt2.ConsumesStack ||
                        expList.IndexOfReference(jt1.TargetToken) != actualList.IndexOfReference(jt2.TargetToken))
                    {
                        throw new Exception(message + "; Jump tokens at index " + i + " do not have the same configuration. Expected:\n" + t1 + "\nActual:\n" + t2);
                    }
                }

                var tt1 = t1 as TypedToken;
                var tt2 = t2 as TypedToken;
                if (tt1 != null && tt2 != null)
                {
                    if (!tt1.Equals(tt2))
                        throw new Exception(message + "; Tokens at index " + i + " have different values. Expected:\n" + t1 + "\nActual:\n" + t2 + " (watch out for int->long conversions in numeric atoms!)");
                }

                if (!t1.Equals(t2))
                    throw new Exception(message + "; Tokens at index " + i + " have different values. Expected:\n" + t1 + "\nActual:\n" + t2 + " (watch out for int->long conversions in numeric atoms!)");
            }

            if (expList.Count != actualList.Count)
                throw new Exception(message + "; Token lists have different token counts");
        }

        /// <summary>
        /// Creates the default generator to use in tests
        /// </summary>
        /// <param name="input">The input string to use in the generator</param>
        /// <returns>A default runtime generator to use in tests</returns>
        public static ZRuntimeGenerator CreateGenerator(string input)
        {
            return new ZRuntimeGenerator(input) { Debug = true };
        }

        /// <summary>
        /// Creates a new ZScriptParser object from a given string
        /// </summary>
        /// <param name="input">The input string to generate the ZScriptParser from</param>
        /// <returns>A ZScriptParser created from the given string</returns>
        public static ZScriptParser CreateParser(string input)
        {
            AntlrInputStream stream = new AntlrInputStream(input);
            ITokenSource lexer = new ZScriptLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            return new ZScriptParser(tokens);
        }

        /// <summary>
        /// Creates a test class definition to use in tests, containing a field named 'field1' of long type with a default value of 10
        /// and a method definition 'func1' with sinature '(->int)' containing a simple return instruction that returns a '10L' literal value
        /// </summary>
        /// <returns>A dummy test class to use in tests</returns>
        public static ClassDefinition CreateTestClassDefinition(string name = "className")
        {
            var classDef = new ClassDefinition(name);

            const string field1Exp = "10";

            var parser = CreateParser(field1Exp);

            classDef.AddMethod(new MethodDefinition("func1", null, new FunctionArgumentDefinition[0]) { ReturnType = TypeDef.IntegerType, Tokens = new TokenList() });
            classDef.AddField(new TypeFieldDefinition("field1") { Type = TypeDef.IntegerType, HasValue = true, ValueExpression = new Expression(parser.expression()) });

            classDef.FinishDefinition();

            return classDef;
        }

        /// <summary>
        /// Creates a test class to use in tests, containing a field named 'field1' of long type with a default value of 10
        /// and a method definition 'func1' with sinature '(->int)' containing a simple return instruction that returns a '10L' literal value
        /// </summary>
        /// <param name="name">The custom class name fo rthe created class</param>
        /// <returns>A dummy test class to use in tests</returns>
        public static ZClassInstance CreateTestClassInstance(string name = "className")
        {
            // 'field1' field
            var fieldTokens = new IntermediaryTokenList { TokenFactory.CreateBoxedValueToken(10) };
            var field1 = new ZClassField("field1", fieldTokens.ToTokenList()) { DefaultValue = 10, Type = typeof(long) };

            // 'func1' method
            var funcTokens = new IntermediaryTokenList { TokenFactory.CreateBoxedValueToken(10L), TokenFactory.CreateInstructionToken(VmInstruction.Ret) };
            var func1 = new ZMethod("func1", funcTokens.ToTokenList(), new FunctionArgument[0], typeof(void));

            var cls = new ZClass(name, new[] { func1 }, new[] { field1 }, new ZMethod(name, new TokenList(), new FunctionArgument[0], typeof(ZClassInstance)), typeof(ZClassInstance), false);

            var inst = new ZClassInstance(cls);

            // Pre-populate the field
            inst.LocalMemory.SetVariable("field1", 10L);

            return inst;
        }
    }

    /// <summary>
    /// Test base class used in a few type-related tests
    /// </summary>
    public class TestBaseClass
    {

    }

    /// <summary>
    /// Test derived class used in a few type-related tests
    /// </summary>
    class TestDerivedClass : TestBaseClass
    {

    }
}