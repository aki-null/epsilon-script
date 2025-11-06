using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Literal;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstBooleanOperationOptimizationTests : AstTestBase
  {
    [Fact]
    public void BooleanAnd_ConstantFalseLeft_OptimizesToFalse()
    {
      // false && expression => false
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new FakeBooleanNode(false); // constant false
      var rightNode = new NonConstantBooleanNode(true); // non-constant expression
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to a constant false BooleanNode
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.False(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanAnd_ConstantTrueLeft_OptimizesToRightExpression()
    {
      // true && expression => expression
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new FakeBooleanNode(true); // constant true
      var rightNode = new NonConstantBooleanNode(false); // non-constant expression
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to the right node itself
      Assert.Same(rightNode, optimized);
    }

    [Fact]
    public void BooleanOr_ConstantTrueLeft_OptimizesToTrue()
    {
      // true || expression => true
      var node = CreateBooleanOperationNode("||");
      var leftNode = new FakeBooleanNode(true); // constant true
      var rightNode = new NonConstantBooleanNode(false); // non-constant expression
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to a constant true BooleanNode
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.True(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanOr_ConstantFalseLeft_OptimizesToRightExpression()
    {
      // false || expression => expression
      var node = CreateBooleanOperationNode("||");
      var leftNode = new FakeBooleanNode(false); // constant false
      var rightNode = new NonConstantBooleanNode(true); // non-constant expression
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to the right node itself
      Assert.Same(rightNode, optimized);
    }

    [Fact]
    public void BooleanAnd_ConstantFalseRight_OptimizesToFalse()
    {
      // expression && false => false (EpsilonScript functions have no side effects)
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new NonConstantBooleanNode(true); // non-constant expression
      var rightNode = new FakeBooleanNode(false); // constant false
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to false since EpsilonScript functions have no side effects
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.False(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanAnd_ConstantTrueRight_OptimizesToLeftExpression()
    {
      // expression && true => expression
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new NonConstantBooleanNode(false); // non-constant expression
      var rightNode = new FakeBooleanNode(true); // constant true
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to the left node itself
      Assert.Same(leftNode, optimized);
    }

    [Fact]
    public void BooleanOr_ConstantTrueRight_OptimizesToTrue()
    {
      // expression || true => true (EpsilonScript functions have no side effects)
      var node = CreateBooleanOperationNode("||");
      var leftNode = new NonConstantBooleanNode(false); // non-constant expression
      var rightNode = new FakeBooleanNode(true); // constant true
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to true since EpsilonScript functions have no side effects
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.True(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanOr_ConstantFalseRight_OptimizesToLeftExpression()
    {
      // expression || false => expression
      var node = CreateBooleanOperationNode("||");
      var leftNode = new NonConstantBooleanNode(true); // non-constant expression
      var rightNode = new FakeBooleanNode(false); // constant false
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to the left node itself
      Assert.Same(leftNode, optimized);
    }

    [Fact]
    public void BooleanAnd_BothConstant_OptimizesToConstantValue()
    {
      // true && false => false
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to a constant boolean value
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.False(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanOr_BothConstant_OptimizesToConstantValue()
    {
      // false || true => true
      var node = CreateBooleanOperationNode("||");
      var leftNode = new FakeBooleanNode(false);
      var rightNode = new FakeBooleanNode(true);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to a constant boolean value
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.True(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanAnd_NeitherConstant_NoOptimization()
    {
      // expression1 && expression2 => no optimization
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new NonConstantBooleanNode(true);
      var rightNode = new NonConstantBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should not be optimized, returns the same node
      Assert.Same(node, optimized);
    }

    [Fact]
    public void BooleanOr_NeitherConstant_NoOptimization()
    {
      // expression1 || expression2 => no optimization
      var node = CreateBooleanOperationNode("||");
      var leftNode = new NonConstantBooleanNode(true);
      var rightNode = new NonConstantBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should not be optimized, returns the same node
      Assert.Same(node, optimized);
    }

    [Fact]
    public void BooleanAnd_ConstantAndFalse_OptimizesToFalse()
    {
      // constant && false => false (when left is constant, no side effects)
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new FakeBooleanNode(true); // constant true
      var rightNode = new FakeBooleanNode(false); // constant false
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to constant false since both are constant
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.False(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanOr_ConstantOrTrue_OptimizesToTrue()
    {
      // constant || true => true (when left is constant, no side effects)
      var node = CreateBooleanOperationNode("||");
      var leftNode = new FakeBooleanNode(false); // constant false
      var rightNode = new FakeBooleanNode(true); // constant true
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should be optimized to constant true since both are constant
      Assert.IsType<BooleanNode>(optimized);
      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.True(optimized.BooleanValue);
    }

    [Fact]
    public void BooleanOperationNode_UnexecutedConstantComparison_HandlesUninitializedBooleanValue()
    {
      // Test to ensure BooleanOperationNode properly executes constant nodes before accessing BooleanValue
      // This prevents bugs where uninitialized BooleanValue (default: false) is used incorrectly

      // Case 1: Constant comparison that evaluates to true
      // Bug: (5 == 5) has uninitialized BooleanValue = false, so (5 == 5) && true becomes false && true = false
      // Fix: Execute (5 == 5) first to get correct BooleanValue = true, so true && true = true
      var node1 = CreateBooleanOperationNode("&&");
      var leftNode1 = new FakeComparisonNode(5, 5, true); // (5 == 5) = true, but BooleanValue uninitialized
      var rightNode1 = new FakeBooleanNode(true);
      var rpn1 = CreateStack(leftNode1, rightNode1);
      var element1 = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      var context1 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      node1.Build(rpn1, element1, context1, Compiler.Options.None, null);
      var optimized1 = node1.Optimize();

      Assert.True(optimized1.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized1.ValueType);
      Assert.True(optimized1.BooleanValue); // Should be true (5 == 5) && true = true

      // Case 2: Different pattern - true comparison || false
      var node2 = CreateBooleanOperationNode("||");
      var leftNode2 = new FakeComparisonNode(10, 5, true); // (10 > 5) = true, but BooleanValue uninitialized
      var rightNode2 = new FakeBooleanNode(false);
      var rpn2 = CreateStack(leftNode2, rightNode2);
      var element2 = new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator);

      var context2 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      node2.Build(rpn2, element2, context2, Compiler.Options.None, null);
      var optimized2 = node2.Optimize();

      Assert.True(optimized2.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized2.ValueType);
      Assert.True(optimized2.BooleanValue); // Should be true: (10 > 5) || false = true
    }

    [Fact]
    public void BooleanOperationNode_ComplexUnexecutedComparison_HandlesUninitializedBooleanValue()
    {
      // More complex case: nested arithmetic in comparison
      // ((1 + 1) == 2) && true should be true
      var node = CreateBooleanOperationNode("&&");
      var leftNode = new FakeComparisonNode(2, 2, true); // Simulates ((1+1) == 2) = true
      var rightNode = new FakeBooleanNode(true);
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      Assert.True(optimized.IsPrecomputable);
      Assert.Equal(ExtendedType.Boolean, optimized.ValueType);
      Assert.True(optimized.BooleanValue); // Should be true: true && true = true
    }

    [Fact]
    public void BooleanOperationNode_NodeReturnedByChildOptimizeWithoutExecution_IsExecuted()
    {
      // Tests that BooleanOperationNode correctly handles constant nodes returned from child Optimize()
      // that haven't been executed yet. If a child's Optimize() returns an unexecuted constant,
      // BooleanOperationNode must execute it before reading its value to apply optimizations.
      //
      // Without execution: uninitialized BooleanValue (false) leads to incorrect optimization
      // With execution: correct BooleanValue allows proper simplification (true && expr => expr)

      // Create a fake node that returns an unexecuted comparison (value=true) from Optimize()
      var leftNode = new FakeNodeReturningUnexecutedNode(true);
      var rightNode = new NonConstantBooleanNode(false); // Non-constant on right

      var node = CreateBooleanOperationNode("&&");
      var rpn = CreateStack(leftNode, rightNode);
      var element = new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      var optimized = node.Optimize();

      // Should simplify to just the right node since left is constant true
      Assert.Same(rightNode, optimized);
    }

    // Simulates a node that returns an unexecuted constant from Optimize()
    private class FakeNodeReturningUnexecutedNode : Node
    {
      private readonly bool _value;

      public FakeNodeReturningUnexecutedNode(bool value)
      {
        _value = value;
        BooleanValue = true; // Set a boolean value to ensure type is Boolean
      }

      public override bool IsPrecomputable => false; // Not constant initially

      public override Node Optimize()
      {
        // Simulate SignOperator behavior: returns a child without executing it
        // This comparison is constant but hasn't been executed
        return new FakeComparisonNode(5, 5, _value);
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
        Compiler.Options options, IVariableContainer variables)
      {
        throw new NotImplementedException();
      }
    }

    private class FakeComparisonNode : Node
    {
      private readonly int _left, _right;
      private readonly bool _expectedResult;

      public FakeComparisonNode(int left, int right, bool expectedResult)
      {
        _left = left;
        _right = right;
        _expectedResult = expectedResult;
        // BooleanValue is intentionally NOT set here to simulate uninitialized state
        // However we need to set the type, so use a dummy value
        BooleanValue = false;
      }

      public override bool IsPrecomputable => true;

      public override void Execute(IVariableContainer variablesOverride)
      {
        // Simulate comparison execution
        BooleanValue = _expectedResult;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
        Compiler.Options options, IVariableContainer variables)
      {
        throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
      }
    }

    // Helper class to simulate a non-constant boolean node (like a variable or function call)
    private class NonConstantBooleanNode : Node
    {
      public NonConstantBooleanNode(bool value)
      {
        BooleanValue = value;
        IntegerValue = value ? 1 : 0;
        FloatValue = IntegerValue;
      }

      public override bool IsPrecomputable => false; // This makes it non-constant

      protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
        Compiler.Options options, IVariableContainer variables)
      {
        throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
      }
    }
  }
}