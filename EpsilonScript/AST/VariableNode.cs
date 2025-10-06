using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class VariableNode : Node
  {
    private VariableId _variableName;
    private IVariableContainer _variables;
    private Type _configuredIntegerType;
    private Type _configuredFloatType;

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      _variableName = element.Token.Text.ToString();
      _variables = variables;

      // Store configured types for auto-conversion
      _configuredIntegerType = intPrecision == Compiler.IntegerPrecision.Integer ? Type.Integer : Type.Long;
      _configuredFloatType = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => Type.Float,
        Compiler.FloatPrecision.Double => Type.Double,
        Compiler.FloatPrecision.Decimal => Type.Decimal,
        _ => Type.Float
      };
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      if (variablesOverride == null || !variablesOverride.TryGetValue(_variableName, out var variable))
      {
        if (_variables == null || !_variables.TryGetValue(_variableName, out variable))
        {
          throw new RuntimeException($"Undefined variable: {_variableName}");
        }
      }

      Variable = variable;

      // Auto-convert variable to match compiler precision configuration
      // Property getters handle conversion from any type
      switch (variable.Type)
      {
        case Type.Integer:
        case Type.Long:
          // Integer types: convert to configured integer precision
          if (_configuredIntegerType == Type.Integer)
          {
            IntegerValue = variable.IntegerValue;
          }
          else
          {
            LongValue = variable.LongValue;
          }

          break;

        case Type.Float:
        case Type.Double:
        case Type.Decimal:
          // Float types: convert to configured float precision
          switch (_configuredFloatType)
          {
            case Type.Float:
              FloatValue = variable.FloatValue;
              break;
            case Type.Double:
              DoubleValue = variable.DoubleValue;
              break;
            case Type.Decimal:
              DecimalValue = variable.DecimalValue;
              break;
          }

          break;

        case Type.Boolean:
          BooleanValue = variable.BooleanValue;
          break;

        case Type.String:
          StringValue = variable.StringValue;
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }
  }
}