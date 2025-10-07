using System.Collections.Generic;
using EpsilonScript.AST;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class AstTestBase
  {
    internal static Stack<Node> CreateStack(params Node[] nodes)
    {
      var stack = new Stack<Node>();
      foreach (var node in nodes)
      {
        stack.Push(node);
      }

      return stack;
    }
  }
}