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
  public class AstAssignmentPrecisionTests : AstTestBase
  {
    private static Token CreateToken(string text, TokenType type)
    {
      return new Token(text.AsMemory(), type);
    }

    private static TokenType GetTokenType(ElementType operatorType)
    {
      return operatorType switch
      {
        ElementType.AssignmentOperator => TokenType.AssignmentOperator,
        ElementType.AssignmentAddOperator => TokenType.AssignmentAddOperator,
        ElementType.AssignmentSubtractOperator => TokenType.AssignmentSubtractOperator,
        ElementType.AssignmentMultiplyOperator => TokenType.AssignmentMultiplyOperator,
        ElementType.AssignmentDivideOperator => TokenType.AssignmentDivideOperator,
        ElementType.AssignmentModuloOperator => TokenType.AssignmentModuloOperator,
        _ => throw new ArgumentOutOfRangeException(nameof(operatorType))
      };
    }

    [Fact]
    internal void Assignment_ConvertsDoubleToFloat_WhenAssigningToFloatVariable()
    {
      // Create variable with Float type
      var variable = new VariableValue(Type.Float);
      variable.FloatValue = 0.0f;
      var leftNode = new FakeVariableNode(variable);

      // Create Double node with high precision value
      var rightNode = new FakeDoubleNode(1.23456789012345);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Double value should be converted to float precision
      Assert.Equal(1.2345679f, variable.FloatValue, precision: 6);
    }

    [Fact]
    internal void Assignment_ConvertsDecimalToFloat_WhenAssigningToFloatVariable()
    {
      // Create variable with Float type
      var variable = new VariableValue(Type.Float);
      variable.FloatValue = 0.0f;
      var leftNode = new FakeVariableNode(variable);

      // Create Decimal node with very high precision
      var rightNode = new FakeDecimalNode(1.234567890123456789m);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Decimal value should be converted to float precision
      Assert.Equal(1.2345679f, variable.FloatValue, precision: 6);
    }

    [Theory]
    [InlineData(Compiler.FloatPrecision.Float)]
    [InlineData(Compiler.FloatPrecision.Double)]
    [InlineData(Compiler.FloatPrecision.Decimal)]
    internal void Assignment_PreservesPrecision_WhenAssigningToDoubleVariable(
      Compiler.FloatPrecision floatPrecision)
    {
      // Create variable with Double type
      var variable = new VariableValue(Type.Double);
      variable.DoubleValue = 0.0;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with high precision value
      var highPrecisionValue = 1.23456789012345;
      Node rightNode = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => new FakeFloatNode((float)highPrecisionValue),
        Compiler.FloatPrecision.Double => new FakeDoubleNode(highPrecisionValue),
        Compiler.FloatPrecision.Decimal => new FakeDecimalNode((decimal)highPrecisionValue),
        _ => throw new ArgumentException("Unsupported precision")
      };

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, floatPrecision, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Double variable receives value with precision limited by right-hand side type
      var expectedValue = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => (float)highPrecisionValue, // Float precision
        Compiler.FloatPrecision.Double => highPrecisionValue, // Double precision
        Compiler.FloatPrecision.Decimal => highPrecisionValue, // Decimal converted to double
        _ => throw new ArgumentException("Unsupported precision")
      };
      Assert.Equal(expectedValue, variable.DoubleValue, precision: 14);
    }

    [Fact]
    internal void Assignment_PreservesPrecision_WhenAssigningToDecimalVariable()
    {
      // Create variable with Decimal type
      var variable = new VariableValue(Type.Decimal);
      variable.DecimalValue = 0m;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with high precision decimal value
      var highPrecisionValue = 1.234567890123456789012345678m;
      var rightNode = new FakeDecimalNode(highPrecisionValue);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Decimal variable should preserve full decimal precision
      Assert.Equal(highPrecisionValue, variable.DecimalValue);
    }

    [Theory]
    [InlineData(ElementType.AssignmentAddOperator, "+=")]
    [InlineData(ElementType.AssignmentSubtractOperator, "-=")]
    [InlineData(ElementType.AssignmentMultiplyOperator, "*=")]
    [InlineData(ElementType.AssignmentDivideOperator, "/=")]
    [InlineData(ElementType.AssignmentModuloOperator, "%=")]
    internal void Assignment_CompoundAssignments_PreserveDoublePrecision(
      ElementType operatorType, string operatorSymbol)
    {
      // Create variable with Double type
      var variable = new VariableValue(Type.Double);
      variable.DoubleValue = 1.0;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with double precision value
      var rightNode = new FakeDoubleNode(1.23456789012345);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Calculate expected value
      var expected = operatorType switch
      {
        ElementType.AssignmentAddOperator => 1.0 + 1.23456789012345,
        ElementType.AssignmentSubtractOperator => 1.0 - 1.23456789012345,
        ElementType.AssignmentMultiplyOperator => 1.0 * 1.23456789012345,
        ElementType.AssignmentDivideOperator => 1.0 / 1.23456789012345,
        ElementType.AssignmentModuloOperator => 1.0 % 1.23456789012345,
        _ => throw new ArgumentException("Unsupported operator")
      };

      // Verify precision is preserved
      Assert.Equal(expected, variable.DoubleValue, precision: 14);
    }

    [Theory]
    [InlineData(ElementType.AssignmentAddOperator, "+=")]
    [InlineData(ElementType.AssignmentSubtractOperator, "-=")]
    [InlineData(ElementType.AssignmentMultiplyOperator, "*=")]
    [InlineData(ElementType.AssignmentDivideOperator, "/=")]
    [InlineData(ElementType.AssignmentModuloOperator, "%=")]
    internal void Assignment_CompoundAssignments_PreserveDecimalPrecision(
      ElementType operatorType, string operatorSymbol)
    {
      // Create variable with Decimal type
      var variable = new VariableValue(Type.Decimal);
      variable.DecimalValue = 1.0m;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with decimal precision value
      var rightValue = 1.234567890123456789m;
      var rightNode = new FakeDecimalNode(rightValue);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Calculate expected value
      var expected = operatorType switch
      {
        ElementType.AssignmentAddOperator => 1.0m + rightValue,
        ElementType.AssignmentSubtractOperator => 1.0m - rightValue,
        ElementType.AssignmentMultiplyOperator => 1.0m * rightValue,
        ElementType.AssignmentDivideOperator => 1.0m / rightValue,
        ElementType.AssignmentModuloOperator => 1.0m % rightValue,
        _ => throw new ArgumentException("Unsupported operator")
      };

      // Verify precision is preserved
      Assert.Equal(expected, variable.DecimalValue);
    }

    [Fact]
    internal void Assignment_PreservesLongPrecision_WhenAssigningToLongVariable()
    {
      // Create variable with Long type
      var variable = new VariableValue(Type.Long);
      variable.LongValue = 0L;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with long value
      var largeValue = 987654321012345678L;
      var rightNode = new FakeLongNode(largeValue);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null), Compiler.Options.None,
        null);
      node.Execute(null);

      // Long variable should preserve full long precision
      Assert.Equal(largeValue, variable.LongValue);
    }

    [Theory]
    [InlineData(ElementType.AssignmentAddOperator, "+=")]
    [InlineData(ElementType.AssignmentSubtractOperator, "-=")]
    [InlineData(ElementType.AssignmentMultiplyOperator, "*=")]
    [InlineData(ElementType.AssignmentDivideOperator, "/=")]
    [InlineData(ElementType.AssignmentModuloOperator, "%=")]
    internal void Assignment_CompoundAssignments_PreserveLongPrecision(
      ElementType operatorType, string operatorSymbol)
    {
      // Create variable with Long type
      var variable = new VariableValue(Type.Long);
      variable.LongValue = 1000000000000L;
      var leftNode = new FakeVariableNode(variable);

      // Create right node with long value
      var rightValue = 123456789L;
      var rightNode = new FakeLongNode(rightValue);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null), Compiler.Options.None,
        null);
      node.Execute(null);

      // Calculate expected value
      var expected = operatorType switch
      {
        ElementType.AssignmentAddOperator => 1000000000000L + rightValue,
        ElementType.AssignmentSubtractOperator => 1000000000000L - rightValue,
        ElementType.AssignmentMultiplyOperator => 1000000000000L * rightValue,
        ElementType.AssignmentDivideOperator => 1000000000000L / rightValue,
        ElementType.AssignmentModuloOperator => 1000000000000L % rightValue,
        _ => throw new ArgumentException("Unsupported operator")
      };

      // Verify precision is preserved
      Assert.Equal(expected, variable.LongValue);
    }

    [Fact]
    internal void Assignment_CompoundAssignment_ConvertsDoubleToFloat()
    {
      // Create Float variable
      var variable = new VariableValue(Type.Float);
      variable.FloatValue = 1.0f;
      var leftNode = new FakeVariableNode(variable);

      // Create Double node
      var rightNode = new FakeDoubleNode(1.23456789012345);

      var node = CreateAssignmentNode(ElementType.AssignmentAddOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Double value should be converted to float for compound assignment
      var expected = 1.0f + 1.2345679f;
      Assert.Equal(expected, variable.FloatValue, precision: 6);
    }

    [Fact]
    internal void Assignment_ConvertsLongToInteger_WithPotentialOverflow()
    {
      // Create Integer variable
      var variable = new VariableValue(Type.Integer);
      variable.IntegerValue = 0;
      var leftNode = new FakeVariableNode(variable);

      // Create Long node with value that fits in int
      var rightNode = new FakeLongNode(2147483647L); // int.MaxValue

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(CreateToken("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      // Long value should be cast to int (truncated but no overflow here)
      Assert.Equal(int.MaxValue, variable.IntegerValue);
    }
  }
}