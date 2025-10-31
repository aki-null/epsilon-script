using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class TupleNode : Node
  {
    public override bool IsPrecomputable
    {
      get
      {
        foreach (var child in TupleValue)
        {
          if (!child.IsPrecomputable)
          {
            return false;
          }
        }

        return true;
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      ValueType = ExtendedType.Tuple;
      TupleValue = new List<Node>();

      if (!rpnStack.TryPop(out var rightNode) || !rpnStack.TryPop(out var leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to create parameter list");
      }

      if (leftNode.ValueType == ExtendedType.Tuple)
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

    public override void Execute(IVariableContainer variablesOverride)
    {
      foreach (var child in TupleValue)
      {
        child.Execute(variablesOverride);
      }
    }

    public override Node Optimize()
    {
      for (var i = 0; i < TupleValue.Count; ++i)
      {
        TupleValue[i] = TupleValue[i].Optimize();
      }

      return this;
    }
  }
}