using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class VariableNode : Node
  {
    private IDictionary<string, VariableValue> _variables;
    private string _variableName;

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      _variableName = element.Token.Text;
      _variables = variables;
    }

    public override void Execute()
    {
      if (_variables == null || !_variables.TryGetValue(_variableName, out var variable))
      {
        throw new RuntimeException($"Undefined variable found: {_variableName}");
      }

      Variable = variable;
      switch (Variable.Type)
      {
        case Type.Integer:
          ValueType = ValueType.Integer;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.Float:
          ValueType = ValueType.Float;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          break;
        case Type.Boolean:
          ValueType = ValueType.Boolean;
          IntegerValue = Variable.IntegerValue;
          BooleanValue = Variable.BooleanValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(Variable.Type), Variable.Type, "Unsupported variable type");
      }
    }
  }
}