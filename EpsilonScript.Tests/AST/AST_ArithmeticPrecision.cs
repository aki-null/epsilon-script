using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_ArithmeticPrecision : AstTestBase
  {
    #region Long Arithmetic Tests

    [Theory]
    [InlineData(5000000000L, 3000000000L, "+", 8000000000L)] // Addition beyond int range
    [InlineData(5000000000L, 3000000000L, "-", 2000000000L)] // Subtraction
    [InlineData(1000000L, 1000000L, "*", 1000000000000L)] // Multiplication
    [InlineData(9000000000L, 3L, "/", 3000000000L)] // Division
    [InlineData(9000000007L, 3L, "%", 1L)] // Modulo
    public void ArithmeticNode_WithLongPrecision_PerformsLongArithmetic(
      long left, long right, string operatorSymbol, long expectedResult)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeLongNode(left);
      var rightNode = new FakeLongNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ValueType.Long, node.ValueType);
      Assert.Equal(expectedResult, node.LongValue);
    }

    [Fact]
    public void ArithmeticNode_WithLongPrecision_OverflowWraps()
    {
      // Test overflow behavior with long arithmetic
      var node = new ArithmeticNode();
      var leftNode = new FakeLongNode(long.MaxValue);
      var rightNode = new FakeLongNode(1L);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ValueType.Long, node.ValueType);
      // Overflow wraps in unchecked context
      Assert.Equal(long.MinValue, node.LongValue);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("%")]
    public void ArithmeticNode_WithLongPrecision_DivideByZero_ThrowsException(string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeLongNode(5000000000L);
      var rightNode = new FakeLongNode(0L);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    #endregion

    #region Double Arithmetic Tests

    [Theory]
    [InlineData(1.5, 2.5, "+", 4.0)]
    [InlineData(5.5, 3.5, "-", 2.0)]
    [InlineData(2.5, 4.0, "*", 10.0)]
    [InlineData(10.0, 2.5, "/", 4.0)]
    [InlineData(10.5, 3.0, "%", 1.5)]
    public void ArithmeticNode_WithDoublePrecision_PerformsDoubleArithmetic(
      double left, double right, string operatorSymbol, double expectedResult)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeDoubleNode(left);
      var rightNode = new FakeDoubleNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ValueType.Double, node.ValueType);
      Assert.Equal(expectedResult, node.DoubleValue, precision: 10);
    }

    [Fact]
    public void ArithmeticNode_WithDoublePrecision_MaintainsPrecision()
    {
      // Test that double maintains more precision than float
      var node = new ArithmeticNode();
      var leftNode = new FakeDoubleNode(0.1);
      var rightNode = new FakeDoubleNode(0.2);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      Assert.Equal(ValueType.Double, node.ValueType);
      // Double should maintain precision better than float
      Assert.Equal(0.3, node.DoubleValue, precision: 15);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("%")]
    public void ArithmeticNode_WithDoublePrecision_DivideByZero_ThrowsException(string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeDoubleNode(5.5);
      var rightNode = new FakeDoubleNode(0.0);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    #endregion

    #region Decimal Arithmetic Tests

    [Theory]
    [MemberData(nameof(DecimalArithmeticData))]
    public void ArithmeticNode_WithDecimalPrecision_PerformsDecimalArithmetic(
      decimal left, decimal right, string operatorSymbol, decimal expectedResult)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeDecimalNode(left);
      var rightNode = new FakeDecimalNode(right);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ValueType.Decimal, node.ValueType);
      Assert.Equal(expectedResult, node.DecimalValue);
    }

    public static IEnumerable<object[]> DecimalArithmeticData
    {
      get
      {
        return new[]
        {
          new object[] { 1.5m, 2.5m, "+", 4.0m },
          new object[] { 5.5m, 3.5m, "-", 2.0m },
          new object[] { 2.5m, 4.0m, "*", 10.0m },
          new object[] { 10.0m, 2.5m, "/", 4.0m },
          new object[] { 10.5m, 3.0m, "%", 1.5m },
          // High precision tests
          new object[] { 0.1m, 0.2m, "+", 0.3m },
          new object[] { 1.0000000000000000001m, 1.0000000000000000001m, "+", 2.0000000000000000002m },
        };
      }
    }

    [Fact]
    public void ArithmeticNode_WithDecimalPrecision_MaintainsHighPrecision()
    {
      // Test that decimal maintains exact precision
      var node = new ArithmeticNode();
      var leftNode = new FakeDecimalNode(0.1m);
      var rightNode = new FakeDecimalNode(0.2m);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      Assert.Equal(ValueType.Decimal, node.ValueType);
      // Decimal should be exactly 0.3 (no floating point errors)
      Assert.Equal(0.3m, node.DecimalValue);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("%")]
    public void ArithmeticNode_WithDecimalPrecision_DivideByZero_ThrowsException(string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeDecimalNode(5.5m);
      var rightNode = new FakeDecimalNode(0.0m);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    [Theory]
    [InlineData("+")]
    [InlineData("*")]
    public void ArithmeticNode_WithDecimalPrecision_Overflow_ThrowsException(string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var leftNode = new FakeDecimalNode(decimal.MaxValue);
      var rightNode = new FakeDecimalNode(operatorSymbol == "+" ? 1m : 2m);
      var rpn = CreateStack(leftNode, rightNode);

      var operatorType = GetElementTypeFromOperator(operatorSymbol);
      var tokenType = GetTokenTypeFromOperator(operatorSymbol);
      var element = new Element(new Token(operatorSymbol, tokenType), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      Assert.Throws<OverflowException>(() => node.Execute(null));
    }

    #endregion

    #region Type Promotion Tests

    [Fact]
    public void ArithmeticNode_WithLongPrecision_PromotesToLong()
    {
      // When integer precision is Long, int + int should produce long
      var node = new ArithmeticNode();
      var leftNode = new IntegerNode(10);
      var rightNode = new IntegerNode(20);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      node.Execute(null);

      // Result should be promoted to Long based on precision setting
      Assert.Equal(ValueType.Long, node.ValueType);
      Assert.Equal(30L, node.LongValue);
    }

    [Fact]
    public void ArithmeticNode_WithDoublePrecision_PromotesToDouble()
    {
      // When float precision is Double, operations should produce double
      var node = new ArithmeticNode();
      var leftNode = new FloatNode(1.5);
      var rightNode = new FloatNode(2.5);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      node.Execute(null);

      // Result should be promoted to Double based on precision setting
      Assert.Equal(ValueType.Double, node.ValueType);
      Assert.Equal(4.0, node.DoubleValue);
    }

    [Fact]
    public void ArithmeticNode_WithDecimalPrecision_PromotesToDecimal()
    {
      // When float precision is Decimal, operations should produce decimal
      var node = new ArithmeticNode();
      var leftNode = new FloatNode(1.5m);
      var rightNode = new FloatNode(2.5m);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      node.Execute(null);

      // Result should be promoted to Decimal based on precision setting
      Assert.Equal(ValueType.Decimal, node.ValueType);
      Assert.Equal(4.0m, node.DecimalValue);
    }

    [Fact]
    public void ArithmeticNode_MixedIntAndFloat_PromotesToFloatPrecision()
    {
      // When mixing int and float, result should use configured float precision
      var node = new ArithmeticNode();
      var leftNode = new IntegerNode(10);
      var rightNode = new FloatNode(2.5);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);
      node.Execute(null);

      // Result should be Double (the configured float precision)
      Assert.Equal(ValueType.Double, node.ValueType);
      Assert.Equal(12.5, node.DoubleValue);
    }

    #endregion

    #region Helper Methods

    private ElementType GetElementTypeFromOperator(string op)
    {
      return op switch
      {
        "+" => ElementType.AddOperator,
        "-" => ElementType.SubtractOperator,
        "*" => ElementType.MultiplyOperator,
        "/" => ElementType.DivideOperator,
        "%" => ElementType.ModuloOperator,
        _ => throw new ArgumentException($"Unknown operator: {op}")
      };
    }

    private TokenType GetTokenTypeFromOperator(string op)
    {
      return op switch
      {
        "+" => TokenType.PlusSign,
        "-" => TokenType.MinusSign,
        "*" => TokenType.MultiplyOperator,
        "/" => TokenType.DivideOperator,
        "%" => TokenType.ModuloOperator,
        _ => throw new ArgumentException($"Unknown operator: {op}")
      };
    }

    #endregion
  }
}