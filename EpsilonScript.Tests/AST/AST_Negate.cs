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
  public class AST_Negate : AstTestBase
  {
    [Theory]
    [InlineData(true, false, 0, 0.0f)]
    [InlineData(false, true, 1, 1.0f)]
    public void AST_Negate_WithBooleanValue_ReturnsNegatedValue(bool inputValue, bool expectedBool, int expectedInt, float expectedFloat)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(inputValue));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.Equal(expectedBool, node.BooleanValue);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.Equal(expectedFloat, node.FloatValue, 6);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void AST_Negate_WithIntegerValue_ThrowsRuntimeException(int value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeIntegerNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Theory]
    [InlineData(3.14f)]
    [InlineData(0.0f)]
    [InlineData(-2.5f)]
    public void AST_Negate_WithFloatValue_ThrowsRuntimeException(float value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeFloatNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("true")]
    [InlineData("false")]
    public void AST_Negate_WithStringValue_ThrowsRuntimeException(string value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeStringNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Fact]
    public void AST_Negate_WithoutChild_ThrowsParserException()
    {
      var node = new NegateNode();
      var rpn = CreateStack(); // Empty stack
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      ErrorTestHelper.BuildNodeExpectingError<ParserException>(node, rpn, element, Compiler.Options.None, null, null,
        "Cannot find value to perform negate operation on");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AST_Negate_IsConstant_ReflectsChildConstantness(bool childValue)
    {
      var node = new NegateNode();
      var childNode = new FakeBooleanNode(childValue); // FakeNode is constant
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.True(node.IsConstant); // Child is constant, so negate should be constant
    }

    [Fact]
    public void AST_Negate_IsConstant_WithVariableChild_ReturnsFalse()
    {
      var node = new NegateNode();
      var childNode = new TestVariableNode(); // Variable node is not constant
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.False(node.IsConstant); // Child is not constant, so negate should not be constant
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AST_Negate_Optimize_WithConstantChild_ReturnsValueNode(bool inputValue, bool expectedValue)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(inputValue));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      var optimizedNode = node.Optimize();

      // Should return a value node since it's constant
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(ValueType.Boolean, optimizedNode.ValueType);
      Assert.Equal(expectedValue, optimizedNode.BooleanValue);
    }

    [Fact]
    public void AST_Negate_Optimize_WithNonConstantChild_ReturnsSelf()
    {
      var node = new NegateNode();
      var childNode = new TestVariableNode();
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      var optimizedNode = node.Optimize();

      // Should return the same node since child is not constant
      Assert.Same(node, optimizedNode);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    public void AST_Negate_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(true));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element, options, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
    }

    [Fact]
    public void AST_Negate_DoubleNegation_ReturnsOriginalValue()
    {
      // Create !!true
      var innerNegate = new NegateNode();
      var innerRpn = CreateStack(new FakeBooleanNode(true));
      var innerElement = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);
      innerNegate.Build(innerRpn, innerElement, Compiler.Options.None, null, null);

      var outerNegate = new NegateNode();
      var outerRpn = CreateStack(innerNegate);
      var outerElement = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);
      outerNegate.Build(outerRpn, outerElement, Compiler.Options.None, null, null);

      outerNegate.Execute(null);

      Assert.Equal(ValueType.Boolean, outerNegate.ValueType);
      Assert.True(outerNegate.BooleanValue); // !!true should be true
    }

    // Helper class for testing non-constant nodes
    private class TestVariableNode : Node
    {
      public override bool IsConstant => false; // Not constant

      public TestVariableNode()
      {
        ValueType = ValueType.Boolean;
        BooleanValue = true;
        IntegerValue = 1;
        FloatValue = 1.0f;
      }

      public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
      {
        throw new NotImplementedException("Test node should not be built from RPN");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        // No-op for testing
      }
    }
  }
}