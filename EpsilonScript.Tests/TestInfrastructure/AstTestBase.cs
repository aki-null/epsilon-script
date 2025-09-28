using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class AstTestBase
  {
    protected static Stack<Node> CreateStack(params Node[] nodes)
    {
      var stack = new Stack<Node>();
      foreach (var node in nodes)
      {
        stack.Push(node);
      }

      return stack;
    }

    protected static Element CreateElement(string text, TokenType tokenType, ElementType elementType)
    {
      return ElementFactory.Create(text, tokenType, elementType);
    }
  }
}
