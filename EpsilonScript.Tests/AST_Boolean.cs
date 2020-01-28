using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_Boolean
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_BuildBoolean_Succeeds(Element element, ValueType expectedNodeType, int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new BooleanNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null,
        null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
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
            ValueType.Boolean,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            ValueType.Boolean,
            0,
            0.0f,
            false
          },
        };
      }
    }
  }
}