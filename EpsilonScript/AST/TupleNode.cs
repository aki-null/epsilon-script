using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST
{
  internal sealed class TupleNode : Node
  {
    private List<Node> _tupleValue;

    public override List<Node> TupleValue => _tupleValue;

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

    internal override void Reset()
    {
      base.Reset();
      // Keep the list for the next build, without holding on to this tree's nodes
      _tupleValue?.Clear();
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Tuple;
      if (_tupleValue == null)
      {
        _tupleValue = new List<Node>();
      }
      else
      {
        _tupleValue.Clear();
      }

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

    internal override int Lower(RegisterCompiler compiler) => compiler.LowerTuple(TupleValue);

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
  }
}