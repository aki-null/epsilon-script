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
  public class AST_Sequence : AstTestBase
  {
    [Theory]
    [InlineData(5, 10, ValueType.Integer)]
    [InlineData(-3, 42, ValueType.Integer)]
    public void AST_Sequence_WithTwoIntegers_ReturnsRightValue(int leftValue, int rightValue, ValueType expectedType)
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(leftValue), new FakeIntegerNode(rightValue));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(expectedType, node.ValueType);
      Assert.Equal(rightValue, node.IntegerValue);
      Assert.Equal((float)rightValue, node.FloatValue);
      Assert.Equal(rightValue != 0, node.BooleanValue);
    }

    [Theory]
    [InlineData(1.5f, 3.7f, ValueType.Float)]
    [InlineData(-2.1f, 0.0f, ValueType.Float)]
    public void AST_Sequence_WithTwoFloats_ReturnsRightValue(float leftValue, float rightValue, ValueType expectedType)
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeFloatNode(leftValue), new FakeFloatNode(rightValue));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(expectedType, node.ValueType);
      Assert.Equal(rightValue, node.FloatValue, 6);
      Assert.Equal((int)rightValue, node.IntegerValue);
    }

    [Theory]
    [InlineData(true, false, ValueType.Boolean)]
    [InlineData(false, true, ValueType.Boolean)]
    public void AST_Sequence_WithTwoBooleans_ReturnsRightValue(bool leftValue, bool rightValue, ValueType expectedType)
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeBooleanNode(leftValue), new FakeBooleanNode(rightValue));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(expectedType, node.ValueType);
      Assert.Equal(rightValue, node.BooleanValue);
      Assert.Equal(rightValue ? 1 : 0, node.IntegerValue);
      Assert.Equal(rightValue ? 1.0f : 0.0f, node.FloatValue);
    }

    [Fact]
    public void AST_Sequence_WithMixedTypes_ReturnsRightType()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(42), new FakeFloatNode(3.14f));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(3.14f, node.FloatValue, 6);
    }

    [Fact]
    public void AST_Sequence_ExecutesBothNodes()
    {
      var leftNode = new TrackingIntegerNode(10);
      var rightNode = new TrackingIntegerNode(20);

      var node = new SequenceNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.True(leftNode.WasExecuted);
      Assert.True(rightNode.WasExecuted);
      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(20, node.IntegerValue); // Returns right value
    }

    [Fact]
    public void AST_Sequence_WithMissingNodes_ThrowsParserException()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Only one node
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element, Compiler.Options.None, null, null));

      Assert.Contains("Cannot find tokens to sequence", exception.Message);
    }

    [Fact]
    public void AST_Sequence_IsConstant_WithTwoConstantNodes_ReturnsTrue()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeIntegerNode(10)); // Both constant
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.True(node.IsConstant); // Both nodes are constant
    }

    [Fact]
    public void AST_Sequence_IsConstant_WithOneVariableNode_ReturnsFalse()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new TestVariableNode(), new FakeIntegerNode(10)); // Left is variable
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      Assert.False(node.IsConstant); // Left node is not constant
    }

    [Fact]
    public void AST_Sequence_Optimize_WithConstantNodes_ReturnsValueNode()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeIntegerNode(10));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      var optimizedNode = node.Optimize();

      // Should return a value node since both children are constant
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(ValueType.Integer, optimizedNode.ValueType);
      Assert.Equal(10, optimizedNode.IntegerValue); // Right value
    }

    [Fact]
    public void AST_Sequence_Optimize_WithNonConstantNodes_ReturnsSelf()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new TestVariableNode(), new FakeIntegerNode(10));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);

      var optimizedNode = node.Optimize();

      // Should return the same node since left child is not constant
      Assert.Same(node, optimizedNode);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    public void AST_Sequence_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, options, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(2, node.IntegerValue);
    }

    [Fact]
    public void AST_Sequence_WithTupleValues_PropagatesTupleFromRightNode()
    {
      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      tupleNode.Build(tupleRpn, tupleElement, Compiler.Options.None, null, null);
      tupleNode.Execute(null);

      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(42), tupleNode);
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Tuple, node.ValueType);
      Assert.NotNull(node.TupleValue);
      Assert.Equal(2, node.TupleValue.Count);
    }

    // Helper classes for testing
    private class TrackingIntegerNode : Node
    {
      public bool WasExecuted { get; private set; }
      private readonly int _value;

      public TrackingIntegerNode(int value)
      {
        _value = value;
        ValueType = ValueType.Integer;
        IntegerValue = value;
        FloatValue = value;
        BooleanValue = value != 0;
      }

      public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
      {
        throw new NotImplementedException("Test node should not be built from RPN");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        WasExecuted = true;
      }
    }

    private class TestVariableNode : Node
    {
      public override bool IsConstant => false; // Not constant

      public TestVariableNode()
      {
        ValueType = ValueType.Integer;
        IntegerValue = 5;
        FloatValue = 5.0f;
        BooleanValue = true;
      }

      public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
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