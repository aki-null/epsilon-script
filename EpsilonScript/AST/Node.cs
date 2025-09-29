using System;
using System.Collections.Generic;
using System.Globalization;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public abstract class Node
  {
    public ValueType ValueType { get; protected set; } = ValueType.Undefined;
    public int IntegerValue { get; protected set; }
    public float FloatValue { get; protected set; }
    public bool BooleanValue { get; protected set; }
    public string StringValue { get; protected set; }
    public List<Node> TupleValue { get; protected set; }
    public VariableValue Variable { get; protected set; }
    public bool IsNumeric => ValueType.IsNumber();
    public virtual bool IsConstant => true;

    public abstract void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions);

    public virtual void Execute(IVariableContainer variablesOverride)
    {
    }

    public virtual Node Optimize()
    {
      return this;
    }

    public Node CreateValueNode()
    {
      switch (ValueType)
      {
        case ValueType.Integer:
          return new IntegerNode(IntegerValue);
        case ValueType.Float:
          return new FloatNode(FloatValue);
        case ValueType.Boolean:
          return new BooleanNode(BooleanValue);
        case ValueType.String:
          return new StringNode(StringValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }

    public override string ToString()
    {
      switch (ValueType)
      {
        case ValueType.Undefined:
          return "Undefined";
        case ValueType.Null:
          return "null";
        case ValueType.Integer:
          return IntegerValue.ToString();
        case ValueType.Float:
          return FloatValue.ToString(CultureInfo.InvariantCulture);
        case ValueType.Boolean:
          return BooleanValue ? "true" : "false";
        case ValueType.Tuple:
          return "Tuple";
        case ValueType.String:
          return StringValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }
  }
}