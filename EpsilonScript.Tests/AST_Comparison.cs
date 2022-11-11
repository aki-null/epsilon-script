/*
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_Comparison
  {
    [Fact]
    public void AST_ComparisonOptimization_Succeeds()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(10);
      var rightNode = new FakeIntegerNode(10);
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null);
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
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null);
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
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null);
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
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null);
      Assert.Throws<RuntimeException>(() => { node.Execute(null); });
    }

    [Fact]
    public void AST_ComparisonStringRight_Fails()
    {
      Node node = new ComparisonNode();
      var leftNode = new FakeIntegerNode(0);
      var rightNode = new FakeStringNode("Hello World");
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        Compiler.Options.None, null);
      Assert.Throws<RuntimeException>(() => { node.Execute(null); });
    }
  }
}
*/

