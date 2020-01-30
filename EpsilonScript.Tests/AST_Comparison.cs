using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
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
          Compiler.Options.None, null, null);
      node = node.Optimize();
      Assert.True(typeof(BooleanNode) == node.GetType());
      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
    }
  }
}
