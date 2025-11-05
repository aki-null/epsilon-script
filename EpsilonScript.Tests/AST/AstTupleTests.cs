using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  public class AstTupleTests : AstTestBase
  {
    [Fact]
    internal void Tuple_WithTwoSimpleNodes_CreatesCorrectTuple()
    {
      var node = new TupleNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeIntegerNode(10));
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.NotNull(node.TupleValue);
      Assert.Equal(2, node.TupleValue.Count);
      Assert.Equal(ExtendedType.Integer, node.TupleValue[0].ValueType);
      Assert.Equal(ExtendedType.Integer, node.TupleValue[1].ValueType);
    }

    [Fact]
    internal void Tuple_WithMixedTypes_CreatesCorrectTuple()
    {
      var node = new TupleNode();
      var rpn = CreateStack(new FakeIntegerNode(42), new FakeFloatNode(3.14f));
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.NotNull(node.TupleValue);
      Assert.Equal(2, node.TupleValue.Count);
      Assert.Equal(42, node.TupleValue[0].IntegerValue);
      Assert.Equal(3.14f, node.TupleValue[1].FloatValue, 6);
    }

    [Fact]
    internal void Tuple_WithNestedTuple_UnfoldsTupleCorrectly()
    {
      // Create first tuple (1, 2)
      var innerTuple = new TupleNode();
      var innerRpn = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var innerElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      innerTuple.Build(innerRpn, innerElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      // Create outer tuple ((1, 2), 3) which should unfold to (1, 2, 3)
      var outerTuple = new TupleNode();
      var outerRpn = CreateStack(innerTuple, new FakeIntegerNode(3));
      var outerElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      outerTuple.Build(outerRpn, outerElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      outerTuple.Execute(null);

      Assert.NotNull(outerTuple.TupleValue);
      Assert.Equal(3, outerTuple.TupleValue.Count); // Should be unfolded
      Assert.Equal(1, outerTuple.TupleValue[0].IntegerValue);
      Assert.Equal(2, outerTuple.TupleValue[1].IntegerValue);
      Assert.Equal(3, outerTuple.TupleValue[2].IntegerValue);
    }

    [Fact]
    internal void Tuple_WithSimpleAndTuple_UnfoldsCorrectly()
    {
      // Create tuple (2, 3)
      var rightTuple = new TupleNode();
      var rightRpn = CreateStack(new FakeIntegerNode(2), new FakeIntegerNode(3));
      var rightElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      rightTuple.Build(rightRpn, rightElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      // Create outer tuple (1, (2, 3)) - right side is not unfolded
      var outerTuple = new TupleNode();
      var outerRpn = CreateStack(new FakeIntegerNode(1), rightTuple);
      var outerElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      outerTuple.Build(outerRpn, outerElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      outerTuple.Execute(null);

      Assert.NotNull(outerTuple.TupleValue);
      Assert.Equal(2, outerTuple.TupleValue.Count); // Right side is not unfolded
      Assert.Equal(1, outerTuple.TupleValue[0].IntegerValue);
      Assert.NotNull(outerTuple.TupleValue[1].TupleValue);
    }

    [Fact]
    internal void Tuple_Execute_ExecutesAllChildren()
    {
      var leftNode = new TrackingIntegerNode(5);
      var rightNode = new TrackingIntegerNode(10);

      var node = new TupleNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.True(leftNode.WasExecuted);
      Assert.True(rightNode.WasExecuted);
    }

    [Fact]
    internal void Tuple_WithMissingNodes_ThrowsParserException()
    {
      var node = new TupleNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Only one node
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
          Compiler.FloatPrecision.Float));

      Assert.Contains("Cannot find values to create parameter list", exception.Message);
    }

    [Fact]
    internal void Tuple_IsPrecomputable_WithAllConstantChildren_ReturnsTrue()
    {
      var node = new TupleNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeIntegerNode(10)); // Both constant
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.True(node.IsPrecomputable); // All children are constant
    }

    [Fact]
    internal void Tuple_IsPrecomputable_WithOneVariableChild_ReturnsFalse()
    {
      var node = new TupleNode();
      var rpn = CreateStack(new TestVariableNode(), new FakeIntegerNode(10)); // Left is variable
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.False(node.IsPrecomputable); // One child is not constant
    }

    [Fact]
    internal void Tuple_Optimize_OptimizesAllChildren()
    {
      var leftNode = new TestOptimizableNode(true);
      var rightNode = new TestOptimizableNode(false);

      var node = new TupleNode();
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      var optimizedNode = node.Optimize();

      Assert.Same(node, optimizedNode); // TupleNode doesn't return a value node when optimized
      Assert.True(leftNode.WasOptimized);
      Assert.True(rightNode.WasOptimized);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    internal void Tuple_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new TupleNode();
      var rpn = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, options, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.NotNull(node.TupleValue);
      Assert.Equal(2, node.TupleValue.Count);
    }

    [Fact]
    internal void Tuple_WithThreeElements_CreatesCorrectTuple()
    {
      // Create (1, 2)
      var firstTuple = new TupleNode();
      var firstRpn = CreateStack(new FakeIntegerNode(1), new FakeIntegerNode(2));
      var firstElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      firstTuple.Build(firstRpn, firstElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      // Create ((1, 2), 3) which unfolds to (1, 2, 3)
      var finalTuple = new TupleNode();
      var finalRpn = CreateStack(firstTuple, new FakeIntegerNode(3));
      var finalElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      finalTuple.Build(finalRpn, finalElement, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      finalTuple.Execute(null);

      Assert.NotNull(finalTuple.TupleValue);
      Assert.Equal(3, finalTuple.TupleValue.Count);
      Assert.Equal(1, finalTuple.TupleValue[0].IntegerValue);
      Assert.Equal(2, finalTuple.TupleValue[1].IntegerValue);
      Assert.Equal(3, finalTuple.TupleValue[2].IntegerValue);
    }

    [Theory]
    [InlineData(ExtendedType.Integer)]
    [InlineData(ExtendedType.Float)]
    [InlineData(ExtendedType.Boolean)]
    internal void Tuple_WithDifferentTypes_HandlesCorrectly(ExtendedType nodeType)
    {
      Node leftNode = nodeType switch
      {
        ExtendedType.Integer => new FakeIntegerNode(1),
        ExtendedType.Float => new FakeFloatNode(1.0f),
        ExtendedType.Boolean => new FakeBooleanNode(true),
        _ => throw new ArgumentOutOfRangeException(nameof(nodeType))
      };

      var node = new TupleNode();
      var rpn = CreateStack(leftNode, new FakeIntegerNode(2));
      var element = new Element(new Token(",", TokenType.Comma), ElementType.Comma);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.NotNull(node.TupleValue);
      Assert.Equal(2, node.TupleValue.Count);
      Assert.Equal(nodeType, node.TupleValue[0].ValueType);
      Assert.Equal(ExtendedType.Integer, node.TupleValue[1].ValueType);
    }

    // Helper classes for testing
    private class TrackingIntegerNode : Node
    {
      public bool WasExecuted { get; private set; }
      private readonly int _value;

      public TrackingIntegerNode(int value)
      {
        _value = value;
        IntegerValue = value;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
        Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
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
      public override bool IsPrecomputable => false; // Not constant

      public TestVariableNode()
      {
        IntegerValue = 5;
        FloatValue = 5.0f;
        BooleanValue = true;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
        Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
      {
        throw new NotImplementedException("Test node should not be built from RPN");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        // No-op for testing
      }
    }

    private class TestOptimizableNode : Node
    {
      private readonly bool _isPrecomputable;
      public bool WasOptimized { get; private set; }

      public override bool IsPrecomputable => _isPrecomputable;

      public TestOptimizableNode(bool isConstant)
      {
        _isPrecomputable = isConstant;
        IntegerValue = 1;
        FloatValue = 1.0f;
        BooleanValue = true;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
        Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
      {
        throw new NotImplementedException("Test node should not be built from RPN");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        // No-op for testing
      }

      public override Node Optimize()
      {
        WasOptimized = true;
        return this;
      }
    }
  }
}