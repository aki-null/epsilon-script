using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class FloatNode : Node
  {
    public FloatNode()
    {
    }

    public FloatNode(float value)
    {
      FloatValue = value;
    }

    public FloatNode(double value)
    {
      DoubleValue = value;
    }

    public FloatNode(decimal value)
    {
      DecimalValue = value;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      switch (floatPrecision)
      {
        case Compiler.FloatPrecision.Float:
          FloatValue = float.Parse(element.Token.Text.Span);
          break;
        case Compiler.FloatPrecision.Double:
          DoubleValue = double.Parse(element.Token.Text.Span);
          break;
        case Compiler.FloatPrecision.Decimal:
          DecimalValue = decimal.Parse(element.Token.Text.Span);
          break;
      }
    }
  }
}