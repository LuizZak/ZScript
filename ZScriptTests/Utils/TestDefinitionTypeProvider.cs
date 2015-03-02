using System.Collections.Generic;
using Antlr4.Runtime;
using ZScript.CodeGeneration.Analysis;
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.Utils
{
    /// <summary>
    /// Test definition type provider used in tests
    /// </summary>
    public class TestDefinitionTypeProvider : IDefinitionTypeProvider
    {
        /// <summary>
        /// Dictionary of custom type definitions mapped into strings
        /// </summary>
        public Dictionary<string, TypeDef> CustomTypes = new Dictionary<string, TypeDef>();

        /// <summary>
        /// Gets or sets the type to issue on 'this' expressions
        /// </summary>
        public TypeDef ThisType { get; set; }

        /// <summary>
        /// Gets or sets the type to issue on 'base' expressions
        /// </summary>
        public TypeDef BaseType { get; set; }

        // 
        // IDefinitionTypeProvider.TypeForDefinition override
        // 
        public TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName)
        {
            // Search first on the custom types
            if (CustomTypes.ContainsKey(definitionName))
            {
                return CustomTypes[definitionName];
            }

            if (definitionName == "i")
                return TypeDef.IntegerType;
            if (definitionName == "f")
                return TypeDef.FloatType;
            if (definitionName == "b")
                return TypeDef.BooleanType;
            if (definitionName == "s")
                return TypeDef.StringType;
            if (definitionName == "o")
                return new ObjectTypeDef();
            if (definitionName == "v")
                return TypeDef.VoidType;
            if (definitionName == "a")
                return TypeDef.AnyType;
            if (definitionName.StartsWith("l"))
                return new ListTypeDef(definitionName.Length == 0 ? TypeDef.IntegerType : TypeForDefinition(context, definitionName.Substring(1)));

            return TypeDef.AnyType;
        }

        // 
        // IDefinitionTypeProvider.TypeForThis override
        // 
        public TypeDef TypeForThis(ParserRuleContext context)
        {
            return ThisType;
        }

        // 
        // IDefinitionTypeProvider.TypeForBase override
        // 
        public TypeDef TypeForBase(ParserRuleContext context)
        {
            return BaseType;
        }
    }
}