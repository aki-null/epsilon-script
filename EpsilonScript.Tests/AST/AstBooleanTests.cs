using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstBooleanTests : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void AST_BuildBoolean_Succeeds(Element element, ExtendedType expectedNodeType, int expectedInt,
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
    internal void AST_CreateBoolean_Succeeds(bool value)
    {
      var node = new BooleanNode(value);
      var expectedInt = value ? 1 : 0;
      var expectedFloat = (float)expectedInt;
      Assert.Equal(ExtendedType.Boolean, node.ValueType);
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
            ExtendedType.Boolean,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            ExtendedType.Boolean,
            0,
            0.0f,
            false
          },
        };
      }
    }
  }
}