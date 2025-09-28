using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class FloatNode : Node
  {
    private void Initialize(float value)
    {
      ValueType = ValueType.Float;
      FloatValue = value;

      // Safer float-to-int conversion with overflow handling
      if (float.IsNaN(value) || float.IsInfinity(value))
      {
        IntegerValue = 0;
      }
      else if (value > int.MaxValue)
      {
        IntegerValue = int.MaxValue;
      }
      else if (value < int.MinValue)
      {
        IntegerValue = int.MinValue;
      }
      else
      {
        IntegerValue = (int)value;
      }

      BooleanValue = FloatValue != 0.0f && !float.IsInfinity(FloatValue) && !float.IsNaN(FloatValue);
    }

    public FloatNode()
    {
    }

    public FloatNode(float value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      Initialize(float.Parse(element.Token.Text.Span));
    }
  }
}