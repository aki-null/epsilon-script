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
    [MemberData(nameof(CorrectBooleanData))]
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

    public static IEnumerable<object[]> CorrectBooleanData
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

    [Theory]
    [MemberData(nameof(CorrectFloatData))]
    internal void AST_BuildFloat_Succeeds(Element element, Instruction expected)
    {
      var node = new FloatNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null);

      var prog = new EpsilonScript.Bytecode.MutableProgram(null);
      byte nextRegisterIdx = 0;
      node.Encode(prog, ref nextRegisterIdx, null);

      ASTTestHelper.AssertSingleInstructionProgram(expected, prog);
    }

    public static IEnumerable<object[]> CorrectFloatData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("0.0", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 0.0f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1.0", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 1.0f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1.2", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 1.2f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1e+5", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 1e+5f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1E+5", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 1e+5f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1.2E+5", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 1.2E+5f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("3.402823E+38", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = 3.402823E+38f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("-3.40282347E+38", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = -3.40282347E+38f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("NaN", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = float.NaN,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("-0.0", TokenType.Float), ElementType.Float),
            new Instruction
            {
              FloatValue = -0.0f,
              Type = InstructionType.LoadFloat,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
        };
      }
    }

    [Theory]
    [MemberData(nameof(CorrectIntegerData))]
    internal void AST_BuildInteger_Succeeds(Element element, Instruction expected)
    {
      var node = new IntegerNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null);

      var prog = new EpsilonScript.Bytecode.MutableProgram(null);
      byte nextRegisterIdx = 0;
      node.Encode(prog, ref nextRegisterIdx, null);

      ASTTestHelper.AssertSingleInstructionProgram(expected, prog);
    }

    public static IEnumerable<object[]> CorrectIntegerData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("0", TokenType.Integer), ElementType.Integer),
            new Instruction
            {
              IntegerValue = 0,
              Type = InstructionType.LoadInteger,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Instruction
            {
              IntegerValue = 1,
              Type = InstructionType.LoadInteger,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("2147483647", TokenType.Integer), ElementType.Integer),
            new Instruction
            {
              IntegerValue = 2147483647,
              Type = InstructionType.LoadInteger,
              reg0 = 0,
              reg1 = 0,
              reg2 = 0
            }
          },
          new object[]
          {
            new Element(new Token("-2147483648", TokenType.Integer), ElementType.Integer),
            new Instruction
            {
              IntegerValue = -2147483648,
              Type = InstructionType.LoadInteger,
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
