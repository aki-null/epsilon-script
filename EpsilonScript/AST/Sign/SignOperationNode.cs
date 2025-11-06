using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Abstract base class for unary sign operations (+x, -x).
  /// Provides shared logic for applying sign operations to numeric values.
  /// </summary>
  internal abstract class SignOperationNode : Node
  {
    protected Node ChildNode;

    public override bool IsPrecomputable => ChildNode.IsPrecomputable;

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if (!rpnStack.TryPop(out ChildNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform sign operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      ChildNode.Execute(variablesOverride);

      if (!ChildNode.IsNumeric)
      {
        throw CreateRuntimeException("Sign of a non-numeric value cannot be changed");
      }

      switch (ChildNode.ValueType)
      {
        case ExtendedType.Integer:
          ApplySignInteger();
          break;
        case ExtendedType.Long:
          ApplySignLong();
          break;
        case ExtendedType.Float:
          ApplySignFloat();
          break;
        case ExtendedType.Double:
          ApplySignDouble();
          break;
        case ExtendedType.Decimal:
          ApplySignDecimal();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ChildNode.ValueType), ChildNode.ValueType,
            "Unsupported value type for sign change");
      }
    }

    /// <summary>
    /// Apply the sign operation to an integer value.
    /// </summary>
    protected abstract void ApplySignInteger();

    /// <summary>
    /// Apply the sign operation to a long value.
    /// </summary>
    protected abstract void ApplySignLong();

    /// <summary>
    /// Apply the sign operation to a float value.
    /// </summary>
    protected abstract void ApplySignFloat();

    /// <summary>
    /// Apply the sign operation to a double value.
    /// </summary>
    protected abstract void ApplySignDouble();

    /// <summary>
    /// Apply the sign operation to a decimal value.
    /// </summary>
    protected abstract void ApplySignDecimal();

    public override void Validate()
    {
      ChildNode?.Validate();
    }

    public override void ConfigureNoAlloc()
    {
      ChildNode?.ConfigureNoAlloc();
    }
  }
}