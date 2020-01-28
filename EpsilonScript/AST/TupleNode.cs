using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class TupleNode : Node
  {
    public override bool IsConstant
    {
      get
      {
        foreach (var child in TupleValue)
        {
          if (!child.IsConstant)
          {
            return false;
          }
        }

        return true;
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Tuple;
      TupleValue = new List<Node>();

      if (!rpnStack.TryPop(out var rightNode) || !rpnStack.TryPop(out var leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to create parameter list");
      }

      if (leftNode.ValueType == ValueType.Tuple)
      {
        // unfold tuple list
        foreach (var parameter in leftNode.TupleValue)
        {
          TupleValue.Add(parameter);
        }
      }
      else
      {
        TupleValue.Add(leftNode);
      }

      TupleValue.Add(rightNode);
    }

    public override void Execute()
    {
      foreach (var child in TupleValue)
      {
        child.Execute();
      }
    }
  }
}