using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using EpsilonScript.Bytecode;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_Boolean
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void AST_BuildBoolean_Succeeds(Element element, Instruction expected)
    {
      var node = new BooleanNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null);

      var prog = new EpsilonScript.Bytecode.MutableProgram(null);
      byte nextRegisterIdx = 0;
      node.Encode(prog, ref nextRegisterIdx, null);

      ASTTestHelper.AssertSingleInstructionProgram(expected, prog);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            new Instruction
            {
              BooleanValue = true,
              Type = InstructionType.LoadBoolean,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            new Instruction
            {
              BooleanValue = false,
              Type = InstructionType.LoadBoolean,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
        };
      }
    }
  }
}
