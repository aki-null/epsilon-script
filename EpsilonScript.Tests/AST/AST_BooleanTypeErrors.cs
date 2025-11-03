using System;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_BooleanTypeErrors : AstTestBase
  {
    #region Arithmetic Operations with Boolean on Left Side

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanLeftIntegerRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeIntegerNode(1));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanLeftFloatRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(false), new FakeFloatNode(1.5f));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanLeftLongRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeLongNode(1000000000L));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanLeftDoubleRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(false), new FakeDoubleNode(2.5));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanLeftDecimalRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeDecimalNode(3.14m));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_BooleanBothSides_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    #endregion

    #region Arithmetic Operations with Boolean on Right Side

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_IntegerLeftBooleanRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(1), new FakeBooleanNode(true));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_FloatLeftBooleanRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(2.5f), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_LongLeftBooleanRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeLongNode(5000000000L), new FakeBooleanNode(true));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_DoubleLeftBooleanRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeDoubleNode(3.14159), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    [Theory]
    [InlineData("+", ElementType.AddOperator, TokenType.PlusSign)]
    [InlineData("-", ElementType.SubtractOperator, TokenType.MinusSign)]
    [InlineData("*", ElementType.MultiplyOperator, TokenType.MultiplyOperator)]
    [InlineData("/", ElementType.DivideOperator, TokenType.DivideOperator)]
    [InlineData("%", ElementType.ModuloOperator, TokenType.ModuloOperator)]
    internal void ArithmeticNode_DecimalLeftBooleanRight_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeDecimalNode(99.99m), new FakeBooleanNode(true));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("Boolean", ex.Message);
    }

    #endregion

    #region Comparison Operations: Boolean vs Non-Boolean

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsInteger_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeIntegerNode(1));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_IntegerVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeIntegerNode(0), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsFloat_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeFloatNode(1.5f));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_FloatVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeFloatNode(0.0f), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsLong_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(false), new FakeLongNode(9999999999L));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_LongVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeLongNode(1L), new FakeBooleanNode(true));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsDouble_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeDoubleNode(2.71828));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_DoubleVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeDoubleNode(1.0), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsDecimal_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(false), new FakeDecimalNode(42.42m));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_DecimalVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeDecimalNode(0.0m), new FakeBooleanNode(true));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);

      var ex = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_BooleanVsString_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeStringNode("true"));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Throws<RuntimeException>(() => node.Execute(null));
    }

    [Theory]
    [InlineData("==", ElementType.ComparisonEqual, TokenType.ComparisonEqual)]
    [InlineData("!=", ElementType.ComparisonNotEqual, TokenType.ComparisonNotEqual)]
    internal void ComparisonNode_StringVsBoolean_ThrowsRuntimeException(
      string operatorSymbol, ElementType elementType, TokenType tokenType)
    {
      var node = new ComparisonNode();
      var rpn = CreateStack(new FakeStringNode("false"), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, tokenType), elementType);

      node.Build(rpn, element, Compiler.Options.None, null, null,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Throws<RuntimeException>(() => node.Execute(null));
    }

    #endregion
  }
}