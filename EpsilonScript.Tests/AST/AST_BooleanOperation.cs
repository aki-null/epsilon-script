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
  public class AST_BooleanOperation : AstTestBase
  {
    [Theory]
    [MemberData(nameof(BooleanOperationData))]
    public void AST_BooleanOperation_Succeeds(ElementType operatorType, string operatorSymbol,
      bool leftValue, bool rightValue, bool expectedResult, int expectedInteger, float expectedFloat)
    {
      var node = new BooleanOperationNode();
      var rpn = CreateStack(new FakeBooleanNode(leftValue), new FakeBooleanNode(rightValue));
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.Equal(expectedResult, node.BooleanValue);
      Assert.Equal(expectedInteger, node.IntegerValue);
      Assert.Equal(expectedFloat, node.FloatValue, 6);
    }

    public static IEnumerable<object[]> BooleanOperationData
    {
      get
      {
        return new[]
        {
          // AND operations
          new object[] { ElementType.BooleanAndOperator, "&&", true, true, true, 1, 1.0f },
          new object[] { ElementType.BooleanAndOperator, "&&", true, false, false, 0, 0.0f },
          new object[] { ElementType.BooleanAndOperator, "&&", false, true, false, 0, 0.0f },
          new object[] { ElementType.BooleanAndOperator, "&&", false, false, false, 0, 0.0f },

          // OR operations
          new object[] { ElementType.BooleanOrOperator, "||", true, true, true, 1, 1.0f },
          new object[] { ElementType.BooleanOrOperator, "||", true, false, true, 1, 1.0f },
          new object[] { ElementType.BooleanOrOperator, "||", false, true, true, 1, 1.0f },
          new object[] { ElementType.BooleanOrOperator, "||", false, false, false, 0, 0.0f }
        };
      }
    }

    [Fact]
    public void AST_BooleanOperation_AndShortCircuit_SkipsRightNode()
    {
      var leftNode = new FakeBooleanNode(false);
      var rightNode = new TrackingBooleanNode(true);

      var node = new BooleanOperationNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
      Assert.False(rightNode.WasExecuted); // Right node should not have been executed
    }

    [Fact]
    public void AST_BooleanOperation_OrShortCircuit_SkipsRightNode()
    {
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new TrackingBooleanNode(false);

      var node = new BooleanOperationNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
      Assert.False(rightNode.WasExecuted); // Right node should not have been executed
    }

    [Theory]
    [InlineData(ElementType.BooleanAndOperator, "&&")]
    [InlineData(ElementType.BooleanOrOperator, "||")]
    public void AST_BooleanOperation_NonBooleanLeftOperand_ThrowsRuntimeException(ElementType operatorType,
      string operatorSymbol)
    {
      var leftNode = new FakeIntegerNode(1); // Non-boolean
      var rightNode = new FakeBooleanNode(true);

      var node = new BooleanOperationNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null,
        "Boolean operation can only be performed on boolean values");
    }

    [Theory]
    [InlineData(ElementType.BooleanAndOperator, "&&", true)] // AND with true left needs to evaluate right
    [InlineData(ElementType.BooleanOrOperator, "||", false)] // OR with false left needs to evaluate right
    public void AST_BooleanOperation_NonBooleanRightOperand_ThrowsRuntimeException(ElementType operatorType,
      string operatorSymbol, bool leftValue)
    {
      var leftNode = new FakeBooleanNode(leftValue);
      var rightNode = new FakeIntegerNode(1); // Non-boolean

      var node = new BooleanOperationNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null,
        "Boolean operation can only be performed on boolean values");
    }

    [Theory]
    [InlineData(ElementType.BooleanAndOperator, "&&")]
    [InlineData(ElementType.BooleanOrOperator, "||")]
    public void AST_BooleanOperation_StackUnderflow_ThrowsParserException(ElementType operatorType,
      string operatorSymbol)
    {
      var node = new BooleanOperationNode();
      var rpn = CreateStack(new FakeBooleanNode(true)); // Only one node, need two
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      ErrorTestHelper.BuildNodeExpectingError<ParserException>(node, rpn, element, Compiler.Options.None, null, null,
        "Cannot find values to perform a boolean operation on");
    }

    [Fact]
    public void AST_BooleanOperation_UnsupportedOperator_ThrowsArgumentOutOfRangeException()
    {
      var node = new BooleanOperationNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeBooleanNode(false));
      var element = new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator); // Wrong operator type

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<ArgumentOutOfRangeException>(node);
    }

    [Theory]
    [InlineData(ElementType.BooleanAndOperator, "&&")]
    [InlineData(ElementType.BooleanOrOperator, "||")]
    public void AST_BooleanOperation_IsConstant_ReturnsCorrectValue(ElementType operatorType, string operatorSymbol)
    {
      var node = new BooleanOperationNode();
      var rpn = CreateStack(new FakeBooleanNode(true), new FakeBooleanNode(false));
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.True(node.IsConstant); // Both operands are constant
    }

    private static TokenType GetTokenType(ElementType operatorType)
    {
      return operatorType switch
      {
        ElementType.BooleanAndOperator => TokenType.BooleanAndOperator,
        ElementType.BooleanOrOperator => TokenType.BooleanOrOperator,
        _ => throw new ArgumentOutOfRangeException(nameof(operatorType))
      };
    }

    // Helper class to track whether a node was executed (for testing short-circuit behavior)
    private class TrackingBooleanNode : Node
    {
      private readonly bool _value;
      public bool WasExecuted { get; private set; }

      public TrackingBooleanNode(bool value)
      {
        _value = value;
        ValueType = ValueType.Boolean;
        BooleanValue = value;
        IntegerValue = value ? 1 : 0;
        FloatValue = IntegerValue;
      }

      public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
      {
        throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        WasExecuted = true;
      }
    }
  }
}