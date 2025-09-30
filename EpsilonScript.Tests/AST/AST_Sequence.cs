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
    public void AST_Sequence_WithSingleNode_ActsAsNoOp()
    {
      var node = new SequenceNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Only one node (trailing semicolon case)
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);

      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(5, node.IntegerValue);
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

    [Fact]
    public void AST_Sequence_WithMultipleSemicolons_ReturnsRightmostValue()
    {
      // Test: 1; 2; 3 should return 3
      var node1 = new SequenceNode();
      var rpn1 = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var element1 = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);
      node1.Build(rpn1, element1, Compiler.Options.None, null, null);

      var node2 = new SequenceNode();
      var rpn2 = CreateStack(node1, new FakeIntegerNode(3));
      var element2 = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);
      node2.Build(rpn2, element2, Compiler.Options.None, null, null);
      node2.Execute(null);

      Assert.Equal(ValueType.Integer, node2.ValueType);
      Assert.Equal(3, node2.IntegerValue);
    }

    [Fact]
    public void AST_Sequence_WithMultipleSemicolons_ExecutesAllNodes()
    {
      // Test: node1; node2; node3 - all should execute
      var node1 = new TrackingIntegerNode(10);
      var node2 = new TrackingIntegerNode(20);
      var node3 = new TrackingIntegerNode(30);

      var seqNode1 = new SequenceNode();
      var rpn1 = CreateStack(node1, node2);
      var element1 = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);
      seqNode1.Build(rpn1, element1, Compiler.Options.None, null, null);

      var seqNode2 = new SequenceNode();
      var rpn2 = CreateStack(seqNode1, node3);
      var element2 = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);
      seqNode2.Build(rpn2, element2, Compiler.Options.None, null, null);
      seqNode2.Execute(null);

      Assert.True(node1.WasExecuted);
      Assert.True(node2.WasExecuted);
      Assert.True(node3.WasExecuted);
      Assert.Equal(30, seqNode2.IntegerValue);
    }

    [Fact]
    public void AST_Sequence_WithConsecutiveSemicolons_HandlesEmptyStatements()
    {
      // Test: value;;;; (multiple trailing semicolons)
      var baseNode = new FakeIntegerNode(42);

      // Build: value;
      var seq1 = new SequenceNode();
      var rpn1 = CreateStack(baseNode);
      var element = new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon);
      seq1.Build(rpn1, element, Compiler.Options.None, null, null);

      // Build: (value;);
      var seq2 = new SequenceNode();
      var rpn2 = CreateStack(seq1);
      seq2.Build(rpn2, element, Compiler.Options.None, null, null);

      // Build: ((value;););
      var seq3 = new SequenceNode();
      var rpn3 = CreateStack(seq2);
      seq3.Build(rpn3, element, Compiler.Options.None, null, null);

      // Build: (((value;);););
      var seq4 = new SequenceNode();
      var rpn4 = CreateStack(seq3);
      seq4.Build(rpn4, element, Compiler.Options.None, null, null);
      seq4.Execute(null);

      Assert.Equal(ValueType.Integer, seq4.ValueType);
      Assert.Equal(42, seq4.IntegerValue);
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