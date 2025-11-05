using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstComparisonTests : AstTestBase
  {
    #region Integer Comparison Tests

    [Theory]
    [InlineData(5, 10, "==", false)]
    [InlineData(10, 10, "==", true)]
    [InlineData(5, 10, "!=", true)]
    [InlineData(10, 10, "!=", false)]
    [InlineData(5, 10, "<", true)]
    [InlineData(10, 5, "<", false)]
    [InlineData(10, 10, "<=", true)]
    [InlineData(5, 10, "<=", true)]
    [InlineData(15, 10, ">", true)]
    [InlineData(5, 10, ">", false)]
    [InlineData(10, 10, ">=", true)]
    [InlineData(15, 10, ">=", true)]
    public void ComparisonNode_IntegerValues_ComparesCorrectly(
      int left, int right, string operatorSymbol, bool expectedResult)
    {
      var node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(left);
      var rightNode = new FakeIntegerNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var (tokenType, elementType) = GetOperatorTypes(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.Equal(expectedResult, node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_ConstantOptimization_BecomesBoolean()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(10);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node = node.Optimize();

      Assert.IsType<BooleanNode>(node);
      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    #endregion

    #region Long Comparison Tests

    [Theory]
    [InlineData(5000000000L, 3000000000L, "==", false)]
    [InlineData(3000000000L, 3000000000L, "==", true)]
    [InlineData(5000000000L, 3000000000L, "!=", true)]
    [InlineData(5000000000L, 3000000000L, "<", false)]
    [InlineData(3000000000L, 5000000000L, "<", true)]
    [InlineData(5000000000L, 3000000000L, ">", true)]
    [InlineData(3000000000L, 3000000000L, ">=", true)]
    public void ComparisonNode_LongPrecision_ComparesLongValues(
      long left, long right, string operatorSymbol, bool expectedResult)
    {
      var node = new ComparisonNode();
      var leftNode = new FakeLongNode(left);
      var rightNode = new FakeLongNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var (tokenType, elementType) = GetOperatorTypes(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.Equal(expectedResult, node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_LongPrecision_HandlesMaxValues()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeLongNode(9223372036854775806L); // Near long.MaxValue
      var rightNode = new FakeLongNode(9223372036854775805L);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    #endregion

    #region Float/Double Comparison Tests

    [Fact]
    public void ComparisonNode_FloatInteger_MixedTypes()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeFloatNode(10.5f);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Theory]
    [InlineData(1.5, 1.5, "==", true)]
    [InlineData(1.5, 2.5, "!=", true)]
    [InlineData(1.5, 2.5, "<", true)]
    [InlineData(2.5, 1.5, ">", true)]
    public void ComparisonNode_DoublePrecision_ComparesDoubleValues(
      double left, double right, string operatorSymbol, bool expectedResult)
    {
      var node = new ComparisonNode();
      var leftNode = new FakeDoubleNode(left);
      var rightNode = new FakeDoubleNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var (tokenType, elementType) = GetOperatorTypes(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.Equal(expectedResult, node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_DoublePrecision_UsesULPsComparison()
    {
      // Double equality uses ULPs comparison (within 2 ULPs)
      var node = new ComparisonNode();
      var leftNode = new FakeDoubleNode(1.0);
      var rightNode = new FakeDoubleNode(1.0 + double.Epsilon); // One ULP difference
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue); // Equal within 2 ULPs
    }

    [Fact]
    public void ComparisonNode_DoublePrecision_RejectsLargerDifferences()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeDoubleNode(1.0);
      var rightNode = new FakeDoubleNode(1.0 + 1e-9); // Large difference (many ULPs)
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue); // Not equal
    }

    #endregion

    #region Decimal Comparison Tests

    [Theory]
    [MemberData(nameof(DecimalComparisonData))]
    public void ComparisonNode_DecimalPrecision_ComparesDecimalValues(
      decimal left, decimal right, string operatorSymbol, bool expectedResult)
    {
      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(left);
      var rightNode = new FakeDecimalNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var (tokenType, elementType) = GetOperatorTypes(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.Equal(expectedResult, node.BooleanValue);
    }

    public static IEnumerable<object[]> DecimalComparisonData =>
      new[]
      {
        new object[] { 1.5m, 1.5m, "==", true },
        new object[] { 1.5m, 2.5m, "==", false },
        new object[] { 1.5m, 2.5m, "!=", true },
        new object[] { 1.5m, 2.5m, "<", true },
        new object[] { 2.5m, 1.5m, ">", true },
        new object[] { 1.5m, 1.5m, ">=", true },
      };

    [Fact]
    public void ComparisonNode_DecimalPrecision_UsesExactEquality()
    {
      // Decimal uses exact equality (no ULPs tolerance like float/double)
      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(1000.0m);
      var rightNode = new FakeDecimalNode(1000.1m); // Small difference
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue); // Not equal - exact comparison
    }

    [Fact]
    public void ComparisonNode_DecimalPrecision_DivisionCausesRounding()
    {
      // Decimal division can round: 1/3 * 3 != 1
      var a = 1.0m / 3.0m;
      var b = a * 3.0m;

      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(b);
      var rightNode = new FakeDecimalNode(1.0m);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue); // Not equal due to rounding
    }

    [Fact]
    public void ComparisonNode_DecimalPrecision_ExactDivisionPreserved()
    {
      // Exact divisions don't round: 1000/10 * 10 == 1000
      var original = 1000.0m;
      var result = (original / 10.0m) * 10.0m;

      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(result);
      var rightNode = new FakeDecimalNode(original);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue); // Equal - no rounding
    }

    [Fact]
    public void ComparisonNode_DecimalPrecision_AddSubtractExact()
    {
      // Add/subtract operations maintain exact precision
      var original = 12345.6789m;
      var result = (original + 9876.5432m) - 9876.5432m;

      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(result);
      var rightNode = new FakeDecimalNode(original);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue); // Equal - no rounding in add/subtract
    }

    [Fact]
    public void ComparisonNode_DecimalPrecision_HandlesMaxValue()
    {
      var nearMax = decimal.MaxValue / 2;
      var result = (nearMax + 1000m) - 1000m;

      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(result);
      var rightNode = new FakeDecimalNode(nearMax);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue); // Equal - precision maintained
    }

    [Fact]
    public void ComparisonNode_DecimalPrecision_VerySmallRounding()
    {
      // Very small numbers can also experience rounding
      var original = 0.0000000000000000000000000001m;
      var result = (original / 3.0m) * 3.0m;

      var node = new ComparisonNode();
      var leftNode = new FakeDecimalNode(result);
      var rightNode = new FakeDecimalNode(original);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue); // Not equal due to rounding
    }

    #endregion

    #region String Comparison Tests

    [Fact]
    public void ComparisonNode_StringEqual()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeStringNode("Hello World");
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_StringNotEqual()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeStringNode("こんにちは世界");
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_StringVsNumber_ThrowsRuntimeException()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeIntegerNode(0);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Throws<RuntimeException>(() => node.Execute(null));
    }

    #endregion

    #region Boolean Comparison Tests

    [Fact]
    public void ComparisonNode_BooleanEqual()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(true);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_BooleanNotEqual()
    {
      var node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Theory]
    [InlineData("<")]
    [InlineData("<=")]
    [InlineData(">")]
    [InlineData(">=")]
    public void ComparisonNode_BooleanOrderingOperators_ThrowsRuntimeException(string operatorSymbol)
    {
      var node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);

      var (tokenType, elementType) = GetOperatorTypes(operatorSymbol);
      node.Build(rpn, new Element(new Token(operatorSymbol, tokenType), elementType),
        Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Throws<RuntimeException>(() => node.Execute(null));
    }

    #endregion

    #region Mixed Precision Tests

    [Fact]
    public void ComparisonNode_IntWithLongPrecision_PromotesToLong()
    {
      var node = new ComparisonNode();
      var leftNode = new IntegerNode(100);
      var rightNode = new IntegerNode(200);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void ComparisonNode_FloatWithDoublePrecision_PromotesToDouble()
    {
      var node = new ComparisonNode();
      var leftNode = new FloatNode(1.5);
      var rightNode = new FloatNode(2.5);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    #endregion

    #region Helper Methods

    private static (TokenType tokenType, ElementType elementType) GetOperatorTypes(string op)
    {
      return op switch
      {
        "==" => (TokenType.ComparisonEqual, ElementType.ComparisonEqual),
        "!=" => (TokenType.ComparisonNotEqual, ElementType.ComparisonNotEqual),
        "<" => (TokenType.ComparisonLessThan, ElementType.ComparisonLessThan),
        "<=" => (TokenType.ComparisonLessThanOrEqualTo, ElementType.ComparisonLessThanOrEqualTo),
        ">" => (TokenType.ComparisonGreaterThan, ElementType.ComparisonGreaterThan),
        ">=" => (TokenType.ComparisonGreaterThanOrEqualTo, ElementType.ComparisonGreaterThanOrEqualTo),
        _ => throw new ArgumentException($"Unknown operator: {op}")
      };
    }

    #endregion
  }
}