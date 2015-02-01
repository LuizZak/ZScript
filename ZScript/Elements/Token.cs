using System;
using ZScript.Runtime.Execution;

namespace ZScript.Elements
{
    /// <summary>
    /// Defines a single token for a virtual machine
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The type of this token
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The token itself. When the Type corresponds to a VM instruction, this value is not used
        /// </summary>
        public readonly Object TokenObject;

        /// <summary>
        /// An instruction that can be executed by a VM, in case this token has a token type of
        /// TokenType.Instruction or TokenType.Operator, otherwise this value should be ignored
        /// </summary>
        public readonly VmInstruction Instruction;

        /// <summary>
        /// Initializes a new instance of the Token class
        /// </summary>
        /// <param name="type">A type of token to create</param>
        /// <param name="tokenObject">The underlying token object to create</param>
        public Token(TokenType type, Object tokenObject)
            : this(type, tokenObject, VmInstruction.Noop)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Token class
        /// </summary>
        /// <param name="type">A type of token to create</param>
        /// <param name="tokenObject">The underlying token object to create</param>
        /// <param name="instruction">An instruction this token represents</param>
        public Token(TokenType type, Object tokenObject, VmInstruction instruction)
        {
            Type = type;
            TokenObject = tokenObject;
            Instruction = instruction;
        }
    }

    /// <summary>
    /// Specifies a type of token
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Specifies a token that contains a name of a variable/member name
        /// </summary>
        MemberName,
        /// <summary>
        /// Specifies a static (numeric or boolean) token
        /// </summary>
        Value,
        /// <summary>
        /// Specifies a static (string) token
        /// </summary>
        String,
        /// <summary>
        /// Specifies an operator token
        /// </summary>
        Operator,
        /// <summary>
        /// Specifies a statement token
        /// </summary>
        Statement,
        /// <summary>
        /// Specifies a token that contains an instruction that can be executed by a VM
        /// </summary>
        Instruction
    }
}