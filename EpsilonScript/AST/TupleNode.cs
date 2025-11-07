using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal sealed class TupleNode : Node
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

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
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

    public override void Validate()
    {
      if (TupleValue != null)
      {
        foreach (var child in TupleValue)
        {
          child?.Validate();
        }
      }
    }

    public override void ConfigureNoAlloc()
    {
      if (TupleValue != null)
      {
        foreach (var child in TupleValue)
        {
          child?.ConfigureNoAlloc();
        }
      }
    }
  }
}