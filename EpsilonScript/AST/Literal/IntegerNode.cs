using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Literal
{
  internal class IntegerNode : Node
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

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      var value = long.Parse(element.Token.Text.Span);

      if (context.IntegerPrecision == Compiler.IntegerPrecision.Integer)
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