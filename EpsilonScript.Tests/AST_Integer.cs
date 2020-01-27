using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests
{
  public class AST_Integer
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_BuildInteger_Succeeds(Element element, ValueType expectedNodeType, int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new IntegerNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null,
        null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Fact]
    public void AST_BuildInteger_Overflows()
    {
      Assert.Throws<OverflowException>(() =>
      {
        var node = new IntegerNode();
        var rpn = new Stack<Node>();
        node.Build(rpn, new Element(new Token("2147483648", TokenType.Integer), ElementType.Integer),
          Compiler.Options.None, null,
          null);
      });
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("0", TokenType.Integer), ElementType.Integer),
            ValueType.Integer,
            0,
            0.0f,
            false
          },
          new object[]
          {
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            ValueType.Integer,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("2147483647", TokenType.Integer), ElementType.Integer),
            ValueType.Integer,
            2147483647,
            2147483647.0f,
            true
          },
        };
      }
    }
  }
}