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
  public class AstAssignmentTests : AstTestBase
  {
    [Theory]
    [MemberData(nameof(SimpleAssignmentData))]
    internal void Assignment_SimpleAssignment_Succeeds(ElementType operatorType, string operatorSymbol,
      Type variableType, object initialValue, object assignValue, object expectedValue, ExtendedType expectedValueType)
    {
      var variable = new VariableValue(variableType);
      SetVariableValue(variable, initialValue);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = CreateValueNode(assignValue);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(expectedValueType, node.ValueType);
      AssertVariableValue(variable, expectedValue);
    }

    public static IEnumerable<object[]> SimpleAssignmentData
    {
      get
      {
        return new[]
        {
          // Integer assignments
          new object[] { ElementType.AssignmentOperator, "=", Type.Integer, 10, 42, 42, ExtendedType.Integer },
          new object[] { ElementType.AssignmentOperator, "=", Type.Integer, 5, -15, -15, ExtendedType.Integer },

          // Float assignments
          new object[] { ElementType.AssignmentOperator, "=", Type.Float, 10.5f, 42.7f, 42.7f, ExtendedType.Float },
          new object[] { ElementType.AssignmentOperator, "=", Type.Float, 5.0f, -15.3f, -15.3f, ExtendedType.Float },

          // Boolean assignments
          new object[] { ElementType.AssignmentOperator, "=", Type.Boolean, true, false, false, ExtendedType.Boolean },
          new object[] { ElementType.AssignmentOperator, "=", Type.Boolean, false, true, true, ExtendedType.Boolean }
        };
      }
    }

    [Theory]
    [MemberData(nameof(CompoundAssignmentData))]
    internal void Assignment_CompoundAssignment_Succeeds(ElementType operatorType, string operatorSymbol,
      Type variableType, object initialValue, object operandValue, object expectedValue, ExtendedType expectedValueType)
    {
      var variable = new VariableValue(variableType);
      SetVariableValue(variable, initialValue);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = CreateValueNode(operandValue);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(expectedValueType, node.ValueType);
      AssertVariableValue(variable, expectedValue);
    }

    public static IEnumerable<object[]> CompoundAssignmentData
    {
      get
      {
        return new[]
        {
          // Integer compound assignments
          new object[] { ElementType.AssignmentAddOperator, "+=", Type.Integer, 10, 5, 15, ExtendedType.Integer },
          new object[] { ElementType.AssignmentSubtractOperator, "-=", Type.Integer, 10, 3, 7, ExtendedType.Integer },
          new object[] { ElementType.AssignmentMultiplyOperator, "*=", Type.Integer, 6, 2, 12, ExtendedType.Integer },
          new object[] { ElementType.AssignmentDivideOperator, "/=", Type.Integer, 12, 3, 4, ExtendedType.Integer },
          new object[] { ElementType.AssignmentModuloOperator, "%=", Type.Integer, 10, 3, 1, ExtendedType.Integer },

          // Float compound assignments
          new object[] { ElementType.AssignmentAddOperator, "+=", Type.Float, 10.5f, 5.2f, 15.7f, ExtendedType.Float },
          new object[]
            { ElementType.AssignmentSubtractOperator, "-=", Type.Float, 10.5f, 3.2f, 7.3f, ExtendedType.Float },
          new object[]
            { ElementType.AssignmentMultiplyOperator, "*=", Type.Float, 6.5f, 2.0f, 13.0f, ExtendedType.Float },
          new object[]
            { ElementType.AssignmentDivideOperator, "/=", Type.Float, 12.0f, 3.0f, 4.0f, ExtendedType.Float },
          new object[] { ElementType.AssignmentModuloOperator, "%=", Type.Float, 10.5f, 3.0f, 1.5f, ExtendedType.Float }
        };
      }
    }

    [Fact]
    internal void Assignment_ImmutableMode_ThrowsParserException()
    {
      var variable = new VariableValue(Type.Integer);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = new FakeIntegerNode(42);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      ErrorTestHelper.BuildNodeExpectingError<ParserException>(node, rpn, element, Compiler.Options.Immutable, null,
        null,
        ErrorMessages.AssignmentOperatorCannotBeUsedForImmutableScript);
    }

    [Fact]
    internal void Assignment_NonVariableLeftSide_ThrowsRuntimeException()
    {
      var leftNode = new FakeIntegerNode(10); // Not a variable
      var rightNode = new FakeIntegerNode(42);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, ErrorMessages.LeftHandSideMustBeVariable);
    }

    [Fact]
    internal void Assignment_FloatToBooleanVariable_ThrowsRuntimeException()
    {
      var variable = new VariableValue(Type.Boolean);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = new FakeFloatNode(3.14f);

      var node = CreateAssignmentNode(ElementType.AssignmentOperator);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null,
        ErrorMessages.FloatValueCannotBeAssignedToBooleanVariable);
    }

    [Theory]
    [InlineData(ElementType.AssignmentAddOperator, "+=")]
    [InlineData(ElementType.AssignmentSubtractOperator, "-=")]
    [InlineData(ElementType.AssignmentMultiplyOperator, "*=")]
    [InlineData(ElementType.AssignmentDivideOperator, "/=")]
    internal void Assignment_CompoundAssignmentWithNonNumeric_ThrowsRuntimeException(ElementType operatorType,
      string operatorSymbol)
    {
      var variable = new VariableValue(Type.Integer);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = new FakeStringNode("not a number");

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null,
        ErrorMessages.ArithmeticOperationOnlyOnNumericValue);
    }

    [Theory]
    [InlineData(ElementType.AssignmentAddOperator, "+=")]
    [InlineData(ElementType.AssignmentSubtractOperator, "-=")]
    [InlineData(ElementType.AssignmentMultiplyOperator, "*=")]
    [InlineData(ElementType.AssignmentDivideOperator, "/=")]
    internal void Assignment_CompoundAssignmentWithBooleanVariable_ThrowsArgumentException(ElementType operatorType,
      string operatorSymbol)
    {
      var variable = new VariableValue(Type.Boolean);
      var leftNode = new FakeVariableNode(variable);
      var rightNode = new FakeIntegerNode(5);

      var node = CreateAssignmentNode(operatorSymbol);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<ArgumentOutOfRangeException>(node);
    }

    private static void SetVariableValue(VariableValue variable, object value)
    {
      switch (variable.Type)
      {
        case Type.Integer:
          variable.IntegerValue = (int)value;
          break;
        case Type.Float:
          variable.FloatValue = (float)value;
          break;
        case Type.Boolean:
          variable.BooleanValue = (bool)value;
          break;
        case Type.String:
          variable.StringValue = (string)value;
          break;
      }
    }

    private static void AssertVariableValue(VariableValue variable, object expectedValue)
    {
      switch (variable.Type)
      {
        case Type.Integer:
          Assert.Equal((int)expectedValue, variable.IntegerValue);
          break;
        case Type.Float:
          Assert.Equal((float)expectedValue, variable.FloatValue, 6);
          break;
        case Type.Boolean:
          Assert.Equal((bool)expectedValue, variable.BooleanValue);
          break;
        case Type.String:
          Assert.Equal((string)expectedValue, variable.StringValue);
          break;
      }
    }

    private static Node CreateValueNode(object value)
    {
      return value switch
      {
        int i => new FakeIntegerNode(i),
        float f => new FakeFloatNode(f),
        bool b => new FakeBooleanNode(b),
        string s => new FakeStringNode(s),
        _ => throw new ArgumentOutOfRangeException(nameof(value), "Unsupported value type")
      };
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
  }
}