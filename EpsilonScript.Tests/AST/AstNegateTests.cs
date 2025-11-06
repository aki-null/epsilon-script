using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Boolean;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  public class AstNegateTests : AstTestBase
  {
    [Theory]
    [InlineData(true, false, 0, 0.0f)]
    [InlineData(false, true, 1, 1.0f)]
    internal void Negate_WithBooleanValue_ReturnsNegatedValue(bool inputValue, bool expectedBool, int expectedInt,
      float expectedFloat)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(inputValue));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.Equal(expectedBool, node.BooleanValue);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.Equal(expectedFloat, node.FloatValue, 6);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    internal void Negate_WithIntegerValue_ThrowsRuntimeException(int value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeIntegerNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Theory]
    [InlineData(3.14f)]
    [InlineData(0.0f)]
    [InlineData(-2.5f)]
    internal void Negate_WithFloatValue_ThrowsRuntimeException(float value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeFloatNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("true")]
    [InlineData("false")]
    internal void Negate_WithStringValue_ThrowsRuntimeException(string value)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeStringNode(value));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Cannot negate a non-boolean value");
    }

    [Fact]
    internal void Negate_WithoutChild_ThrowsParserException()
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
    internal void Negate_IsPrecomputable_ReflectsChildConstantness(bool childValue)
    {
      var node = new NegateNode();
      var childNode = new FakeBooleanNode(childValue); // FakeNode is constant
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.True(node.IsPrecomputable); // Child is constant, so negate should be constant
    }

    [Fact]
    internal void Negate_IsPrecomputable_WithVariableChild_ReturnsFalse()
    {
      var node = new NegateNode();
      var childNode = new TestVariableNode(); // Variable node is not constant
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.False(node.IsPrecomputable); // Child is not constant, so negate should not be constant
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    internal void Negate_Optimize_WithConstantChild_ReturnsValueNode(bool inputValue, bool expectedValue)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(inputValue));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      var optimizedNode = node.Optimize();

      // Should return a value node since it's constant
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(ExtendedType.Boolean, optimizedNode.ValueType);
      Assert.Equal(expectedValue, optimizedNode.BooleanValue);
    }

    [Fact]
    internal void Negate_Optimize_WithNonConstantChild_ReturnsSelf()
    {
      var node = new NegateNode();
      var childNode = new TestVariableNode();
      var rpn = CreateStack(childNode);
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      var optimizedNode = node.Optimize();

      // Should return the same node since child is not constant
      Assert.Same(node, optimizedNode);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    internal void Negate_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new NegateNode();
      var rpn = CreateStack(new FakeBooleanNode(true));
      var element = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);

      var context = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      node.Build(rpn, element, context, options, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
    }

    [Fact]
    internal void Negate_DoubleNegation_ReturnsOriginalValue()
    {
      // Create !!true
      var innerNegate = new NegateNode();
      var innerRpn = CreateStack(new FakeBooleanNode(true));
      var innerElement = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);
      var innerContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      innerNegate.Build(innerRpn, innerElement, innerContext, Compiler.Options.None, null);

      var outerNegate = new NegateNode();
      var outerRpn = CreateStack(innerNegate);
      var outerElement = new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator);
      var outerContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      outerNegate.Build(outerRpn, outerElement, outerContext, Compiler.Options.None, null);

      outerNegate.Execute(null);

      Assert.Equal(ExtendedType.Boolean, outerNegate.ValueType);
      Assert.True(outerNegate.BooleanValue); // !!true should be true
    }

    // Helper class for testing non-constant nodes
    private class TestVariableNode : Node
    {
      public override bool IsPrecomputable => false; // Not constant

      public TestVariableNode()
      {
        BooleanValue = true;
        IntegerValue = 1;
        FloatValue = 1.0f;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
        Compiler.Options options, IVariableContainer variables)
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