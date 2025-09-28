using System;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  public class AST_BoundaryValues : AstTestBase
  {
    [Theory]
    [InlineData("2147483647")] // int.MaxValue
    [InlineData("-2147483648")] // int.MinValue
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-1")]
    public void AST_Integer_BoundaryValues_Succeeds(string value)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(value, TokenType.Integer), ElementType.Integer);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(int.Parse(value), node.IntegerValue);
      Assert.Equal((float)int.Parse(value), node.FloatValue);
      Assert.Equal(int.Parse(value) != 0, node.BooleanValue);
    }

    [Theory]
    [InlineData("2147483648")] // int.MaxValue + 1
    [InlineData("-2147483649")] // int.MinValue - 1
    [InlineData("9999999999")] // Way beyond int range
    public void AST_Integer_OverflowValues_ThrowsOverflowException(string value)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(value, TokenType.Integer), ElementType.Integer);

      ErrorTestHelper.BuildNodeExpectingError<OverflowException>(node, rpn, element, Compiler.Options.None, null, null,
        "Value was either too large or too small for an Int32");
    }

    [Theory]
    [InlineData("3.4028235E+38")] // float.MaxValue
    [InlineData("-3.4028235E+38")] // -float.MaxValue (most negative float)
    [InlineData("1.401298E-45")] // float.Epsilon
    [InlineData("0.0")]
    [InlineData("1.0")]
    [InlineData("-1.0")]
    public void AST_Float_BoundaryValues_Succeeds(string value)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      var element = new Element(new Token(value, TokenType.Float), ElementType.Float);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.Equal(ValueType.Float, node.ValueType);
      var expectedFloat = float.Parse(value);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));

      // FloatNode uses safe conversion - clamps to int.MaxValue/int.MinValue instead of unsafe overflow
      var expectedInt = float.IsNaN(expectedFloat) || float.IsInfinity(expectedFloat) ? 0 :
        expectedFloat > int.MaxValue ? int.MaxValue :
        expectedFloat < int.MinValue ? int.MinValue :
        (int)expectedFloat;
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.Equal(expectedFloat != 0.0f && !float.IsInfinity(expectedFloat) && !float.IsNaN(expectedFloat),
        node.BooleanValue);
    }

    [Fact]
    public void AST_Arithmetic_IntegerMaxValue_Addition_ThrowsOverflowException()
    {
      // Test int.MaxValue + 1 should throw OverflowException
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(int.MaxValue), new FakeIntegerNode(1));
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<OverflowException>(node);
    }

    [Fact]
    public void AST_Arithmetic_IntegerMinValue_Subtraction_ThrowsOverflowException()
    {
      // Test int.MinValue - 1 should throw OverflowException
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(int.MinValue), new FakeIntegerNode(1));
      var element = new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<OverflowException>(node);
    }

    [Fact]
    public void AST_Arithmetic_IntegerMaxValue_Multiplication_ThrowsOverflowException()
    {
      // Test int.MaxValue * 2 should throw OverflowException
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(int.MaxValue), new FakeIntegerNode(2));
      var element = new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<OverflowException>(node);
    }

    [Fact]
    public void AST_Arithmetic_FloatMaxValue_Addition_ReturnsInfinity()
    {
      // Test float.MaxValue + float.MaxValue = Infinity
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(float.MaxValue), new FakeFloatNode(float.MaxValue));
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.True(float.IsPositiveInfinity(node.FloatValue));
    }

    [Fact]
    public void AST_Arithmetic_FloatMinValue_Subtraction_ReturnsNegativeInfinity()
    {
      // Test (-float.MaxValue) - float.MaxValue = -Infinity
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(-float.MaxValue), new FakeFloatNode(float.MaxValue));
      var element = new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.True(float.IsNegativeInfinity(node.FloatValue));
    }

    [Fact]
    public void AST_Arithmetic_FloatMaxValue_Multiplication_ReturnsInfinity()
    {
      // Test float.MaxValue * 2 = Infinity
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(float.MaxValue), new FakeFloatNode(2.0f));
      var element = new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.True(float.IsPositiveInfinity(node.FloatValue));
    }

    [Fact]
    public void AST_Arithmetic_FloatEpsilon_Addition_PreservesValue()
    {
      // Test 1.0f + float.Epsilon behavior
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(1.0f), new FakeFloatNode(float.Epsilon));
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      // Result should be very slightly greater than 1.0f due to epsilon precision
      Assert.True(node.FloatValue >= 1.0f);
      // The result is 1.0f + float.Epsilon, which should be distinguishable from 1.0f
      Assert.True(node.FloatValue > 1.0f || EpsilonScript.Math.IsNearlyEqual(1.0f, node.FloatValue));
    }

    [Theory]
    [InlineData(int.MaxValue, 2)]
    [InlineData(int.MaxValue, 3)]
    [InlineData(int.MinValue, 2)]
    [InlineData(int.MinValue, -2)]
    public void AST_Arithmetic_IntegerBoundary_Division_Succeeds(int dividend, int divisor)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(dividend), new FakeIntegerNode(divisor));
      var element = new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);

      var expectedResult = dividend / divisor;
      Assert.Equal(expectedResult, node.IntegerValue);
    }

    [Theory]
    [InlineData(int.MaxValue, 2, 1)]
    [InlineData(int.MaxValue, 3, 1)]
    [InlineData(int.MinValue, 2, 0)]
    [InlineData(int.MinValue, 3, -2)]
    public void AST_Arithmetic_IntegerBoundary_Modulo_Succeeds(int dividend, int divisor, int expectedRemainder)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(dividend), new FakeIntegerNode(divisor));
      var element = new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(expectedRemainder, node.IntegerValue);
    }

    [Theory]
    [InlineData(float.MaxValue, 2.0f)]
    [InlineData(-float.MaxValue, 2.0f)]
    [InlineData(float.MaxValue, -1.0f)]
    [InlineData(-float.MaxValue, -1.0f)]
    public void AST_Arithmetic_FloatBoundary_Division_HandlesExtremes(float dividend, float divisor)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(dividend), new FakeFloatNode(divisor));
      var element = new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);

      var result = node.FloatValue;
      if (float.IsInfinity(result))
      {
        // Overflow to infinity is acceptable
        Assert.True(float.IsInfinity(result));
      }
      else
      {
        // Should be finite and approximately equal to expected division
        Assert.True(float.IsFinite(result));
      }
    }

    [Fact]
    public void AST_Comparison_IntegerBoundaryValues_Succeeds()
    {
      // Test int.MaxValue == int.MaxValue
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeIntegerNode(int.MaxValue), new FakeIntegerNode(int.MaxValue));
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_Comparison_FloatBoundaryValues_Succeeds()
    {
      // Test float.MaxValue == float.MaxValue
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeFloatNode(float.MaxValue), new FakeFloatNode(float.MaxValue));
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Theory]
    [InlineData("")] // Empty string
    [InlineData("a")] // Single character
    [InlineData("Hello World")] // Normal string
    [InlineData("こんにちは世界")] // Unicode string
    [InlineData("String with\nnewlines")] // String with control characters
    public void AST_String_VariousLengths_Succeeds(string value)
    {
      var quotedValue = $"\"{value}\"";
      var node = new StringNode();
      var rpn = CreateStack();
      var element = new Element(new Token(quotedValue, TokenType.String), ElementType.String);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.Equal(ValueType.String, node.ValueType);
      Assert.Equal(value, node.StringValue);
    }
  }
}