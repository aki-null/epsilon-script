using System;
using System.Collections.Generic;
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
    public bool IsNumeric => ValueType == ValueType.Integer || ValueType == ValueType.Float;
    public virtual bool IsConstant => true;

    public abstract void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions);

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
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType,
            "Unsupported value type");
      }
    }
  }
}