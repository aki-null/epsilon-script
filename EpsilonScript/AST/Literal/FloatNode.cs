using System.Collections.Generic;
using EpsilonScript.Helper;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Literal
{
  internal class FloatNode : Node
  {
    internal override int Lower(RegisterCompiler compiler) => compiler.LowerConstant(this);

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

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      switch (context.FloatPrecision)
      {
        case Compiler.FloatPrecision.Float:
          FloatValue = float.Parse(element.Token.Text.FastSpan());
          break;
        case Compiler.FloatPrecision.Double:
          DoubleValue = double.Parse(element.Token.Text.FastSpan());
          break;
        case Compiler.FloatPrecision.Decimal:
          DecimalValue = decimal.Parse(element.Token.Text.FastSpan());
          break;
      }
    }
  }
}