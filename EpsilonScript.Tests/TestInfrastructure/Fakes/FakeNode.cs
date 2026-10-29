using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  /// <summary>
  /// Leaf node with a fixed value, lowered to a constant register.
  /// </summary>
  internal abstract class FakeNode : Node
  {
    internal override int Lower(RegisterCompiler compiler) => compiler.LowerConstant(this);
  }

  /// <summary>
  /// Leaf node that runs <see cref="OnExecute"/> each time the VM evaluates it. It is lowered to a call of a
  /// non-deterministic custom function, which returns the value the node holds after the hook.
  /// </summary>
  internal abstract class ScriptedFakeNode : Node
  {
    protected abstract void OnExecute();

    internal override int Lower(RegisterCompiler compiler)
    {
      CustomFunction function;
      switch (ValueType)
      {
        case ExtendedType.Integer:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return IntegerValue;
          });
          break;
        case ExtendedType.Long:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return LongValue;
          });
          break;
        case ExtendedType.Float:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return FloatValue;
          });
          break;
        case ExtendedType.Double:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return DoubleValue;
          });
          break;
        case ExtendedType.Decimal:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return DecimalValue;
          });
          break;
        case ExtendedType.Boolean:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return BooleanValue;
          });
          break;
        case ExtendedType.String:
          function = CustomFunction.Create("fake", () =>
          {
            OnExecute();
            return StringValue;
          });
          break;
        default:
          throw new NotSupportedException($"Scripted fake nodes cannot hold {ValueType}");
      }

      var overload = new CustomFunctionOverload(function, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      return compiler.LowerCall(new List<Node>(), overload, null, Location);
    }
  }
}
