using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class VariableNode : Node
  {
    private string _variableName;
    private IDictionary<string, VariableValue> _variables;

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      _variableName = element.Token.Text.ToString();
      _variables = variables;
    }

    public override void Execute(IDictionary<string, VariableValue> variablesOverride)
    {
      if (variablesOverride == null || !variablesOverride.TryGetValue(_variableName, out var variable))
      {
        if (_variables == null || !_variables.TryGetValue(_variableName, out variable))
        {
          throw new RuntimeException($"Undefined variable: {_variableName}");
        }
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