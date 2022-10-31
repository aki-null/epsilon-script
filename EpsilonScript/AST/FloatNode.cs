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
      IntegerValue = (int)FloatValue;
      BooleanValue = IntegerValue != 0;
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

namespace EpsilonScript.AST
{
  public class StringNode : Node
  {
    private void Initialize(string value)
    {
      ValueType = ValueType.String;
      FloatValue = float.TryParse(value, out var f) ? f : 0f;
      IntegerValue = int.TryParse(value, out var i) ? i : 0;
      BooleanValue = IntegerValue != 0;
      StringValue = value;
    }

    public StringNode()
    {
    }

    public StringNode(string value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      var span = element.Token.Text.Span;
      Initialize(span.Slice(1, span.Length - 2).ToString());
    }
  }
}