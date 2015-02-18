using ZScript.CodeGeneration.Analysis;
using ZScript.Runtime.Typing.Elements;

namespace ZScriptTests.Utils
{
    /// <summary>
    /// Test definition type provider used in tests
    /// </summary>
    public class TestDefinitionTypeProvider : IDefinitionTypeProvider
    {
        public TypeDef TypeForDefinition(ZScriptParser.MemberNameContext context, string definitionName)
        {
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
    }
}