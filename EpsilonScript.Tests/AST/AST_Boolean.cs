using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Boolean : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_BuildBoolean_Succeeds(Element element, Type expectedNodeType, int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new BooleanNode();
      var rpn = CreateStack();
      node.Build(rpn, element, Compiler.Options.None, null,
        null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AST_CreateBoolean_Succeeds(bool value)
    {
      var node = new BooleanNode(value);
      var expectedInt = value ? 1 : 0;
      var expectedFloat = (float)expectedInt;
      Assert.Equal(Type.Boolean, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(value, node.BooleanValue);
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
            Type.Boolean,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            Type.Boolean,
            0,
            0.0f,
            false
          },
        };
      }
    }
  }
}