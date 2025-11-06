using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class VariableNode : Node
  {
    private VariableId _variableName;
    private IVariableContainer _variables;
    private ExtendedType _configuredIntegerType;
    private ExtendedType _configuredFloatType;

    public override bool IsPrecomputable => false;

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      _variableName = element.Token.Text.ToString();
      _variables = variables;

      // Store configured types for auto-conversion
      _configuredIntegerType = context.ConfiguredIntegerType;
      _configuredFloatType = context.ConfiguredFloatType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      if (variablesOverride == null || !variablesOverride.TryGetValue(_variableName, out var variable))
      {
        if (_variables == null || !_variables.TryGetValue(_variableName, out variable))
        {
          throw CreateRuntimeException($"Undefined variable: {_variableName}");
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
          if (_configuredIntegerType == ExtendedType.Integer)
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
            case ExtendedType.Float:
              FloatValue = variable.FloatValue;
              break;
            case ExtendedType.Double:
              DoubleValue = variable.DoubleValue;
              break;
            case ExtendedType.Decimal:
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