using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_BinaryOperator
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    private void AST_BinaryOperator_Succeeds(Element element, Node leftNode, Node rightNode,
      Bytecode.InstructionType expectedType)
    {
      var node = new BinaryOperatorNode();
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, element, Compiler.Options.None, null);
      var prog = new EpsilonScript.Bytecode.MutableProgram(null);
      byte nextRegisterIdx = 0;
      node.Encode(prog, ref nextRegisterIdx, null);

      byte depNextRegisterIdx = 0;
      ASTTestHelper.ValidateDependencyProgramEncode(ref depNextRegisterIdx, out var depCount, prog, leftNode,
        rightNode);

      Assert.Equal(depCount + 1, prog.Instructions.Count);
      Assert.Equal(expectedType, prog.Instructions[depCount].Type);
      Assert.Equal(depNextRegisterIdx - 2, prog.Instructions[depCount].reg0);
      Assert.Equal(depNextRegisterIdx - 2, prog.Instructions[depCount].reg1);
      Assert.Equal(depNextRegisterIdx - 1, prog.Instructions[depCount].reg2);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Add
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode(""),
            new FakeStringNode(""),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Hello "),
            new FakeStringNode("World"),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Life "),
            new FakeIntegerNode(42),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Pi "),
            new FakeFloatNode(3.14159265359f),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("true "),
            new FakeBooleanNode(true),
            Bytecode.InstructionType.Add
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("false "),
            new FakeBooleanNode(false),
            Bytecode.InstructionType.Add
          },
          // Subtract
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Subtract
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Subtract
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Subtract
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Subtract
          },
          // Multiply
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Multiply
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Multiply
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Multiply
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Multiply
          },
          // Divide
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Divide
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Divide
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Divide
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Divide
          },
          // Modulo
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Modulo
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Modulo
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.Modulo
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.Modulo
          },
          // ComparisonEqual
          new object[]
          {
            new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonEqual
          },
          new object[]
          {
            new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonEqual
          },
          new object[]
          {
            new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonEqual
          },
          new object[]
          {
            new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonEqual
          },
          // ComparisonNotEqual
          new object[]
          {
            new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonNotEqual
          },
          new object[]
          {
            new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonNotEqual
          },
          new object[]
          {
            new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonNotEqual
          },
          new object[]
          {
            new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonNotEqual
          },
          // ComparisonLessThan
          new object[]
          {
            new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonLessThan
          },
          new object[]
          {
            new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonLessThan
          },
          new object[]
          {
            new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonLessThan
          },
          new object[]
          {
            new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonLessThan
          },
          // ComparisonGreaterThan
          new object[]
          {
            new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonGreaterThan
          },
          new object[]
          {
            new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonGreaterThan
          },
          new object[]
          {
            new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonGreaterThan
          },
          new object[]
          {
            new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonGreaterThan
          },
          // ComparisonLessThanOrEqualTo
          new object[]
          {
            new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo), ElementType.ComparisonLessThanOrEqualTo),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonLessThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo), ElementType.ComparisonLessThanOrEqualTo),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonLessThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo), ElementType.ComparisonLessThanOrEqualTo),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonLessThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo), ElementType.ComparisonLessThanOrEqualTo),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonLessThanOrEqualTo
          },
          // ComparisonGreaterThanOrEqualTo
          new object[]
          {
            new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonGreaterThanOrEqualTo),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonGreaterThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonGreaterThanOrEqualTo),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonGreaterThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonGreaterThanOrEqualTo),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            Bytecode.InstructionType.ComparisonGreaterThanOrEqualTo
          },
          new object[]
          {
            new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonGreaterThanOrEqualTo),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            Bytecode.InstructionType.ComparisonGreaterThanOrEqualTo
          },
        };
      }
    }
  }
}

