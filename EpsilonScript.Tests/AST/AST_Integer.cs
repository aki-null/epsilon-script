using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Integer : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_BuildInteger_Succeeds(Element element, ValueType expectedNodeType, int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      node.Build(rpn, element, Compiler.Options.None, null,
        null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2147483647)]
    public void AST_CreateInteger_Succeeds(int value)
    {
      var node = new IntegerNode(value);
      var expectedFloat = (float)value;
      var expectedBool = value != 0;
      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(value, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
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