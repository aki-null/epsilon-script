using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class IntegerNode : Node
  {
    public IntegerNode()
    {
    }

    public IntegerNode(int value)
    {
      IntegerValue = value;
    }

    public IntegerNode(long value)
    {
      LongValue = value;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      var value = long.Parse(element.Token.Text.Span);

      if (intPrecision == Compiler.IntegerPrecision.Integer)
      {
        if (value > int.MaxValue || value < int.MinValue)
        {
          throw new System.OverflowException("Value was either too large or too small for an Int32.");
        }

        IntegerValue = (int)value;
      }
      else
      {
        LongValue = value;
      }
    }
  }
}