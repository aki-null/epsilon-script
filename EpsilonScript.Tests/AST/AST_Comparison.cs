using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  public class AST_Comparison : AstTestBase
  {
    [Fact]
    public void AST_ComparisonOptimization_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(10);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      node = node.Optimize();
      Assert.True(typeof(BooleanNode) == node.GetType());
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonStringEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeStringNode("Hello World");
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonStringNotEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeStringNode("こんにちは世界");
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonStringLeft_Fails()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeStringNode("Hello World");
      var rightNode = new FakeIntegerNode(0);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      Assert.Throws<RuntimeException>(() => { node.Execute(null); });
    }

    [Fact]
    public void AST_ComparisonStringRight_Fails()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(0);
      var rightNode = new FakeStringNode("Hello World");
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      Assert.Throws<RuntimeException>(() => { node.Execute(null); });
    }

    [Fact]
    public void AST_ComparisonNotEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(5);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonLessThan_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(5);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonLessThanOrEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(10);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn,
        new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo), ElementType.ComparisonLessThanOrEqualTo),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonGreaterThan_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(15);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonGreaterThanOrEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(10);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn,
        new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
          ElementType.ComparisonGreaterThanOrEqualTo),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonFloatInteger_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeFloatNode(10.5f);
      var rightNode = new FakeIntegerNode(10);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonBooleanEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(true);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_ComparisonBooleanNotEqual_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
        Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }

    [Theory]
    [InlineData("<")]
    [InlineData("<=")]
    [InlineData(">")]
    [InlineData(">=")]
    public void AST_ComparisonBooleanOrderingOperators_ThrowsRuntimeException(string operatorSymbol)
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeBooleanNode(true);
      var rightNode = new FakeBooleanNode(false);
      var rpn = CreateStack(leftNode, rightNode);

      var tokenType = operatorSymbol switch
      {
        "<" => TokenType.ComparisonLessThan,
        "<=" => TokenType.ComparisonLessThanOrEqualTo,
        ">" => TokenType.ComparisonGreaterThan,
        ">=" => TokenType.ComparisonGreaterThanOrEqualTo,
        _ => throw new System.ArgumentException($"Unsupported operator: {operatorSymbol}")
      };

      var elementType = operatorSymbol switch
      {
        "<" => ElementType.ComparisonLessThan,
        "<=" => ElementType.ComparisonLessThanOrEqualTo,
        ">" => ElementType.ComparisonGreaterThan,
        ">=" => ElementType.ComparisonGreaterThanOrEqualTo,
        _ => throw new System.ArgumentException($"Unsupported operator: {operatorSymbol}")
      };

      node.Build(rpn, new Element(new Token(operatorSymbol, tokenType), elementType),
        Compiler.Options.None, null, null);
      Assert.Throws<RuntimeException>(() => { node.Execute(null); });
    }

    [Fact]
    public void AST_ComparisonConstantOptimization_AllOperators_Succeeds()
    {
      var testCases = new[]
      {
        ("==", TokenType.ComparisonEqual, ElementType.ComparisonEqual, 5, 5, true),
        ("!=", TokenType.ComparisonNotEqual, ElementType.ComparisonNotEqual, 5, 3, true),
        ("<", TokenType.ComparisonLessThan, ElementType.ComparisonLessThan, 3, 5, true),
        ("<=", TokenType.ComparisonLessThanOrEqualTo, ElementType.ComparisonLessThanOrEqualTo, 5, 5, true),
        (">", TokenType.ComparisonGreaterThan, ElementType.ComparisonGreaterThan, 8, 5, true),
        (">=", TokenType.ComparisonGreaterThanOrEqualTo, ElementType.ComparisonGreaterThanOrEqualTo, 5, 5, true)
      };

      foreach (var (op, tokenType, elementType, left, right, expected) in testCases)
      {
        Node node = new ComparisonNode();
        var leftNode = new FakeIntegerNode(left);
        var rightNode = new FakeIntegerNode(right);
        var rpn = CreateStack(leftNode, rightNode);
        node.Build(rpn, new Element(new Token(op, tokenType), elementType), Compiler.Options.None, null, null);
        node = node.Optimize();
        Assert.True(typeof(BooleanNode) == node.GetType());
        Assert.Equal(ValueType.Boolean, node.ValueType);
        Assert.Equal(expected, node.BooleanValue);
      }
    }
  }
}